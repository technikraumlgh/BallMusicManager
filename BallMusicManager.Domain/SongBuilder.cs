using System.IO;

namespace BallMusicManager.Domain;

// HashCode has to be based on constant values otherwise the DataGrid will crash
public sealed class SongBuilder
{
    public string Path { get; set; } = string.Empty; //string because it can be a path or an archive entry name //TODO: fix this!!!
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

    public SongBuilder() { }
    public SongBuilder(SongBuilder song)
    {
        Path = song.Path;
        Index = song.Index;
        Title = song.Title;
        Artist = song.Artist;
        Dance = song.Dance;
        Duration = song.Duration;
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

    public SongBuilder FromFileName(string fileName)
    {
        // this complicated logic exists to support the old naming convention Index_Dance_SongName
        var split = fileName.Split("_");
        return split.Length switch
        {
            1 => SetIndex(-1).SetTitle(fileName),
            2 => SetIndex(-1).SetTitle(split[1]).SetDanceFromSlug(split[0]), // is this the best?
            3 => SetIndex(split[0].TryParse<int>().Or(-1)).SetDanceFromSlug(split[1]).SetTitle(split[2]),
            > 3 => SetIndex(split[0].TryParse<int>().Or(-1)).SetDanceFromSlug(split[1]).SetTitle(split.Skip(2).Dump(' ')),
            _ => throw new ArgumentException($"{fileName} does not match naming conventions")
        };
    }

    public SongBuilder Copy() => new()
    {
        Path = Path,
        Index = Index,
        Title = Title,
        Artist = Artist,
        Dance = Dance,
        Duration = Duration,
    };

    public Song Build() => new(Path, Index, Title, Artist, Dance, Duration);
}
