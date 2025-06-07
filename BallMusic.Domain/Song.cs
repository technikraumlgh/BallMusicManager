namespace BallMusic.Domain;

public record Song(SongLocation Path, int Index, string Title, string Artist, string Dance, TimeSpan Duration)
{
    public SongLocation Path { get; init; } = Path is FileLocation or ArchiveLocation ? Path : throw new ArgumentException(nameof(Path), "Cannot create a song without clear song location");
}

/// <summary>
/// Allows to define a song by its properties<br/>
/// This is used to define tips in BallMusic.Tips
/// </summary>
public record FakeSong(string Title, string Artist, string Dance);