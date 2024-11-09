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
            var min = (int)(songs.Length * minPercentage);
            var max = (int)(songs.Length * maxPercentage);
            if (!dances.TryGetValue(dance, out var count) || count < min)
            {
                yield return new(Severity.Tip, $"Deine Playlist enthält zu wenig {dance}. (Empfehlung: mindestens {min})");
            }
            else if (count > max)
            {
                yield return new(Severity.Tip, $"Deine Playlist enthält zu viel {dance}. (Empfehlung: maximal {max})");
            }
        }
    }
}
