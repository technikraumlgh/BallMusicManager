using Microsoft.AspNetCore.SignalR;

namespace BallMusicManager.Server;

internal sealed class SignalHub(DisplayService _displayService) : Hub{
    private readonly DisplayService _displayService = _displayService;

    //public static ConcurrentDictionary<string, string> ConnectedClients = new();

    public override async Task OnConnectedAsync(){
        _displayService.InitNewClient(Clients.Caller);
        //ConnectedClients.TryAdd(Context.ConnectionId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    //public override Task OnDisconnectedAsync(Exception? exception){
    //    //ConnectedClients.TryRemove(Context.ConnectionId, out _);
    //    return base.OnDisconnectedAsync(exception);
    //}
}