using Ametrin.Optional;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.IO;
using System.IO.Compression;

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

    //public static Option<Stream> CacheArchiveFile(SongBuilder song)
    //{
          // cannot return stream because stuff has to be disposed
    //    var stream

    //    return LibFile.WhereExists()
    //        .Select(file => new ZipArchive(file.OpenRead()))
    //        .Select(archive => archive.GetEntry(song.FileHash) ?? Option.None<ZipArchiveEntry>())
    //        .Select(entry => entry.Open())
    //        ;
    //}
}
