using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Hardcodet.Wpf.TaskbarNotification;

using Wpf.Ui.Appearance;

namespace HyperVProxyManager.Tray;

public partial class TrayIcon : TaskbarIcon
{
    public TrayIcon(object viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();

        if (Application.Current?.MainWindow != null && ContextMenu != null)
        {
            ContextMenu.PlacementTarget = Application.Current.MainWindow;
        }

        // 1. 初始化时设置一次图标
        UpdateTrayIcon(ApplicationThemeManager.GetAppTheme());

        // 2. 订阅主题变更事件
        ApplicationThemeManager.Changed += OnThemeChanged;

        // 订阅 DataContext 绑定变更事件
        DataContextChanged += (s, e)
            => ContextMenu?.DataContext = e.NewValue;
    }

    private void OnLeftClick(object sender, RoutedEventArgs e)
        => TrayService.ShowMainWindow();

    private void OnShowWindow(object sender, RoutedEventArgs e)
        => TrayService.ShowMainWindow();

    private void OnExit(object sender, RoutedEventArgs e)
        => TrayService.ExitApplication();

    private void OnThemeChanged(ApplicationTheme currentTheme, Color systemAccent)
        => Dispatcher.Invoke(() => UpdateTrayIcon(currentTheme));

    private void UpdateTrayIcon(ApplicationTheme theme)
    {
        string iconFileName = theme == ApplicationTheme.Dark
            ? "AppIcon_Dark.ico"
            : "AppIcon_Light.ico";

        try
        {
            var iconUri = new Uri($"pack://application:,,,/Assets/{iconFileName}", UriKind.Absolute);
            IconSource = new BitmapImage(iconUri);
        }
        catch
        {
            // 容错：如果找不到对应图标，可以设置一个默认值
        }
    }

    // 重新实现 Dispose 以取消订阅事件
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ApplicationThemeManager.Changed -= OnThemeChanged;
        }
        base.Dispose(disposing);
    }
}
