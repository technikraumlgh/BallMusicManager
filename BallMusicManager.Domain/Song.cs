namespace BallMusicManager.Domain;

public record Song(SongLocation Path, int Index, string Title, string Artist, string Dance, TimeSpan Duration)
{
    public SongLocation Path { get; init; } = Path is FileLocation or ArchiveLocation ? Path : throw new ArgumentNullException(nameof(Path), "Cannot create a song without clear song location");
}
