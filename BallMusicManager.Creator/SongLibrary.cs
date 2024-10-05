using Ametrin.Utils.Optional;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.Collections.ObjectModel;
using System.IO;

namespace BallMusicManager.Creator;

public sealed class SongLibrary(ObservableCollection<SongBuilder> songs) : SongBuilderCollection(songs)
{
    static readonly FileInfo LibFile = new("Library/lib.plibz");
    public void Save()
    {
        PlaylistBuilder.ToArchive(LibFile, Songs);
    }

    public static SongLibrary LoadOrNew()
    {
        if (!Directory.Exists("Library"))
        {
            Directory.CreateDirectory("Library");
        }

        if (File.Exists("lib.plibz") && !LibFile.Exists)
        {
            File.Move("lib.plibz", LibFile.FullName);
        }
        LibFile.Refresh();
        return new(new(PlaylistBuilder.EnumerateArchive(LibFile).Reduce([])));
    }
}
