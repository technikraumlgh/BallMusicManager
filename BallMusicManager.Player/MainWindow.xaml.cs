using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Ametrin.Utils;
using Ametrin.Utils.WPF;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;

namespace BallMusicManager.Player;

public sealed partial class MainWindow : Window, IHostProvider{
    private readonly DispatcherTimer Timer = new();
    private PlaylistPlayer? Playlist{
        get => playlist;
        set{
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

    public MainWindow(){
        InitializeComponent();
        Server = new(this);
        Timer.Interval = TimeSpan.FromMilliseconds(100);
        //Closing += MusicPlayer.OnExit;
        Timer.Tick += Tick;
    }

    public void SetServerOnline(bool value){
        ServerOffline.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
    }

    public void SetMissingFiles(bool value){
        MissingFiles.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    private void PlayToggleClicked(object sender, RoutedEventArgs e){
        if(Playlist is null) return;
        
        if(Playlist.IsPlaying){
            Playlist.Pause();
            PlayToggle.Content = "Play";
        }else{
            Playlist.Play();
            PlayToggle.Content = "Pause";
        }
    }

    private void Skip(object sender, RoutedEventArgs args){
        if(Playlist is null) return;
        Playlist.Skip();
        SongsGrid.SelectedIndex = Playlist.CurrentIndex;
    }

    private void OpenFromFolder(object sender, RoutedEventArgs args){
        using var dialog = new FolderBrowserDialog();

        if (dialog.ShowDialog() is not System.Windows.Forms.DialogResult.OK) return;

        Playlist = PlaylistBuilder.FromFolder(new(dialog.SelectedPath));
    }

    private void UpdateServer(object sender, RoutedEventArgs args){
        Server.UpdateServer();
    }

    private void UpdateInfo(){
        CurrentTitle.Text = Playlist?.Current?.Title ?? "Title";
        CurrentArtist.Text = Playlist?.Current?.Artist ?? "Artist";
        CurrentDance.Text = Playlist?.Current?.Dance ?? "Dance";
        Server.UpdateServer();
    }

    private void UpdatePlaylistInfo(){
        CurrentPlaylist.Text = $"{Playlist?.Path} ({Playlist?.Length})";
    }

    private void Tick(object? sender, EventArgs args){
        if(Playlist is null){
            RemaningTime.Text = "Time";
            Timer.Stop();
            return;
        }
        RemaningTime.Text = (Playlist.Player.CurrentSongLength - Playlist.Player.CurrentTime).ToString(@"mm\:ss"); ;
    }

    // private void FixIndices(object sender, RoutedEventArgs args){
    //     for (var i = 0; i < MusicPlayer.Playlist.Length; i++)
    //     {
    //         var fileName = Path.GetFileName(MusicPlayer.Playlist[i].Path);
    //         var splitName = fileName.Split('_');
    //         var directory = Path.GetDirectoryName(MusicPlayer.Playlist[i].Path);
    //         var idx = i + 1;
    //         if (!splitName[0].TryParse(out int idxFromFileName))
    //         {
    //             if (splitName.Length < 3)
    //             {
    //                 var targetPath = $"{directory}/{idx}_{fileName}";
    //                 Trace.TraceInformation($"Moved {MusicPlayer.Playlist[i].Path} to {targetPath}");
    //                 File.Move(MusicPlayer.Playlist[i].Path, targetPath);
    //                 continue;
    //             }
    //         }

    //         if (idxFromFileName != idx)
    //         {
    //             splitName[0] = idx.ToString();
    //             fileName = splitName.Dump('_');
    //             var targetPath = Path.Join(directory, fileName);
    //             //Trace.TraceInformation($"Moved {MusicPlayer.Songs[i].Path} to {targetPath}");
    //             File.Move(MusicPlayer.Playlist[i].Path, targetPath);
    //             continue;
    //         }
    //     }

    //     MusicPlayer.ReloadPlaylist();
    // }

    private void ReloadPlaylist(object sender, RoutedEventArgs args){
        //MusicPlayer.ReloadPlaylist();
    }

    private void SkipTo(object sender, MouseButtonEventArgs args){
        if(Playlist is null) return;
        var row = (args.OriginalSource as DependencyObject)!.FindParent<DataGridRow>();
        if (row is null || row.Item is not Song song) return;
        Playlist.SetCurrent(Playlist.Songs.IndexOf(song));
    }

    private void OpenFromCSV(object sender, RoutedEventArgs e){
        var dialog = DialogUtils.GetFileDialog(extension: "csv", filterDescription: "CSV");

        if (dialog.ShowDialog() is not true) return;

        Playlist = PlaylistBuilder.FromCSV(new(dialog.FileName));
    }

    private void EnterPlaying(object sender, RoutedEventArgs e){
        Server.UpdateServer();
    }

    private void EnterWelcome(object sender, RoutedEventArgs e){
        _ = Server.SendMessage("Willkommen");
    }

    private void EnterEnd(object sender, RoutedEventArgs e){
        _ = Server.SendMessage("Tschüss");
    }
}
