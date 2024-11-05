using BallMusicManager.Domain;
using System.Windows.Input;
using System.Windows;
using BallMusicManager.Infrastructure;
using System.Windows.Threading;
using Ametrin.Utils.WPF;
using System.Windows.Controls;

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

        if(song is null)
        {
            return;
        }

        UpdatePlayback(song);
        song.SetDuration(_player.CurrentSongLength);
        _player.Play();
        _playbackProgressUpdater.Start();
        PlayButton.Content = "\uE769";
    }

    private void PausePlayback()
    {
        _player.Pause();
        _playbackProgressUpdater.Stop();
        PlayButton.Content = "\uE768";
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
