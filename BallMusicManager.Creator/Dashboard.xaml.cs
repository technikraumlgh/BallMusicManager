using BallMusic.Tips;
using System.Collections.Immutable;
using System.Windows;
using System.Windows.Controls;

namespace BallMusicManager.Creator;

public partial class Dashboard : Window
{
    private readonly IRule PlaylistRule = new CombinedRule([
        new DurationBetween(TimeSpan.FromHours(4), TimeSpan.FromHours(6)),
        new PartyDurationBetween(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(20)),
        new EndWithSong(new FakeSong("Can You Feel The Love Tonight", "Elton John", Dance.LangsamerWalzer), Severity.Error),
        
        // these limits should be very loose, just prevent something really stupid
        // they should be configured very carefully under consideration of what people can (especially Unterstufe) and like to dance
        new CountOfDance([                          // given 70 songs
            (Dance.ChaChaCha, 0.14f, 0.5f),         // 9 - 35
            (Dance.Discofox, 0.14f, 0.5f),          // 9 - 35
            (Dance.LangsamerWalzer, 0.1f, 0.4f),    // 7 - 28
            (Dance.WienerWalzer, 0.1f, 0.4f),       // 7 - 28
            (Dance.Rumba, 0.06f, 0.3f),             // 4 - 21
            (Dance.Tango, 0.06f, 0.3f),             // 4 - 21
            (Dance.Foxtrott, 0.05f, 0.2f),          // 3 - 14
            (Dance.Jive, 0f, 0.09f),                // 0 - 6
            (Dance.Samba, 0f, 0.09f),               // 0 - 6
            (Dance.Salsa, 0f, 0.09f),               // 0 - 6
        ]),
    ]);

    public Dashboard(ImmutableArray<Song> songs)
    {
        InitializeComponent();

        Update(songs);
    }

    public void Update(ImmutableArray<Song> songs)
    {
        RecommendationsView.Items.Clear();
        SongCountView.Children.Clear();

        DurationLabel.Content = $"Länge:\t{songs.Sum(s => s.Duration):hh\\:mm\\:ss}";
        DancesDurationLabel.Content = $" - Tänze:\t{songs.Where(s => s.Dance != Dance.Party).Sum(s => s.Duration):hh\\:mm\\:ss}";
        PartyDurationLabel.Content = $" - Party:\t{songs.Where(s => s.Dance == Dance.Party).Sum(s => s.Duration):hh\\:mm\\:ss}";

        SongCountView.Children.Add(new Label
        {
            Content = $"Songs: {songs.Length}",
            FontSize = 14,
        });

        foreach (var dance in songs.CountBy(s => s.Dance))
        {
            SongCountView.Children.Add(new Label
            {
                Content = $" - {dance.Key}: {dance.Value}",
                FontSize = 14,
            });
        }


        if (songs.IsEmpty)
        {
            RecommendationsView.Items.Add(new Tip(Severity.Warning, "Playlist is Empty"));
            return;
        }

        foreach (var tip in PlaylistRule.GetTips(songs))
        {
            RecommendationsView.Items.Add(tip);
        }
    }
}
