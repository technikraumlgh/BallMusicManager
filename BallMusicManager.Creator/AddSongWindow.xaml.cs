using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.Windows;

namespace BallMusicManager.Creator {
    /// <summary>
    /// Interaction logic for AddSongWindow.xaml
    /// </summary>
    public sealed partial class AddSongWindow : Window {
        public Song Song { get; private set; }
        private readonly string Path;
        public AddSongWindow(string path) {
            InitializeComponent();
            Path = path;
            PathText.Text = System.IO.Path.GetFileName(path);
            var song = SongBuilder.FromPath(new(path)).Reduce((Song)null!);
            Song = song;
            if(song is null) return;
            TitleField.Text = song.Title;
            ArtistField.Text = song.Artist;
            DanceField.Text = song.Dance;
        }

        private void AddSong(object sender, RoutedEventArgs e) {
            Song = new(Path, -1, TitleField.Text.Trim(), ArtistField.Text.Trim(), Dance.FromKey(DanceField.Text.Trim()), TimeSpan.Zero);
            DialogResult = true;
            //Close();
        }
    }
}
