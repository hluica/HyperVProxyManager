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

    // 单例控制变量
    private const string AppUniqueName = "HyperVProxyManager_Singleton_Mutex_V1";
    private static Mutex? _mutex;
    private static EventWaitHandle? _eventWaitHandle;

    // 配置托盘图标
    private TrayIcon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        // 1. 单实例检查
        if (!TryAcquireSingleton())
        {
            NotifyExistingInstance();
            Shutdown();
            return;
        }

        // 2. 启动跨进程唤醒监听
        StartSignalListener();

        base.OnStartup(e);

        // 3. 构建依赖注入容器
        ServiceProvider = BuildServiceProvider();

        // 4. 创建并初始化主窗口（始终完成一次 Show/Hide）
        var mainWindow = InitializeMainWindow();

        // 5. 根据启动参数决定是否对用户可见
        if (ShouldStartInBackground(e.Args))
        {
            mainWindow.Hide();
        }
        else
        {
            mainWindow.Show();
        }

        // 6. 初始化托盘（依赖已完成初始化的窗口）
        _trayIcon = new TrayIcon(mainWindow.DataContext);
    }


    protected override void OnExit(ExitEventArgs e)
    {
        // 释放托盘资源
        _trayIcon?.Dispose();

        // 释放单例互斥体
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();

        base.OnExit(e);
    }

    private static bool TryAcquireSingleton()
    {
        _mutex = new Mutex(true, AppUniqueName, out bool isNewInstance);
        return isNewInstance;
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        return services.BuildServiceProvider();
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

    private MainWindow InitializeMainWindow()
    {
        var window = ServiceProvider!.GetRequiredService<MainWindow>();

        window.Show();
        window.Hide();

        MainWindow = window;
        return window;
    }

    private static bool ShouldStartInBackground(string[] args)
        => args.Any(arg =>
            string.Equals(arg, "--background", StringComparison.OrdinalIgnoreCase));


    // 通知已存在的应用实例
    private static void NotifyExistingInstance()
    {
        try
        {
            // 打开已存在进程创建的等待句柄，并发送信号 (.Set())
            // 信号名称加后缀以区分 Mutex
            using var handle = EventWaitHandle.OpenExisting(AppUniqueName + "_Signal");
            handle.Set();
        }
        catch (Exception)
        {
            // 忽略异常，防止在极端情况下崩溃
        }
    }

    // 启动监听线程，等待信号以恢复窗口
    private void StartSignalListener()
    {
        // 创建一个自动重置的事件，名称与 NotifyExistingInstance 中一致
        _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, AppUniqueName + "_Signal");

        _ = Task.Factory.StartNew(() =>
        {
            while (true)
            {
                // 阻塞等待信号
                _eventWaitHandle.WaitOne();

                // 收到信号后，切换到 UI 线程恢复窗口
                Dispatcher.Invoke(()
                    => TrayService.ShowMainWindow());
            }
        },
        TaskCreationOptions.LongRunning);
    }
}
