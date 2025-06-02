using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using BallMusic.WPF;
using BallMusic.WPF.FileDialogs;

namespace BallMusic.Creator;

public sealed partial class MainWindow : Window
{
    private SongBuilderCollection Playlist
    {
        get;
        set
        {
            Playlist.CollectionChanged -= UpdateLengthDisplay;
            Playlist.CollectionChanged -= UpdateDashboard;
            field = value;
            PlaylistGrid.ItemsSource = Playlist;
            Playlist.CollectionChanged += UpdateLengthDisplay;
            Playlist.CollectionChanged += UpdateDashboard;
            UpdateLengthDisplay();
            UpdateDashboard();
        }
    } = [];
    private SongLibrary Library = [];
    private volatile bool _isSaving = false;

    public MainWindow()
    {
        InitializeComponent();
        PlaylistGrid.ItemsSource = Playlist;
        Playlist = [];
        //Playlist.CollectionChanged += UpdateLengthDisplay;
        _playbackProgressUpdater.Tick += UpdatePlaybackSliderValue;

        Loaded += async (sender, e) =>
        {
            UpdateLengthDisplay();
            await LoadLibrary();

            SongCache.EnsureCacheExists();

            if (playlistBackup.Exists && MessageBoxHelper.Ask("Looks like the program crashed.\nDo you want to restore the playlist?") is MessageBoxResult.Yes)
            {
                var songs = Library.AddAllOrReplaceWithExisting(PlaylistBuilder.EnumeratePlaylistFile(playlistBackup));
                Playlist = new(songs);
            }
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
        if (_isSaving)
        {
            e.Cancel = true;
            return;
        }

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

    private async void SavePlaylist(object sender, RoutedEventArgs e)
    {
        if (_isSaving)
        {
            return;
        }

        var dialog = new SaveFileDialog().AddExtensionFilter("Playlist", ".plz");

        await dialog.GetFileInfo().ConsumeAsync(async file =>
        {

            _isSaving = true;
            var bar = new LoadingBar(true)
            {
                Owner = this,
            };
            bar.LabelProgress.Report("Saving Playlist...");
            bar.Show();


            (await Task.Run(() => PlaylistBuilder.ToArchive(file, Playlist))).Consume(error: e => MessageBoxHelper.ShowError($"Failed saving Playlist:\n{e.Message}", owner: this));

            bar.Close();
            _isSaving = false;
        });
    }

    private async void OpenPlaylist(object sender, RoutedEventArgs e)
    {
        await new OpenFileDialog().AddExtensionFilter("Playlist", ".plz").GetFileInfo().ConsumeAsync(async file =>
        {
            var bar = new LoadingBar(true)
            {
                Owner = this,
            };
            bar.LabelProgress.Report("Loading Playlist...");
            bar.Show();

            var songs = await Task.Run(() => PlaylistBuilder.EnumerateArchiveEntries(file));

            songs.Map(songs => Library.AddAllOrReplaceWithExisting(songs)).Consume(songs =>
            {
                Playlist = new(songs);
            },
            error =>
            {
                _ = error switch
                {
                    FileNotFoundException => MessageBoxHelper.ShowError($"Could not find\t{file}", owner: this),
                    _ => MessageBoxHelper.ShowError($"Failed opening Playlist:\n{error.Message}", owner: this),
                };
            });

            bar.Close();
        });
    }

    private void OpenPlaylistLegacy(object sender, RoutedEventArgs e)
    {
        new OpenFileDialog().AddExtensionFilter("Playlist", ".playlist").GetFileInfo().Consume(file =>
        {
            var songs = Library.AddAllOrReplaceWithExisting(PlaylistBuilder.EnumeratePlaylistFile(file));
            Playlist = new(songs);
        });
    }

    private void UpdateLengthDisplay(object? sender = null, NotifyCollectionChangedEventArgs? e = default)
    {
        var durationInTicks = Playlist.Sum(s => s.Duration.Ticks);
        LengthDisplay.Content = durationInTicks == 0 ? "Playlist" : $"Playlist ({TimeSpan.FromTicks(durationInTicks):hh\\:mm\\:ss})";
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
        if (Library.Count <= 0 || _isSaving)
        {
            return;
        }
        _isSaving = true;
        IsEnabled = false;
        var bar = new LoadingBar(true)
        {
            Owner = this,
        };
        bar.LabelProgress.Report("Saving Library...");
        bar.Show();
        await Task.Run(Library!.Save);
        bar.Close();
        _isSaving = false;
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
                dashboard = null;
            }
        }

        if (dashboard is null)
        {
            dashboard = new Dashboard([.. Playlist.Select(song => song.Build())]);
            dashboard.Show();
        }
    }

    private void ReplaceFile_Click(object sender, RoutedEventArgs e)
    {
        if (LibraryGrid.SelectedItems.Count is 0)
        {
            return;
        }

        if (LibraryGrid.SelectedItems.Count is > 1)
        {
            MessageBoxHelper.ShowWaring("More than one Song selected.\nPlease select only one song to replace");
            return;
        }

        var song = (SongBuilder)LibraryGrid.SelectedItems[0]!;

        var fileDialog = new OpenFileDialog
        {
            Title = $"Select file to override {song.Title} ({song.Dance}) by {song.Artist}",
        };

        fileDialog.AddExtensionFilter("Audio Files", string.Join(";*", PlaylistBuilder.AllowedFileTypes));
        fileDialog.AddFilter(FileFilter.AllFiles);

        var fileInfo = fileDialog.GetFileInfo();

        fileInfo.Consume(fileInfo =>
        {
            if (!PlaylistBuilder.ValidFile(fileInfo))
            {
                MessageBoxHelper.ShowError("Invalid file extension", owner: this);
                return;
            }
            song.Path = new FileLocation(fileInfo);
        });
    }

    private void ExportFile_Click(object sender, RoutedEventArgs e)
    {
        foreach (var song in LibraryGrid.SelectedItems.Cast<SongBuilder>())
        {
            using var sourceStream = SongCache.CacheFromArchive(song).OpenRead();

            var fileExtension = TryGetFileExtension(sourceStream).Or("");

            var fileDialog = new SaveFileDialog
            {
                Title = $"Save {song.Title} ({song.Dance}) by {song.Artist}",
                DefaultFileName = $"{song.Title}_{song.Dance}{fileExtension}", // this allows for a quick reimport (see SongBuilder.FromFileName) 
            };
            var targetFile = fileDialog.GetPath();

            // SaveFileDialog already asks the user whether they want to override the selected file

            targetFile.Consume(success: (targetFile) =>
            {
                using (var targetStream = File.Create(targetFile))
                {
                    sourceStream.CopyTo(targetStream);
                }

                TryWriteMetadata(targetFile, song);
            });
        }

        static Option<string> TryGetFileExtension(Stream stream)
        {
            const int HEADER_SIZE = 16;
            Span<byte> buffer = stackalloc byte[HEADER_SIZE];
            stream.ReadExactly(buffer);
            stream.Seek(-HEADER_SIZE, SeekOrigin.Current);

            return buffer switch
            {
                [0x49, 0x44, 0x33, ..] => ".mp3", // 'ID3'
                [0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C, ..] => ".wma", // guid
                [0x52, 0x49, 0x46, 0x46, _, _, _, _, 0x57, 0x41, 0x56, 0x45, ..] => ".wav", // 'FIFF....WAVE'
                [_, _, _, _, 0x66, 0x74, 0x79, 0x70, 0x4D, 0x34, 0x41, ..] => ".m4a", // '....ftypM4A....'
                [_, _, _, _, 0x66, 0x74, 0x79, 0x70, 0x64, 0x61, 0x73, 0x68, ..] => ".mp4", // '....ftypdash....'
                _ => Option.Error<string>(),
            };
        }

        static void TryWriteMetadata(string targetFile, SongBuilder song)
        {
            try
            {
                using var songFile = TagLib.File.Create(targetFile);
                songFile.Tag.Title = song.Title;
                songFile.Tag.Performers = song.Artist.Split(", ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                songFile.Save();
            }
            catch (Exception)
            {
                // fails when the target file has no extension
            }
        }
    }

    private static readonly FileInfo playlistBackup = SongCache.CacheDirectory.File("$quicksave.json");
    private void QuickSavePlaylist()
    {
        using var stream = playlistBackup.Create();
        JsonSerializer.Serialize(stream, Playlist);
    }

    private void UpdateDashboard(object? sender = null, EventArgs? e = default) => dashboard?.Update([.. Playlist.Select(song => song.Build())]);
}