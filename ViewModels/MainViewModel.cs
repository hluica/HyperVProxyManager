using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HyperVProxyManager.Services.Interfaces;

namespace HyperVProxyManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IProxyStateStore _store;
    private readonly IProxyService _proxyService;

    public MainViewModel(IProxyStateStore store, IProxyService proxyService)
    {
        _store = store;
        _proxyService = proxyService;

        _store.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IProxyStateStore.HostIpAddress))
            {
                OnPropertyChanged(nameof(HostIpAddress));
                OnPropertyChanged(nameof(CanQuickSet));
            }

            if (e.PropertyName == nameof(IProxyStateStore.CurrentProxy))
            {
                OnPropertyChanged(nameof(CurrentProxyAddress));
                OnPropertyChanged(nameof(IsProxyEnabled));
            }
        };
    }

    public string HostIpAddress
        => _store.HostIpAddress;

    public bool CanQuickSet
        => !HostIpAddress.Contains("未检测") && !HostIpAddress.Contains("正在检测");

    public bool IsProxyEnabled
        => _store.CurrentProxy.IsEnabled;

    public string CurrentProxyAddress
        => _store.CurrentProxy.IsEnabled
            ? _store.CurrentProxy.ServerAddress
            : "未启用";

    // 依然保留一个 UI 状态属性
    [ObservableProperty]
    private string _statusMessage = "就绪";

    [RelayCommand]
    private async Task OnLoaded()
        => await _store.RefreshAsync();

    [RelayCommand]
    private async Task OnRefresh()
    {
        StatusMessage = "正在刷新...";
        await _store.RefreshAsync();
        StatusMessage = "刷新完成";

        // 强制通知 CanQuickSet 更新
        OnPropertyChanged(nameof(CanQuickSet));
    }

    [RelayCommand]
    private async Task OnQuickSet()
    {
        if (!CanQuickSet)
            return;

        string targetProxy = $"{HostIpAddress}:7890";
        var result = _proxyService.SetSystemProxy(targetProxy);
        StatusMessage = result.Message;

        if (result.IsSuccess)
            await _store.RefreshAsync();
    }

    [RelayCommand]
    private async Task OnDisableProxy()
    {
        var result = _proxyService.DisableSystemProxy();
        StatusMessage = result.Message;

        if (result.IsSuccess)
            await _store.RefreshAsync();
    }
}
