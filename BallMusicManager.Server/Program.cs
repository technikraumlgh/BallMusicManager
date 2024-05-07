using Ametrin.Utils;
using BallMusicManager.Domain;
using BallMusicManager.Server;
using Microsoft.AspNetCore.Mvc;

const string KEY = "WB2023LGH";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyHeader()));
builder.Services.AddSignalR();
builder.Services.AddSingleton<SignalService>();

var app = builder.Build();

var url = $"http://{SystemExtensions.LocalIPv4Address()}";
app.Urls.Add("http://localhost");
app.Urls.Add(url);
app.UseRouting();
app.UseCors();

var logger = app.Services.GetService<ILogger<Program>>()!;
logger.LogInformation(SystemExtensions.LocalIPv4Addresses().Dump('\n'));

var signalService = app.Services.GetService<SignalService>()!;

var playing = SongDTO.None;
var next = playing;
//var state = State.Welcome;

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

app.MapPost("message", async ([FromBody] MessageDTO msg, string? key) =>{
    if (key is null || key != KEY) return Results.Forbid();

    await signalService.SendMessage(msg.text);

    return Results.Ok();
});

app.MapGet("/", ()=> Results.Redirect("display"));

app.Run();

//record Song(string title, string artist, string dance);
//enum State { Welcome, Playing, End }