using System;
using System.Runtime.Serialization;
using BallMusicManager.Domain;

namespace BallMusicManager.Creator.Recommendations;

public abstract class Rule
{
    public abstract IEnumerable<(Severity severity, string tip)> GetTips(IEnumerable<Song> songs);
    public enum Severity { Recommendation, Warning, Error }
}

public sealed class EndWithSong(Song song, Rule.Severity severity) : Rule
{
    private readonly Song song = song;
    private readonly Severity severity = severity;

    public override IEnumerable<(Severity severity, string tip)> GetTips(IEnumerable<Song> songs)
    {
        if (SongEqualityComparer.ByLooseProperties.Equals(song, songs.Last()))
        {
            yield return (severity, $"Die Playlist endet nicht mit '{song.Title}'");
        }
    }
}
