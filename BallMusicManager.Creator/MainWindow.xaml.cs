using Ametrin.Utils.WPF;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.Collections.ObjectModel;
using Ametrin.Utils;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Windows.Threading;
using Ametrin.Utils.Optional;

namespace BallMusicManager.Creator; 
public sealed partial class MainWindow : Window {
    private ObservableCollection<MutableSong> Playlist = [];
    private SongLibrary Library;
    private TimeSpan Duration = TimeSpan.Zero;
    private readonly MusicPlayer _player = new();
    private readonly DispatcherTimer _timer = new();
    private MutableSong? _draggedSong;
    private MutableSong? _lastPlayed;
    private MutableSong? _selectedSong;
    public MainWindow() {
        InitializeComponent();
        SongsGrid.ItemsSource = Playlist;
        Playlist.CollectionChanged += UpdateLengthDisplay;
        UpdateLengthDisplay();
        _timer.Interval = TimeSpan.FromSeconds(.5);
        _timer.Tick += UpdatePlaybackSliderValue;

        Loaded += async (sender, e) => {

            var bar = new LoadingBar(true) {
                Owner = this,
            };
            bar.LabelProgress.Report("Loading Library...");
            bar.Show();
            
            Library = await Task.Run(SongLibrary.LoadOrNew);
            App.OnAppExit += Library.Save;
            LibraryGrid.ItemsSource = Library.Songs;
            
            bar.Close();
        };
    }


    private void Save(object sender, RoutedEventArgs e) {
        var dialog = DialogUtils.SaveFileDialog(filterDescription: "Playlist", extension: "plz");
        if(dialog.ShowDialog() is not true) return;

        PlaylistBuilder.ToArchive(new(dialog.FileName), Playlist);
    }

    private async void Open(object sender, RoutedEventArgs e) {
        var dialog = DialogUtils.GetFileDialog(filterDescription: "Playlists", extension: "plz");
        if(dialog.ShowDialog() is not true) return;

        var bar = new LoadingBar(true) {
            Owner = this,
        };
        bar.LabelProgress.Report("Loading Playlist...");
        bar.Show();

        var songs = await Task.Run(() => PlaylistBuilder.EnumerateArchive(new(dialog.FileName)));

        songs.Resolve(songs => {
            Playlist.CollectionChanged -= UpdateLengthDisplay;
            Playlist = new(songs);
            SongsGrid.ItemsSource = Playlist;
            Playlist.CollectionChanged += UpdateLengthDisplay;
            UpdateLengthDisplay();
            Library.AddAllIfNew(songs);
        }, flag => {
            _ = flag switch {
                ResultFlag.PathNotFound => MessageBox.Show($"Could not find {dialog.FileName}"),
                ResultFlag.InvalidFile => MessageBox.Show($"{dialog.FileName} is not a valid playlist"),
                _ => MessageBox.Show("Failed opening Playlist"),
            };
        });

        bar.Close();
    }
    
    private void OpenLegacy(object sender, RoutedEventArgs e) {
        var dialog = DialogUtils.GetFileDialog(filterDescription: "Playlists", extension: "playlist");
        if(dialog.ShowDialog() is not true) return;

        var songs = PlaylistBuilder.EnumerateFile(new(dialog.FileName));
        Playlist.CollectionChanged -= UpdateLengthDisplay;
        Playlist = new(songs);
        SongsGrid.ItemsSource = Playlist;
        Playlist.CollectionChanged += UpdateLengthDisplay;
        UpdateLengthDisplay();
        Library.AddAllIfNew(songs);
    }

    private void DeleteSong(object sender, RoutedEventArgs e) {
        if((sender as MenuItem)!.Parent is not ContextMenu menu) return;
        if(menu.PlacementTarget is not DataGridRow row) return;

        var song = row.Item as MutableSong;
        Playlist.Remove(song!);
    }

    private void UpdateLengthDisplay(object? sender = null, NotifyCollectionChangedEventArgs? e = default) {
        Duration = Playlist.Sum(s => s.Duration);
        LengthDisplay.Content = $"Duration: {Duration:hh\\:mm\\:ss}";
    }

    private void Close(object sender, RoutedEventArgs e) {
        _player.Pause();
        if(Playlist.Count == 0) return;

        var shouldSave = MessageBox.Show("Save the current Playlist?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        switch(shouldSave) {
            case MessageBoxResult.Cancel: 
                return;
            case MessageBoxResult.Yes:
                Save(sender, e);
                break;
        }

        Playlist.Clear();
    }

    private void UnsortPlaylist(object sender, RoutedEventArgs e) {
        CollectionViewSource.GetDefaultView(SongsGrid.ItemsSource).SortDescriptions.Clear();
    }

    private void PlayKeyPressed(object sender, KeyEventArgs e) {
        if(e.Key is not Key.Space || _selectedSong is null) return;

        if(_player.IsPlaying) {
            _player.Pause();
            _timer.Stop();
        }

        if(_selectedSong == _lastPlayed) return;

        UpdatePlayback(_selectedSong);
        _selectedSong?.SetDuration(_player.CurrentSongLength);
        _timer.Start();
        _lastPlayed = _selectedSong;
    }

    private void PlaybackSelectionChanged(object sender, SelectionChangedEventArgs e) {
        if(!_player.IsPlaying) return;
        UpdatePlayback((sender as DataGrid)?.SelectedItem as Song);
    }


    private void PlaybackSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
        _player.CurrentTime = TimeSpan.FromSeconds(e.NewValue);
    }

    private void UpdatePlayback(ISong? song) {
        if(song is null) return;
        
        _player.SetSong(song);
        PlaybackSlider.Maximum = _player.CurrentSongLength.TotalSeconds;
        _player.Play();
    }

    private void UpdatePlaybackSliderValue(object? sender, EventArgs e) {
        PlaybackSlider.ValueChanged -= PlaybackSlider_ValueChanged;
        PlaybackSlider.Value = _player.CurrentTime.TotalSeconds;
        PlaybackSlider.ValueChanged += PlaybackSlider_ValueChanged;
    }


    #region Playlist Drag&Drop
    private void Songs_Drop(object sender, DragEventArgs e) {
        if(e.Data.GetDataPresent(DataFormats.FileDrop)) {
            foreach(var song in FileDrop(e)) {
                Playlist.Add(song);
            }
            return;
        }

        if(_draggedSong is null || e.OriginalSource is not DependencyObject obj)
            return;

        var targetItem = obj.FindParentOrSelf<DataGridRow>()?.Item as MutableSong;
        if(targetItem is null) {
            Playlist.Remove(_draggedSong);
            Playlist.Add(_draggedSong);
        }else if(!ReferenceEquals(_draggedSong, targetItem)){
            var index = Playlist.IndexOf(targetItem);
            Playlist.Remove(_draggedSong);
            Playlist.Insert(index, _draggedSong);
        }
        _draggedSong = null;
    }

    private void SongsGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if(e.OriginalSource is not DependencyObject obj) return;

        var row = obj.FindParentOrSelf<DataGridRow>();
        if(row?.Item is MutableSong song) {
            _draggedSong = song;
            _selectedSong = song;
        }
    }

    private void SongsGrid_PreviewMouseMove(object sender, MouseEventArgs e) {
        if(e.LeftButton == MouseButtonState.Pressed && _draggedSong != null) {
            DragDrop.DoDragDrop(SongsGrid, _draggedSong, DragDropEffects.Move);
        }
    }
    #endregion

    #region Library Drag&Drop
    private void LibraryDrop(object sender, DragEventArgs e) {
        if(e.Data.GetDataPresent(DataFormats.FileDrop)) {
            foreach(var _ in FileDrop(e)) { }
            return;
        }
    }

    private void LibraryGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => SongsGrid_MouseLeftButtonDown(sender, e);

    private void LibraryGrid_PreviewMouseMove(object sender, MouseEventArgs e) => SongsGrid_PreviewMouseMove(sender, e);
    #endregion

    private IEnumerable<MutableSong> FileDrop(DragEventArgs e) {
        if(!e.Data.GetDataPresent(DataFormats.FileDrop)) yield break;

        var paths = (e.Data.GetData(DataFormats.FileDrop) as string[] ?? []);

        var files = paths.SelectMany(path => Directory.Exists(path)
            ? Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
            : [path]).Where(File.Exists)
            .Select(file => new FileInfo(file));


        foreach(var file in files.Where(file => file.Exists).Where(PlaylistBuilder.ValidFile)) {
            var window = new AddSongWindow(file) {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if(window.ShowDialog() is not true) 
                continue;
            Library.AddIfNew(window.Song);
            yield return window.Song;
        }
    }
}