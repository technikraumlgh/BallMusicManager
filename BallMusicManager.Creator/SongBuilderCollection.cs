using BallMusicManager.Domain;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BallMusicManager.Creator;

public class SongBuilderCollection(ObservableCollection<SongBuilder> songs) : IEnumerable<SongBuilder>, INotifyCollectionChanged, INotifyPropertyChanged
{
    internal ObservableCollection<SongBuilder> Songs { get; } = songs;
    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => Songs.CollectionChanged += value; 
        remove => Songs.CollectionChanged -= value;
    }

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => ((INotifyPropertyChanged)Songs).PropertyChanged += value;
        remove => ((INotifyPropertyChanged)Songs).PropertyChanged -= value;
    }

    public int Count => Songs.Count;
    public void Clear() => Songs.Clear();
    public bool Remove(SongBuilder builder) => Songs.Remove(builder);
    public int IndexOf(SongBuilder builder) => Songs.IndexOf(builder);
    public bool Contains(SongBuilder builder) => Songs.Contains(builder);
    public void Insert(int index, SongBuilder builder) => Songs.Insert(index, builder);

    public SongBuilderCollection(IEnumerable<SongBuilder> songs) : this(new(songs)) { }

    public bool ContainsSong(SongBuilder song, IEqualityComparer<SongBuilder>? equalityComparer = null) => Songs.Contains(song, equalityComparer ?? SongEqualityComparer.FileHash);

    public bool AddIfNew(SongBuilder song, IEqualityComparer<SongBuilder>? equalityComparer = null)
    {
        if (ContainsSong(song, equalityComparer))
        {
            return false;
        }
        Songs.Add(song);

        return true;
    }

    public SongBuilder AddOrGetExisting(SongBuilder song, IEqualityComparer<SongBuilder>? equalityComparer = null)
    {
        equalityComparer ??= SongEqualityComparer.FileHash;
        var match = Songs.FirstOrDefault(other => equalityComparer.Equals(song, other));
        if (match is null)
        {
            Songs.Add(song);
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

    public IEnumerator<SongBuilder> GetEnumerator() => Songs.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
