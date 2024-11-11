using System.Collections.Immutable;

namespace BallMusic.Tips;

public abstract class Rule
{
    public abstract IEnumerable<Tip> GetTips(ImmutableArray<Song> songs);
    public enum Severity { Tip, Warning, Error }
}
