using NAudio.Wave;

namespace BallMusicManager.Infrastructure;

public sealed class MusicPlayer
{
    public event Action? OnSongChanged;
    public event Action? OnSongPaused;
    public event Action? OnSongContinued;
    public event Action? OnSongStopped;
    public event Action? OnSongStarted;
    public event Action? OnSongFinished;
    //public event Action? OnStateChanged;

    public TimeSpan CurrentSongLength => _currentAudioWave?.TotalTime ?? TimeSpan.Zero;
    public TimeSpan CurrentTime
    {
        get => _currentAudioWave?.CurrentTime ?? TimeSpan.Zero;
        set
        {
            if (_currentAudioWave is null) return;
            _currentAudioWave.CurrentTime = value;
        }
    }

    public PlaybackState PlaybackState => _player.PlaybackState;

    public bool IsPlaying => PlaybackState is PlaybackState.Playing;
    private readonly WaveOutEvent _player = new();
    private WaveStream? _currentAudioWave;
    private bool _wasStoppedManually = false;

    public MusicPlayer()
    {
        _player.PlaybackStopped += OnPlaybackStopped;
    }

    public void PlaySong(Song song)
    {
        SetSong(song);
        Play();
    }

    public void SetSong(Song song) => SetAudioFile(song.Path);
    public void SetAudioFile(string path)
    {
        Stop();
        _currentAudioWave?.Dispose();
        _currentAudioWave = new AudioFileReader(path);
        _player.Init(_currentAudioWave);
        OnSongChanged?.Invoke();
    }

    public void Pause()
    {
        if (_player.PlaybackState is not PlaybackState.Playing || _currentAudioWave is null) return;
        _player.Pause();
        //OnStateChanged?.Invoke();
        OnSongPaused?.Invoke();
    }

    public void Stop()
    {
        if (_player.PlaybackState is PlaybackState.Stopped) return;
        _wasStoppedManually = true;
        _player.Stop();
    }

    public void Play()
    {
        if (_player.PlaybackState is PlaybackState.Playing || _currentAudioWave is null) return;
        _player.Play();
        //OnStateChanged?.Invoke();
        if (CurrentTime == TimeSpan.Zero)
        {
            OnSongStarted?.Invoke();
        }
        else
        {
            OnSongContinued?.Invoke();
        }
    }

    public void Restart()
    {
        Stop();
        CurrentTime = TimeSpan.Zero;
        Play();
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        //OnStateChanged?.Invoke();
        if (_wasStoppedManually)
        {
            OnSongStopped?.Invoke();
        }
        else
        {
            OnSongFinished?.Invoke();
        }

        _wasStoppedManually = false;
    }

    ~MusicPlayer()
    {
        _player.Dispose();
    }
}
