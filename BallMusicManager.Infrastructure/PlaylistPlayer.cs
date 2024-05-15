using BallMusicManager.Domain;
using System.Collections.Immutable;

namespace BallMusicManager.Infrastructure;

public sealed class PlaylistPlayer {
    public readonly MusicPlayer Player = new();
    public readonly string Path;
    public readonly ImmutableArray<Song> Songs;

    public bool IsPlaying => Player.IsPlaying;
    public ISong? Current => IsEmpty ? null : Songs[CurrentIndex];
    public ISong? Peek => IsEnd || IsEmpty ? null : Songs[CurrentIndex + 1];
    public int Length => Songs.Length;
    public bool IsEmpty => Length == 0;
    public bool IsEnd => IsEmpty || CurrentIndex == LastIndex;

    private int LastIndex => Length-1;
    public int CurrentIndex { get; private set; } = -1;

    public PlaylistPlayer(string path, IEnumerable<Song> songs){
        Path = path;
        Songs = songs.ToImmutableArray();
        SetCurrent(0);
        Player.OnSongFinished += AutoPlayNext;
    }

    public void SetCurrent(int idx){
        if(IsEmpty) return;
        var old = CurrentIndex;
        var wasPlaying = IsPlaying;
        CurrentIndex = Math.Clamp(idx, 0, LastIndex);
        if (old != CurrentIndex) Player.SetSong(Current!);
        if(wasPlaying) Player.Play();
    }

    public void Play() => Player.Play();
    public void Pause() => Player.Pause();
    public void Skip(int amount = 1){
        if(IsEmpty) return;
        if (amount == 0) return;
        SetCurrent(CurrentIndex + amount);
    }
    private void AutoPlayNext(){
        Skip();
        Play();
    }
}
