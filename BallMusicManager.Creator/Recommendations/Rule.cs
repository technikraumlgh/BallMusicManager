using System.Collections.Immutable;
using BallMusicManager.Domain;
using Ametrin.Utils;

namespace BallMusicManager.Creator.Recommendations;

public abstract class Rule
{
    public abstract IEnumerable<(Severity severity, string tip)> GetTips(ImmutableArray<Song> songs);
    public enum Severity { Recommendation, Warning, Error }
}

public sealed class DurationBetween(TimeSpan min, TimeSpan max) : Rule
{
    private readonly TimeSpan min = min;
    private readonly TimeSpan max = max;

    public override IEnumerable<(Severity severity, string tip)> GetTips(ImmutableArray<Song> songs)
    {
        var duration = songs.Sum(s => s.Duration);
        if (duration > max)
        {
            yield return (Severity.Recommendation, $"Deine Playlist könnte zu lang sein. Empfehlung: {min.Hours} - {max.Hours} Stunden für einen normalen Ball");
        }
        if (duration < min / 2)
        {
            yield return (Severity.Error, $"Deine Playlist ist zu kurz. Empfehlung: {min.Hours} - {max.Hours} Stunden für einen normalen Ball");
        }
        else if (duration < min)
        {
            yield return (Severity.Warning, $"Deine Playlist könnte zu kurz sein. Empfehlung: {min.Hours} - {max.Hours} Stunden für einen normalen Ball");
        }
    }
}

public sealed class EndWithSong(Song song, Rule.Severity severity) : Rule
{
    private readonly Song song = song;
    private readonly Severity severity = severity;

    public override IEnumerable<(Severity severity, string tip)> GetTips(ImmutableArray<Song> songs)
    {
        if (SongEqualityComparer.ByLooseProperties.Equals(song, songs[^1]))
        {
            yield return (severity, $"Die Playlist endet nicht mit '{song.Title}' ({song.Dance}) von {song.Artist}");
        }
    }
}
