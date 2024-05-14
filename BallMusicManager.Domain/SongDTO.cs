using System.Text.Json;

namespace BallMusicManager.Domain;

public record SongDTO(string title, string artist, string dance) {
    public static readonly SongDTO None = new("Nothing", "Nobody", "Nothing");

    public string ToJson() => JsonSerializer.Serialize(this);

    public static SongDTO From(ISong? song) {
        if(song is null) return None;
        return new(song.Title, song.Artist, song.Dance);
    }
};
