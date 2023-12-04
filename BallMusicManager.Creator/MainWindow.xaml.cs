using BallMusicManager.Domain;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BallMusicManager.Creator; 
public sealed partial class MainWindow : Window {
    private readonly ObservableCollection<Song> Songs = [];
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
                var window = new AddSongWindow(path);
                if(window.ShowDialog() is not true) continue;
                Songs.Add(window.Song);
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
            if(obj is T objTyped)
                return objTyped;
            obj = VisualTreeHelper.GetParent(obj);
        }
        return null;
    }
}