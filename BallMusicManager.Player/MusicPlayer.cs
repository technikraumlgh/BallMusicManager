using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using NAudio.Wave;

namespace BallMusicManager.Player;
internal static class MusicPlayer
{
    private static readonly string[] AllowedFileTypes = [".mp3", ".wav", ".mp4", ".acc", ".m4a"];
    private static bool _IsPlaying { get; set; }
    private static bool WasPlaying { get; set; }
    public static bool IsPlaying
    {
        get => _IsPlaying;
        set
        {
            if (_IsPlaying == value) return;

            //WasPlaying = _IsPlaying;
            _IsPlaying = value;

            if (value)
            {
                Player.Play();
                OnPlaybackStarted?.Invoke();
            }
            else
            {
                Player.Pause();
                OnPlaybackPaused?.Invoke();
            }
        }
    }

    public static Song Current => Songs[CurrentIndex];
    public static Song? Peek => CurrentIndex + 1 < Songs.Length ? Songs[CurrentIndex + 1] : null;
    public static Song[] Songs { get; private set; } = Array.Empty<Song>();
    public static string? CurrentPlaylist { get; private set; } = null;

    public static event Action? OnSongChanged;
    public static event Action? OnPlaylistChanged;
    public static event Action? OnPlaybackPaused;
    public static event Action? OnPlaybackStarted;

    private static readonly IWavePlayer Player;
    public static int CurrentIndex { get; private set; }
    private static AudioFileReader? CurrentAudioWave;
    public static TimeSpan CurrentSongLength => CurrentAudioWave!.TotalTime;
    public static TimeSpan CurrentTime => CurrentAudioWave!.CurrentTime;

    static MusicPlayer()
    {
        Player = new WaveOutEvent();
        Player.PlaybackStopped += OnPlaybackStopped;
    }

    public static void Move(int amount = 1)
    {
        var newIndex = CurrentIndex + amount;
        Set(newIndex);
    }

    public static void Set(int idx)
    {
        if (idx >= Songs.Length) return;
        WasPlaying = IsPlaying;
        SetSong(idx);
        //IsPlaying = wasPlaying;
    }

    public static void ReloadPlaylist()
    {
        if (CurrentPlaylist is null) return;
        WasPlaying = IsPlaying;
        if (CurrentPlaylist.EndsWith(".csv"))
        {
            LoadPlaylistFromCSV(CurrentPlaylist);
        }
        else
        {
            LoadPlaylistFromFolder(CurrentPlaylist);
        }
        OnPlaylistChanged?.Invoke();
        SetSong(CurrentIndex);
    }

    public static void OpenPlaylist(string path)
    {
        if (path == CurrentPlaylist) return;
        if (path.EndsWith(".csv"))
        {
            LoadPlaylistFromCSV(path);
        }
        else
        {
            LoadPlaylistFromFolder(path);
        }
        if (Songs.Length == 0)
        {
            CurrentPlaylist = null;
            OnPlaylistChanged?.Invoke();
            return;
        }
        CurrentPlaylist = path;
        OnPlaylistChanged?.Invoke();
        SetSong(0);
    }

    private static void LoadPlaylistFromFolder(string path)
    {
        CurrentAudioWave = null;
        Player.Stop();
        MainWindow.SetMissingFiles(false);
        var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Where(path => AllowedFileTypes.Contains(Path.GetExtension(path))).ToArray();
        Songs = new Song[files.Length];
        for (var i = 0; i < files.Length; i++)
        {
            Songs[i] = Song.FromPath(files[i], i + 1).ReduceOrThrow();
        }
        Songs = [.. Songs.OrderBy(song => song.Index)];
    }

    private static void LoadPlaylistFromCSV(string path)
    {
        CurrentAudioWave = null;
        Player.Stop();
        MainWindow.SetMissingFiles(false);
        using var stream = new StreamReader(path);
        var songs = new List<Song>();
        var counter = 1;
        while (stream.ReadLine() is string line)
        {
            var values = line.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var loc = Path.Combine(Path.GetDirectoryName(path)!, values[3]);
            if (!File.Exists(loc))
            {
                Path.Combine(Path.GetDirectoryName(path)!, values[3]);
            }
            songs.Add(new(loc, counter, values[0], values[1], Song.GetDance(values[2])));
            counter++;
        }
        Songs = [.. songs];
    }

    private static void SetSong(int idx)
    {
        IsPlaying = false;
        Player.Stop();
        CurrentIndex = idx;
        CurrentAudioWave?.Dispose();
        CurrentAudioWave = new(Current.Path);
        Player.Init(CurrentAudioWave);
        OnSongChanged?.Invoke();
    }

    public static void OnExit(object? sender, CancelEventArgs args)
    {
        Player.Stop();
        if (args.Cancel) return;
        Player.Dispose();
        CurrentAudioWave?.Dispose();
        ServerConnection.SendNothing();
    }

    private static void OnPlaybackStopped(object? sender, StoppedEventArgs args)
    {
        if (IsPlaying)
        {
            Move();
            IsPlaying = true;
        }
        else if (WasPlaying)
        {
            IsPlaying = true;
            WasPlaying = false;
        }
    }
}
