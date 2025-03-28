using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace BallMusicManager.Player;

internal sealed class MainViewModel : INotifyPropertyChanged
{
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

    public MainViewModel()
    {
        Instance = this;
    }


    public Visibility ShowFixIndicesButton
    {
        get => _ShowFixIndicesButton;
        set
        {
            _ShowFixIndicesButton = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
