namespace BallMusicManager.Infrastructure;

public static class SongBuilderExtensions
{
    public static SongBuilder FromMetaData(this SongBuilder songBuilder)
    {
        using var file = TagLib.File.Create(songBuilder.Path switch
        {
            FileLocation location => location.FileInfo.FullName,
            _ => throw new ArgumentException("Cannot read metadata from a song without a file"),
        });
        return songBuilder.SetDuration(file.Properties.Duration).SetArtist(file.Tag.FirstPerformer);
    }

    public static Option<Song> FromPath(FileInfo fileInfo)
    {
        var fileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);

        try
        {
            return new SongBuilder()
                .SetLocation(fileInfo)
                .FromMetaData()
                .FromFileName(fileName).Build();
        }
        catch
        {
            return default;
        }
    }
}
