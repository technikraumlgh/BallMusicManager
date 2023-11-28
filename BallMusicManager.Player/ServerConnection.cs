using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BallMusicManager.Player;
internal static class ServerConnection {
    private static readonly Song NoSong = new ("", MusicPlayer.Songs.Length, "Ende", "Niemand", "");

    static ServerConnection(){
        MusicPlayer.OnSongChanged += UpdateServer;
    }

    public static void Init() {
        _ = Ping($"http://{MainWindow.Instance?.Host.Text}/current");
    }

    public static void UpdateServer() {
        SendSongToServer(MusicPlayer.Current);
        SendNextSongToServer(MusicPlayer.Peek ?? NoSong);
    }

    public static void SendNothing(){
        SendSongToServer(NoSong);
        SendNextSongToServer(NoSong);
    }

    public static void SendSongToServer(Song song) {
        SendSong("playing", song);
    }
    
    public static void SendNextSongToServer(Song? song) {
        SendSong("nextup", song);
    }

    private static async void SendSong(string endpoint, Song song){
        using var httpClient = new HttpClient();
        try{
            var res = await httpClient.PostAsJsonAsync($"http://{MainWindow.Instance?.Host.Text}/{endpoint}?key={MainWindow.Instance?.HostPW.Password}", (SongDTO)song);
            MainWindow.SetServerOnline(res.StatusCode is HttpStatusCode.OK);
        }catch{
            MainWindow.SetServerOnline(false);
        }
    }
    
    public static async void SendMessage(string msg){
        using var httpClient = new HttpClient();
        try{
            var res = await httpClient.PostAsJsonAsync($"http://{MainWindow.Instance?.Host.Text}/message?key={MainWindow.Instance?.HostPW.Password}", new MessageDTO(msg));
            MainWindow.SetServerOnline(res.StatusCode is HttpStatusCode.OK);
        }catch{
            MainWindow.SetServerOnline(false);
        }
    }

    private static async Task Ping(string url) {
        using var client = new HttpClient();
        try {
            await client.GetAsync(url);
            MainWindow.SetServerOnline(true);
        } catch(Exception) {
            MainWindow.SetServerOnline(false);
        }
    }

    record MessageDTO(string text);
}
