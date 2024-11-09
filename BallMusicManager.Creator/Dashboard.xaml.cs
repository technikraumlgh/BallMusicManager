using BallMusic.Tips;
using System.Collections.Immutable;
using System.Windows;
using System.Windows.Controls;

namespace BallMusicManager.Creator;

public partial class Dashboard : Window
{
    private readonly ImmutableArray<Rule> Rules =
    [
        new EndWithSong(new FakeSong("Can You Feel The Love Tonight", "Elton John", "Langsamer Walzer"), Rule.Severity.Error),
        new DurationBetween(TimeSpan.FromHours(4, 30), TimeSpan.FromHours(6)),
        new CountOfDance([("ChaChaCha", 0.14f, 0.5f), ("Discofox", 0.14f, 0.5f), ("Langsamer Walzer", 0.1f, 0.4f), ("Wiener Walzer", 0.1f, 0.4f), ("Rumba", 0.06f, 0.3f), ("Tango", 0.06f, 0.3f), ("Foxtrott", 0.05f, 0.3f), ("Samba", 0f, 0.1f), ("Jive", 0.01f, 0.1f), ("Salsa", 0f, 0.1f)]),
    ];

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
        DancesDurationLabel.Content = $" - Tänze:\t{songs.Where(s => s.Dance != "Party").Sum(s => s.Duration):hh\\:mm\\:ss}";
        PartyDurationLabel.Content = $" - Party:\t{songs.Where(s => s.Dance == "Party").Sum(s => s.Duration):hh\\:mm\\:ss}";

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
            RecommendationsView.Items.Add(new Tip(Rule.Severity.Warning, "Playlist is Empty"));
            return;
        }

        foreach (var rule in Rules)
        {
            foreach (var tip in rule.GetTips(songs))
            {
                RecommendationsView.Items.Add(tip);
            }
        }
    }
}
