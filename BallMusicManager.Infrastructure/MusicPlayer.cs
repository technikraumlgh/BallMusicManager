using System.Diagnostics;
using Ametrin.Utils;
using BallMusicManager.Domain;
using NAudio.Wave;

namespace BallMusicManager.Infrastructure;

public sealed class MusicPlayer{
    public event Action? OnSongChanged;
    public event Action? OnSongPaused;
    public event Action? OnSongContinued;
    public event Action? OnSongStarted;
    public event Action? OnSongFinished;

    public TimeSpan CurrentSongLength => CurrentAudioWave?.TotalTime ?? TimeSpan.Zero;
    public TimeSpan CurrentTime => CurrentAudioWave?.CurrentTime ?? TimeSpan.Zero;
    public PlaybackState PlaybackState => Player.PlaybackState;
    public bool IsPlaying => PlaybackState is PlaybackState.Playing;
    private readonly WaveOutEvent Player = new();
    private AudioFileReader? CurrentAudioWave;

    public MusicPlayer(){
        Player.PlaybackStopped += OnPlaybackStopped;
    }

    public void PlaySong(Song song){
        SetSong(song);
        Play();
    }

    public void SetSong(Song song){
        Player.Stop();
        CurrentAudioWave?.Dispose();
        CurrentAudioWave = new(song.Path);
        Player.Init(CurrentAudioWave);
        OnSongChanged?.Invoke();
    }

    public void Pause(){
        if(Player.PlaybackState is not PlaybackState.Playing || CurrentAudioWave is null) return;
        Player.Pause();
        OnSongPaused?.Invoke();
    }

    public void Play(){
        if(Player.PlaybackState is PlaybackState.Playing || CurrentAudioWave is null) return;
        Player.Play();
        if(CurrentTime == TimeSpan.Zero){
            OnSongStarted?.Invoke();
        }else{
            OnSongContinued?.Invoke();
        }
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e){
        if(CurrentTime.Approximately(CurrentSongLength, TimeSpan.FromSeconds(0.5))){
            OnSongFinished?.Invoke();
        }
    }

    ~MusicPlayer(){
        Player.Dispose();
    }
}
