using System.Collections.Immutable;
using System.Diagnostics;
using Ametrin.Utils.Optional;
using BallMusicManager.Domain;

namespace BallMusicManager.Infrastructure;

public static class PlaylistBuilder{
    private static readonly ImmutableArray<string> AllowedFileTypes = [".mp3", ".wav", ".mp4", ".acc", ".m4a"];

    public static Playlist FromFolder(DirectoryInfo folder){
        var files = folder.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Where(ValidFile);
        //var files = Directory.GetFiles(path, , ).Where(path => Song.).ToArray();
        return new(folder.FullName, files.Select(SongBuilder.FromPath).ReduceFiltered());
    }

    public static Playlist FromCSV(FileInfo file){
        using var stream = file.OpenText();
        var songs = new List<Song>();
        
        return new(file.FullName, ParseCSV());

        IEnumerable<Song> ParseCSV(){
            var counter = 0;
            while (stream.ReadLine() is string line){
                counter++;
                var values = line.Split(';', StringSplitOptions.TrimEntries);
                var loc = Path.Combine(file.FullName, values[3]);
                if(!File.Exists(loc)) Trace.TraceError($"Song at '{loc}' does not exist");
                yield return new SongBuilder()
                    .Path(loc)
                    .Index(counter)
                    .Title(values[0])
                    .Artist(values[1])
                    .DanceFromKey(values[2])
                    .Build();
            }
        }
    }

    public static bool ValidFile(FileInfo path) => AllowedFileTypes.Contains(path.Extension);
}
