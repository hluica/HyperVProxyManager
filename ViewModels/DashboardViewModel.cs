using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HyperVProxyManager.Services.Interfaces;

namespace HyperVProxyManager.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IProxyStateStore _store;
    private readonly IProxyService _proxyService;
    private readonly ISettingsService _settingsService;

    private CancellationTokenSource? _statusMessageCts;

    public DashboardViewModel(
        IProxyStateStore store,
        IProxyService proxyService,
        ISettingsService settingsService)
    {
        _store = store;
        _proxyService = proxyService;
        _settingsService = settingsService;

        _store.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HostIpAddress));
            OnPropertyChanged(nameof(IsProxyEnabled));
            OnPropertyChanged(nameof(CanQuickSet));
            OnPropertyChanged(nameof(CurrentProxyAddress));
        };

        _settingsService.Config.PropertyChanged += (_, _) =>
            OnPropertyChanged(nameof(TargetPort));
    }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = l10n.StatusMessageDefault;

    public string HostIpAddress
        => _store.HostIpAddress;

    public bool IsProxyEnabled
        => _store.CurrentProxy.IsEnabled;

    public bool CanQuickSet
        => !HostIpAddress.Contains(l10n.HostIpAddressNotDetected)
        && !HostIpAddress.Contains(l10n.HostIpAddressDetecting)
        && !IsProxyEnabled;

    public string CurrentProxyAddress
        => _store.CurrentProxy.IsEnabled
            ? _store.CurrentProxy.ServerAddress
            : l10n.CurrentProxyNotEnable;

    public int TargetPort
        => _settingsService.Config.ProxyPort;

    // 辅助方法：获取新的取消令牌，并自动取消上一次未完成的延时任务
    private CancellationToken GetNewStatusToken()
    {
        _statusMessageCts?.Cancel();
        _statusMessageCts?.Dispose();
        _statusMessageCts = new CancellationTokenSource();
        return _statusMessageCts.Token;
    }

    // 公共辅助方法：等待两秒后将状态恢复为“就绪”
    private async Task DelayAndResetStatusAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(2000, token);
            StatusMessage = l10n.StatusMessageDefault;
        }
        catch (OperationCanceledException)
        { /* 被其他方法打断，静默处理，什么都不做 */ }
    }

    [RelayCommand]
    private async Task OnLoaded()
        => await _store.RefreshAsync();

    [RelayCommand]
    private async Task OnRefresh()
    {
        var token = GetNewStatusToken();

        StatusMessage = l10n.StatusMessageRefreshing;
        await _store.RefreshAsync();
        StatusMessage = l10n.StatusMessageRefreshed;
        OnPropertyChanged(nameof(CanQuickSet));

        if (token.IsCancellationRequested)
            return;
        await DelayAndResetStatusAsync(token);
    }

    [RelayCommand]
    private async Task OnQuickSet()
    {
        if (!CanQuickSet)
            return;

        var token = GetNewStatusToken();

        // 使用 SettingsService 中的端口配置
        string targetProxy = $"{HostIpAddress}:{TargetPort}";

        var result = _proxyService.SetSystemProxy(targetProxy);
        StatusMessage = result.Message;

        if (result.IsSuccess)
        {
            await _store.RefreshAsync();
            if (token.IsCancellationRequested)
                return;
            await DelayAndResetStatusAsync(token);
        }
    }

    [RelayCommand]
    private async Task OnDisableProxy()
    {
        var token = GetNewStatusToken();

        var result = _proxyService.DisableSystemProxy();
        StatusMessage = result.Message;

        if (result.IsSuccess)
        {
            await _store.RefreshAsync();
            if (token.IsCancellationRequested)
                return;
            await DelayAndResetStatusAsync(token);
        }
    }
}
