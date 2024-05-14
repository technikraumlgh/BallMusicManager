namespace BallMusicManager.Domain;

public interface ISong {
    public string Path { get; }
    public string Title { get; }
    public string Artist { get; }
    public string Dance { get; }
    public TimeSpan Duration { get; }
}
