using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using NAudio.Wave;

namespace BallMusicManager.Player;
internal static class MusicPlayer
{
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
    public static Playlist? Playlist { get; private set; }



    static MusicPlayer(){
        Player = new WaveOutEvent();
        Player.PlaybackStopped += OnPlaybackStopped;
    }

    public static void Move(int amount = 1){
        var newIndex = CurrentIndex + amount;
        Set(newIndex);
    }

    public static void Set(int idx){
        if (idx >= Playlist.Length) return;
        WasPlaying = IsPlaying;
        SetSong(idx);
        //IsPlaying = wasPlaying;
    }

    public static void ReloadPlaylist(){
        if (Playlist is null) return;

        OpenPlaylist(Playlist.Path);
        return;
    }

    public static void OpenPlaylist(string path){
        CurrentAudioWave = null;
        Player.Stop();
        MainWindow.SetMissingFiles(false);
        Playlist = path.EndsWith(".csv") ? PlaylistBuilder.FromCSV(new(path)) : PlaylistBuilder.FromFolder(new(path));
        OnPlaylistChanged?.Invoke();
        SetSong(0);
    }

    private static void SetSong(int idx){
        IsPlaying = false;
        Player.Stop();
        CurrentIndex = idx;
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
