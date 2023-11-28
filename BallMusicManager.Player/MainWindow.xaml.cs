using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Ametrin.Utils;
using Ametrin.Utils.WPF;

namespace BallMusicManager.Player;

public sealed partial class MainWindow : Window{
    private static readonly DispatcherTimer Timer = new();
    public static MainWindow? Instance { get; private set; }


    static MainWindow()
    {
        Timer.Interval = TimeSpan.FromMilliseconds(100);
        MusicPlayer.OnPlaybackStarted += Timer.Start;
        MusicPlayer.OnPlaybackPaused += Timer.Stop;
    }

    public MainWindow()
    {
        InitializeComponent();
        Closing += MusicPlayer.OnExit;
        MusicPlayer.OnSongChanged += UpdateInfo;
        MusicPlayer.OnPlaylistChanged += UpdatePlaylistInfo;
        Timer.Tick += Tick;
        Instance = this;
        ServerConnection.Init();
    }

    public static void SetServerOnline(bool value)
    {
        Instance!.ServerOffline.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
    }

    public static void SetMissingFiles(bool value)
    {
        Instance!.MissingFiles.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    private void PlayToggleClicked(object sender, RoutedEventArgs e)
    {
        MusicPlayer.IsPlaying = !MusicPlayer.IsPlaying;
        PlayToggle.Content = MusicPlayer.IsPlaying ? "Pause" : "Play";
    }

    private void Skip(object sender, RoutedEventArgs args)
    {
        MusicPlayer.Move();
    }

    private void OpenFromFolder(object sender, RoutedEventArgs args)
    {
        using var dialog = new FolderBrowserDialog();

        if (dialog.ShowDialog() is not System.Windows.Forms.DialogResult.OK) return;

        MusicPlayer.OpenPlaylist(dialog.SelectedPath);
    }

    private void UpdateServer(object sender, RoutedEventArgs args)
    {
        ServerConnection.UpdateServer();
    }

    private void UpdateInfo()
    {
        CurrentTitle.Text = MusicPlayer.Current.Title;
        CurrentArtist.Text = MusicPlayer.Current.Artist;
        CurrentDance.Text = MusicPlayer.Current.Dance;
        SongsGrid.SelectedIndex = MusicPlayer.CurrentIndex;
    }

    private void UpdatePlaylistInfo()
    {
        CurrentPlaylist.Text = $"{MusicPlayer.CurrentPlaylist} ({MusicPlayer.Songs.Length})";
    }

    private void Tick(object? sender, EventArgs args)
    {
        RemaningTime.Text = (MusicPlayer.CurrentSongLength - MusicPlayer.CurrentTime).ToString(@"mm\:ss");
    }

    private void FixIndices(object sender, RoutedEventArgs args)
    {
        for (var i = 0; i < MusicPlayer.Songs.Length; i++)
        {
            var fileName = Path.GetFileName(MusicPlayer.Songs[i].Path);
            var splitName = fileName.Split('_');
            var directory = Path.GetDirectoryName(MusicPlayer.Songs[i].Path);
            var idx = i + 1;
            if (!splitName[0].TryParse(out int idxFromFileName))
            {
                if (splitName.Length < 3)
                {
                    var targetPath = $"{directory}/{idx}_{fileName}";
                    Trace.TraceInformation($"Moved {MusicPlayer.Songs[i].Path} to {targetPath}");
                    File.Move(MusicPlayer.Songs[i].Path, targetPath);
                    continue;
                }
            }

            if (idxFromFileName != idx)
            {
                splitName[0] = idx.ToString();
                fileName = splitName.Dump('_');
                var targetPath = Path.Join(directory, fileName);
                //Trace.TraceInformation($"Moved {MusicPlayer.Songs[i].Path} to {targetPath}");
                File.Move(MusicPlayer.Songs[i].Path, targetPath);
                continue;
            }
        }

        MusicPlayer.ReloadPlaylist();
    }

    private void ReloadPlaylist(object sender, RoutedEventArgs args)
    {
        MusicPlayer.ReloadPlaylist();
    }

    private void SkipTo(object sender, MouseButtonEventArgs args)
    {
        var row = (args.OriginalSource as DependencyObject)!.FindParent<DataGridRow>();

        if (row is null) return;
        MusicPlayer.Set(Array.IndexOf(MusicPlayer.Songs, row.Item));
    }

    private void OpenFromCSV(object sender, RoutedEventArgs e)
    {
        var dialog = DialogUtils.GetFileDialog(extension: "csv", filterDescription: "CSV");

        if (dialog.ShowDialog() is not true) return;

        MusicPlayer.OpenPlaylist(dialog.FileName);
    }

    private void EnterPlaying(object sender, RoutedEventArgs e)
    {
        ServerConnection.UpdateServer();
    }

    private void EnterWelcome(object sender, RoutedEventArgs e)
    {
        ServerConnection.SendMessage("Willkommen");
    }

    private void EnterEnd(object sender, RoutedEventArgs e)
    {
        ServerConnection.SendMessage("Tschüss");
    }
}
