using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BallMusicManager.Domain;

/// <summary>
/// this is a type union.<br/>
/// The song file location can either be a file, an archive entry or undefined.<br/>
/// undefined is an error state.
/// </summary>
public abstract record SongLocation;

/// <summary>
/// The song file is at a specific location.<br/>
/// This means the song has been cached or was newly added.
/// </summary>
public sealed record FileLocation(FileInfo FileInfo) : SongLocation
{
    public FileInfo FileInfo { get; init; } = FileInfo.Exists ? FileInfo : throw new FileNotFoundException(null, FileInfo.FullName);
    public static FileLocation Of(string path) => new(new FileInfo(path));
}

/// <summary>
/// The song file is in an archive with the given entry name.<br/>
/// once the song gets cached this gets replaced by <see cref="FileLocation"/>.
/// </summary>
public sealed record ArchiveLocation(string EntryName, FileInfo ArchiveFileInfo) : SongLocation
{
    public FileInfo ArchiveFileInfo { get; init; } = ArchiveFileInfo.Exists ? ArchiveFileInfo : throw new FileNotFoundException(null, ArchiveFileInfo.FullName);
}

/// <summary>
/// The song file is in the current archive.<br/>
/// The entry name is the FileHash.<br/>
/// this is only valid while parsing a plz file
/// </summary>
public sealed record HashEmbeddedLocation : SongLocation;

/// <summary>
/// the song has not file assigned.<br/>
/// this is an error state
/// </summary>
public sealed record UndefinedLocation : SongLocation;

/// <summary>
/// The song file is at a relative path. (No longer used)
/// for backwards compatibility reasons only. gets replaced on load
/// </summary>
public sealed record LegacyLocation(string Path) : SongLocation;

public sealed class SongLocationJsonConverter : JsonConverter<SongLocation>
{
    public override SongLocation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // for legacy purposes
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
            _ => throw new JsonException("Invalid file location")
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
                throw new UnreachableException("The given SongLocation cannot be serialized");
        }

        writer.WriteEndObject();
    }
}