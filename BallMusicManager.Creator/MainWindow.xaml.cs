using Ametrin.Serialization;
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
    private readonly SongLibrary Library = SongLibrary.LoadOrNew();
    private TimeSpan Duration = TimeSpan.Zero;
    private MutableSong? DraggedItem;
    private readonly MusicPlayer _player = new();
    private readonly DispatcherTimer _timer = new();
    public MainWindow() {
        InitializeComponent();
        SongsGrid.ItemsSource = Playlist;
        LibraryGrid.ItemsSource = Library.Songs;
        Playlist.CollectionChanged += UpdateLengthDisplay;
        UpdateLengthDisplay();
        _timer.Interval = TimeSpan.FromSeconds(.5);
        _timer.Tick += UpdatePlaybackSliderValue;
    }

    private void Songs_Drop(object sender, DragEventArgs e) {
        if(e.Data.GetDataPresent(DataFormats.FileDrop)) {
            foreach(var song in FileDrop(e)) {
                Playlist.Add(song);
            }
            return;
        }

        if(DraggedItem is null || e.OriginalSource is not DependencyObject obj) return;

        var targetItem = obj.FindAncestorOrSelf<DataGridRow>()?.Item as MutableSong;
        if(targetItem is not null && !ReferenceEquals(DraggedItem, targetItem)) {
            var index = Playlist.IndexOf(targetItem);
            Playlist.Remove(DraggedItem);
            Playlist.Insert(index, DraggedItem);
        }
        DraggedItem = null;
    }

    private void SongsGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if(e.OriginalSource is not DependencyObject obj) return;
        var row = obj.FindAncestorOrSelf<DataGridRow>();
        if(row != null) {
            DraggedItem = row.Item as MutableSong;
        }
    }

    private void SongsGrid_PreviewMouseMove(object sender, MouseEventArgs e) {
        if(e.LeftButton == MouseButtonState.Pressed && DraggedItem != null) {
            DragDrop.DoDragDrop(SongsGrid, DraggedItem, DragDropEffects.Move);
        }
    }

    private void Save(object sender, RoutedEventArgs e) {
        var dialog = DialogUtils.SaveFileDialog(filterDescription: "Playlist", extension: "plz");
        if(dialog.ShowDialog() is not true) return;

        PlaylistBuilder.ToArchive(new(dialog.FileName), Playlist);
    }

    private void Open(object sender, RoutedEventArgs e) {
        var dialog = DialogUtils.GetFileDialog(filterDescription: "Playlists", extension: "plz");
        if(dialog.ShowDialog() is not true) return;

        PlaylistBuilder.EnumerateArchive(new(dialog.FileName)).Resolve(songs => {
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
        SongCache.Clear();
    }

    private void UnsortPlaylist(object sender, RoutedEventArgs e) {
        CollectionViewSource.GetDefaultView(SongsGrid.ItemsSource).SortDescriptions.Clear();
    }

    private void PlayKeyPressed(object sender, KeyEventArgs e) {
        if(e.Key is not Key.Space) return;

        if(_player.IsPlaying) {
            _player.Pause();
            _timer.Stop();
            return;
        }

        var song = (sender as DependencyObject)?.FindChild<DataGrid>()?.SelectedItem as MutableSong;

        UpdatePlayback(song);
        song?.SetDuration(_player.CurrentSongLength);
        _timer.Start();
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

    private void LibraryDrop(object sender, DragEventArgs e) {
        if(!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        foreach(var _ in FileDrop(e)) { }
    }


    private IEnumerable<MutableSong> FileDrop(DragEventArgs e) {
        if(!e.Data.GetDataPresent(DataFormats.FileDrop)) yield break;

        var files = (e.Data.GetData(DataFormats.FileDrop) as string[] ?? []).Select(path=> new FileInfo(path));
        foreach(var path in files) {
            var window = new AddSongWindow(path) {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if(window.ShowDialog() is not true) continue;
            AddSongToLib(window.Song);
            yield return window.Song;
        }
    }

    private void AddSongToLib(MutableSong song) {
        Library.AddIfNew(song);
    }
}