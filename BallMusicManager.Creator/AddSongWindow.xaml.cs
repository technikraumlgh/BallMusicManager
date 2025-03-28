using System.IO;
using System.Windows;
using System.Windows.Input;

namespace BallMusicManager.Creator;

public sealed partial class AddSongWindow : Window
{
    public SongBuilder Song { get; private set; }
    public AddSongWindow(FileInfo fileInfo)
    {
        InitializeComponent();
        DanceField.ItemsSource = Dance.DanceSlugs.Values.Distinct();
        AddButton.Focus();
        PathText.Text = fileInfo.FullName;
        Song = new SongBuilder()
            .SetLocation(fileInfo)
            .FromMetaData();


        try
        {
            Song.FromFileName(fileInfo.NameWithoutExtension());
            TitleField.Text = Song.Title;
            ArtistField.Text = Song.Artist;
            DanceField.Text = Song.Dance;
        }
        catch { }
    }

    private void AddSong(object sender, RoutedEventArgs? e = default)
    {
        Song.SetTitle(TitleField.Text.Trim())
            .SetArtist(ArtistField.Text.Trim())
            .SetDanceFromSlug(DanceField.Text.Trim());
        DialogResult = true;
    }

    // TODO: jump to next field and only add song when last field
    private void StackPanel_KeyDown(object sender, KeyEventArgs e)
    {
        if(e.Key is not Key.Enter)
        {
            return;
        }

        AddSong(sender);
    }

    private void DanceField_LostFocus(object sender, RoutedEventArgs e)
    {
        DanceField.Text = Dance.FromSlug(DanceField.Text);
    }
}
