using System.Windows;
using Ametrin.Utils.WPF;

namespace BallMusicManager.Player;

internal sealed class MainViewModel : ObservableObject{
    public static MainViewModel Instance = default!;

    private bool _HasPlaylist = false;
    public bool HasPlaylist
    {
        get => _HasPlaylist;
        set
        {
            _HasPlaylist = value;
            OnPropertyChanged();
        }
    }

    private Visibility _ShowFixIndicesButton = Visibility.Collapsed;
    public Visibility ShowFixIndicesButton
    {
        get => _ShowFixIndicesButton;
        set
        {
            _ShowFixIndicesButton = value;
            OnPropertyChanged();
        }
    }

    public Song[] Songs => MusicPlayer.Songs;

    public MainViewModel()
    {
        Instance = this;
        MusicPlayer.OnPlaylistChanged += UpdateHasPlaylist;
    }

    private void UpdateHasPlaylist()
    {
        HasPlaylist = MusicPlayer.CurrentPlaylist is not null;
        OnPropertyChanged(nameof(Songs));
    }
}
