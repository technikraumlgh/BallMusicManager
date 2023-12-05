using Ametrin.Serialization;
using Ametrin.Utils.WPF;
using BallMusicManager.Domain;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BallMusicManager.Creator; 
public sealed partial class MainWindow : Window {
    private ObservableCollection<Song> Songs = [];
    private Song? DraggedItem;
    public MainWindow() {
        InitializeComponent();
        SongsGrid.ItemsSource = Songs;
    }

    private void Songs_Drop(object sender, DragEventArgs e) {
        if(DraggedItem is null) {
            FileDrop();
            return;
        }

        var targetItem = FindAncestorOrSelf<DataGridRow>(e.OriginalSource as DependencyObject)?.Item as Song;
        if(targetItem is not null && !ReferenceEquals(DraggedItem, targetItem)) {
            // Perform the swap or reorder in your data source here
            // For example, using an ObservableCollection:
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
        var dialog = new SaveFileDialog() {
            AddExtension = true,
            DefaultExt = "playlist"
        };
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
        var dialog = new SaveFileDialog() {
            AddExtension = true,
            DefaultExt = "playlist"
        };
        if(dialog.ShowDialog() is not true) return;

        Songs.WriteToJsonFile(dialog.FileName);
    }

    private void Open(object sender, RoutedEventArgs e) {
        var dialog = DialogUtils.GetFileDialog(filterDescription: "Playlists", extension: "playlist");
        if(dialog.ShowDialog() is not true) return;

        Songs = JsonExtensions.ReadFromJsonFile<ObservableCollection<Song>>(dialog.FileName).Reduce(Songs);
        SongsGrid.ItemsSource = Songs;
    }

    private void DeleteSong(object sender, RoutedEventArgs e) {
        Trace.TraceInformation(sender.ToString());
        if((sender as MenuItem)!.Parent is not ContextMenu menu) return;
        Trace.TraceInformation(menu.ToString());
        if(menu.PlacementTarget is not DataGridRow row) return;

        var song = row.Item as Song;

        Songs.Remove(song!);
    }
}