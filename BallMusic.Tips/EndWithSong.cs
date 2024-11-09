using System.Collections.Immutable;

namespace BallMusic.Tips;

public sealed class EndWithSong(FakeSong song, Rule.Severity severity) : Rule
{
    internal readonly FakeSong song = song;
    internal readonly Severity severity = severity;

    public override IEnumerable<Tip> GetTips(ImmutableArray<Song> songs)
    {
        if (!SongEqualityComparer.ByLooseProperties.Equals(song, songs[^1]))
        {
            yield return new (severity, $"Die Playlist endet nicht mit '{song.Title}' ({song.Dance}) von {song.Artist}");
        }
    }
}
