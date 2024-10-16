using System.IO;

namespace BallMusicManager.Domain;

public sealed record SongBuilder()
{
    public string Path { get; set; } = string.Empty; //Needs to be string because can be file path or archive entry name
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
                var fileInfo = new FileInfo(Path);
                hash = fileInfo.ComputeSha256Hash();
            }
            return hash;
        }
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

    public SongBuilder SetPath(FileInfo file) => SetPath(file.FullName);
    public SongBuilder SetPath(string path)
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
            return SetIndex(split[0].TryParse<int>().Reduce(-1)).SetDanceFromKey(split[1]).SetTitle(split[2]);
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
            return SetIndex(split[0].TryParse<int>().Reduce(-1)).SetDanceFromKey(split[1]).SetTitle(split.Skip(2).Dump(' '));
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
