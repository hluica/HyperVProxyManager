using System.Windows.Media.Imaging;

using HyperVProxyManager.ViewModels;

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace HyperVProxyManager.Views;

public partial class MainWindow : FluentWindow
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        // 1. 初始化系统主题监听 (处理窗口背景色)
        SystemThemeWatcher.Watch(this);

        // 2. 订阅主题变更事件 (处理图标切换)
        ApplicationThemeManager.Changed += OnThemeChanged;

        // 3. 首次加载时，根据当前主题设置一次图标
        UpdateWindowIcon(ApplicationThemeManager.GetAppTheme());

        DataContext = viewModel;
    }

    // 事件处理器：当系统或应用主题发生变化时触发
    private void OnThemeChanged(ApplicationTheme currentTheme, System.Windows.Media.Color systemAccent)
    {
        UpdateWindowIcon(currentTheme);
    }

    // 核心逻辑：根据主题加载对应的嵌入资源图标
    private void UpdateWindowIcon(ApplicationTheme theme)
    {
        string iconFileName;

        if (theme == ApplicationTheme.Dark)
        {
            iconFileName = "AppIcon_Dark.ico";
        }
        else
        {
            iconFileName = "AppIcon_Light.ico";
        }

        // 使用 Pack URI 路径读取嵌入资源
        try
        {
            var iconUri = new Uri($"pack://application:,,,/Assets/{iconFileName}", UriKind.Absolute);
            Icon = new BitmapImage(iconUri);
        }
        catch
        {
            // 容错处理：如果图标加载失败，保持原样或不处理
        }
    }
}
