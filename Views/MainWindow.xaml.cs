using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using HyperVProxyManager.ViewModels;

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace HyperVProxyManager.Views;

public partial class MainWindow : FluentWindow
{
    public MainWindow(MainViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();

        SystemThemeWatcher.Watch(this);
        ApplicationThemeManager.Changed += OnThemeChanged;
        UpdateWindowIcon(ApplicationThemeManager.GetAppTheme());
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true; // 阻止销毁
        Hide();          // 仅隐藏
    }

    private void OnThemeChanged(ApplicationTheme currentTheme, Color systemAccent)
        => UpdateWindowIcon(currentTheme);

    private void UpdateWindowIcon(ApplicationTheme theme)
    {
        string iconFileName = theme == ApplicationTheme.Dark
            ? "AppIcon_Dark.ico"
            : "AppIcon_Light.ico";
        try
        {
            var iconUri = new Uri($"pack://application:,,,/Assets/{iconFileName}", UriKind.Absolute);
            Icon = new BitmapImage(iconUri);
        }
        catch { /* 忽略资源加载错误 */ }
    }
}
