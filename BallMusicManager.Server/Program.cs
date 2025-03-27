using System.Drawing;
using System.Net;
using System.Net.Sockets;
using Ametrin.Optional;
using BallMusicManager.Domain;
using BallMusicManager.Server;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

const string KEY = "WB2023LGH"; //TODO: proper api keys
const string QR_CODE_FILE = "qr.png";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

//builder.Services.AddCors(options => options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyHeader()));
//builder.Services.AddCors();
builder.Services.AddSignalR();
builder.Services.AddSingleton<DisplayService>();
builder.Services.AddAuthentication(); // TODO: proper auth

var app = builder.Build();
var logger = app.Services.GetService<ILogger<Program>>()!;

app.Urls.Add("http://localhost");

LocalIPAddress().Map(ip => $"http://{ip}").Consume(url =>
{
    app.Urls.Add(url);
    OutputQRCode(url);
    logger.LogInformation("Running on {url}", url);
}, () =>
{
    logger.LogWarning("No public IP found!");
});


app.UseRouting();
app.UseAuthorization();
//app.UseCors();

var displayService = app.Services.GetService<DisplayService>()!;

app.MapHub<SignalHub>("signal");

app.MapPost("playing", ([FromBody] SongDTO song, string? key) =>
{
    if (key is null || key != KEY) return Results.NotFound();
    displayService.SetCurrent(song);
    return Results.Ok();
});

app.MapPost("nextup", ([FromBody] SongDTO song, string? key) =>
{
    if (key is null || key != KEY) return Results.NotFound();
    displayService.SetNext(song);
    return Results.Ok();
});

app.MapGet("display", async (HttpResponse response, CancellationToken cancellationToken) =>
{
    await response.SendFileAsync("SongDisplay.html", cancellationToken);
});


app.MapPost("message", ([FromBody] MessageDTO msg, string? key) =>
{
    if (key is null || key != KEY) return Results.NotFound();

    displayService.SendMessage(msg.text);

    return Results.Ok();
});

app.MapPost("news", ([FromBody] MessageDTO msg, string? key) =>
{
    if (key is null || key != KEY) return Results.NotFound();

    displayService.SendNews(msg.text);

    return Results.Ok();
});

app.MapGet("snow.js", async (HttpResponse response, CancellationToken cancellationToken) =>
{
    await response.SendFileAsync("snow.js", cancellationToken);
});

app.MapGet("qr-code", async (HttpResponse response, CancellationToken cancellationToken) =>
{
    await response.SendFileAsync(QR_CODE_FILE, cancellationToken);
});
app.MapGet("/", () => Results.Redirect("display"));

app.Run();

static void OutputQRCode(string url, int size = 20)
{
    using var qrGenerator = new QRCodeGenerator();
    using var qrDataData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
    using var qrCode = new PngByteQRCode(qrDataData);
    using var ms = File.Create(QR_CODE_FILE);
    ms.Write(qrCode.GetGraphic(size, Color.Black, Color.White));
}

static Option<IPAddress> LocalIPAddress()
{
    try
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect("8.8.8.8", 65530);
        var endPoint = socket.LocalEndPoint as IPEndPoint;
        return endPoint!.Address;
    }
    catch
    {
        return default;
    }
}