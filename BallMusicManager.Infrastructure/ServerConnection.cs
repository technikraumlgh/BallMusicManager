using System.Net;
using System.Net.Http.Json;
using BallMusicManager.Domain;

namespace BallMusicManager.Infrastructure;
public sealed class ServerConnection{
    public PlaylistPlayer? Playlist{
        get => playlistPlayer;
        set{
            if(playlistPlayer is not null){
                playlistPlayer.Player.OnSongChanged -= UpdateServer;
            }
            playlistPlayer = value;

            if(playlistPlayer is null) return;

            playlistPlayer.Player.OnSongChanged += UpdateServer;
        }
    }
    private readonly IHostProvider HostProvider;
    private PlaylistPlayer? playlistPlayer;

    public ServerConnection(IHostProvider hostProvider){
        HostProvider = hostProvider;
        _ = Ping($"http://{HostProvider.Host}/current");
    }

    public void UpdateServer() {
        SendSongToServer(Playlist?.Current ?? SongDTO.None);
        SendNextSongToServer(Playlist?.Peek ?? SongDTO.None);
    }

    public void SendNothing(){
        SendSongToServer(SongDTO.None);
        SendNextSongToServer(SongDTO.None);
    }

    public void SendSongToServer(SongDTO song) {
        _ = SendSong("playing", song);
    }
    
    public void SendNextSongToServer(SongDTO song) {
        _ = SendSong("nextup", song);
    }

    private async Task SendSong(string endpoint, SongDTO song){
        using var httpClient = new HttpClient();
        try{
            var res = await httpClient.PostAsJsonAsync($"http://{HostProvider.Host}/{endpoint}?key={HostProvider.Password}", song);
            HostProvider.SetServerOnline(res.StatusCode is HttpStatusCode.OK);
        }catch{
            HostProvider.SetServerOnline(false);
        }
    }
    
    public  async Task SendMessage(string msg){
        using var httpClient = new HttpClient();
        try{
            var res = await httpClient.PostAsJsonAsync($"http://{HostProvider.Host}/message?key={HostProvider.Password}", new MessageDTO(msg));
            HostProvider.SetServerOnline(res.StatusCode is HttpStatusCode.OK);
        }catch{
            HostProvider.SetServerOnline(false);
        }
    }

    private async Task Ping(string url) {
        using var client = new HttpClient();
        try {
            await client.GetAsync(url);
            HostProvider.SetServerOnline(true);
        } catch(Exception) {
            HostProvider.SetServerOnline(false);
        }
    }

}

public interface IHostProvider{
    public string Host {get;}
    public string Password {get;}

    void SetServerOnline(bool isOnline);
}
