using System.Drawing;
using System.Net;
using System.Net.Sockets;
using Ametrin.Optional;
using BallMusic.Domain;
using BallMusic.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using QRCoder;

const string QR_CODE_FILE = "qr.png";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<DisplayService>();
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

var app = builder.Build();
var logger = app.Services.GetService<ILogger<Program>>()!;

var themeIndex = args.IndexOf("--theme");
if (themeIndex == -1)
{
    logger.LogError("No Theme specified. Use '--theme (spring|winter)'");
    return;
}
var theme = args[themeIndex + 1];
if (!Path.Exists($"src/{theme}.css"))
{
    logger.LogError("Theme {theme} not found", theme);
    return;
}

logger.LogInformation("Theme: {theme}", theme);

var displayHtml = File.ReadAllText("src/display.html").Replace("${theme}", theme);

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

var useAuth = !args.Contains("--noauth");
var activeApiKey = useAuth ? Guid.NewGuid().ToString()[..23] : string.Empty;
if (useAuth)
{
    logger.LogInformation("API key for this session: {key}", activeApiKey);
}

logger.LogInformation("Press Ctrl-C or close the console to shutdown");

var displayService = app.Services.GetService<DisplayService>()!;

app.MapHub<SignalHub>("signal");

app.MapPost("playing", ([FromBody] SongDTO song) =>
{
    displayService.SetCurrent(song);
    return Results.Ok();
}).AddEndpointFilter(RequiresApiKey);

app.MapPost("nextup", ([FromBody] SongDTO song) =>
{
    displayService.SetNext(song);
    return Results.Ok();
}).AddEndpointFilter(RequiresApiKey);

app.MapGet("display", (HttpResponse response, CancellationToken cancellationToken) =>
{
    return Results.Content(displayHtml, "text/html");
});


app.MapPost("message", ([FromBody] MessageDTO msg) =>
{
    displayService.SendMessage(msg.text);

    return Results.Ok();
}).AddEndpointFilter(RequiresApiKey);

app.MapPost("news", ([FromBody] MessageDTO msg) =>
{
    displayService.SendNews(msg.text);

    return Results.Ok();
}).AddEndpointFilter(RequiresApiKey);

app.MapGet("qr-code", async (HttpResponse response, CancellationToken cancellationToken) =>
{
    await response.SendFileAsync(QR_CODE_FILE, cancellationToken);
});
app.MapGet("/", () => Results.Redirect("display"));

string resourcePath = Path.GetFullPath("src/");
app.MapGet("src/{file}", async (HttpResponse response, [FromRoute] string file, FileExtensionContentTypeProvider contentTypeProvider, CancellationToken cancellationToken) =>
{
    var fullPath = Path.GetFullPath(file, resourcePath);
    if (!fullPath.StartsWith(resourcePath)) return Results.NotFound();
    if (!Path.Exists(fullPath)) return Results.NotFound();

    if (!contentTypeProvider.TryGetContentType(fullPath, out var contentType))
    {
        contentType = "application/octet-stream"; // fallback
    }

    response.ContentType = contentType;

    await response.SendFileAsync(fullPath, cancellationToken);
    return Results.Empty;
});

app.Run();

async ValueTask<object?> RequiresApiKey(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
{
    var httpContext = context.HttpContext;
    var requestApiKey = httpContext.Request.Headers.Authorization.FirstOrDefault();

    if (useAuth && requestApiKey != activeApiKey)
    {
        return Results.Unauthorized();
    }

    return await next(context);
}

static void OutputQRCode(string url, int size = 20)
{
    using var qrGenerator = new QRCodeGenerator();
    using var qrDataData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
    using var qrCode = new PngByteQRCode(qrDataData);
    using var stream = File.Create(QR_CODE_FILE);
    stream.Write(qrCode.GetGraphic(size, Color.Black, Color.White));
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
