using Ametrin.Utils.Optional;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;

namespace BallMusicManager.Creator;

public sealed class SongLibrary(ObservableCollection<SongBuilder> songs) : IEnumerable<SongBuilder> {
    public readonly ObservableCollection<SongBuilder> Songs = songs;
    public bool ContainsSong(SongBuilder song) => Songs.Contains(song, SongEqualityComparer.Instance);

    public bool AddIfNew(SongBuilder song) {
        if(ContainsSong(song)) {
            return false;
        }
        Songs.Add(song);

        return true;
    }
    public void AddAllIfNew(IEnumerable<SongBuilder> songs) {
        foreach (var song in songs) {
            AddIfNew(song);
        }
    }

    static readonly FileInfo LibFile = new("lib.plibz");
    public void Save() {
        PlaylistBuilder.ToArchive(LibFile, Songs);
    }
    public static SongLibrary LoadOrNew() {
        return new(new(PlaylistBuilder.EnumerateArchive(LibFile).Reduce([])));
    }

    public IEnumerator<SongBuilder> GetEnumerator() => Songs.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
