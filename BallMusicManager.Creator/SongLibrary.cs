using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.IO;

namespace BallMusicManager.Creator;

public sealed class SongLibrary(IEnumerable<SongBuilder> songs) : SongBuilderCollection(songs)
{
    public SongLibrary() : this([]) { }

    public void Save()
    {
        PlaylistBuilder.ToArchive(LibFile, this);
    }

    static readonly FileInfo LibFile = new("Library/lib.plibz");
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
        return new(PlaylistBuilder.EnumerateArchive(LibFile).Or([]));
    }
}
