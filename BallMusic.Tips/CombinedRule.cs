using System.Collections.Immutable;

namespace BallMusic.Tips;

public sealed class CombinedRule(ImmutableArray<Rule> rules) : Rule
{
    private readonly ImmutableArray<Rule> rules = rules;

    public override IEnumerable<Tip> GetTips(ImmutableArray<Song> songs) 
        => rules.SelectMany(rule => rule.GetTips(songs));
}
