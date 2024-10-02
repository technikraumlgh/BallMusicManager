using Ametrin.Utils.Optional;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace BallMusicManager.Creator;

public sealed class SongLibrary(ObservableCollection<SongBuilder> songs) : SongBuilderCollection(songs)
{
    static readonly FileInfo LibFile = new("lib.plibz");
    public void Save()
    {
        PlaylistBuilder.ToArchive(LibFile, Songs);
    }
    
    public static SongLibrary LoadOrNew()
    {
        Debug.WriteLine(LibFile.FullName);
        return new(new(PlaylistBuilder.EnumerateArchive(LibFile).Reduce([])));
    }
}
