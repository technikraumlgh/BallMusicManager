using System.Windows;
using BallMusicManager.Infrastructure;

namespace BallMusicManager.Creator; 

public partial class App : Application {
    public static event Action? OnAppExit;
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    protected override void OnExit(ExitEventArgs e) {
        OnAppExit?.Invoke();
        PerformCleanup();
        base.OnExit(e);
    }


    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
        PerformCleanup();
    }

    private static void PerformCleanup() {
        SongCache.Clear();
    }
}
