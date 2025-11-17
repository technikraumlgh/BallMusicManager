using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BallMusic.Player;

// this class got a bit unnecessay over time. feel free to refactor it away
internal sealed class MainViewModel : INotifyPropertyChanged
{
    public bool HasPlaylist
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
