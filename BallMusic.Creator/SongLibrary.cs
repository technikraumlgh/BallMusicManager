using System.IO;

namespace BallMusic.Creator;

public sealed class SongLibrary(IEnumerable<SongBuilder> songs) : SongBuilderCollection(songs)
{
    public SongLibrary() : this([]) { }

    public void Save()
    {
        PlaylistBuilder.ToArchive(LibFile, this);
    }

    public static readonly FileInfo LibFile = new("Library/lib.plibz");
    public static SongLibrary LoadOrNew()
    {
        return new(PlaylistBuilder.EnumerateArchiveEntries(LibFile).Or([]));
    }
}
