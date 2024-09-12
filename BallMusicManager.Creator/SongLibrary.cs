using Ametrin.Utils.Optional;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace BallMusicManager.Creator;

public sealed class SongLibrary(ObservableCollection<SongBuilder> songs) : IEnumerable<SongBuilder> {
    public readonly ObservableCollection<SongBuilder> Songs = songs;
    public bool ContainsSong(SongBuilder song, IEqualityComparer<SongBuilder>? equalityComparer = null) => Songs.Contains(song, equalityComparer ?? SongEqualityComparer.FileHash);

    public bool AddIfNew(SongBuilder song, IEqualityComparer<SongBuilder>? equalityComparer = null) {
        if(ContainsSong(song, equalityComparer)) {
            return false;
        }
        Songs.Add(song);

        return true;
    }

    public SongBuilder AddOrGetExisting(SongBuilder song, IEqualityComparer<SongBuilder>? equalityComparer = null)
    {
        equalityComparer ??= SongEqualityComparer.FileHash;
        var match = Songs.FirstOrDefault(other => equalityComparer.Equals(song, other));
        if(match is null)
        {
            Songs.Add(song);
            match = song;
        }

        return match;
    }
    public void AddAllIfNew(IEnumerable<SongBuilder> songs, IEqualityComparer<SongBuilder>? equalityComparer = null) {
        foreach (var song in songs) {
            AddIfNew(song, equalityComparer);
        }
    }
    
    public IEnumerable<SongBuilder> AddAllOrReplaceWithExisting(IEnumerable<SongBuilder> songs, IEqualityComparer<SongBuilder>? equalityComparer = null) {
        foreach (var song in songs) {
            yield return AddOrGetExisting(song, equalityComparer);
        }
    }

    static readonly FileInfo LibFile = new("lib.plibz");
    public void Save() {
        PlaylistBuilder.ToArchive(LibFile, Songs);
    }
    public static SongLibrary LoadOrNew() {
        Debug.WriteLine(LibFile.FullName);
        return new(new(PlaylistBuilder.EnumerateArchive(LibFile).Reduce([])));
    }

    public IEnumerator<SongBuilder> GetEnumerator() => Songs.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
