using Ametrin.Utils;
using BallMusicManager.Domain;

namespace BallMusicManager.Infrastructure; 

public static class SongCache {
    public static readonly DirectoryInfo CacheDirectory = new("$cache");
    public static FileInfo Cache(MutableSong song) {
        using var stream = File.OpenRead(song.Path);
        return Cache(stream, Path.GetFileName(song.Path));
    }
    public static FileInfo Cache(Stream stream, string name) {
        if(!CacheDirectory.Exists) CacheDirectory.Create();
        var file = CacheDirectory.File(name);
        if(!file.Exists) {
            using var fileStream = file.Create();
            stream.CopyTo(fileStream);
        }
        return file;
    }

    public static void Clear() {
        if(CacheDirectory.Exists) {
            CacheDirectory.Delete(true);
        }
    }
}
