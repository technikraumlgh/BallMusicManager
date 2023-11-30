using BallMusicManager.Domain;

namespace BallMusicManager.Infrastructure;

public sealed class PlaylistPlayer{
    public readonly Playlist Playlist;
    public readonly MusicPlayer Player = new();
    public bool IsPlaying => Player.IsPlaying;
    public Song? Current => Playlist.Current;

    public PlaylistPlayer(Playlist playlist){
        Playlist = playlist;
        if(Playlist.IsEmpty) return;
        Player.SetSong(Playlist.Current!);
    }

    public void Play() => Player.Play();
    public void Pause() => Player.Pause();
    public void Skip(int amount = 1){
        if(Playlist.IsEmpty) return;
        Playlist.Skip(amount);
        Player.SetSong(Playlist.Current!);
    }
}
