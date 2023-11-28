using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

namespace BallMusicManager.Server;

internal class SignalService{
    private readonly IHubContext<SignalHub> hubContext;

    public SignalService(IHubContext<SignalHub> _hubContext)
    {
        hubContext = _hubContext;
    }

    internal async Task SendMessage(string msg, CancellationToken? cancellationToken = null)
    {
        await SendToAllClients("message", msg, cancellationToken);
    }

    internal async Task SendCurrentSong(Song song, CancellationToken? cancellationToken = null)
    {
        await SendSongToAllClients("current", song, cancellationToken);
    }
    internal async Task SendNextSong(Song song, CancellationToken? cancellationToken = null)
    {
        await SendSongToAllClients("next", song, cancellationToken);
    }
    internal async Task SendSongToAllClients(string method, Song song, CancellationToken? cancellationToken = null)
    {
        await SendToAllClients(method, JsonSerializer.Serialize(song), cancellationToken);
    }

    internal async Task SendToAllClients(string method, string message, CancellationToken? cancellationToken = null)
    {
        await hubContext.Clients.All.SendAsync(method, message, cancellationToken);
    }
}