using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace BallMusicManager.Server;

internal sealed class SignalHub : Hub{
    public static ConcurrentDictionary<string, string> ConnectedClients = new();

    public override Task OnConnectedAsync(){
        ConnectedClients.TryAdd(Context.ConnectionId, Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception){
        ConnectedClients.TryRemove(Context.ConnectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }
}