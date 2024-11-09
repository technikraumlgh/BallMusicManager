﻿using System.IO;

namespace BallMusicManager.Creator;

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
        if (!Directory.Exists("Library"))
        {
            Directory.CreateDirectory("Library");
        }

        //TODO: remove (this only exists to move the lib file from older versions to the LibraryFolder)
        if (File.Exists("lib.plibz") && !LibFile.Exists)
        {
            File.Move("lib.plibz", LibFile.FullName);
        }

        LibFile.Refresh();
        return new(PlaylistBuilder.EnumerateArchiveEntries(LibFile).Or([]));
    }
}
