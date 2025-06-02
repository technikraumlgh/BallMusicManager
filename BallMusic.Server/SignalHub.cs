using Microsoft.AspNetCore.SignalR;

namespace BallMusic.Server;

/// <summary>
/// triggers <see cref="DisplayService.InitNewClient(ISingleClientProxy)"/>
/// </summary>
/// <param name="_displayService"></param>
internal sealed class SignalHub(DisplayService _displayService) : Hub
{
    private readonly DisplayService _displayService = _displayService;

    public override async Task OnConnectedAsync()
    {
        _displayService.InitNewClient(Clients.Caller);
        await base.OnConnectedAsync();
    }
}