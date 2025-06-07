using System.Diagnostics.CodeAnalysis;

namespace BallMusic.Domain;

public sealed class SongEqualityComparer : IEqualityComparer<Song>, IEqualityComparer<SongBuilder>
{
    public static readonly SongEqualityComparer ByProperties = new();
    public static readonly FileHashComparer ByFileHash = new();
    public static readonly LoosePropertiesComparer ByLooseProperties = new(); // this ignores capitalization
    public static readonly LoosePropertiesOrFileHashComparer ByLoosePropertiesOrFileHash = new();

    public bool Equals(Song? x, Song? y)
        => x is null
            ? y is null
            : y is not null
            && x.Title == y.Title
            && x.Artist == y.Artist
            && x.Dance == y.Dance;

    public bool Equals(SongBuilder? x, SongBuilder? y)
        => x is null
            ? y is null
            : y is not null
            && x.Title == y.Title
            && x.Artist == y.Artist
            && x.Dance == y.Dance;

    public int GetHashCode([DisallowNull] Song obj) => HashCode.Combine(obj.Title.GetHashCode(), obj.Artist.GetHashCode(), obj.Dance.GetHashCode());
    public int GetHashCode([DisallowNull] SongBuilder obj) => HashCode.Combine(obj.Title.GetHashCode(), obj.Artist.GetHashCode(), obj.Dance.GetHashCode());

    public sealed class FileHashComparer : IEqualityComparer<SongBuilder>
    {
        public bool Equals(SongBuilder? x, SongBuilder? y)
            => x is null ? y is null : y is not null && x.FileHash == y.FileHash;

        public int GetHashCode([DisallowNull] SongBuilder obj) => obj.FileHash.GetHashCode();
    }

    public sealed class LoosePropertiesComparer : IEqualityComparer<SongBuilder>, IEqualityComparer<Song>
    {
        public bool Equals(SongBuilder? x, SongBuilder? y)
            => x is null
                ? y is null
                : y is not null
                && x.Title.Equals(y.Title, StringComparison.OrdinalIgnoreCase)
                && x.Artist.Equals(y.Artist, StringComparison.OrdinalIgnoreCase)
                && x.Dance.Equals(y.Dance, StringComparison.OrdinalIgnoreCase);

        public bool Equals(Song? x, Song? y)
            => x is null
                ? y is null
                : y is not null
                && x.Title.Equals(y.Title, StringComparison.OrdinalIgnoreCase)
                && x.Artist.Equals(y.Artist, StringComparison.OrdinalIgnoreCase)
                && x.Dance.Equals(y.Dance, StringComparison.OrdinalIgnoreCase);
        
        public bool Equals(FakeSong? x, Song? y)
            => x is null
                ? y is null
                : y is not null
                && x.Title.Equals(y.Title, StringComparison.OrdinalIgnoreCase)
                && x.Artist.Equals(y.Artist, StringComparison.OrdinalIgnoreCase)
                && x.Dance.Equals(y.Dance, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode([DisallowNull] SongBuilder obj)
            => HashCode.Combine(obj.Title.ToLower().GetHashCode(), obj.Artist.ToLower().GetHashCode(), obj.Dance.ToLower().GetHashCode());

        public int GetHashCode([DisallowNull] Song obj)
            => HashCode.Combine(obj.Title.ToLower().GetHashCode(), obj.Artist.ToLower().GetHashCode(), obj.Dance.ToLower().GetHashCode());
    }

    public sealed class LoosePropertiesOrFileHashComparer : IEqualityComparer<SongBuilder>
    {
        public bool Equals(SongBuilder? x, SongBuilder? y) => ByFileHash.Equals(x, y) || ByLooseProperties.Equals(x, y);
        public int GetHashCode([DisallowNull] SongBuilder obj) => HashCode.Combine(ByFileHash.GetHashCode(obj), ByLooseProperties.GetHashCode(obj));
    }
}
