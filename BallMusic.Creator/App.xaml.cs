using System.Windows;

namespace BallMusic.Creator;

public partial class App : Application
{
    public static event Action? OnAppExit;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        OnAppExit?.Invoke();
        PerformCleanup();
        base.OnExit(e);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // lets not clean-up after a crash to keep all data including the playlist quicksave
        // PerformCleanup();
    }

    private static void PerformCleanup()
    {
        SongCache.Clear();
    }
}
