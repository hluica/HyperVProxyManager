using System.Windows;
using HyperVProxyManager.ViewModels;
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
            _trayIcon.TrayLeftMouseUp += OnTrayClick;
            _trayIcon.TrayRightMouseUp += OnTrayClick;

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

    private void OnTrayClick(object sender, RoutedEventArgs e)
    {
        if (_menuWindow == null)
            return;

        // 调用自定义窗口的定位显示方法
        _menuWindow.ShowAtCursor();
    }
}
