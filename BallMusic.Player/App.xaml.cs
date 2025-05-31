using BallMusic.Infrastructure;
using System.Windows;

namespace BallMusic.Player;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        PerformCleanup();
        base.OnExit(e);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        PerformCleanup();
    }

    private static void PerformCleanup()
    {
        SongCache.Clear();
    }
}
