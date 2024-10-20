using BallMusicManager.Domain;
using System.Collections.ObjectModel;

namespace BallMusicManager.Creator;

public class SongBuilderCollection(IEnumerable<SongBuilder> songs) : ObservableCollection<SongBuilder>(songs)
{
    public SongBuilderCollection() : this([]) { }
    public bool ContainsSong(SongBuilder song, IEqualityComparer<SongBuilder>? equalityComparer = null) => this.Contains(song, equalityComparer ?? SongEqualityComparer.ByFileHash);

    public bool AddIfNew(SongBuilder song, IEqualityComparer<SongBuilder>? equalityComparer = null)
    {
        if (ContainsSong(song, equalityComparer))
        {
            return false;
        }
        Add(song);

        return true;
    }

    public SongBuilder AddOrGetExisting(SongBuilder song, IEqualityComparer<SongBuilder>? equalityComparer = null)
    {
        equalityComparer ??= SongEqualityComparer.ByLoosePropertiesOrFileHash;
        var match = this.FirstOrDefault(other => equalityComparer.Equals(song, other));
        if (match is null)
        {
            Add(song);
            match = song;
        }

        return match;
    }

    public void AddAllIfNew(IEnumerable<SongBuilder> songs, IEqualityComparer<SongBuilder>? equalityComparer = null)
    {
        foreach (var song in songs)
        {
            AddIfNew(song, equalityComparer);
        }
    }

    public IEnumerable<SongBuilder> AddAllOrReplaceWithExisting(IEnumerable<SongBuilder> songs, IEqualityComparer<SongBuilder>? equalityComparer = null)
    {
        foreach (var song in songs)
        {
            yield return AddOrGetExisting(song, equalityComparer);
        }
    }
}
