using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Compression;
using Ametrin.Serialization;
using Ametrin.Utils;
using Ametrin.Utils.Optional;
using BallMusicManager.Domain;

namespace BallMusicManager.Infrastructure;

public static class PlaylistBuilder {
    private static readonly ImmutableHashSet<string> AllowedFileTypes = [".mp3", ".wav", ".mp4", ".acc", ".m4a"];

    public static PlaylistPlayer FromFolder(DirectoryInfo folder) {
        var files = folder.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Where(ValidFile);
        //var files = Directory.GetFiles(path, , ).Where(path => Song.).ToArray();
        return new(folder.FullName, files.Select(SongBuilderExtensions.FromPath).ReduceSome());
    }

    public static PlaylistPlayer FromFile(FileInfo file) {
        return new(file.DirectoryName!, EnumerateFile(file).Select(b => b.Build()));
    }

    const string SONG_LIST_ENTRY_NAME = "song_list.json";
    const string MANIFEST_ENTRY_NAME = "manifest.json";
    public static Result<PlaylistPlayer> FromArchive(FileInfo file) => EnumerateArchive(file).Map(songs => new PlaylistPlayer(file.FullName, songs.Select(s => s.Build())));
    public static Result<IEnumerable<SongBuilder>> EnumerateArchive(FileInfo file) {
        var archive = file.ToResultWhereExists()
            .Map(file => new ZipArchive(file.OpenRead()));

        var songs = archive.Map(archive => archive.GetEntry(SONG_LIST_ENTRY_NAME), ResultFlag.InvalidFile)
            .Map(ParseSongList).WhereNotEmpty();

        (archive, songs).Resolve((archive, songs) => {
            foreach(var song in songs) {
                var songEntry = archive.GetEntry(song.Path)!;
                using var songStream = songEntry.Open();
                song.Path = SongCache.Cache(songStream, song.Path).FullName;
            }
        });

        // don't yield return to allow multi-threading
        return songs;

        static IEnumerable<SongBuilder> ParseSongList(ZipArchiveEntry entry) {
            using var stream = entry.Open();
            return JsonExtensions.Deserialize<SongBuilder[]>(stream).Map<IEnumerable<SongBuilder>>(songs => songs.OrderBy(s => s.Index)).Reduce([]);
        }
    }

    public static ResultFlag ToArchive(FileInfo file, IEnumerable<Song> songs) {
        if(!songs.Any()) {
            return ResultFlag.NullOrEmpty;
        }

        return ToArchiveImpl(file, songs.Select((song, index) => new SongBuilder(song).SetIndex(index)).ToArray());
    }
    
    public static ResultFlag ToArchive(FileInfo file, IEnumerable<SongBuilder> songs) {
        if(!songs.Any()) {
            return ResultFlag.NullOrEmpty;
        }

        return ToArchiveImpl(file, songs.Select((song, index) => song.Copy().SetIndex(index)).ToArray());
    }

    private const int ARCHIVE_VERSION = 1;
    private static ResultFlag ToArchiveImpl(FileInfo file, SongBuilder[] songs)
    {
        var hashes = new HashSet<string>();
        using var archive = file.Exists 
            ? new ZipArchive(file.Open(FileMode.Open), ZipArchiveMode.Update) 
            : new ZipArchive(file.Create(), ZipArchiveMode.Update);


        foreach(var song in songs)
        {
            if(hashes.Contains(song.FileHash)) //TODO: properly handle this case
            {
                var other = songs.Where(other => song.FileHash == other.FileHash).FirstOrDefault();
                //throw new UnreachableException($"{song.Path} produced an already existing hash!");
            }

            if(archive.GetEntry(song.FileHash) is null)
            {
                archive.CreateEntryFromFile(song.Path, song.FileHash);
                hashes.Add(song.FileHash);
            }

            // filename becomes it's hash to prevent saving the same file twice...
            song.SetPath(song.FileHash);
        }

        WriteSongList();

        WriteManifest();

        return ResultFlag.Succeeded;

        void WriteSongList()
        {
            var entry = archive.OverwriteEntry(SONG_LIST_ENTRY_NAME);
            using var stream = entry.Open();
            songs.WriteToStream(stream);
        }

        void WriteManifest()
        {
            var manifest = new ArchiveManifest(ARCHIVE_VERSION, songs.Length, DateTime.Now);
            var entry = archive.OverwriteEntry(MANIFEST_ENTRY_NAME);
            using var stream = entry.Open();
            manifest.WriteToStream(stream);
        }
    }

    public static IEnumerable<SongBuilder> EnumerateFile(FileInfo file) {
        return JsonExtensions.ReadFromJsonFile<List<SongBuilder>>(file).Map(MapFrom).Reduce([]);

        IEnumerable<SongBuilder> MapFrom(IEnumerable<SongBuilder> songs) {
            foreach(var song in songs) {
                yield return Path.IsPathRooted(song.Path) ? song : song with { Path = Path.Combine(file.DirectoryName!, song.Path) };
            }
        }
    }

    public static bool ValidFile(FileInfo path) => AllowedFileTypes.Contains(path.Extension);
    
    private record ArchiveManifest(int Version, int SongCount, DateTime SavedAt);
}
