﻿using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace BallMusicManager.Infrastructure;

public static class PlaylistBuilder
{
    private static readonly ImmutableHashSet<string> AllowedFileTypes = [".mp3", ".wav", ".mp4", ".acc", ".m4a"];

    public static PlaylistPlayer FromFolder(DirectoryInfo folder)
    {
        var files = folder.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Where(ValidFile);
        return new(folder.FullName, files.Select(SongBuilderExtensions.FromPath).WhereSuccess());
    }

    public static PlaylistPlayer FromFile(FileInfo file)
    {
        return new(file.DirectoryName!, EnumerateFile(file).Select(b => b.Build()));
    }

    const string SONG_LIST_ENTRY_NAME = "song_list.json";
    const string MANIFEST_ENTRY_NAME = "manifest.json";
    public static Result<PlaylistPlayer> FromArchive(FileInfo file)
    {
        var songs = EnumerateArchiveEntries(file);
        songs.Consume(songs =>
        {
            foreach (var song in songs)
            {
                SongCache.CacheFromArchive(song);
            }
        });
        return songs.Map(songs => new PlaylistPlayer(file.FullName, songs.Select(s => s.Build())));
    }

    public static Result<SongBuilder[]> EnumerateArchiveEntries(FileInfo file)
    {
        var archive = OpenArchive(file);

        var manifest = archive.Map(archive => archive.GetEntry(MANIFEST_ENTRY_NAME)!).Map(ReadManifest);

        var songs = archive.Map(archive => archive.GetEntry(SONG_LIST_ENTRY_NAME)!)
            .Map(ParseSongList).Map(songs => songs.Select(song => song.Path switch
                {
                    LegacyLocation legacy => song.SetLocation(new ArchiveLocation(legacy.Path, file)),
                    HashEmbeddedLocation hashEmbedded => song.SetLocation(new ArchiveLocation(song.FileHash, file)),
                    _ => song
                })).RejectEmpty().Map(Enumerable.ToArray);

        // just to support archives where the hash wasn't used yet (only existed during development)
        // remove in the future
        (archive, songs).Consume((archive, songs) =>
        {
            foreach (var song in songs)
            {
                if (!song.HasFileHash && song.Path is ArchiveLocation location)
                {
                    var songEntry = archive.GetEntry(location.EntryName)!;
                    song.SetHash(GetFileHash(songEntry));
                }
            }
        });

        archive.Dispose();

        return songs;

        static IEnumerable<SongBuilder> ParseSongList(ZipArchiveEntry entry)
        {
            using var stream = entry.Open();
            return JsonSerializer.Deserialize<SongBuilder[]>(stream).ToOption().Map<IEnumerable<SongBuilder>>(songs => songs.OrderBy(s => s.Index)).Or([]);
        }

        static byte[] GetFileHash(ZipArchiveEntry entry)
        {
            using var stream = entry.Open();
            return SongBuilder.ComputeSHA256Hash(stream);
        }

        static Option<ArchiveManifest> ReadManifest(ZipArchiveEntry entry)
        {
            using var stream = entry.Open();
            return JsonSerializer.Deserialize<ArchiveManifest>(stream);
        }
    }

    public static Result<ZipArchive> OpenArchive(FileInfo file)
    {
        return file.ToResult().RequireExists()
            .Map(file => new ZipArchive(file.OpenRead()));
    }

    public static Option ToArchive(FileInfo file, IEnumerable<SongBuilder> songs)
    {
        if (!songs.Any())
        {
            return false;
        }

        return ToArchiveImpl(file, [.. songs.Select((song, index) => song.Copy().SetIndex(index))]);
    }

    private const int ARCHIVE_VERSION = 1;
    private static Option ToArchiveImpl(FileInfo archiveFile, SongBuilder[] songs)
    {
        var usedEntries = new HashSet<string>();
        using var archive = archiveFile.Exists
            ? new ZipArchive(archiveFile.Open(FileMode.Open), ZipArchiveMode.Update)
            : new ZipArchive(archiveFile.Create(), ZipArchiveMode.Update);


        foreach (var song in songs)
        {
            if (usedEntries.Contains(song.FileHash)) //TODO: properly handle this case
            {
                var other = songs.Where(other => song.FileHash == other.FileHash).FirstOrDefault();
                throw new UnreachableException($"{song.Path} produced an already existing hash!");
            }
            usedEntries.Add(song.FileHash);

            if (archive.GetEntry(song.FileHash) is null)
            {
                switch (song.Path)
                {
                    case FileLocation fileLocation:
                        archive.CreateEntryFromFile(fileLocation.FileInfo.FullName, song.FileHash);
                        break;

                    case ArchiveLocation archiveLocation:
                        Debug.Assert(archiveLocation.ArchiveFileInfo != archiveFile);
                        var target = archive.CreateEntry(song.FileHash);
                        using (var targetStream = target.Open())
                        {
                            using var sourceArchiveStream = new ZipArchive(archiveLocation.ArchiveFileInfo.Open(FileMode.Open), ZipArchiveMode.Read);
                            var source = sourceArchiveStream.GetEntry(archiveLocation.EntryName)!;
                            using var sourceStream = source.Open();
                            sourceStream.CopyTo(targetStream);
                        }
                        break;

                    default:
                        throw new InvalidOperationException($"Location of {song} not defined properly");
                }
            }

            song.SetLocation(new HashEmbeddedLocation());
        }

        // cleans the archive of unused files
        // ToArray to isolate the deletion from archive.Entries as it will change
        foreach (var entry in archive.Entries.Where(entry => !usedEntries.Contains(entry.Name)).ToArray())
        {
            entry.Delete();
        }

        WriteSongList();

        WriteManifest();

        return true;

        void WriteSongList()
        {
            var entry = archive.OverwriteEntry(SONG_LIST_ENTRY_NAME);
            using var stream = entry.Open();
            JsonSerializer.Serialize(stream, songs);
        }

        void WriteManifest()
        {
            var manifest = new ArchiveManifest(ARCHIVE_VERSION, songs.Length, DateTime.Now);
            var entry = archive.OverwriteEntry(MANIFEST_ENTRY_NAME);
            using var stream = entry.Open();
            JsonSerializer.Serialize(stream, manifest);
        }
    }

    public static IEnumerable<SongBuilder> EnumerateFile(FileInfo file)
    {
        using var stream = file.OpenRead();
        return MapFrom(JsonSerializer.Deserialize<List<SongBuilder>>(stream)!);

        IEnumerable<SongBuilder> MapFrom(IEnumerable<SongBuilder> songs)
        {
            foreach (var song in songs)
            {
                yield return song.Path switch
                {
                    ArchiveLocation archive => new SongBuilder(song).SetLocation(FileLocation.Of(Path.Combine(file.DirectoryName!, archive.EntryName))),
                    FileLocation => song,
                    _ => throw new UnreachableException(),
                };
            }
        }
    }

    public static bool ValidFile(FileInfo path) => AllowedFileTypes.Contains(path.Extension);

    private record ArchiveManifest(int Version, int SongCount, DateTime SavedAt);
}
