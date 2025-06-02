namespace BallMusic.Infrastructure;

public static class SongCache
{
    public static readonly DirectoryInfo CacheDirectory = new("$cache");

    /// <summary>
    /// Returns a fixed file location to  the audio file of the given song. If the song already has a <see cref="FileLocation"/> this location is returned.
    /// If it is stored in an archive it gets exported into cache (if not already present) and the cached file gets returned. Other SongLocations result in an exception.
    /// </summary>
    /// <param name="song"></param>
    /// <returns>Fixed file location to the audio file of the song</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="NullReferenceException"></exception>
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

    /// <summary>
    /// Caches the given <paramref name="sourceStream"/> into a file in cache called <paramref name="identifier"/> and returns its location. 
    /// If a file with the same identifier is already present nothing get cached and this file is returned.
    /// </summary>
    /// <param name="sourceStream">the stream to cache</param>
    /// <param name="identifier">identifier used in the cache</param>
    /// <returns></returns>
    public static FileInfo Cache(Stream sourceStream, string identifier)
    {
        EnsureCacheExists();

        var file = CacheDirectory.File(identifier);
        if (!file.Exists)
        {
            using var targetStream = file.Create();
            sourceStream.CopyTo(targetStream);
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
            CacheDirectory.Refresh();
        }
    }

    public static void EnsureCacheExists()
    {
        if (!CacheDirectory.Exists)
        {
            CacheDirectory.Create();
            File.SetAttributes(CacheDirectory.FullName, FileAttributes.Hidden);
            CacheDirectory.Refresh();
        }
    }
}
