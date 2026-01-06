using System.Windows;

using HyperVProxyManager.Views;

namespace HyperVProxyManager.Tray;

public static class TrayService
{
    public static void ShowMainWindow()
    {
        // 获取主窗口实例
        if (Application.Current.MainWindow is not MainWindow window)
            return;

        // 1. 如果窗口是隐藏的（被关闭到了托盘），让它显示
        if (!window.IsVisible)
            window.Show();

        // 2. 如果窗口是最小化的，还原它
        if (window.WindowState == WindowState.Minimized)
            window.WindowState = WindowState.Normal;

        // 3. 核心增强：强制前台显示
        // 单纯的 Activate 在某些情况下（如从后台进程唤起）可能抢不到焦点
        // 通过临时 Topmost 可以强制窗口浮到最上层
        window.Activate();
        window.Topmost = true;
        window.Topmost = false;
        window.Focus();
    }

    public static void HideMainWindow()
        => Application.Current.MainWindow?.Hide();

    public static void ExitApplication()
        => Application.Current.Shutdown();
}
