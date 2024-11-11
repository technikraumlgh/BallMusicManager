using System.Collections.Immutable;

namespace BallMusic.Tips;

public sealed class CountOfDance(ImmutableArray<(string dance, float minPercentage, float maxPercentage)> requiredDances) : Rule
{
    internal readonly ImmutableArray<(string dance, float minPercentage, float maxPercentage)> requiredDances = requiredDances;

    public override IEnumerable<Tip> GetTips(ImmutableArray<Song> songs)
    {
        var dances = songs.CountBy(song => song.Dance).ToDictionary();

        foreach (var (dance, minPercentage, maxPercentage) in requiredDances)
        {
            if(!dances.TryGetValue(dance, out var count))
            {
                continue;
            }
            
            if(minPercentage > 0)
            {
                var min = (int)(songs.Length * minPercentage);
                if (min > 0 && count < min)
                {
                    yield return new(Severity.Tip, $"Playlist enthält zu wenig {dance}. (Empfehlung: mindestens {min})");
                }
            }
            if(maxPercentage < 1)
            {
                var max = (int) (songs.Length * maxPercentage);
                if (max > 0 && count > max)
                {
                    yield return new(Severity.Tip, $"Playlist enthält zu viel {dance}. (Empfehlung: maximal {max})");
                }
            }
        }
    }
}
