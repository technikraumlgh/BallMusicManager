using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Ametrin.Utils.WPF;
using BallMusicManager.Domain;

namespace BallMusicManager.Player;

internal sealed class MainViewModel : ObservableObject{
    public static MainViewModel Instance = default!;

    private bool _HasPlaylist = false;
    public bool HasPlaylist{
        get => _HasPlaylist;
        set{
            _HasPlaylist = value;
            OnPropertyChanged();
        }
    }

    private Visibility _ShowFixIndicesButton = Visibility.Collapsed;
    public Visibility ShowFixIndicesButton{
        get => _ShowFixIndicesButton;
        set{
            _ShowFixIndicesButton = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<Song> Songs => MusicPlayer.Playlist?.Songs ?? Enumerable.Empty<Song>();

    public MainViewModel(){
        Instance = this;
        MusicPlayer.OnPlaylistChanged += UpdateHasPlaylist;
    }

    private void UpdateHasPlaylist(){
        HasPlaylist = MusicPlayer.Playlist is not null;
        OnPropertyChanged(nameof(Songs));
    }
}
