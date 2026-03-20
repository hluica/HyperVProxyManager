using CommunityToolkit.Mvvm.ComponentModel;

using HyperVProxyManager.Models;
using HyperVProxyManager.Services.Interfaces;

namespace HyperVProxyManager.Services;

public partial class ProxyStateStore(
    INetworkService networkService,
    IProxyService proxyService)
    : ObservableObject, IProxyStateStore
{
    private readonly INetworkService _networkService = networkService;
    private readonly IProxyService _proxyService = proxyService;

    [ObservableProperty]
    public partial string HostIpAddress { get; set; } = "正在检测...";

    [ObservableProperty]
    public partial ProxyState CurrentProxy { get; set; } = ProxyState.Empty;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = false;

    public async Task RefreshAsync()
    {
        IsLoading = true;
        try
        {
            // 并行执行检测任务
            var ipTask = _networkService.GetHyperVHostIpAsync();

            // 同步获取注册表状态
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
