using System.Windows;
using BallMusic.WPF;

namespace BallMusic.Creator;

public sealed partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        PerformCleanup();
        base.OnExit(e);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = (Exception)e.ExceptionObject;
        MessageBoxHelper.ShowError($"Woops, looks like we crashed!\nKeine Sorge deine Playlist wurde gespeichert. Starte die App einfach neu.\nGib folgenden Fehler bitte an einen Techniker weiter:\n{exception.GetType().Name}: {exception.Message}");
        // don't clean-up after crash to keep all data including the playlist quicksave in cache
    }

    private static void PerformCleanup()
    {
        SongCache.Clear();
    }
}
