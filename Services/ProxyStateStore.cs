using CommunityToolkit.Mvvm.ComponentModel;

using HyperVProxyManager.Models;
using HyperVProxyManager.Services.Interfaces;

namespace HyperVProxyManager.Services;

public partial class ProxyStateStore(INetworkService networkService, IProxyService proxyService) : ObservableObject, IProxyStateStore
{
    private readonly INetworkService _networkService = networkService;
    private readonly IProxyService _proxyService = proxyService;

    [ObservableProperty]
    private string _hostIpAddress = "正在检测...";

    [ObservableProperty]
    private ProxyState _currentProxy = ProxyState.Empty;

    [ObservableProperty]
    private bool _isLoading = false;

    public async Task RefreshAsync()
    {
        IsLoading = true;
        try
        {
            // 并行执行检测任务
            var ipTask = _networkService.GetHyperVHostIpAsync();

            // 同步获取注册表状态（通常很快，无需 Task.Run）
            var proxyState = _proxyService.GetSystemProxy();

            string? ip = await ipTask;

            HostIpAddress = string.IsNullOrEmpty(ip) ? "未检测到虚拟机网关" : ip;
            CurrentProxy = proxyState;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
