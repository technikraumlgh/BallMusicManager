using BallMusic.Domain;
using Microsoft.AspNetCore.SignalR;

namespace BallMusic.Server;

internal sealed class DisplayService(IHubContext<SignalHub> _hubContext)
{
    const string CURRENT_SIGNAL = "current";
    const string NEXT_SIGNAL = "next";
    const string MESSAGE_SIGNAL = "message";
    const string NEWS_SIGNAL = "news";
    private readonly IHubContext<SignalHub> _hubContext = _hubContext;

    private CurrentState _currentState = CurrentState.Songs;
    private string _lastMessage = string.Empty;
    private string _lastNews = string.Empty;

    public SongDTO[] Playlist { get; set; } = [SongDTO.None, SongDTO.None];

    public void SetCurrent(SongDTO song)
    {
        _currentState = CurrentState.Songs;
        Playlist[0] = song;
        _ = SendSongToAllClients(CURRENT_SIGNAL, Playlist[0]);
    }

    public void SetNext(SongDTO song)
    {
        _currentState = CurrentState.Songs;
        Playlist[1] = song;
        _ = SendSongToAllClients(NEXT_SIGNAL, Playlist[1]);
    }

    public void SendMessage(string message)
    {
        _currentState = CurrentState.Message;
        _lastMessage = message;
        _ = SendToAllClients(MESSAGE_SIGNAL, message);
    }

    public void SendNews(string news)
    {
        _lastNews = news;
        _ = SendToAllClients(NEWS_SIGNAL, news);
    }

    public void InitNewClient(ISingleClientProxy client)
    {
        switch (_currentState)
        {
            case CurrentState.Songs:
                client.SendAsync(CURRENT_SIGNAL, Playlist[0].ToJson());
                client.SendAsync(NEXT_SIGNAL, Playlist[1].ToJson());
                break;
            case CurrentState.Message:
                client.SendAsync(MESSAGE_SIGNAL, _lastMessage);
                break;
        }
        client.SendAsync(NEWS_SIGNAL, _lastNews);
    }

    public async Task SendSongToAllClients(string method, SongDTO song, CancellationToken cancellationToken = default) =>
        await SendToAllClients(method, song.ToJson(), cancellationToken);

    public async Task SendToAllClients(string method, string message, CancellationToken cancellationToken = default) =>
        await _hubContext.Clients.All.SendAsync(method, message, cancellationToken: cancellationToken);

    enum CurrentState { Songs, Message }
}
