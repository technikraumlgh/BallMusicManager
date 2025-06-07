using System.Collections.Immutable;

namespace BallMusic.Tips;

public sealed class DurationBetween(TimeSpan min, TimeSpan max) : IRule
{
    internal readonly TimeSpan min = min;
    internal readonly TimeSpan max = max;

    public IEnumerable<Tip> GetTips(ImmutableArray<Song> songs)
    {
        var duration = TimeSpan.FromTicks(songs.Sum(static s => s.Duration.Ticks));
        if (duration > max)
        {
            yield return new (Severity.Tip, $"Playlist könnte zu lang sein. Empfehlung: {min:hh\\:mm} - {max:hh\\:mm} Stunden für einen normalen Ball");
        }
        else if (duration < min / 2)
        {
            yield return new (Severity.Error, $"Playlist ist zu kurz. Empfehlung: {min:hh\\:mm} - {max:hh\\:mm} Stunden für einen normalen Ball");
        }
        else if (duration < min)
        {
            yield return new (Severity.Warning, $"Playlist könnte zu kurz sein. Empfehlung: {min:hh\\:mm} - {max:hh\\:mm} Stunden für einen normalen Ball");
        }
    }
}
