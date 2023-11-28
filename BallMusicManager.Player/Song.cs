using Ametrin.Utils;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BallMusicManager.Player;

internal record Song(string Path, int Index, string Title, string Artist, string Dance) {

    public static readonly IReadOnlyDictionary<string, string> DanceKeys = new Dictionary<string, string>{
        {"CCC", "ChaChaCha" },
        {"LW", "Langsamer Walzer" },
        {"JVE", "Jive" },
        {"DFX", "Discofox" },
        {"FXTR", "Foxtrott" },
        {"RMB", "Rumba" },
        {"WW", "Wiener Walzer" },
        {"TGO", "Tango" },
        {"SMB", "Samba" },
        {"SLS", "Salsa" },
        {"RNR", "Rock'n Roll" },
        {"PT", "Party" },
        {"FS", "Freestyle" },
        {"", "Freestyle" },
    }.ToFrozenDictionary();

    public static Result<Song> FromPath(string path, int shouldIdx) {
        using var file = TagLib.File.Create(path);
        var fileName = System.IO.Path.GetFileNameWithoutExtension(path);

        return ParseFileName(fileName, shouldIdx).Map(fileNameInfo=>{
            if (file.Properties.Duration.TotalMinutes < 1.5){
                Trace.TraceWarning($"{fileName} is probably not a full song");
            }
            return new Song(path, fileNameInfo.idx, fileNameInfo.title, file.Tag.FirstPerformer, fileNameInfo.dance);
        });

        static Result<(int idx, string title, string dance)> ParseFileName(string fileName, int shouldIdx) {
            var splited = fileName.Split("_");
            if(splited.Length == 3) {
                return ParseInfos(splited[0], splited[2], splited[1]);
            }

            Trace.TraceWarning($"{fileName} violates naming conventions");

            if(splited.Length > 3) return ParseInfos(splited[0], splited.Skip(2).Dump(' '), splited[1]);

            if(splited.Length == 2) return ParseInfos(splited[0], splited[1]);
            
            if(splited.Length == 1) return (shouldIdx, splited[0], "");
            

            return ResultFlag.InvalidArgument;

            (int idx, string title, string dance) ParseInfos(string? idx, string title, string dance = "") {
                if(DanceKeys.TryGetValue(dance, out var fullDance)) {
                    dance = fullDance;
                } else {
                    Trace.TraceWarning($"{dance} in {fileName} is a unkown short");
                }

                if(idx is null || !idx.TryParse(out int index)) {
                    //MainViewModel.Instance.ShowFixIndicesButton = System.Windows.Visibility.Visible;
                    Trace.TraceError($"{fileName} does not have a valid Index");
                    return (shouldIdx, title, dance);
                }

                if(index != shouldIdx) {
                    //MainViewModel.Instance.ShowFixIndicesButton = System.Windows.Visibility.Visible;
                    //Trace.TraceWarning($"{fileName} should have Index {shouldIdx}");
                }

                return (index, title, dance);
            }
        }
    }

    public static string GetDance(string key){
        if (DanceKeys.TryGetValue(key, out var fullDance)){
            return fullDance;
        }
        Trace.TraceWarning($"{key} in is a unkown short");
        return key;
    }

    public static implicit operator SongDTO(Song song){
        return new(song.Title, song.Artist, song.Dance);
    }
}

public record SongDTO(string title, string artist, string dance);
