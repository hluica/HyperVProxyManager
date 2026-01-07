using System.Windows;

using Microsoft.Extensions.DependencyInjection;

using HyperVProxyManager.Services;
using HyperVProxyManager.Services.Interfaces;
using HyperVProxyManager.Tray;
using HyperVProxyManager.ViewModels;
using HyperVProxyManager.Views;

namespace HyperVProxyManager;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    private const string AppUniqueName = "HyperVProxyManager_Singleton_Mutex_V1";
    private static Mutex? _mutex;
    private static EventWaitHandle? _eventWaitHandle;

    private ITrayService? _trayService;

    protected override void OnStartup(StartupEventArgs e)
    {
        if (!TryAcquireSingleton())
        {
            NotifyExistingInstance();
            Shutdown();
            return;
        }

        StartSignalListener();
        base.OnStartup(e);

        ServiceProvider = BuildServiceProvider();

        // 1. 初始化托盘服务 (这是必须的，无论是否静默启动)
        _trayService = ServiceProvider.GetRequiredService<ITrayService>();
        _trayService.Initialize();

        // 2. 根据参数决定是否显示主窗口
        bool startInBackground = ShouldStartInBackground(e.Args);

        if (!startInBackground)
        {
            // 只有需要显示时才解析 MainWindow，避免后台启动时的闪烁
            var window = ServiceProvider.GetRequiredService<MainWindow>();
            window.Show();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayService?.Shutdown(); // 清理托盘资源
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // Core Services
        services.AddSingleton<INetworkService, NetworkService>();
        services.AddSingleton<IProxyService, ProxyService>();

        // State Store (Singleton Source of Truth)
        services.AddSingleton<IProxyStateStore, ProxyStateStore>();

        // Tray Services
        services.AddSingleton<ITrayService, TrayService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<TrayViewModel>();

        // Views (Windows should be Singleton to maintain state when hidden)
        services.AddSingleton<MainWindow>();
        services.AddSingleton<TrayMenuWindow>();

        return services.BuildServiceProvider();
    }


    private static bool TryAcquireSingleton()
    {
        _mutex = new Mutex(true, AppUniqueName, out bool isNewInstance);
        return isNewInstance;
    }

    private static bool ShouldStartInBackground(string[] args)
        => args.Any(arg => string.Equals(arg, "--background", StringComparison.OrdinalIgnoreCase));

    private static void NotifyExistingInstance()
    {
        try
        {
            using var handle = EventWaitHandle.OpenExisting(AppUniqueName + "_Signal");
            handle.Set();
        }
        catch { }
    }

    private void StartSignalListener()
    {
        _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, AppUniqueName + "_Signal");

        _ = Task.Factory.StartNew(() =>
        {
            while (true)
            {
                _eventWaitHandle.WaitOne();
                Dispatcher.Invoke(() =>
                {
                    // 收到信号唤醒主窗口
                    var window = ServiceProvider?.GetRequiredService<MainWindow>();
                    if (window != null)
                    {
                        window.Show();
                        window.Activate();
                        window.Topmost = true;
                        window.Topmost = false;
                        window.Focus();
                    }
                });
            }
        },
        TaskCreationOptions.LongRunning);
    }
}
