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
        EnsureCacheExists();

        var file = CacheDirectory.File(name);
        if (!file.Exists)
        {
            using var fileStream = file.Create();
            stream.CopyTo(fileStream);
        }
        return file;
    }

    public static Option<FileInfo> GetFile(string name)
    {
        EnsureCacheExists();
        return CacheDirectory.File(name).WhereExists();
    }

    public static void Clear()
    {
        if (CacheDirectory.Exists)
        {
            CacheDirectory.Delete(true);
        }
    }

    private static void EnsureCacheExists()
    {
        if (!CacheDirectory.Exists)
        {
            CacheDirectory.Create();
            FileSystemInfoExtensions.GenerateCacheTag(CacheDirectory, "BallMusicManger"); //just a marker file, does not have to exists
            File.SetAttributes(CacheDirectory.FullName, FileAttributes.Hidden);
        }
    }
}
