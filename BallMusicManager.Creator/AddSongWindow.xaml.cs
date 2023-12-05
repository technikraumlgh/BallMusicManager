using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.Windows;
using System.Windows.Input;

namespace BallMusicManager.Creator {
    /// <summary>
    /// Interaction logic for AddSongWindow.xaml
    /// </summary>
    public sealed partial class AddSongWindow : Window {
        public SongBuilder Song { get; private set; }
        private readonly string Path;
        public AddSongWindow(string path) {
            InitializeComponent();
            AddButton.Focus();
            Path = path;
            PathText.Text = System.IO.Path.GetFileName(path);
            Song = new SongBuilder()
                .Path(path)
                .FromMetaData();
            try {
                Song.FromFileName(System.IO.Path.GetFileNameWithoutExtension(path));
                TitleField.Text = Song._Title;
                ArtistField.Text = Song._Artist;
                DanceField.Text = Song._Dance;
            } catch { }

        }

        private void AddSong(object sender, RoutedEventArgs? e = default) {
            Song.Title(TitleField.Text.Trim())
                .Artist(ArtistField.Text.Trim())
                .DanceFromKey(Dance.FromKey(DanceField.Text.Trim()));
            DialogResult = true;
        }

        private void StackPanel_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key is not Key.Enter) return;

            AddSong(sender);
        }
    }
}
