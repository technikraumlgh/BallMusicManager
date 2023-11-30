using System.Collections.Immutable;
using Ametrin.Utils.Optional;

namespace BallMusicManager.Domain;

public sealed class Playlist(string path, IEnumerable<Song> songs){
    public string Path { get; } = path;
    public ImmutableArray<Song> Songs { get; } = songs.ToImmutableArray();

    public Song? Current => IsEmpty ? null : Songs[CurrentIndex];
    public Song? Peek => IsEnd || IsEmpty ? null : Songs[CurrentIndex + 1];
    public int Length => Songs.Length;
    public bool IsEmpty => Length == 0;
    public bool IsEnd => IsEmpty || CurrentIndex == Length-1;
    public event Action? OnCurrentChanged;

    private int CurrentIndex = 0;

    public void Skip(int amount = 1){
        if(amount == 0) return;
        SetCurrent(CurrentIndex + amount);
    }

    public void SetCurrent(int idx){
        var old = CurrentIndex;
        CurrentIndex = Math.Min(idx, Length-1);
        if(old != CurrentIndex) OnCurrentChanged?.Invoke();
    }
}
