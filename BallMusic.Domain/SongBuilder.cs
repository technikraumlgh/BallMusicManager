﻿using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace BallMusic.Domain;

/// <summary>
/// This is a mutable version of <see cref="Song"/>.<br/>
/// additionally it contains information required by the Creator to manage the songs (e.g. the file hash)  
/// </summary>
public sealed class SongBuilder
{
    // do not rename these properties (json serialization)
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
            if (!HasFileHash)
            {
                SetHash(ComputeHash());
            }
            return hash;
        }

        // for json deserialization
        set => hash = value;
    }

    [JsonIgnore]
    public bool HasFileHash => !string.IsNullOrWhiteSpace(hash);

    [JsonIgnore]
    private string hash = string.Empty;

    public SongBuilder() { }
    public SongBuilder(SongBuilder song)
    {
        Path = song.Path;
        Index = song.Index;
        Title = song.Title;
        Artist = song.Artist;
        Dance = song.Dance;
        Duration = song.Duration;
        hash = song.hash;
    }

    public SongBuilder(Song song)
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
    public SongBuilder SetDanceFromSlug(string key) => SetDance(Domain.Dance.FromSlug(key));
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

    public SongBuilder SetHash(byte[] bytes)
    {
        hash = Convert.ToHexString(bytes);
        return this;
    }

    public byte[] ComputeHash()
    {
        return Path switch
        {
            FileLocation file => ComputeSHA256Hash(file.FileInfo),
            ArchiveLocation file => GetHash(file),
            _ => throw new InvalidOperationException("Cannot compute hash for a song without proper location"),
        };

        static byte[] GetHash(ArchiveLocation location)
        {
            using var archive = new ZipArchive(location.ArchiveFileInfo.OpenRead(), ZipArchiveMode.Read);
            var entry = archive.GetEntry(location.EntryName)!;
            using var stream = entry.Open();
            return ComputeSHA256Hash(stream);
        }
    }

    public static byte[] ComputeSHA256Hash(FileInfo fileInfo)
    {
        using var stream = fileInfo.OpenRead();
        return ComputeSHA256Hash(stream);
    }

    public static byte[] ComputeSHA256Hash(Stream stream)
    {
        using var hasher = SHA256.Create();
        return hasher.ComputeHash(stream);
    }

    public SongBuilder FromFileName(string fileName)
    {
        // this complicated logic exists to support the old naming convention Index_Dance_Title
        // you may want to change parts of this in the future to prevent wierd interpretations
        var split = fileName.Split("_");
        return split switch
        {
            [var title] => SetIndex(-1).SetTitle(title),
            [var title, var dance] => SetIndex(-1).SetTitle(title).SetDanceFromSlug(dance), // the export file function follows this pattern to allow for quick reimports
            [var indexRaw, var dance, var title] => SetIndex(int.TryParse(indexRaw, out var index) ? index : -1).SetDanceFromSlug(dance).SetTitle(title),
            [var indexRaw, var dance, ..] => SetIndex(int.TryParse(indexRaw, out var index) ? index : -1).SetDanceFromSlug(dance).SetTitle(string.Join(' ', split.AsSpan(2)!)),
            _ => this,
        };
    }

    public SongBuilder Copy() => new(this);

    public Song Build() => new(Path, Index, Title, Artist, Dance, Duration);
}
