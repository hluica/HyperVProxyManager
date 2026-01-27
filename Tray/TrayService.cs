using System.Windows;

using HyperVProxyManager.Views;

using Microsoft.Extensions.DependencyInjection;

namespace HyperVProxyManager.Tray;

public interface ITrayService
{
    void Initialize();
    void Shutdown();
}

public class TrayService(IServiceProvider serviceProvider) : ITrayService, IDisposable
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private TrayIcon? _trayIcon;
    private TrayMenuWindow? _menuWindow;

    public void Initialize()
        // 在 UI 线程创建托盘图标
        => Application.Current.Dispatcher.Invoke(() =>
        {
            // 1. 创建 TrayIcon (XAML定义的外观和资源)
            // 此时不设置 DataContext，因为该图标本身已简化为纯 UI 容器
            _trayIcon = new TrayIcon();

            // 2. 订阅点击事件
            _trayIcon.TrayLeftMouseUp += OnLeftClick;
            _trayIcon.TrayRightMouseUp += OnRightClick;

            // 3. 预加载菜单窗口 (确保单例存在)
            _menuWindow = _serviceProvider.GetRequiredService<TrayMenuWindow>();
        });

    public void Shutdown()
    {
        _trayIcon?.Dispose();
        _menuWindow?.Close();
    }

    public void Dispose()
    {
        Shutdown();
        GC.SuppressFinalize(this);
    }

    private void OnRightClick(object sender, RoutedEventArgs e)
        // 调用自定义窗口的定位显示方法
        => _menuWindow?.ShowAtCursor();

    private void OnLeftClick(object sender, RoutedEventArgs e)
    {
        // 通过 DI 获取主窗口单例
        var window = _serviceProvider.GetRequiredService<MainWindow>();

        // 1. 显示窗口
        window.Show();

        // 2. 如果是最小化状态，还原它
        if (window.WindowState == WindowState.Minimized)
            window.WindowState = WindowState.Normal;

        // 3. 强制前台显示 (Hack: Topmost toggle)
        _ = window.Activate();
        window.Topmost = true;
        window.Topmost = false;
        _ = window.Focus();
    }
}
