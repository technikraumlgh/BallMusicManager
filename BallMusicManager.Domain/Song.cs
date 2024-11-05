namespace BallMusicManager.Domain;

public record Song(SongLocation Path, int Index, string Title, string Artist, string Dance, TimeSpan Duration);
