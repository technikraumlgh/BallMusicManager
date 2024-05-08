using BallMusicManager.Infrastructure;
using System.Collections.ObjectModel;

namespace BallMusicManager.Creator;

public sealed class SongLibrary {
    public readonly ObservableCollection<SongBuilder> Songs = [];
    
    public bool ContainsSong(SongBuilder song) {
        if(Songs.Any(s=>s._Title==song._Title) && Songs.Any(s=>s._Artist==song._Artist) && Songs.Any(s=>s._Dance==song._Dance)) return false; 
        
        return true;
    }
}
