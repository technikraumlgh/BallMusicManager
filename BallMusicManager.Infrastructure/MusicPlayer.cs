using Ametrin.Utils;
using BallMusicManager.Domain;
using NAudio.Wave;

namespace BallMusicManager.Infrastructure;

public sealed class MusicPlayer{
    public event Action? OnSongChanged;
    public event Action? OnSongPaused;
    public event Action? OnSongContinued;
    public event Action? OnSongStopped;
    public event Action? OnSongStarted;
    public event Action? OnSongFinished;
    //public event Action? OnStateChanged;

    public TimeSpan CurrentSongLength => _currentAudioWave?.TotalTime ?? TimeSpan.Zero;
    public TimeSpan CurrentTime => _currentAudioWave?.CurrentTime ?? TimeSpan.Zero;
    public PlaybackState PlaybackState => _player.PlaybackState;
    public bool IsPlaying => PlaybackState is PlaybackState.Playing;
    private readonly WaveOutEvent _player = new();
    private AudioFileReader? _currentAudioWave;
    private bool _wasStopped = false;

    public MusicPlayer(){
        _player.PlaybackStopped += OnPlaybackStopped;
    }

    public void PlaySong(Song song){
        SetSong(song);
        Play();
    }

    public void SetSong(Song song){
        Stop();
        _currentAudioWave?.Dispose();
        _currentAudioWave = new(song.Path);
        _player.Init(_currentAudioWave);
        OnSongChanged?.Invoke();
    }

    public void Pause(){
        if(_player.PlaybackState is not PlaybackState.Playing || _currentAudioWave is null) return;
        _player.Pause();
        //OnStateChanged?.Invoke();
        OnSongPaused?.Invoke();
    }
    public void Stop() {
        if(!IsPlaying) _wasStopped = true;
        
        _player.Stop();
    }

    public void Play(){
        if(_player.PlaybackState is PlaybackState.Playing || _currentAudioWave is null) return;
        _player.Play();
        //OnStateChanged?.Invoke();
        if(CurrentTime == TimeSpan.Zero){
            OnSongStarted?.Invoke();
        }else{
            OnSongContinued?.Invoke();
        }
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e){
        //OnStateChanged?.Invoke();
        if(_wasStopped) OnSongStopped?.Invoke();
        else OnSongFinished?.Invoke();
        
        _wasStopped = false;
    }

    ~MusicPlayer(){
        _player.Dispose();
    }
}
