using Ametrin.Serialization;
using Ametrin.Utils.WPF;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.Collections.ObjectModel;
using Ametrin.Utils;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Specialized;

namespace BallMusicManager.Creator; 
public sealed partial class MainWindow : Window {
    private ObservableCollection<Song> Songs = [];
    private TimeSpan Duration = TimeSpan.Zero;
    private Song? DraggedItem;
    public MainWindow() {
        InitializeComponent();
        SongsGrid.ItemsSource = Songs;
        Songs.CollectionChanged += UpdateLengthDisplay;
        UpdateLengthDisplay();
    }

    private void Songs_Drop(object sender, DragEventArgs e) {
        if(e.Data.GetDataPresent(DataFormats.FileDrop)) {
            FileDrop();
            return;
        }

        if(DraggedItem is null) return;

        var targetItem = FindAncestorOrSelf<DataGridRow>(e.OriginalSource as DependencyObject)?.Item as Song;
        if(targetItem is not null && !ReferenceEquals(DraggedItem, targetItem)) {
            var index = Songs.IndexOf(targetItem);
            Songs.Remove(DraggedItem);
            Songs.Insert(index, DraggedItem);
        }
        DraggedItem = null;

        void FileDrop() {
            if(!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var files = e.Data.GetData(DataFormats.FileDrop) as string[] ?? [];
            foreach(var path in files) {
                Trace.TraceInformation(path);
                var window = new AddSongWindow(path) {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                if(window.ShowDialog() is not true) continue;
                Songs.Add(window.Song.Build());
            }
        }
    }

    private void SongsGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        var row = FindAncestorOrSelf<DataGridRow>(e.OriginalSource as DependencyObject);
        if(row != null) {
            DraggedItem = row.Item as Song;
        }
    }

    private void SongsGrid_PreviewMouseMove(object sender, MouseEventArgs e) {
        if(e.LeftButton == MouseButtonState.Pressed && DraggedItem != null) {
            DragDrop.DoDragDrop(SongsGrid, DraggedItem, DragDropEffects.Move);
        }
    }

    private static T FindAncestorOrSelf<T>(DependencyObject obj) where T : DependencyObject {
        while(obj != null) {
            if(obj is T objTyped) return objTyped;
            obj = VisualTreeHelper.GetParent(obj);
        }
        return null;
    }

    private void Export(object sender, RoutedEventArgs e) {
        var dialog = DialogUtils.SaveFileDialog(filterDescription: "Playlist", extension: "playlist");
        if(dialog.ShowDialog() is not true) return;

        var temp = new List<Song>(Songs.Count);
        var directory = new FileInfo(dialog.FileName).DirectoryName;
        for(int i = 0; i < Songs.Count; i++) {
            var fileName = Path.GetFileName(Songs[i].Path);
            var target = Path.Join(directory, fileName);
            if(target != Songs[i].Path) File.Copy(Songs[i].Path, target, true);
            temp.Add(Songs[i] with {
                Index = i + 1,
                Path = fileName,
            });
        }
        temp.WriteToJsonFile(dialog.FileName);
    }

    private void Save(object sender, RoutedEventArgs e) {
        var dialog = DialogUtils.SaveFileDialog(filterDescription: "Playlist", extension: "playlist");
        if(dialog.ShowDialog() is not true) return;

        Songs.WriteToJsonFile(dialog.FileName);
    }

    private void Open(object sender, RoutedEventArgs e) {
        var dialog = DialogUtils.GetFileDialog(filterDescription: "Playlists", extension: "playlist");
        if(dialog.ShowDialog() is not true) return;

        Songs.CollectionChanged -= UpdateLengthDisplay;
        Songs = new(PlaylistBuilder.EnumerateFile(new(dialog.FileName)));
        SongsGrid.ItemsSource = Songs;
        Songs.CollectionChanged += UpdateLengthDisplay;
        UpdateLengthDisplay();
    }

    private void DeleteSong(object sender, RoutedEventArgs e) {
        if((sender as MenuItem)!.Parent is not ContextMenu menu) return;
        if(menu.PlacementTarget is not DataGridRow row) return;

        var song = row.Item as Song;
        Songs.Remove(song!);
    }

    private void UpdateLengthDisplay(object? sender = null, NotifyCollectionChangedEventArgs? e = default) {
        Duration = Songs.Sum(s => s.Duration);
        LengthDisplay.Text = $"Duration: {Duration:hh\\:mm\\:ss}";
    }

    private void Close(object sender, RoutedEventArgs e) {
        if(Songs.Count == 0) return;

        var shouldSave = MessageBox.Show("Save the current Playlist?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
    }
}