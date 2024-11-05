namespace BallMusicManager.Infrastructure;

public static class SongCache
{
    public static readonly DirectoryInfo CacheDirectory = new("$cache");

    public static FileInfo CacheFromArchive(SongBuilder song, FileInfo Archive)
    {
        using var archive = PlaylistBuilder.OpenArchive(Archive).OrThrow();

        var entry = archive.GetEntry(song.FileHash) ?? throw new ArgumentException("");

        using var stream = entry.Open();
        var result = Cache(stream, song.FileHash);
        song.SetPath(result);
        return result;
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
