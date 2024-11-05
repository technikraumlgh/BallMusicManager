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
    private const string SONG_DRAG_FORMAT = "lghBallSong";
    private Point _startPoint = new();

    private void Song_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(null);
    }

    #region Playlist Drag&Drop
    private void PlaylistGrid_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            Playlist.AddAllIfNew(HandleFileDrop(e));
            return;
        }

        if (!e.Data.GetDataPresent(SONG_DRAG_FORMAT) || e.Data.GetData(SONG_DRAG_FORMAT) is not SongBuilder draggedSong || e.OriginalSource is not DependencyObject obj)
        {
            return;
        }

        if (obj.FindParentOrSelf<DataGridRow>()?.Item is not SongBuilder targetItem)
        {
            //Playlist.Remove(draggedSong);
            Playlist.AddIfNew(draggedSong);
        }
        else if (!ReferenceEquals(draggedSong, targetItem))
        {
            var index = Playlist.IndexOf(targetItem);
            Playlist.Remove(draggedSong);
            Playlist.Insert(index, draggedSong);
        }
    }

    private void PlaylistGrid_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        TryDrag(sender, e);
    }
    #endregion

    #region Library Drag&Drop
    private void LibraryDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            foreach (var _ in HandleFileDrop(e))
            { }
            return;
        }
    }

    private void LibraryGrid_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        TryDrag(sender, e);
    }
    #endregion

    private IEnumerable<SongBuilder> HandleFileDrop(DragEventArgs e)
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

            yield return Library.AddOrGetExisting(window.Song); //TODO: display duplicate waring? or just skip?
        }
    }

    private void TryDrag(object sender, MouseEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source)
        {
            return;
        }

        var mousePosDiff = _startPoint - e.GetPosition(null);

        if (e.LeftButton == MouseButtonState.Pressed &&
            (Math.Abs(mousePosDiff.X) > SystemParameters.MinimumHorizontalDragDistance ||
             Math.Abs(mousePosDiff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
            if (sender is not DataGrid grid)
            {
                return;
            }
            var row = source.FindParentOrSelf<DataGridRow>();

            if (row is null)
            {
                return;
            }

            var song = grid.ItemContainerGenerator.ItemFromContainer(row);
            DragDrop.DoDragDrop(row, new DataObject(SONG_DRAG_FORMAT, song), DragDropEffects.Move);
        }
    }
}
