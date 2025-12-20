using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HyperVProxyManager.Services;

namespace HyperVProxyManager.ViewModels;

public partial class MainViewModel(INetworkService networkService, IProxyService proxyService) : ObservableObject
{
    private readonly INetworkService _networkService = networkService;
    private readonly IProxyService _proxyService = proxyService;

    // 固定的默认端口要求
    private const string DefaultPort = "7890";

    [ObservableProperty]
    private string _hostIpAddress = "正在检测...";

    [ObservableProperty]
    private string _currentProxyAddress = "未知";

    [ObservableProperty]
    private bool _isProxyEnabled = false;

    [ObservableProperty]
    private string _statusMessage = "就绪";

    [ObservableProperty]
    private bool _canQuickSet = false;

    [RelayCommand]
    private async Task OnLoaded() => await RefreshDataAsync();

    [RelayCommand]
    private async Task OnRefresh()
    {
        StatusMessage = "正在刷新...";
        await RefreshDataAsync();
        StatusMessage = "刷新完成";
    }

    [RelayCommand]
    private void OnQuickSet()
    {
        if (string.IsNullOrEmpty(HostIpAddress) || !CanQuickSet)
        {
            StatusMessage = "错误：未检测到有效的宿主机 IP";
            return;
        }

        var targetProxy = $"{HostIpAddress}:{DefaultPort}";
        var success = _proxyService.SetSystemProxy(targetProxy);

        if (success)
        {
            StatusMessage = $"成功：已设置代理为 {targetProxy}";
            UpdateProxyDisplay(); // 立即刷新显示
        }
        else
        {
            StatusMessage = "错误：设置代理失败，请检查权限";
        }
    }

    [RelayCommand]
    private void OnDisableProxy()
    {
        var success = _proxyService.DisableSystemProxy();
        if (success)
        {
            StatusMessage = "成功：系统代理已禁用";
            UpdateProxyDisplay();
        }
        else
        {
            StatusMessage = "错误：禁用代理失败";
        }
    }

    private async Task RefreshDataAsync()
    {
        // 1. 获取宿主机 IP
        var ip = await _networkService.GetHyperVHostIpAsync();
        if (!string.IsNullOrEmpty(ip))
        {
            HostIpAddress = ip;
            CanQuickSet = true;
        }
        else
        {
            HostIpAddress = "未检测到虚拟机网关";
            CanQuickSet = false;
        }

        // 2. 获取当前代理状态
        UpdateProxyDisplay();
    }

    private void UpdateProxyDisplay()
    {
        var state = _proxyService.GetSystemProxy();
        IsProxyEnabled = state.IsEnabled;
        CurrentProxyAddress = state.IsEnabled ? state.ServerAddress : "未启用";
    }
}
