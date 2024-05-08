using Ametrin.Utils;
using BallMusicManager.Domain;
using BallMusicManager.Server;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

const string KEY = "WB2023LGH"; //TODO: proper api keys
const string QR_CODE_FILE = "qr.png";

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddCors(options => options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyHeader()));
//builder.Services.AddCors();
builder.Services.AddSignalR();
builder.Services.AddSingleton<SignalService>();
//builder.Services.AddAuthentication();

var app = builder.Build();

var ip = SystemHelper.LocalIPAddress();
var url = $"http://{ip}";
app.Urls.Add(url);
OutputQRCode(url);

app.Urls.Add("http://localhost");

app.UseRouting();
//app.UseCors();

var signalService = app.Services.GetService<SignalService>()!;
var logger = app.Services.GetService<ILogger<Program>>()!;


var playing = SongDTO.None;
var next = playing;
logger.LogInformation("Running on {url}", url);


app.MapHub<SignalHub>("signal");

app.MapGet("playing", () => Results.Json(playing));

app.MapGet("nextup", () => Results.Json(next));

app.MapPost("playing", ([FromBody] Song song, string? key) => {
    if(key is null || key != KEY) return Results.Forbid();
    playing = song;
    _ = signalService.SendCurrentSong(playing);
    return Results.Ok();
});

app.MapPost("nextup", ([FromBody] Song song, string? key) => {
    if(key is null || key != KEY) return Results.Forbid();
    next = song;
    _ = signalService.SendNextSong(next);
    return Results.Ok();
});

app.MapGet("display", async (HttpResponse response, CancellationToken cancellationToken) => {
    await response.SendFileAsync("SongDisplay.html", cancellationToken);
});

app.MapGet("snow.js", async (HttpResponse response, CancellationToken cancellationToken) => {
    await response.SendFileAsync("snow.js", cancellationToken);
});

app.MapGet("qr-code", async (HttpResponse response, CancellationToken cancellationToken) => {
    await response.SendFileAsync(QR_CODE_FILE, cancellationToken);
});

app.MapPost("message", async ([FromBody] MessageDTO msg, string? key) =>{
    if (key is null || key != KEY) return Results.Forbid();

    await signalService.SendMessage(msg.text);

    return Results.Ok();
});

app.MapGet("/", ()=> Results.Redirect("display"));

app.Run();

static void OutputQRCode(string url, int size = 20) {
    using var qrGenerator = new QRCodeGenerator();
    using var qrDataData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
    using var qrCode = new PngByteQRCode(qrDataData);
    using var ms = File.Create(QR_CODE_FILE);
    ms.Write(qrCode.GetGraphic(size));
}