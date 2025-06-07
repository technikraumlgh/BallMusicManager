using BallMusic.Infrastructure;
using BallMusic.WPF;
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
        MessageBoxHelper.ShowError($"You're fucked. The player crashed. Try to restart as fast as possible.\n{((Exception)e.ExceptionObject).Message}");
        PerformCleanup(); 
    }

    private static void PerformCleanup()
    {
        SongCache.Clear();
    }
}
