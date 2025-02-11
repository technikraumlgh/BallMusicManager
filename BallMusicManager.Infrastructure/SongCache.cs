namespace BallMusicManager.Infrastructure;

public static class SongCache
{
    public static readonly DirectoryInfo CacheDirectory = new("$cache");

    public static FileInfo CacheFromArchive(SongBuilder song)
    {
        return song.Path switch
        {
            FileLocation file => file.FileInfo,
            ArchiveLocation archive => Impl(song, archive),
            _ => throw new InvalidOperationException($"Cannot cache Song at {song.Path}")
        };

        static FileInfo Impl(SongBuilder song, ArchiveLocation location)
        {
            using var archive = PlaylistBuilder.OpenArchive(location.ArchiveFileInfo).OrThrow();

            var entry = archive.GetEntry(location.EntryName) ?? throw new NullReferenceException("The referenced file did not exists in the archive");

            using var stream = entry.Open();
            var result = Cache(stream, song.FileHash);
            song.SetLocation(result);
            return result;
        }
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
        return CacheDirectory.File(name).RequireExists();
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
            File.SetAttributes(CacheDirectory.FullName, FileAttributes.Hidden);
        }
    }
}
