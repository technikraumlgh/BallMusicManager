using Ametrin.Utils.Optional;
using Ametrin.Utils;
using BallMusicManager.Domain;

namespace BallMusicManager.Infrastructure; 

public static class SongBuilderExtensions {
    public static MutableSong FromMetaData(this MutableSong songBuilder) {
        using var file = TagLib.File.Create(songBuilder.Path);
        //if(file.Properties.Duration.TotalMinutes < 1.5) Trace.TraceWarning($"{_Path} is probably not a full song");
        return songBuilder.SetDuration(file.Properties.Duration).SetArtist(file.Tag.FirstPerformer);
    }

    public static Option<Song> FromPath(FileInfo fileInfo) {
        var fileName = fileInfo.NameWithoutExtension();

        try {
            return new MutableSong()
                .SetPath(fileInfo)
                .FromMetaData()
                .FromFileName(fileName).Build();
        } catch {
            return Option<Song>.None();
        }
    }
}
