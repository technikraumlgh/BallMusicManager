using System.Text.Json;

namespace BallMusicManager.Domain;

public record Song(string Path, int Index, string Title, string Artist, string Dance, TimeSpan Duration) {

    public static implicit operator SongDTO(Song song){
        return new(song.Title, song.Artist, song.Dance);
    }
}

public record SongDTO(string title, string artist, string dance){
    public static readonly SongDTO None = new("Nothing", "Nobody", "Nothing");

    public string ToJson() => JsonSerializer.Serialize(this);
};
