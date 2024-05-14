using System.Collections.Immutable;
using System.IO.Compression;
using Ametrin.Serialization;
using Ametrin.Utils.Optional;
using BallMusicManager.Domain;

namespace BallMusicManager.Infrastructure;

public static class PlaylistBuilder{
    private static readonly IImmutableSet<string> AllowedFileTypes = [".mp3", ".wav", ".mp4", ".acc", ".m4a"];

    public static PlaylistPlayer FromFolder(DirectoryInfo folder){
        var files = folder.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Where(ValidFile);
        //var files = Directory.GetFiles(path, , ).Where(path => Song.).ToArray();
        return new(folder.FullName, files.Select(SongBuilderExtensions.FromPath).ReduceSome());
    }

    public static PlaylistPlayer FromFile(FileInfo file) {
        return new(file.DirectoryName!, EnumerateFile(file).Select(b=>b.Build()));
    }

    public static Result<PlaylistPlayer> FromArchive(FileInfo file) => EnumerateArchive(file).Map(songs => new PlaylistPlayer(file.FullName, songs));
    public static Result<IEnumerable<MutableSong>> EnumerateArchive(FileInfo file) {
        var archive = file.ToResultWhereExists()
            .Map(file => new ZipArchive(file.OpenRead()));

        var songs = archive.Map(archive => archive.GetEntry("song_list.json"), ResultFlag.InvalidFile)
            .Map(songListEntry => ParseSongList(songListEntry.Open())).WhereNotEmpty();


        using var archive = new ZipArchive(file.OpenRead());
        var songListEntry = archive.GetEntry("song_list.json");
        if(songListEntry is null) {
            return ResultFlag.InvalidFile;
        }

        using var songListStream = songListEntry.Open();
        var songs = ParseSongList(songListStream);

        if(!songs.Any()) {
            return ResultFlag.Null;
        }

        foreach(var song in songs) {
            var songEntry = archive.GetEntry(song.Path)!;
            using var songStream = songEntry.Open();
            song.Path = SongCache.Cache(songStream, song.Path).FullName;
        }

        return Result<IEnumerable<MutableSong>>.Of(songs);
        
        static IEnumerable<MutableSong> ParseSongList(Stream stream) {
            return JsonExtensions.Deserialize<MutableSong[]>(stream).Map<IEnumerable<MutableSong>>(songs => songs.OrderBy(s => s.Index)).Reduce([]);
        }
    }
    
    public static ResultFlag ToArchive(FileInfo file, IEnumerable<ISong> songs) {
        if(!songs.Any()) {
            return ResultFlag.Null;
        }

        using var archive = new ZipArchive(file.Create(), ZipArchiveMode.Create);

        var songsCopy = songs.Select((song, index) => new MutableSong(song, index)).ToArray();

        foreach(var song in songsCopy) {
            var entryName = Path.GetFileName(song.Path);
            var songEntry = archive.CreateEntry(entryName)!;
            using var songStream = songEntry.Open();
            using var fileStream = File.OpenRead(song.Path);
            fileStream.CopyTo(songStream);
            song.SetPath(entryName);
        }

        var songListEntry = archive.CreateEntry("song_list.json")!;

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
