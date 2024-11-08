using BallMusicManager.Creator.Tips;
using BallMusicManager.Domain;
using System.Collections.Immutable;
using System.Windows;
using System.Windows.Controls;

namespace BallMusicManager.Creator;

public partial class Dashboard : Window
{
    private readonly ImmutableArray<Rule> Rules = [
            new EndWithSong(new FakeSong("Can You Feel The Love Tonight", "Elton John", "Langsamer Walzer"), Rule.Severity.Error),
            new DurationBetween(TimeSpan.FromHours(5), TimeSpan.FromHours(6)),
        ];
    private readonly MainWindow mainWindow;

    public Dashboard(MainWindow mainWindow)
    {
        this.mainWindow = mainWindow;
        InitializeComponent();

        Update();
    }

    public void Update()
    {
        RecommendationsView.Items.Clear();
        var songs = mainWindow.Playlist.Select(s => s.Build()).ToImmutableArray();

        DurationLabel.Content = $"Länge:\t{songs.Sum(s => s.Duration):hh\\:mm\\:ss}";
        DancesDurationLabel.Content = $" - Tänze:\t{songs.Where(s => s.Dance != "Party").Sum(s => s.Duration):hh\\:mm\\:ss}";
        PartyDurationLabel.Content = $" - Party:\t{songs.Where(s => s.Dance == "Party").Sum(s => s.Duration):hh\\:mm\\:ss}";

        SongCountLabel.Content = $"Songs: {songs.Length}";

        foreach(var dance in songs.CountBy(s => s.Dance))
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
            foreach(var tip in rule.GetTips(songs))
            {
                RecommendationsView.Items.Add(tip);
            }
        }
    }
}
