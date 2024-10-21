using Ametrin.Utils;
using System.Windows.Controls;

namespace BallMusicManager.Creator;

public partial class MainWindow
{
    private volatile bool _isSelectionUpdating = false;

    private void PlaylistGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _ = SyncSelection(PlaylistGrid, LibraryGrid, Library);
    }
    private void LibraryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _ = SyncSelection(LibraryGrid, PlaylistGrid, Playlist);
    }

    private async Task SyncSelection(DataGrid from, DataGrid to, SongBuilderCollection toContext)
    {
        if (_isSelectionUpdating)
        {
            return;
        }

        _isSelectionUpdating = true;

        await Task.Delay(200);
        to.SelectedItems.Clear();
        foreach (var selected in from.SelectedItems)
        {
            if (toContext.Contains(selected))
            {
                to.SelectedItems.Add(selected);
            }
        }
        if (to.SelectedItems.Count > 0)
        {
            to.ScrollIntoView(to.SelectedItems[^1]);
        }

        _isSelectionUpdating = false;

        //Dispatcher.BeginInvoke(() =>
        //{
        //    to.SelectedItems.Clear();
        //    foreach (var selected in from.SelectedItems)
        //    {
        //        if (toContext.Contains(selected))
        //        {
        //            to.SelectedItems.Add(selected);
        //        }
        //    }
        //    if (to.SelectedItems.Count > 0)
        //    {
        //        to.ScrollIntoView(to.SelectedItems[^1]);
        //    }

        //    _isSelectionUpdating = false;
        //});
    }
}
