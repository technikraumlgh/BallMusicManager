namespace BallMusic.Infrastructure;

public static class SongBuilderExtensions
{
    public static SongBuilder FromMetaData(this SongBuilder songBuilder)
    {
        using var file = TagLib.File.Create(songBuilder.Path switch
        {
            FileLocation location => location.FileInfo.FullName,
            _ => throw new ArgumentException("Cannot read metadata from a song without a file"),
        });

        if (file.Tag.Performers.Length > 0)
        {
            songBuilder.SetArtist(string.Join(", ", file.Tag.Performers));
        }

        return songBuilder.SetDuration(file.Properties.Duration);
    }
}
