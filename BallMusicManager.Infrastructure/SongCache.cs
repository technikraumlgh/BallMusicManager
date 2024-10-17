namespace BallMusicManager.Infrastructure;

public static class SongCache
{
    public static readonly DirectoryInfo CacheDirectory = new("$cache");
    public static FileInfo Cache(SongBuilder song)
    {
        using var stream = File.OpenRead(song.Path);
        return Cache(stream, Path.GetFileName(song.Path));
    }
    
    public static FileInfo Cache(Stream stream, string name)
    {
        if (!CacheDirectory.Exists)
        {
            CacheDirectory.Create();
            File.SetAttributes(CacheDirectory.FullName, FileAttributes.Hidden);
        }

        var file = CacheDirectory.File(name);
        if (!file.Exists)
        {
            using var fileStream = file.Create();
            stream.CopyTo(fileStream);
        }
        return file;
    }

    public static void Clear()
    {
        if (CacheDirectory.Exists)
        {
            CacheDirectory.Delete(true);
        }
    }
}
