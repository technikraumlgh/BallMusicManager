using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace BallMusicManager.Player;

internal sealed class MainViewModel : INotifyPropertyChanged
{

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

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
