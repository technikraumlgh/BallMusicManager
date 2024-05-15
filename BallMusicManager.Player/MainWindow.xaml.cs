using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Ametrin.Utils.Optional;
using Ametrin.Utils.WPF;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;

namespace BallMusicManager.Player;

public sealed partial class MainWindow : Window, IHostProvider {
    private readonly DispatcherTimer Timer = new();
    private PlaylistPlayer? Playlist {
        get => playlist;
        set {
            Timer.Stop();
            SongsGrid.ItemsSource = null;
            if(playlist is not null){
                playlist.Player.OnSongStarted -= Timer.Start;
                playlist.Player.OnSongContinued -= Timer.Start;
                playlist.Player.OnSongPaused -= Timer.Stop;
                playlist.Player.OnSongChanged -= UpdateInfo;
            }

            playlist = value;

            if(playlist is null) return;
            playlist.Player.OnSongStarted += Timer.Start;
            playlist.Player.OnSongContinued += Timer.Start;
            playlist.Player.OnSongPaused += Timer.Stop;
            playlist.Player.OnSongChanged += UpdateInfo;
            Server.Playlist = playlist;
            SongsGrid.ItemsSource = playlist.Songs;
            UpdatePlaylistInfo();
            UpdateInfo();

            if(playlist.IsEmpty) return;
            MainViewModel.Instance.HasPlaylist = true;
            SongsGrid.SelectedIndex = 0;
        }
    }
    private readonly ServerConnection Server;
    private PlaylistPlayer? playlist;

    string IHostProvider.Host => Host.Text;
    string IHostProvider.Password => HostPW.Password;

    public MainWindow() {
        InitializeComponent();
        Server = new(this);
        Timer.Interval = TimeSpan.FromMilliseconds(250);
        //Closing += MusicPlayer.OnExit;
        Timer.Tick += UpdateDuration;
    }

    public void SetServerOnline(bool value) {
        ServerOffline.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
    }

    public void SetMissingFiles(bool value) {
        MissingFiles.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    private void PlayToggleClicked(object sender, RoutedEventArgs e) {
        if(Playlist is null) return;
        
        if(Playlist.IsPlaying) {
            Playlist.Pause();
            PlayToggle.Content = "Play";
        }else {
            Playlist.Play();
            PlayToggle.Content = "Pause";
            Server.Update();
        }
    }

    private void Skip(object sender, RoutedEventArgs args) {
        if(Playlist is null) return;
        Playlist.Skip();
    }

    private void UpdateServer(object sender, RoutedEventArgs args) {
        Server.Update();
    }

    private void UpdateInfo() {
        if (Playlist is not null && !Playlist.IsEmpty) SongsGrid.SelectedIndex = Playlist.CurrentIndex;
        CurrentTitle.Text = Playlist?.Current?.Title ?? "Title";
        CurrentArtist.Text = Playlist?.Current?.Artist ?? "Artist";
        CurrentDance.Text = Playlist?.Current?.Dance ?? "Dance";
        RemaningTime.Text = Playlist?.Player.CurrentSongLength.ToString("mm\\:ss") ?? "Duration";
        PlaybackBar.Maximum = Playlist?.Player.CurrentSongLength.TotalSeconds ?? 0;
        PlaybackBar.Value = Playlist?.Player.CurrentSongLength.TotalSeconds ?? 0;
        UpdateDuration();
        Server.Update();
    }

    private void UpdatePlaylistInfo() {
        CurrentPlaylist.Text = $"{Playlist?.Path} ({Playlist?.Length})";
    }

    private void UpdateDuration(object? sender = default, EventArgs? args = default) {
        if(Playlist is null){
            RemaningTime.Text = "Duration";
            PlaybackBar.Value = 0;
            Timer.Stop();
            return;
        }
        RemaningTime.Text = (Playlist.Player.CurrentSongLength - Playlist.Player.CurrentTime).ToString(@"mm\:ss"); ;
        PlaybackBar.Value = Playlist.Player.CurrentTime.TotalSeconds;
    }

    private void SkipTo(object sender, MouseButtonEventArgs args) {
        if(Playlist is null) return;
        var row = (args.OriginalSource as DependencyObject)!.FindParent<DataGridRow>();
        if (row is null || row.Item is not Song song) return;
        if(Playlist.Current == song) {
            Playlist.Player.Restart();
        } else {
            Playlist.SetCurrent(Playlist.Songs.IndexOf(song));
        }
    }

    private void OpenFromPlaylist(object sender, RoutedEventArgs e) {
        var dialog = DialogUtils.GetFileDialog(filterDescription: "Playlists", extension: "plz");
        if(dialog.ShowDialog() is not true) return;

        PlaylistBuilder.FromArchive(new(dialog.FileName)).Resolve(playlist => Playlist = playlist);
    }

    private void OpenMessageWindow(object sender, RoutedEventArgs e) {
        new MessageWindow(Server).Show();
        //Task.Run(()=> );
    }
}
