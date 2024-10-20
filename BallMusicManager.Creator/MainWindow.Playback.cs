using BallMusicManager.Domain;
using System.Windows.Input;
using System.Windows;
using BallMusicManager.Infrastructure;
using System.Windows.Threading;

namespace BallMusicManager.Creator;

public partial class MainWindow
{
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
        else
        {
            PlaySelectedSong();
        }
    }

    private void PlaybackSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _player.CurrentTime = TimeSpan.FromSeconds(e.NewValue);
    }

    private void SongsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        PlaySelectedSong();
        SelectSong(_selection);
    }

    private void PlaySelectedSong()
    {
        if (!_selection.HasSelection)
        {
            return;
        }

        _player.Stop();

        UpdatePlayback(_selection.Song!);
        _selection.Song!.SetDuration(_player.CurrentSongLength);
        _player.Play();
        _playbackProgressUpdater.Start();
        PlayButton.Content = "\uE769";
    }

    private void PausePlayback()
    {
        _player.Pause();
        PlayButton.Content = "\uE768";
        _playbackProgressUpdater.Stop();
    }

    private void UpdatePlayback(SongBuilder? song)
    {
        if (song is null)
        {
            return;
        }

        if (song != _lastPlayed)
        {
            _player.SetSong(song.Build());
            _lastPlayed = song;
            PlaybackSlider.Maximum = _player.CurrentSongLength.TotalSeconds;
        }
    }

    private void UpdatePlaybackSliderValue(object? sender, EventArgs e)
    {
        PlaybackSlider.ValueChanged -= PlaybackSlider_ValueChanged;
        PlaybackSlider.Value = _player.CurrentTime.TotalSeconds;
        PlaybackSlider.ValueChanged += PlaybackSlider_ValueChanged;
    }
}
