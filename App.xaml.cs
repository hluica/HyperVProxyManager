using System.Windows;

using HyperVProxyManager.Services;
using HyperVProxyManager.Tray;
using HyperVProxyManager.ViewModels;
using HyperVProxyManager.Views;

using Microsoft.Extensions.DependencyInjection;

namespace HyperVProxyManager;

public partial class App : Application
{
    // 依赖注入容器
    public static IServiceProvider? ServiceProvider { get; private set; }

    // 配置托盘图标
    private TrayIcon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 配置服务
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        // 启动主窗口
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();

        // 绑定托盘 DataContext
        _trayIcon = new TrayIcon(mainWindow.DataContext);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose(); // 释放托盘资源
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // 注册 Services
        services.AddSingleton<INetworkService, NetworkService>();
        services.AddSingleton<IProxyService, ProxyService>();

        // 注册 ViewModels
        services.AddTransient<MainViewModel>();

        // 注册 Views
        services.AddSingleton<MainWindow>();
    }
}
