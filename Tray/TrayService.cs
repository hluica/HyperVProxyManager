using System.Windows;

using HyperVProxyManager.Views;

namespace HyperVProxyManager.Tray;

public static class TrayService
{
    public static void ShowMainWindow()
    {
        if (Application.Current.MainWindow is not MainWindow window)
            return;

        if (!window.IsVisible)
            window.Show();

        window.WindowState = WindowState.Normal;
        window.Activate();
    }

    public static void HideMainWindow()
        => Application.Current.MainWindow?.Hide();

    public static void ExitApplication()
        => Application.Current.Shutdown();
}
