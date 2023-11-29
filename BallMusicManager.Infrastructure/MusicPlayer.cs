using BallMusicManager.Domain;
using NAudio.Wave;

namespace BallMusicManager.Infrastructure;

public sealed class MusicPlayer{
    public event Action? OnSongChanged;
    public event Action? OnPlaylistChanged;
    public event Action? OnPlaybackPaused;
    public event Action? OnPlaybackStarted;

    public TimeSpan CurrentSongLength => CurrentAudioWave?.TotalTime ?? TimeSpan.Zero;
    public TimeSpan CurrentTime => CurrentAudioWave?.CurrentTime ?? TimeSpan.Zero;
    private readonly IWavePlayer Player = new WaveOutEvent();
    private AudioFileReader? CurrentAudioWave;
    public void PlaySong(Song song){
        CurrentAudioWave?.Dispose();
        CurrentAudioWave = new(song.Path);
        Player.Init(CurrentAudioWave);
        OnSongChanged?.Invoke();
    }
}
