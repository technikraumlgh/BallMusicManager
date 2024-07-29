using System.Diagnostics.CodeAnalysis;

namespace BallMusicManager.Domain;

public sealed class SongEqualityComparer : IEqualityComparer<Song>, IEqualityComparer<SongBuilder>{
    public static readonly SongEqualityComparer Instance = new ();

    public bool Equals(Song? x, Song? y) {
        if(x is null) {
            return y is null;
        }
        if(y is null) return false;

        return x.Title == y.Title 
            && x.Artist == y.Artist 
            && x.Dance == y.Dance;
    }

    public bool Equals(SongBuilder? x, SongBuilder? y) {
        if(x is null) {
            return y is null;
        }
        if(y is null) return false;

        return x.Title == y.Title 
            && x.Artist == y.Artist 
            && x.Dance == y.Dance;
    }

    public int GetHashCode([DisallowNull] Song obj) => HashCode.Combine(obj.Title.GetHashCode(), obj.Artist.GetHashCode(), obj.Dance.GetHashCode());
    public int GetHashCode([DisallowNull] SongBuilder obj) => HashCode.Combine(obj.Title.GetHashCode(), obj.Artist.GetHashCode(), obj.Dance.GetHashCode());
}
