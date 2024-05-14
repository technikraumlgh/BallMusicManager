using Ametrin.Utils;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace BallMusicManager.Creator;

public sealed partial class AddSongWindow : Window {
    public MutableSong Song { get; private set; }
    public AddSongWindow(FileInfo fileInfo) {
        InitializeComponent();
        DanceField.ItemsSource = Dance.DanceKeys;
        AddButton.Focus();
        PathText.Text = fileInfo.FullName;
        Song = new MutableSong()
            .SetPath(fileInfo)
            .FromMetaData();


        try {
            Song.FromFileName(fileInfo.NameWithoutExtension());
            TitleField.Text = Song.Title;
            ArtistField.Text = Song.Artist;
            DanceField.Text = Song.Dance;
        } catch { }
    }

    private void AddSong(object sender, RoutedEventArgs? e = default) {
        Song.SetTitle(TitleField.Text.Trim())
            .SetArtist(ArtistField.Text.Trim())
            .SetDanceFromKey(DanceField.Text.Trim());
        DialogResult = true;
    }

    private void StackPanel_KeyDown(object sender, KeyEventArgs e) {
        if(e.Key is not Key.Enter) return;

        AddSong(sender);
    }

    private void DanceField_LostFocus(object sender, RoutedEventArgs e) {
        DanceField.Text = Dance.FromKey(DanceField.Text);
    }
}
