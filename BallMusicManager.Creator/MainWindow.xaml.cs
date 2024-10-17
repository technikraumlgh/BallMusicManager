using Ametrin.Utils;
using Ametrin.Utils.WPF;
using Ametrin.Utils.WPF.FileDialogs;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace BallMusicManager.Creator;

public sealed partial class MainWindow : Window
{
    private SongBuilderCollection Playlist = [];
    private SongLibrary Library = [];
    private TimeSpan Duration = TimeSpan.Zero;
    private readonly MusicPlayer _player = new();
    private readonly DispatcherTimer _playbackProgressUpdater = new();
    private SongBuilder? _draggedSong;
    private SongBuilder? _lastPlayed;
    private SongBuilder? _selectedSong;
    private SongBuilderCollection? _selectionContext;

    public MainWindow()
    {
        InitializeComponent();
        SongsGrid.ItemsSource = Playlist;
        Playlist.CollectionChanged += UpdateLengthDisplay;
        _playbackProgressUpdater.Interval = TimeSpan.FromSeconds(.5);
        _playbackProgressUpdater.Tick += UpdatePlaybackSliderValue;

        Loaded += (sender, e) =>
        {
            UpdateLengthDisplay();
            _ = LoadLibrary();
        };

        Closing += OnWindowClosingAsync;
    }

    private bool shouldSaveBeforeClosing = true;
    private async void OnWindowClosingAsync(object? sender, CancelEventArgs e)
    {
        if (shouldSaveBeforeClosing)
        {
            e.Cancel = true;
            await SaveLibrary();
            shouldSaveBeforeClosing = false;
            Close();
        }
    }

    private void SavePlaylist(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog().AddExtensionFilter("Playlist", "plz");

        dialog.GetFileInfo().Consume((file) => PlaylistBuilder.ToArchive(file, Playlist));
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

            var songs = await Task.Run(() => PlaylistBuilder.EnumerateArchive(file));

            songs.Select(songs => Library.AddAllOrReplaceWithExisting(songs)).Consume(songs =>
            {
                Playlist.CollectionChanged -= UpdateLengthDisplay;
                Playlist = new(songs);
                SongsGrid.ItemsSource = Playlist;
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
            SongsGrid.ItemsSource = Playlist;
            Playlist.CollectionChanged += UpdateLengthDisplay;
            UpdateLengthDisplay();
        });
    }

    private void UpdateLengthDisplay(object? sender = null, NotifyCollectionChangedEventArgs? e = default)
    {
        Duration = Playlist.Sum(s => s.Duration);
        LengthDisplay.Content = $"Duration: {Duration:hh\\:mm\\:ss}";
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

    private void UpdatePlaybackSliderValue(object? sender, EventArgs e)
    {
        PlaybackSlider.ValueChanged -= PlaybackSlider_ValueChanged;
        PlaybackSlider.Value = _player.CurrentTime.TotalSeconds;
        PlaybackSlider.ValueChanged += PlaybackSlider_ValueChanged;
    }

    private void SongsGrid_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Delete)
        {
            if (_selectedSong is not null && _selectionContext == Playlist)
            {
                _selectionContext.Remove(_selectedSong);
                SongsGrid.SelectedItem = null;
                _selectionContext = null;
                _selectedSong = null;
            }
        }
    }

    private void LibraryGrid_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Delete)
        {
            if (_selectedSong is not null && _selectionContext == Library)
            {
                if (Playlist.Contains(_selectedSong))
                {
                    MessageBoxHelper.ShowWaring("Cannot delete a song that is in the active playlist");
                    return;
                }

                if (MessageBoxHelper.Ask($"Do you really want to delete {_selectedSong.Title}?") is MessageBoxResult.Yes)
                {
                    _selectionContext.Remove(_selectedSong);
                    LibraryGrid.SelectedItem = null;
                    _selectionContext = null;
                    _selectedSong = null;
                }
            }
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
        var bar = new LoadingBar(true)
        {
            Owner = this,
        };
        bar.LabelProgress.Report("Saving Library...");
        IsEnabled = false;
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

    private void Search_TextChanged(object sender, TextChangedEventArgs e)
    {
        var view = CollectionViewSource.GetDefaultView(LibraryGrid.ItemsSource);
        if (string.IsNullOrWhiteSpace(SearchBox.Text))
        {
            SearchLabel.Visibility = Visibility.Visible;
            view.Filter = null;
        }
        else
        {
            SearchLabel.Visibility = Visibility.Collapsed;
            view.Filter = (obj) =>
            {
                if (obj is SongBuilder song)
                {
                    var compareInfo = CultureInfo.CurrentCulture.CompareInfo;
                    return compareInfo.IndexOf(song.Title, SearchBox.Text, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols) >= 0 ||
                           compareInfo.IndexOf(song.Artist, SearchBox.Text, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols) >= 0;
                }

                return false;
            };
        }
    }

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        if (_player.IsPlaying)
        {
            PausePlayback();
        }
        else
        {
            PlaySelected();
        }
    }

    private void PlaybackSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_player.IsPlaying)
        {
            return;
        }

        if (_selectedSong != _lastPlayed)
        {
            //PausePlayback();
        }
    }

    private void PlaybackSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _player.CurrentTime = TimeSpan.FromSeconds(e.NewValue);
    }

    private void SongsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_player.IsPlaying)
        {
            PausePlayback();
        }
        else
        {
            PlaySelected();
        }
    }

    private void PlaySelected()
    {
        if (_selectedSong is null)
        {
            return;
        }

        UpdatePlayback(_selectedSong);
        _selectedSong?.SetDuration(_player.CurrentSongLength);
        _player.Play();
        _playbackProgressUpdater.Start();
        PlayButton.Content = "\uE769";
    }

    private void PausePlayback()
    {
        _player.Pause();
        PlayButton.Content = "\uE768";
        _playbackProgressUpdater.Stop();
    }

    private void UpdatePlayback(SongBuilder? song)
    {
        if (song is null)
        {
            return;
        }

        if (song != _lastPlayed)
        {
            _player.SetSong(song.Build());
            _lastPlayed = song;
            PlaybackSlider.Maximum = _player.CurrentSongLength.TotalSeconds;
        }
    }
}