using Ametrin.Utils.Optional;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;

namespace BallMusicManager.Creator;

public sealed class SongLibrary(ObservableCollection<MutableSong> songs) : IEnumerable<MutableSong> {
    public readonly ObservableCollection<MutableSong> Songs = songs;
    public bool ContainsSong(MutableSong song) => Songs.Contains(song, SongEqualityComparer.Instance);

    public bool AddIfNew(MutableSong song) {
        if(ContainsSong(song)) {
            return false;
        }
        Songs.Add(song);

        return true;
    }
    public void AddAllIfNew(IEnumerable<MutableSong> songs) {
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

    public IEnumerator<MutableSong> GetEnumerator() => Songs.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
