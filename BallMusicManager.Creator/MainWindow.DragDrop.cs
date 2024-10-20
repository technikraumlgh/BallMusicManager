using Ametrin.Utils.WPF;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace BallMusicManager.Creator;

public partial class MainWindow
{
    private SongBuilder? _draggedSong;

    #region Playlist Drag&Drop
    private void Songs_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            Playlist.AddAllIfNew(FileDrop(e));
            return;
        }

        if (_draggedSong is null || e.OriginalSource is not DependencyObject obj)
        {
            return;
        }

        if (obj.FindParentOrSelf<DataGridRow>()?.Item is not SongBuilder targetItem)
        {
            Playlist.Remove(_draggedSong);
            Playlist.AddIfNew(_draggedSong);
        }
        else if (!ReferenceEquals(_draggedSong, targetItem))
        {
            var index = Playlist.IndexOf(targetItem);
            Playlist.Remove(_draggedSong);
            Playlist.Insert(index, _draggedSong);
        }
        _draggedSong = null;
    }

    private void Song_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject obj)
        {
            return;
        }

        var row = obj.FindParentOrSelf<DataGridRow>();
        if (row?.Item is not SongBuilder song)
        {
            _draggedSong = null;
            return;
        }

        var grid = obj.FindParent<DataGrid>();

        _draggedSong = song;
        SelectSong(song, grid?.ItemsSource as SongBuilderCollection);
    }

    // we use a custom selection logic to allow syncing the song selection between the grids
    private void UnselectSong() => SelectSong(null, null);
    private void SelectSong(SongSelection selection) => SelectSong(selection.Song, selection.Context);
    private void SelectSong(SongBuilder? song, SongBuilderCollection? context)
    {
        _selection = new(song, context);
        if(song is null)
        {
            PlaylistGrid.SelectedItem = null;
            LibraryGrid.SelectedItem = null;
            return;
        }
        SelectIfExistsOrUnselect(PlaylistGrid, song);
        SelectIfExistsOrUnselect(LibraryGrid, song);

        static void SelectIfExistsOrUnselect(DataGrid grid, SongBuilder song)
        {
            if (ReferenceEquals(grid.SelectedItem, song))
            {
                return;
            }

            grid.CommitEdit();
            grid.SelectionUnit = DataGridSelectionUnit.FullRow;

            if (grid.Items.Contains(song))
            {
                grid.SelectedItem = song;
                grid.ScrollIntoView(song);
            }
            else
            {
                grid.SelectedItem = null;
            }
        }
    }

    private void SongsGrid_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && _draggedSong != null)
        {
            TryDrag(PlaylistGrid);
        }
    }
    #endregion

    #region Library Drag&Drop
    private void LibraryDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            foreach (var _ in FileDrop(e))
            { }
            return;
        }

        _draggedSong = null;
    }

    private void LibraryGrid_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && _draggedSong != null)
        {
            TryDrag(LibraryGrid);
        }
    }
    #endregion

    private IEnumerable<SongBuilder> FileDrop(DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            yield break;
        }

        var paths = e.Data.GetData(DataFormats.FileDrop) as string[] ?? [];

        var files = paths.SelectMany(path => Directory.Exists(path)
            ? Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
            : [path]).Where(File.Exists)
            .Select(file => new FileInfo(file));


        foreach (var file in files.Where(file => file.Exists).Where(PlaylistBuilder.ValidFile))
        {
            var window = new AddSongWindow(file)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if (window.ShowDialog() is not true)
            {
                continue;
            }

            yield return Library.AddOrGetExisting(window.Song); //display duplicate waring? or just skip?
        }
    }

    private void TryDrag(DependencyObject source)
    {
        try
        {
            DragDrop.DoDragDrop(source, _draggedSong, DragDropEffects.Link);
        }
        catch
        {
            // to prevent crashes when mouse leaves window and enters again
            _draggedSong = null;
        }
    }
}
