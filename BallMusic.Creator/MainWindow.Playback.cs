﻿using System.Windows.Input;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using BallMusic.WPF;

namespace BallMusic.Creator;

public partial class MainWindow
{
    private const string PauseIcon = "\uE769";
    private const string PlayIcon = "\uE768";
    private SongBuilder? _lastPlayed;
    private readonly MusicPlayer _player = new();
    private readonly DispatcherTimer _playbackProgressUpdater = new()
    {
        Interval = TimeSpan.FromMilliseconds(200)
    };

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        if (_player.IsPlaying)
        {
            PausePlayback();
        }
        else if (LibraryGrid.SelectedItems.Count > 0)
        {
            PlaySong(LibraryGrid.SelectedItems[^1] as SongBuilder);
        }
    }

    private void PlaybackSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _player.CurrentTime = TimeSpan.FromSeconds(e.NewValue);
    }

    private void SongsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source
            || sender is not DataGrid grid
            || source.FindParentOrSelf<DataGridRow>() is not DataGridRow row
            || grid.ItemContainerGenerator.ItemFromContainer(row) is not SongBuilder song)
        {
            return;
        }

        PlaySong(song);
    }

    private void PlaySong(SongBuilder? song)
    {
        _player.Stop();

        if (song is null)
        {
            return;
        }

        UpdatePlayback(song);
        song.SetDuration(_player.CurrentSongLength); // some files somehow have wrong metadata for their duration so we overwrite it here
        _player.CurrentTime = TimeSpan.Zero;
        _player.Play();
        _playbackProgressUpdater.Start();
        PlayButton.Content = PauseIcon;
    }

    private void PausePlayback()
    {
        _player.Pause();
        _playbackProgressUpdater.Stop();
        PlayButton.Content = PlayIcon;
    }

    private void UpdatePlayback(SongBuilder song)
    {
        if (song == _lastPlayed)
        {
            return;
        }
        
        if (song.Path is ArchiveLocation)
        {
            SongCache.CacheFromArchive(song);
        }
        if (song.Path is not FileLocation)
        {
            MessageBoxHelper.ShowError($"{song.Title} has no linked file");
            return;
        }

        _player.SetSong(song.Build());
        _lastPlayed = song;
        PlaybackSlider.Maximum = _player.CurrentSongLength.TotalSeconds;
    }

    private void UpdatePlaybackSliderValue(object? sender, EventArgs e)
    {
        PlaybackSlider.ValueChanged -= PlaybackSlider_ValueChanged;
        PlaybackSlider.Value = _player.CurrentTime.TotalSeconds;
        PlaybackSlider.ValueChanged += PlaybackSlider_ValueChanged;
    }
}
