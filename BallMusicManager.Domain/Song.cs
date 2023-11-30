namespace BallMusicManager.Domain;

public record Song(string Path, int Index, string Title, string Artist, string Dance) {

    public static implicit operator SongDTO(Song song){
        return new(song.Title, song.Artist, song.Dance);
    }
}

public record SongDTO(string title, string artist, string dance);
