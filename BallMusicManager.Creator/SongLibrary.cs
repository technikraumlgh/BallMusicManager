using BallMusicManager.Domain;
using System.Collections;
using System.Collections.ObjectModel;

namespace BallMusicManager.Creator;

public sealed class SongLibrary : IEnumerable<MutableSong> {
    public readonly ObservableCollection<MutableSong> Songs = [];

    public bool ContainsSong(MutableSong song) => Songs.Contains(song, SongEqualityComparer.Instance);
    
    public bool AddIfNew(MutableSong song) {
        if(ContainsSong(song)) {
            return false;
        }
        Songs.Add(song);

        return true;
    }

    public IEnumerator<MutableSong> GetEnumerator() => Songs.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
