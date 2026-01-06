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
        // 1. 检查是否已有实例运行
        _mutex = new Mutex(true, AppUniqueName, out bool isNewInstance);

        if (!isNewInstance)
        {
            // 如果不是新实例，说明程序已经在运行
            NotifyExistingInstance();
            // 关闭当前重复启动的进程
            Shutdown();
            return;
        }

        // 2. 如果是新实例，启动监听线程等待唤醒信号
        StartSignalListener();

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
        // 释放托盘资源
        _trayIcon?.Dispose();

        // 释放单例互斥体
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();

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
