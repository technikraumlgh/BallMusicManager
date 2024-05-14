namespace BallMusicManager.Domain;

public record Song(string Path, int Index, string Title, string Artist, string Dance, TimeSpan Duration) : ISong;
