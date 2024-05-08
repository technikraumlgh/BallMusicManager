using System.Diagnostics;
using Ametrin.Utils;
using Ametrin.Utils.Optional;
using BallMusicManager.Domain;

namespace BallMusicManager.Infrastructure;

public sealed class SongBuilder{
    public string _Path = string.Empty;
    public int _Index = -1;
    public string _Title = string.Empty;
    public string _Artist = string.Empty;
    public string _Dance = string.Empty;
    public TimeSpan _Duration = TimeSpan.Zero;

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
    public SongBuilder FromMetaData(){
        using var file = TagLib.File.Create(_Path);
        //if(file.Properties.Duration.TotalMinutes < 1.5) Trace.TraceWarning($"{_Path} is probably not a full song");
        return Duration(file.Properties.Duration).Artist(file.Tag.FirstPerformer);
    }

    public SongBuilder FromFileName(string fileName){
        var split = fileName.Split("_");
        if(split.Length == 3) {
            return Index(split[0].TryParse<int>().Reduce(-1)).DanceFromKey(split[1]).Title(split[2]);
        }
        if(split.Length == 1) {
            return Index(-1).Title(fileName);
        }
        if(split.Length == 2) {
            return Index(-1).Title(split[1]).DanceFromKey(split[0]);
        }
        if(split.Length > 3) {
            return Index(split[0].TryParse<int>().Reduce(-1)).DanceFromKey(split[1]).Title(split.Skip(2).Dump(' '));
        }
        throw new InvalidDataException($"{fileName} does not match naming conventions");
    }

    public Song Build(){
        return new(_Path, _Index, _Title, _Artist, _Dance, _Duration);
    }


    public static Option<Song> FromPath(FileInfo fileInfo){
        var fileName = fileInfo.NameWithoutExtension();

        try{
            return new SongBuilder()
                .File(fileInfo)
                .FromMetaData()
                .FromFileName(fileName).Build();
        }catch{
            return Option<Song>.None();
        }
    }
}
