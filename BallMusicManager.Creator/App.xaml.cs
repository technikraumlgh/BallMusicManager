using System.Windows;
using BallMusicManager.Infrastructure;

namespace BallMusicManager.Creator; 

public partial class App : Application {
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);

        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private void OnProcessExit(object? sender, EventArgs e) {
        PerformCleanup();
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
        PerformCleanup();
    }

    private static void PerformCleanup() {
        SongCache.Clear();
    }
}
