using System.Windows.Media;
using System.Windows.Media.Imaging;

using Hardcodet.Wpf.TaskbarNotification;

using Wpf.Ui.Appearance;

namespace HyperVProxyManager.Tray;

public partial class TrayIcon : TaskbarIcon
{
    public TrayIcon()
    {
        InitializeComponent();

        UpdateTrayIcon(ApplicationThemeManager.GetAppTheme());
        ApplicationThemeManager.Changed += OnThemeChanged;
    }

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
        catch { /* 忽略资源加载错误 */ }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            ApplicationThemeManager.Changed -= OnThemeChanged;
        base.Dispose(disposing);
    }
}
