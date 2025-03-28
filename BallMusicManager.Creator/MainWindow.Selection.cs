using System.Windows.Controls;

namespace BallMusicManager.Creator;

public partial class MainWindow
{
    private volatile bool _isSelectionUpdating = false;

    private void PlaylistGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SyncSelection(PlaylistGrid, LibraryGrid, Library);
    }
    private void LibraryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SyncSelection(LibraryGrid, PlaylistGrid, Playlist);
    }

    private void SyncSelection(DataGrid from, DataGrid to, SongBuilderCollection toContext)
    {
        if (_isSelectionUpdating)
        {
            return;
        }

        _isSelectionUpdating = true;

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
    }
}
