using Ametrin.Utils.WPF;
using Ametrin.Utils.WPF.FileDialogs;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace BallMusicManager.Creator;

public sealed partial class MainWindow : Window
{
    private SongBuilderCollection Playlist = [];
    private SongLibrary Library = [];

    public MainWindow()
    {
        InitializeComponent();
        PlaylistGrid.ItemsSource = Playlist;
        Playlist.CollectionChanged += UpdateLengthDisplay;
        _playbackProgressUpdater.Tick += UpdatePlaybackSliderValue;

        Loaded += (sender, e) =>
        {
            UpdateLengthDisplay();
            _ = LoadLibrary();
        };

        Closing += OnWindowClosingAsync;

        Closed += (sender, args) =>
        {
            dashboard?.Close();
        };
    }

    private bool shouldSaveBeforeClosing = true;
    private async void OnWindowClosingAsync(object? sender, CancelEventArgs e)
    {
        if (shouldSaveBeforeClosing)
        {
            var task = SaveLibrary();
            if (task.IsCompleted)
            {
                return;
            }
            e.Cancel = true;
            await task;
            shouldSaveBeforeClosing = false;
            Close();
        }
    }

    private void SavePlaylist(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog().AddExtensionFilter("Playlist", "plz");

        dialog.GetFileInfo().Consume(async file =>
        {

            var bar = new LoadingBar(true)
            {
                Owner = this,
            };
            bar.LabelProgress.Report("Saving Playlist...");
            bar.Show();


            (await Task.Run(() => PlaylistBuilder.ToArchive(file, Playlist))).Consume(
                error: () =>
                {
                    MessageBoxHelper.ShowError($"Failed saving Playlist", owner: this);
                }
            );

            bar.Close();
        });
    }

    private void OpenPlaylist(object sender, RoutedEventArgs e)
    {
        new OpenFileDialog().AddExtensionFilter("Playlist", "plz").GetFileInfo().Consume(async file =>
        {
            var bar = new LoadingBar(true)
            {
                Owner = this,
            };
            bar.LabelProgress.Report("Loading Playlist...");
            bar.Show();

            var songs = await Task.Run(() => PlaylistBuilder.EnumerateArchiveEntries(file));

            songs.Select(songs => Library.AddAllOrReplaceWithExisting(songs)).Consume(songs =>
            {
                Playlist.CollectionChanged -= UpdateLengthDisplay;
                Playlist = new(songs);
                PlaylistGrid.ItemsSource = Playlist;
                Playlist.CollectionChanged += UpdateLengthDisplay;
                UpdateLengthDisplay();
            },
            error =>
            {
                _ = error switch
                {
                    FileNotFoundException => MessageBoxHelper.ShowError($"Could not find {file}", owner: this),
                    InvalidDataException => MessageBoxHelper.ShowError($"{file} is not a valid playlist file", owner: this),
                    _ => MessageBoxHelper.ShowError($"Failed opening Playlist ({error.Message})", owner: this),
                };
            });

            bar.Close();
        });
    }

    private void OpenPlaylistLegacy(object sender, RoutedEventArgs e)
    {
        new OpenFileDialog().AddExtensionFilter("Playlist", "playlist").GetFileInfo().Consume(file =>
        {
            var songs = Library.AddAllOrReplaceWithExisting(PlaylistBuilder.EnumerateFile(file));
            Playlist.CollectionChanged -= UpdateLengthDisplay;
            Playlist = new(songs);
            PlaylistGrid.ItemsSource = Playlist;
            Playlist.CollectionChanged += UpdateLengthDisplay;
            UpdateLengthDisplay();
        });
    }

    private void UpdateLengthDisplay(object? sender = null, NotifyCollectionChangedEventArgs? e = default)
    {
        var duration = Playlist.Sum(s => s.Duration);
        LengthDisplay.Content = duration.Ticks == 0 ? "Playlist" : $"Playlist ({duration:hh\\:mm\\:ss})";
    }

    private void ClosePlaylist(object sender, RoutedEventArgs e)
    {
        _player.Pause();
        if (Playlist.Count == 0)
        {
            return;
        }

        var shouldSave = MessageBox.Show("Save the current Playlist?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        switch (shouldSave)
        {
            case MessageBoxResult.Cancel:
                return;
            case MessageBoxResult.Yes:
                SavePlaylist(sender, e);
                break;
        }

        Playlist.Clear();
    }

    private void PlaylistGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Delete || PlaylistGrid.SelectedItem is not SongBuilder song)
        {
            return;
        }

        e.Handled = true;
        Playlist.Remove(song);
    }

    private void LibraryGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Delete || LibraryGrid.SelectedItem is not SongBuilder song || IsLibraryEditing())
        {
            return;
        }

        e.Handled = true;

        if (Playlist.Contains(song))
        {
            MessageBoxHelper.ShowWaring("Cannot delete a song that is in the current playlist");
            return;
        }

        if (MessageBoxHelper.Ask($"Do you really want to delete '{song.Title}'?") is MessageBoxResult.Yes)
        {
            Library.Remove(song);
        }

        bool IsLibraryEditing()
        {
            if (LibraryGrid.ItemContainerGenerator.ContainerFromItem(LibraryGrid.SelectedItem) is DataGridRow selectedRow)
            {
                if (selectedRow.IsEditing)
                {
                    return true;
                }
            }

            foreach (var item in LibraryGrid.Items)
            {
                if (LibraryGrid.ItemContainerGenerator.ContainerFromItem(item) is not DataGridRow row)
                {
                    continue;
                }

                if (row.IsEditing)
                {
                    return true;
                }

            }
            return false;
        }
    }

    private async Task LoadLibrary()
    {
        var bar = new LoadingBar(true)
        {
            Owner = this,
        };
        bar.LabelProgress.Report("Loading Library...");
        IsEnabled = false;
        bar.Show();

        Library = await Task.Run(SongLibrary.LoadOrNew);
        LibraryGrid.ItemsSource = Library;
        bar.Close();
        IsEnabled = true;
    }

    private async Task SaveLibrary()
    {
        if (Library.Count <= 0)
        {
            return;
        }
        var bar = new LoadingBar(true)
        {
            Owner = this,
        };
        bar.LabelProgress.Report("Saving Library...");
        bar.Show();
        await Task.Run(Library!.Save);
        bar.Close();
        IsEnabled = true;
    }

    private void LibrarySaveClick(object sender, RoutedEventArgs e)
    {
        _ = SaveLibrary();
    }

    private void LibraryReloadClick(object sender, RoutedEventArgs e)
    {
        _ = LoadLibrary();
    }

    const CompareOptions SEARCH_SETTINGS = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols;
    private void Search_TextChanged(object sender, TextChangedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(LibraryGrid.ItemsSource);
        if (string.IsNullOrWhiteSpace(SearchBox.Text))
        {
            SearchLabel.Visibility = Visibility.Visible;
            view.Filter = null;
            return;
        }

        SearchLabel.Visibility = Visibility.Collapsed;
        view.Filter = (obj) =>
        {
            if (obj is SongBuilder song)
            {
                var compareInfo = CultureInfo.CurrentCulture.CompareInfo;
                return compareInfo.IndexOf(song.Title, SearchBox.Text, SEARCH_SETTINGS) >= 0 ||
                        compareInfo.IndexOf(song.Artist, SearchBox.Text, SEARCH_SETTINGS) >= 0;
            }

            return false;
        };
    }

    private Dashboard? dashboard = null;
    private void OpenDashboard_Click(object sender, RoutedEventArgs e)
    {
        if (dashboard is not null)
        {
            if (!dashboard.Activate())
            {
                dashboard.Activated -= UpdateDashboard;
                dashboard = null;
            }
        }

        if (dashboard is null)
        {
            dashboard = new Dashboard(Playlist.Select(song => song.Build()).ToImmutableArray());
            dashboard.Show();
            dashboard.Activated += UpdateDashboard;
        }
    }


    private void UpdateDashboard(object? sender = null, EventArgs? e = default) => dashboard?.Update(Playlist.Select(song => song.Build()).ToImmutableArray());
}