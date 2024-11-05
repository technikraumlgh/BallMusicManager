﻿using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BallMusicManager.Domain;

public sealed record SongBuilder()
{
    [JsonConverter(typeof(SongLocationJsonConverter))]
    public SongLocation Path { get; set; } = new UndefinedLocation(); // do not rename (json serialization)
    public int Index { get; set; } = -1;
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Dance { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
    public string FileHash
    {
        get
        {
            if (!HasFileHash)
            {
                hash = Path switch
                {
                    FileLocation file => file.FileInfo.ComputeSha256Hash(),
                    ArchiveLocation file => GetHash(file),
                    _ => throw new InvalidOperationException("Cannot compute hash for a song without proper location"),
                };
            }
            return hash;

            static string GetHash(ArchiveLocation location){
                using var archive = new ZipArchive(location.ArchiveFileInfo.OpenRead(), ZipArchiveMode.Read);
                var entry = archive.GetEntry(location.EntryName)!;
                using var stream = entry.Open();
                using var hasher = SHA256.Create();
                return hasher.ComputeHash(stream).ToHexString();
            }
        }

        // for json deserialization
        set => hash = value;
    }

    [JsonIgnore]
    public bool HasFileHash => !string.IsNullOrWhiteSpace(hash);

    [JsonIgnore]
    private string hash = string.Empty;

    public SongBuilder(Song song) : this()
    {
        Path = song.Path;
        Index = song.Index;
        Title = song.Title;
        Artist = song.Artist;
        Dance = song.Dance;
        Duration = song.Duration;
    }

    public SongBuilder SetLocation(FileInfo file) => SetLocation(new FileLocation(file));
    public SongBuilder SetLocation(SongLocation path)
    {
        Path = path;
        return this;
    }
    public SongBuilder SetIndex(int index)
    {
        Index = index;
        return this;
    }
    public SongBuilder SetTitle(string title)
    {
        Title = title;
        return this;
    }
    public SongBuilder SetArtist(string artist)
    {
        Artist = artist;
        return this;
    }
    public SongBuilder SetDanceFromKey(string key) => SetDance(Domain.Dance.FromKey(key));
    public SongBuilder SetDance(string dance)
    {
        Dance = dance;
        return this;
    }
    public SongBuilder SetDuration(TimeSpan duration)
    {
        Duration = duration;
        return this;
    }

    public SongBuilder FromFileName(string fileName)
    {
        var split = fileName.Split("_");
        if (split.Length == 3)
        {
            return SetIndex(split[0].TryParse<int>().Or(-1)).SetDanceFromKey(split[1]).SetTitle(split[2]);
        }
        if (split.Length == 1)
        {
            return SetIndex(-1).SetTitle(fileName);
        }
        if (split.Length == 2)
        {
            return SetIndex(-1).SetTitle(split[1]).SetDanceFromKey(split[0]);
        }
        if (split.Length > 3)
        {
            return SetIndex(split[0].TryParse<int>().Or(-1)).SetDanceFromKey(split[1]).SetTitle(split.Skip(2).Dump(' '));
        }
        throw new InvalidDataException($"{fileName} does not match naming conventions");
    }

    public SongBuilder Copy()
    {
        return new()
        {
            Path = Path,
            Index = Index,
            Title = Title,
            Artist = Artist,
            Dance = Dance,
            Duration = Duration,
            hash = hash,
        };
    }

    public Song Build() => new(Path, Index, Title, Artist, Dance, Duration);
}

// this is a type union. The song location can either be a file, an archive entry or undefined. undefined is an error state.
public abstract record SongLocation;
public sealed record FileLocation(FileInfo FileInfo) : SongLocation
{
    public static FileLocation Of(string path) => new(new FileInfo(path));
}
public sealed record ArchiveLocation(string EntryName, FileInfo ArchiveFileInfo) : SongLocation
{
    public FileInfo ArchiveFileInfo { get; init; } = ArchiveFileInfo.Exists ? ArchiveFileInfo : throw new FileNotFoundException(null, ArchiveFileInfo.FullName);
}
public sealed record HashEmbeddedLocation : SongLocation;
public sealed record UndefinedLocation : SongLocation;
// for backcompat reasons when deserialising. gets replaced after loading
public sealed record LegacyLocation(string Path) : SongLocation;

public sealed class SongLocationJsonConverter : JsonConverter<SongLocation>
{
    public override SongLocation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        //for legacy purposes
        if (reader.TokenType == JsonTokenType.String)
        {
            var legacy = reader.GetString();
            if (string.IsNullOrWhiteSpace(legacy))
            {
                return new UndefinedLocation();
            }

            if (Path.IsPathFullyQualified(legacy))
            {
                return new FileLocation(new(legacy));
            }

            return new LegacyLocation(legacy);
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        reader.Read();

        string? type = null;
        string? path = null;
        string? entry = null;

        while (reader.TokenType == JsonTokenType.PropertyName)
        {
            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "type":
                    type = reader.GetString();
                    break;
                case "path":
                    path = reader.GetString();
                    break;
                case "entry":
                    entry = reader.GetString();
                    break;
                default:
                    reader.Skip();
                    break;
            }

            reader.Read();
        }

        if (reader.TokenType != JsonTokenType.EndObject)
            throw new JsonException();

        return type switch
        {
            "file" when path != null => new FileLocation(new FileInfo(path)),
            "archive" when path != null && entry != null => new ArchiveLocation(entry, new FileInfo(path)),
            "hash_embedded" => new HashEmbeddedLocation(),
            "null" => new UndefinedLocation(),
            _ => throw new JsonException()
        };
    }


    public override void Write(Utf8JsonWriter writer, SongLocation value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        switch (value)
        {
            case FileLocation file:
                writer.WriteString("type", "file");
                writer.WriteString("path", file.FileInfo.FullName);
                break;

            case ArchiveLocation archive:
                writer.WriteString("type", "archive");
                writer.WriteString("entry", archive.EntryName);
                writer.WriteString("path", archive.ArchiveFileInfo.FullName);
                break;

            case HashEmbeddedLocation:
                writer.WriteString("type", "hash_embedded");
                break;

            case UndefinedLocation:
                writer.WriteString("type", "null");
                break;

            default:
                throw new UnreachableException();
        }

        writer.WriteEndObject();
    }
}