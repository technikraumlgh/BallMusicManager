namespace BallMusicManager.Domain;

public record Song(SongLocation Path, int Index, string Title, string Artist, string Dance, TimeSpan Duration)
{
    public SongLocation Path { get; init; } = Path is not UndefinedLocation ? Path : throw new ArgumentNullException(nameof(Path));
}
