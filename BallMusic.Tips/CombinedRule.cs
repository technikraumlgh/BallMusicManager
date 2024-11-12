using System.Collections.Immutable;

namespace BallMusic.Tips;

public sealed class CombinedRule(ImmutableArray<IRule> rules) : IRule
{
    private readonly ImmutableArray<IRule> rules = rules;

    public IEnumerable<Tip> GetTips(ImmutableArray<Song> songs) 
        => rules.SelectMany(rule => rule.GetTips(songs));
}
