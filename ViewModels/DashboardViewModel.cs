using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HyperVProxyManager.Services.Interfaces;

namespace HyperVProxyManager.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IProxyStateStore _store;
    private readonly IProxyService _proxyService;
    private readonly ISettingsService _settingsService;

    public DashboardViewModel(
        IProxyStateStore store,
        IProxyService proxyService,
        ISettingsService settingsService)
    {
        _store = store;
        _proxyService = proxyService;
        _settingsService = settingsService;

        _store.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(HostIpAddress));
            OnPropertyChanged(nameof(IsProxyEnabled));
            OnPropertyChanged(nameof(CanQuickSet));
            OnPropertyChanged(nameof(CurrentProxyAddress));
        };

        _settingsService.Config.PropertyChanged += (s, e)
            => OnPropertyChanged(nameof(TargetPort));
    }

    [ObservableProperty]
    private string _statusMessage = "就绪";

    public string HostIpAddress
        => _store.HostIpAddress;

    public bool IsProxyEnabled
        => _store.CurrentProxy.IsEnabled;

    public bool CanQuickSet
        => !HostIpAddress.Contains("未检测")
        && !HostIpAddress.Contains("正在检测")
        && !IsProxyEnabled;

    public string CurrentProxyAddress
        => _store.CurrentProxy.IsEnabled
            ? _store.CurrentProxy.ServerAddress
            : "未启用";

    public int TargetPort
        => _settingsService.Config.ProxyPort;

    [RelayCommand]
    private async Task OnLoaded()
        => await _store.RefreshAsync();

    [RelayCommand]
    private async Task OnRefresh()
    {
        StatusMessage = "正在刷新...";
        await _store.RefreshAsync();
        StatusMessage = "刷新完成";
        OnPropertyChanged(nameof(CanQuickSet));
    }

    [RelayCommand]
    private async Task OnQuickSet()
    {
        if (!CanQuickSet)
            return;

        // 使用 SettingsService 中的端口配置
        string targetProxy = $"{HostIpAddress}:{TargetPort}";

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
