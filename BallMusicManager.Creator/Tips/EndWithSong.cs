using System.Collections.Immutable;
using BallMusicManager.Domain;

namespace BallMusicManager.Creator.Tips;

public sealed class EndWithSong(FakeSong song, Rule.Severity severity) : Rule
{
    private readonly FakeSong song = song;
    private readonly Severity severity = severity;

    public override IEnumerable<Tip> GetTips(ImmutableArray<Song> songs)
    {
        if (!SongEqualityComparer.ByLooseProperties.Equals(song, songs[^1]))
        {
            yield return new (severity, $"Die Playlist endet nicht mit '{song.Title}' ({song.Dance}) von {song.Artist}");
        }
    }
}
