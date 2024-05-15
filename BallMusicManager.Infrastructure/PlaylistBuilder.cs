using System.Collections.Immutable;
using System.IO.Compression;
using Ametrin.Serialization;
using Ametrin.Utils;
using Ametrin.Utils.Optional;
using BallMusicManager.Domain;

namespace BallMusicManager.Infrastructure;

public static class PlaylistBuilder {
    private static readonly IImmutableSet<string> AllowedFileTypes = [".mp3", ".wav", ".mp4", ".acc", ".m4a"];

    public static PlaylistPlayer FromFolder(DirectoryInfo folder){
        var files = folder.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Where(ValidFile);
        //var files = Directory.GetFiles(path, , ).Where(path => Song.).ToArray();
        return new(folder.FullName, files.Select(SongBuilderExtensions.FromPath).ReduceSome());
    }

    public static PlaylistPlayer FromFile(FileInfo file) {
        return new(file.DirectoryName!, EnumerateFile(file).Select(b=>b.Build()));
    }

    const string SONG_LIST_ENTRY_NAME = "song_list.json";
    public static Result<PlaylistPlayer> FromArchive(FileInfo file) => EnumerateArchive(file).Map(songs => new PlaylistPlayer(file.FullName, songs.Select(s=>s.Build())));
    public static Result<IEnumerable<MutableSong>> EnumerateArchive(FileInfo file) {
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


        return songs;
        
        static IEnumerable<MutableSong> ParseSongList(ZipArchiveEntry entry) {
            using var stream = entry.Open();
            return JsonExtensions.Deserialize<MutableSong[]>(stream).Map<IEnumerable<MutableSong>>(songs => songs.OrderBy(s => s.Index)).Reduce([]);
        }
    }
    
    public static ResultFlag ToArchive(FileInfo file, IEnumerable<ISong> songs) {
        if(!songs.Any()) {
            return ResultFlag.Null;
        }

        using var archive = file.Exists ? new ZipArchive(file.Open(FileMode.Open), ZipArchiveMode.Update) : new ZipArchive(file.Create(), ZipArchiveMode.Update);

        var songsCopy = songs.Select((song, index) => new MutableSong(song, index)).ToArray();

        foreach(var song in songsCopy) {
            var fileInfo = new FileInfo(song.Path);

            // filename becomes it's hash to prevent saving the same file twice...
            var entryName = fileInfo.ComputeMd5Hash();
            if(archive.GetEntry(entryName) is not null) continue;

            var songEntry = archive.CreateEntryFromFile(song.Path, entryName)!;
            song.SetPath(entryName);
        }

        archive.GetEntry(SONG_LIST_ENTRY_NAME)?.Delete();
        var songListEntry = archive.CreateEntry(SONG_LIST_ENTRY_NAME)!;

        using var songListStream = songListEntry.Open();
        WriteSongList(songListStream, songsCopy);

        return ResultFlag.Succeeded;
        
        static void WriteSongList(Stream stream, IEnumerable<MutableSong> data) {
            data.WriteToStream(stream);
        }
    }

    public static IEnumerable<MutableSong> EnumerateFile(FileInfo file) {
        return JsonExtensions.ReadFromJsonFile<List<MutableSong>>(file).Map(MapFrom).Reduce([]);

        IEnumerable<MutableSong> MapFrom(IEnumerable<MutableSong> songs) {
            foreach(var song in songs) {
                yield return Path.IsPathRooted(song.Path) ? song : song with { Path = Path.Combine(file.DirectoryName!, song.Path) };
            }
        }
    }

    public static bool ValidFile(FileInfo path) => AllowedFileTypes.Contains(path.Extension);
}
