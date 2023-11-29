using System.Collections.Immutable;
using Ametrin.Utils.Optional;

namespace BallMusicManager.Domain;

public sealed class Playlist(string path, IEnumerable<Song> songs){
    public string Path { get; } = path;
    public ImmutableArray<Song> Songs { get; } = songs.ToImmutableArray();

    public Option<Song> Current => Songs[CurrentIndex];
    public Option<Song> Peek => CurrentIndex + 1 < Songs.Length ? Songs[CurrentIndex + 1] : Option<Song>.None();
    public int Length => Songs.Length;
    public bool IsEmpty => Length == 0;

    private int CurrentIndex;

}
