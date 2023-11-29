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

    public SongBuilder FromFileName(string fileName){
        var splited = fileName.Split("_");
        if(splited.Length != 3) throw new InvalidDataException($"{fileName} violates naming conventions");
        
        return Index(splited[0].Parse<int>()).DanceFromKey(splited[1]).Title(splited[2]);
    }

    public Song Build(){
        return new(_Path, _Index, _Title, _Artist, _Dance);
    }


    public static Option<Song> FromPath(FileInfo fileInfo){
        using var file = TagLib.File.Create(fileInfo.FullName);
        var fileName = fileInfo.NameWithoutExtension();
        if (file.Properties.Duration.TotalMinutes < 1.5) Trace.TraceWarning($"{fileName} is probably not a full song");

        try{
            return new SongBuilder()
                .File(fileInfo)
                .Artist(file.Tag.FirstPerformer)
                .FromFileName(fileName).Build();
        }catch{
            return Option<Song>.None();
        }


        // ValueOption<(int idx, string title, string dance)> ParseFileName(string fileName){
        //     var splited = fileName.Split("_");
        //     if (splited.Length == 3){
        //         return ParseInfos(splited[0], splited[2], splited[1]);
        //     }

        //     Trace.TraceWarning($"{fileName} violates naming conventions");

        //     if (splited.Length > 3) return ParseInfos(splited[0], splited.Skip(2).Dump(' '), splited[1]);
        //     if (splited.Length == 2) return ParseInfos(splited[0], splited[1]);
        //     if (splited.Length == 1) return (shouldIdx, splited[0], "");

        //     return ValueOption<(int, string, string)>.None();

        //     (int idx, string title, string dance) ParseInfos(string? idx, string title, string dance = ""){
        //         if (idx is null || !idx.TryParse(out int index)){
        //             Trace.TraceError($"{fileName} does not have a valid Index");
        //             return (index, title, Dance.FromKey(dance));
        //         }

        //         return (index, title, Dance.FromKey(dance));
        //     }
        // }
    }
}
