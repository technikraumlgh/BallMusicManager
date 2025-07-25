﻿namespace BallMusic.Tips;

public sealed class PartyDurationBetween(TimeSpan min, TimeSpan max) : IRule
{
    private readonly TimeSpan min = min;
    private readonly TimeSpan max = max;

    public IEnumerable<Tip> GetTips(ImmutableArray<Song> songs)
    {
        var duration = TimeSpan.FromTicks(songs.Where(static s => s.Dance is Dance.Party).Sum(static s => s.Duration.Ticks));

        if (duration > max)
        {
            yield return new(Severity.Warning, $"Party Teil könnte zu lang sein. Empfehlung: {min:mm} - {max:mm} Minuten für einen normalen Ball");
        }
        else if (duration < min)
        {
            yield return new(Severity.Tip, $"Party Teil könnte zu kurz sein. Empfehlung: {min:mm} - {max:mm} Minuten für einen normalen Ball");
        }
    }
}