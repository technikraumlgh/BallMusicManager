using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BallMusicManager.Domain;

public sealed record SongBuilder()
{
    [JsonConverter(typeof(SongLocationJsonConverter))]
    public SongLocation Path { get; set; } = new UndefinedLocation();
    public int Index { get; set; } = -1;
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Dance { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
    public string FileHash
    {
        get
        {
            if (string.IsNullOrEmpty(hash))
            {
                if (Path is FileLocation file)
                {
                    hash = file.FileInfo.ComputeSha256Hash();
                }
                else
                {
                    throw new Exception("Cannot compute hash for an song without a FileLocation");
                }

            }
            return hash;
        }

        // for json deserialization
        set => hash = value;
    }

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

    public SongBuilder SetPath(FileInfo file) => SetPath(new FileLocation(file));
    public SongBuilder SetPath(SongLocation path)
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
        };
    }

    public Song Build() => new(Path, Index, Title, Artist, Dance, Duration);
}

public abstract record SongLocation;
public sealed record FileLocation(FileInfo FileInfo) : SongLocation
{
    public static FileLocation Of(string path) => new(new FileInfo(path));
}
public sealed record ArchiveLocation(string EntryName) : SongLocation;
public sealed record UndefinedLocation : SongLocation;

public sealed class SongLocationJsonConverter : JsonConverter<SongLocation>
{
    public override SongLocation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return new UndefinedLocation();
        }
        if (Path.IsPathFullyQualified(value))
        {
            return new FileLocation(new(value));
        }
        return new ArchiveLocation(value);
    }

    public override void Write(Utf8JsonWriter writer, SongLocation value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            FileLocation file => file.FileInfo.FullName,
            ArchiveLocation archive => archive.EntryName,
            UndefinedLocation => string.Empty,
            _ => throw new UnreachableException(),
        });
    }
}