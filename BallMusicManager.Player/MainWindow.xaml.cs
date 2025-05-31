using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using BallMusic.WPF;
using BallMusic.WPF.FileDialogs;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;

namespace BallMusicManager.Player;

public sealed partial class MainWindow : Window, IHostProvider
{
    private readonly DispatcherTimer Timer = new();
    private MainViewModel ViewModel => (MainViewModel) DataContext;
    private PlaylistPlayer? Playlist
    {
        get => playlist;
        set
        {

            Timer.Stop();
            SongsGrid.ItemsSource = null;
            if (playlist is not null)
            {
                playlist.Player.OnSongChanged -= SendCurrentSongsToServer;
                playlist.Player.OnSongStarted -= Timer.Start;
                playlist.Player.OnSongContinued -= Timer.Start;
                playlist.Player.OnSongPaused -= Timer.Stop;
                playlist.Player.OnSongChanged -= UpdateInfo;
            }

            playlist = value;

            if (playlist is null) return;
            playlist.Player.OnSongChanged += SendCurrentSongsToServer;
            playlist.Player.OnSongStarted += Timer.Start;
            playlist.Player.OnSongContinued += Timer.Start;
            playlist.Player.OnSongPaused += Timer.Stop;
            playlist.Player.OnSongChanged += UpdateInfo;
            SongsGrid.ItemsSource = playlist.Songs;
            UpdatePlaylistInfo();
            UpdateInfo();

            if (playlist.IsEmpty) return;
            ViewModel.HasPlaylist = true;
            SongsGrid.SelectedIndex = 0;
        }
    }
    private readonly ServerConnection Server;
    private PlaylistPlayer? playlist;

    string IHostProvider.Host => Host.Text;
    string IHostProvider.Password => HostPW.Password;

    public MainWindow()
    {
        InitializeComponent();
        Server = new(this);
        Timer.Interval = TimeSpan.FromMilliseconds(250);
        Timer.Tick += UpdateDuration;

        Closed += (_, _) =>
        {
            _messageWindow?.Close();
            Server.Dispose();
        };
    }

    public void SetServerOnline(bool value)
    {
        ServerOffline.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
    }

    private void PlayToggleClicked(object sender, RoutedEventArgs e)
    {
        if (Playlist is null) return;

        if (Playlist.IsPlaying)
        {
            Playlist.Pause();
            PlayToggle.Content = "Play";
        }
        else
        {
            Playlist.Play();
            PlayToggle.Content = "Pause";
            SendCurrentSongsToServer();
        }
    }

    private void Skip(object sender, RoutedEventArgs args)
    {
        if (Playlist is null) return;
        Playlist.Skip();
    }

    private void UpdateServer(object sender, RoutedEventArgs args)
    {
        SendCurrentSongsToServer();
    }

    private void UpdateInfo()
    {
        if (Playlist is not null && !Playlist.IsEmpty) SongsGrid.SelectedIndex = Playlist.CurrentIndex;
        CurrentTitle.Content = Playlist?.Current?.Title ?? "Title";
        CurrentArtist.Content = Playlist?.Current?.Artist ?? "Artist";
        CurrentDance.Content = Playlist?.Current?.Dance ?? "Dance";
        RemaningTime.Content = Playlist?.Player.CurrentSongLength.ToString("mm\\:ss") ?? "Duration";
        PlaybackBar.Maximum = Playlist?.Player.CurrentSongLength.TotalSeconds ?? 0;
        PlaybackBar.Value = Playlist?.Player.CurrentSongLength.TotalSeconds ?? 0;
        UpdateDuration();
        SendCurrentSongsToServer();
    }

    private void UpdatePlaylistInfo()
    {
        CurrentPlaylist.Text = $"{Playlist?.Path} ({Playlist?.Length})";
    }

    private void UpdateDuration(object? sender = default, EventArgs? args = default)
    {
        if (Playlist is null)
        {
            RemaningTime.Content = "Duration";
            PlaybackBar.Value = 0;
            Timer.Stop();
            return;
        }
        RemaningTime.Content = (Playlist.Player.CurrentSongLength - Playlist.Player.CurrentTime).ToString(@"mm\:ss"); ;
        PlaybackBar.Value = Playlist.Player.CurrentTime.TotalSeconds;
    }

    private void SkipTo(object sender, MouseButtonEventArgs args)
    {
        if (Playlist is null) return;
        var row = (args.OriginalSource as DependencyObject)!.FindParent<DataGridRow>();
        if (row is null || row.Item is not Song song) return;
        if (Playlist.Current == song)
        {
            Playlist.Player.Restart();
        }
        else
        {
            Playlist.SetCurrent(Playlist.Songs.IndexOf(song));
        }
    }

    private void OpenFromPlaylist(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog().AddExtensionFilter("Playlist", "plz");

        // we swallow error from GetFileInfo because it means the user canceled
        dialog.GetFileInfo().Consume(
            success: (file) => PlaylistBuilder.FromArchive(file).Consume(
                success: playlist => Playlist = playlist,
                error: e => MessageBoxHelper.ShowError($"Playlist corrupted:\n{e.Message}", owner: this)
            )
        );
            
    }

    public void SendCurrentSongsToServer()
    {
        Server.SendSongToServer(SongDTO.From(Playlist?.Current));
        Server.SendNextSongToServer(SongDTO.From(Playlist?.Peek));
    }

    private MessageWindow? _messageWindow;
    private void OpenMessageWindow(object sender, RoutedEventArgs e)
    {
        if (_messageWindow is not null)
        {
            if (!_messageWindow.Activate())
            {
                _messageWindow = null;
            }
        }

        if (_messageWindow is null)
        {
            _messageWindow = new MessageWindow(Server);
            _messageWindow.Show();
        }
    }
}
