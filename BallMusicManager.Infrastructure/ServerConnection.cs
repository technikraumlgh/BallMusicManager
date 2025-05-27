using System.Net;
using System.Net.Http.Headers;
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
        _ = PostRequest("playing", song);
    }

    public void SendNextSongToServer(SongDTO song)
    {
        _ = PostRequest("nextup", song);
    }

    public Task SendMessage(string msg) => PostRequest("message", new MessageDTO(msg));
    public Task SendNews(string news) => PostRequest("news", new MessageDTO(news));

    private async Task PostRequest<T>(string endpoint, T data)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"http://{HostProvider.Host}/{endpoint}")
            {
                Content = JsonContent.Create(data)
            };
            if (!string.IsNullOrEmpty(HostProvider.Password))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(HostProvider.Password);
            }

            var result = await httpClient.SendAsync(request);

            HostProvider.SetServerOnline(result.StatusCode is HttpStatusCode.OK);
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
