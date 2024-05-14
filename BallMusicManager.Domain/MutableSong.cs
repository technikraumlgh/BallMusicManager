using Ametrin.Utils;
using Ametrin.Utils.Optional;

namespace BallMusicManager.Domain;

public sealed record MutableSong() : ISong {
    public string Path { get; set; } = string.Empty;
    public int Index { get; set; } = -1;
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Dance { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;

    public MutableSong(ISong song, int index) : this() {
        Path = song.Path;
        Index = index;
        Title = song.Title;
        Artist = song.Artist;
        Dance = song.Dance;
        Duration = song.Duration;
    }

    public MutableSong SetPath(FileInfo path) => SetPath(path.FullName);
    public MutableSong SetPath(string path) {
        Path = path;
        return this;
    }
    public MutableSong SetIndex(int index) {
        Index = index;
        return this;
    }
    public MutableSong SetTitle(string title) {
        Title = title;
        return this;
    }
    public MutableSong SetArtist(string artist) {
        Artist = artist;
        return this;
    }
    public MutableSong SetDanceFromKey(string key) => SetDance(Domain.Dance.FromKey(key));
    public MutableSong SetDance(string dance) {
        Dance = dance;
        return this;
    }
    public MutableSong SetDuration(TimeSpan duration) {
        Duration = duration;
        return this;
    }

    public MutableSong FromFileName(string fileName) {
        var split = fileName.Split("_");
        if(split.Length == 3) {
            return SetIndex(split[0].TryParse<int>().Reduce(-1)).SetDanceFromKey(split[1]).SetTitle(split[2]);
        }
        if(split.Length == 1) {
            return SetIndex(-1).SetTitle(fileName);
        }
        if(split.Length == 2) {
            return SetIndex(-1).SetTitle(split[1]).SetDanceFromKey(split[0]);
        }
        if(split.Length > 3) {
            return SetIndex(split[0].TryParse<int>().Reduce(-1)).SetDanceFromKey(split[1]).SetTitle(split.Skip(2).Dump(' '));
        }
        throw new InvalidDataException($"{fileName} does not match naming conventions");
    }

    public Song Build() => new(Path, Index, Title, Artist, Dance, Duration);
}
