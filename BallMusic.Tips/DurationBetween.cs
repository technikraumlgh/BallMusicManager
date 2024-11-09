using System.Collections.Immutable;

namespace BallMusic.Tips;

public sealed class DurationBetween(TimeSpan min, TimeSpan max) : Rule
{
    internal readonly TimeSpan min = min;
    internal readonly TimeSpan max = max;

    public override IEnumerable<Tip> GetTips(ImmutableArray<Song> songs)
    {
        var duration = songs.Sum(s => s.Duration);
        if (duration > max)
        {
            yield return new (Severity.Tip, $"Deine Playlist könnte zu lang sein. Empfehlung: {min.Hours} - {max.Hours} Stunden für einen normalen Ball");
        }
        else if (duration < min / 2)
        {
            yield return new (Severity.Error, $"Deine Playlist ist zu kurz. Empfehlung: {min.Hours} - {max.Hours} Stunden für einen normalen Ball");
        }
        else if (duration < min)
        {
            yield return new (Severity.Warning, $"Deine Playlist könnte zu kurz sein. Empfehlung: {min.Hours} - {max.Hours} Stunden für einen normalen Ball");
        }
    }
}
