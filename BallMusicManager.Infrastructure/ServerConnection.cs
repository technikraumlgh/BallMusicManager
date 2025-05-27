using System.Net;
using System.Net.Http.Json;

namespace BallMusicManager.Infrastructure;

public sealed class ServerConnection : IDisposable
{
    public PlaylistPlayer? Playlist
    {
        get => playlistPlayer;
        set
        {
            if (playlistPlayer is not null)
            {
                playlistPlayer.Player.OnSongChanged -= Update;
            }
            playlistPlayer = value;

            if (playlistPlayer is null) return;

            playlistPlayer.Player.OnSongChanged += Update;
        }
    }
    private readonly IHostProvider HostProvider;
    private PlaylistPlayer? playlistPlayer;
    private HttpClient httpClient;

    public ServerConnection(IHostProvider hostProvider)
    {
        HostProvider = hostProvider;
        httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(500),
        };
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", HostProvider.Password);
        _ = Ping($"http://{HostProvider.Host}/");
    }

    public void Update()
    {
        SendSongToServer(SongDTO.From(Playlist?.Current));
        SendNextSongToServer(SongDTO.From(Playlist?.Peek));
    }

    public void SendNothing()
    {
        SendSongToServer(SongDTO.None);
        SendNextSongToServer(SongDTO.None);
    }

    public void SendSongToServer(SongDTO song)
    {
        _ = SendSong("playing", song);
    }

    public void SendNextSongToServer(SongDTO song)
    {
        _ = SendSong("nextup", song);
    }

    private async Task SendSong(string endpoint, SongDTO song)
    {
        try
        {
            var res = await httpClient.PostAsJsonAsync($"http://{HostProvider.Host}/{endpoint}", song);
            HostProvider.SetServerOnline(res.StatusCode is HttpStatusCode.OK);
        }
        catch
        {
            HostProvider.SetServerOnline(false);
        }
    }

    public async Task SendMessage(string msg)
    {
        try
        {
            var res = await httpClient.PostAsJsonAsync($"http://{HostProvider.Host}/message", new MessageDTO(msg));
            HostProvider.SetServerOnline(res.StatusCode is HttpStatusCode.OK);
        }
        catch
        {
            HostProvider.SetServerOnline(false);
        }
    }

    public async Task SendNews(string news)
    {
        try
        {
            var res = await httpClient.PostAsJsonAsync($"http://{HostProvider.Host}/news", new MessageDTO(news));
            HostProvider.SetServerOnline(res.StatusCode is HttpStatusCode.OK);
        }
        catch
        {
            HostProvider.SetServerOnline(false);
        }
    }

    private async Task Ping(string url)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMilliseconds(500);
        try
        {
            await httpClient.GetAsync(url);
            HostProvider.SetServerOnline(true);
        }
        catch (Exception)
        {
            HostProvider.SetServerOnline(false);
        }
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
}

public interface IHostProvider
{
    public string Host { get; }
    public string Password { get; }

    void SetServerOnline(bool isOnline);
}
