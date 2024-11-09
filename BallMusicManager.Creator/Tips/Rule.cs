using System.Collections.Immutable;

namespace BallMusicManager.Creator.Tips;

public abstract class Rule
{
    public abstract IEnumerable<Tip> GetTips(ImmutableArray<Song> songs);
    public enum Severity { Recommendation, Warning, Error }
}

public sealed record Tip(Rule.Severity Severity, string Description);