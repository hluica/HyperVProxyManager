using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HyperVProxyManager.Services.Interfaces;
using HyperVProxyManager.Views;
using Microsoft.Extensions.DependencyInjection;

namespace HyperVProxyManager.ViewModels;

public partial class TrayViewModel : ObservableObject
{
    private readonly IProxyStateStore _store;
    private readonly IProxyService _proxyService;
    private readonly IServiceProvider _serviceProvider;

    public TrayViewModel(IProxyStateStore store, IProxyService proxyService, IServiceProvider serviceProvider)
    {
        _store = store;
        _proxyService = proxyService;
        _serviceProvider = serviceProvider;

        // 监听 Store 变化更新 UI
        _store.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IProxyStateStore.CurrentProxy))
            {
                OnPropertyChanged(nameof(IsProxyEnabled));
                OnPropertyChanged(nameof(ProxyStatusText));
            }
        };
    }

    public string HostIpAddress
        => _store.HostIpAddress;
    public bool CanQuickSet
        => !HostIpAddress.Contains("未检测") && !HostIpAddress.Contains("正在检测");

    public bool IsProxyEnabled
        => _store.CurrentProxy.IsEnabled;

    public string ProxyStatusText
        => _store.CurrentProxy.IsEnabled
            ? $"{_store.CurrentProxy.ServerAddress}"
            : "系统代理已禁用";

    [RelayCommand]
    private async Task OnQuickSet()
    {
        if (string.IsNullOrEmpty(_store.HostIpAddress) || _store.HostIpAddress.Contains("检测"))
            // 如果 Store 还没数据，先刷新一次
            await _store.RefreshAsync();

        if (!string.IsNullOrEmpty(_store.HostIpAddress) && !_store.HostIpAddress.Contains("未检测"))
        {
            _proxyService.SetSystemProxy($"{_store.HostIpAddress}:7890");
            await _store.RefreshAsync();
        }
    }

    [RelayCommand]
    private async Task OnDisableProxy()
    {
        _proxyService.DisableSystemProxy();
        await _store.RefreshAsync();
    }

    [RelayCommand]
    private void OnShowMainWindow()
    {
        // 懒加载主窗口
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        mainWindow.Show();
        if (mainWindow.WindowState == WindowState.Minimized)
            mainWindow.WindowState = WindowState.Normal;

        mainWindow.Activate();
        mainWindow.Topmost = true;
        mainWindow.Topmost = false;
        mainWindow.Focus();
    }

    [RelayCommand]
    private static void OnExit()
        => Application.Current.Shutdown();
}
