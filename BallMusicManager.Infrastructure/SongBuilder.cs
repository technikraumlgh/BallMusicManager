using System.Diagnostics;
using Ametrin.Utils;
using Ametrin.Utils.Optional;
using BallMusicManager.Domain;

namespace BallMusicManager.Infrastructure;

public sealed class SongBuilder{
    private string _Path = string.Empty;
    private int _Index = -1;
    private string _Title = string.Empty;
    private string _Artist = string.Empty;
    private string _Dance = string.Empty;
    private TimeSpan _Duration = TimeSpan.Zero;

    public SongBuilder File(FileInfo file){
        return Path(file.FullName);
    }
    public SongBuilder Path(string path){
        _Path = path;
        return this;
    }
    public SongBuilder Index(int index){
        _Index = index;
        return this;
    }
    public SongBuilder Title(string title){
        _Title = title;
        return this;
    }
    public SongBuilder Artist(string artist){
        _Artist = artist;
        return this;
    }
    public SongBuilder Dance(string dance){
        _Dance = dance;
        return this;
    }
    public SongBuilder DanceFromKey(string key){
        return Dance(Domain.Dance.FromKey(key));
    }
    public SongBuilder Duration(TimeSpan duration){
        _Duration = duration;
        return this;
    }

    public SongBuilder FromFileName(string fileName){
        var split = fileName.Split("_");
        if(split.Length != 3) throw new InvalidDataException($"{fileName} violates naming conventions");
        
        return Index(split[0].Parse<int>()).DanceFromKey(split[1]).Title(split[2]);
    }

    public Song Build(){
        return new(_Path, _Index, _Title, _Artist, _Dance, _Duration);
    }


    public static Option<Song> FromPath(FileInfo fileInfo){
        using var file = TagLib.File.Create(fileInfo.FullName);
        var fileName = fileInfo.NameWithoutExtension();
        if (file.Properties.Duration.TotalMinutes < 1.5) Trace.TraceWarning($"{fileName} is probably not a full song");

        try{
            return new SongBuilder()
                .File(fileInfo)
                .Artist(file.Tag.FirstPerformer)
                .Duration(file.Properties.Duration)
                .FromFileName(fileName).Build();
        }catch{
            return Option<Song>.None();
        }
    }
}
