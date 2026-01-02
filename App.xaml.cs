using System.Windows;

using HyperVProxyManager.Services;
using HyperVProxyManager.ViewModels;
using HyperVProxyManager.Views;

using Microsoft.Extensions.DependencyInjection;

namespace HyperVProxyManager;

public partial class App : Application
{
    // 依赖注入容器
    public static IServiceProvider? ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        ServiceProvider = serviceCollection.BuildServiceProvider();

        // 启动主窗口
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // 注册服务 (Service Layer)
        services.AddSingleton<INetworkService, NetworkService>();
        services.AddSingleton<IProxyService, ProxyService>();

        // 注册 ViewModels
        services.AddTransient<MainViewModel>();

        // 注册 Views
        services.AddSingleton<MainWindow>();
    }
}
