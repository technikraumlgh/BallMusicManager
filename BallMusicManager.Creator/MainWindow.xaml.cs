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
using Ametrin.Utils.WPF.FileDialogs;

namespace BallMusicManager.Creator; 
public sealed partial class MainWindow : Window {
    private ObservableCollection<SongBuilder> Playlist = [];
    private SongLibrary Library = default!;
    private TimeSpan Duration = TimeSpan.Zero;
    private readonly MusicPlayer _player = new();
    private readonly DispatcherTimer _playbackProgressUpdater = new();
    private SongBuilder? _draggedSong;
    private SongBuilder? _lastPlayed;
    private SongBuilder? _selectedSong;
    public MainWindow() {
        InitializeComponent();
        SongsGrid.ItemsSource = Playlist;
        Playlist.CollectionChanged += UpdateLengthDisplay;
        UpdateLengthDisplay();
        _playbackProgressUpdater.Interval = TimeSpan.FromSeconds(.5);
        _playbackProgressUpdater.Tick += UpdatePlaybackSliderValue;

        Loaded += async (sender, e) => {

            var bar = new LoadingBar(true) {
                Owner = this,
            };
            bar.LabelProgress.Report("Loading Library...");
            bar.Show();
            
            Library = await Task.Run(SongLibrary.LoadOrNew);
            LibraryGrid.ItemsSource = Library.Songs;
            bar.Close();

        };

        Closed += (sender, e) =>
        {
            var bar = new LoadingBar(true);
            bar.LabelProgress.Report("Saving Library...");
            bar.Show();
            //cannot run async because cleanup is deleting the cached files
            Library!.Save();
            bar.Close();
        };
    }

    private void Save(object sender, RoutedEventArgs e) {
        var dialog = new SaveFileDialog().AddExtensionFilter("Playlist", "plz");

        dialog.GetFileInfo().Resolve((file) => PlaylistBuilder.ToArchive(file, Playlist));
    }

    private void Open(object sender, RoutedEventArgs e) {
        new OpenFileDialog().AddExtensionFilter("Playlist", "plz").GetFileInfo().Resolve(async file =>
        {
            var bar = new LoadingBar(true)
            {
                Owner = this,
            };
            bar.LabelProgress.Report("Loading Playlist...");
            bar.Show();

            var songs = await Task.Run(() => PlaylistBuilder.EnumerateArchive(file));

            songs.Resolve(songs => {
                Playlist.CollectionChanged -= UpdateLengthDisplay;
                Playlist = new(songs);
                SongsGrid.ItemsSource = Playlist;
                Playlist.CollectionChanged += UpdateLengthDisplay;
                UpdateLengthDisplay();
                Library.AddAllIfNew(songs);
            }, flag => {
                _ = flag switch
                {
                    ResultFlag.PathNotFound => MessageBox.Show($"Could not find {file}"),
                    ResultFlag.InvalidFile => MessageBox.Show($"{file} is not a valid playlist file"),
                    _ => MessageBox.Show("Failed opening Playlist"),
                };
            });

            bar.Close();
        });
    }
    
    private void OpenLegacy(object sender, RoutedEventArgs e) 
    {
        new OpenFileDialog().AddExtensionFilter("Playlist", "playlist").GetFileInfo().Resolve(file =>
        {
            var songs = PlaylistBuilder.EnumerateFile(file);
            Playlist.CollectionChanged -= UpdateLengthDisplay;
            Playlist = new(songs);
            SongsGrid.ItemsSource = Playlist;
            Playlist.CollectionChanged += UpdateLengthDisplay;
            UpdateLengthDisplay();
            Library.AddAllIfNew(songs);
        });
    }

    private void DeleteSong(object sender, RoutedEventArgs e) 
    {
        if((sender as MenuItem)!.Parent is not ContextMenu menu) return;
        if(menu.PlacementTarget is not DataGridRow row) return;

        var song = row.Item as SongBuilder;
        Playlist.Remove(song!);
    }

    private void UpdateLengthDisplay(object? sender = null, NotifyCollectionChangedEventArgs? e = default) 
    {
        Duration = Playlist.Sum(s => s.Duration);
        LengthDisplay.Content = $"Duration: {Duration:hh\\:mm\\:ss}";
    }

    private void ClosePlaylist(object sender, RoutedEventArgs e) {
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

       
    }

    private void PlaybackSelectionChanged(object sender, SelectionChangedEventArgs e) {
        if(!_player.IsPlaying) return;

        if(_selectedSong != _lastPlayed)
        {
            _player.Stop();
        }
    }


    private void PlaybackSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
        _player.CurrentTime = TimeSpan.FromSeconds(e.NewValue);
    }

    private void UpdatePlayback(SongBuilder? song) {
        if(song is null) return;
        
        _player.SetSong(song.Build());
        _lastPlayed = song;
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

        var targetItem = obj.FindParentOrSelf<DataGridRow>()?.Item as SongBuilder;
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

    private void SongsGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if(e.OriginalSource is not DependencyObject obj)
        {
            return;
        }

        var row = obj.FindParentOrSelf<DataGridRow>();
        if(row?.Item is not SongBuilder song)
        {
            return;
        }

        _draggedSong = song;
        _selectedSong = song;
        LibraryGrid.SelectedItem = song;
        SongsGrid.SelectedItem = song;
    }

    private void SongsGrid_PreviewMouseMove(object sender, MouseEventArgs e) {
        if(e.LeftButton == MouseButtonState.Pressed && _draggedSong != null) {
            try
            {
                DragDrop.DoDragDrop(SongsGrid, _draggedSong, DragDropEffects.Move);
            }
            catch
            {

            }
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

    private IEnumerable<SongBuilder> FileDrop(DragEventArgs e) 
    {
        if(!e.Data.GetDataPresent(DataFormats.FileDrop)) yield break;

        var paths = (e.Data.GetData(DataFormats.FileDrop) as string[] ?? []);

        var files = paths.SelectMany(path => Directory.Exists(path)
            ? Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
            : [path]).Where(File.Exists)
            .Select(file => new FileInfo(file));


        foreach(var file in files.Where(file => file.Exists).Where(PlaylistBuilder.ValidFile)) 
        {
            var window = new AddSongWindow(file) 
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if(window.ShowDialog() is not true)
            {
                continue;
            }

            Library.AddIfNew(window.Song);
            yield return window.Song;
        }
    }

    private void SongsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if(_player.IsPlaying)
        {
            _player.Pause();
            _playbackProgressUpdater.Stop();
        }

        if(_selectedSong == _lastPlayed) return;

        UpdatePlayback(_selectedSong);
        _selectedSong?.SetDuration(_player.CurrentSongLength);
        _playbackProgressUpdater.Start();
    }
}