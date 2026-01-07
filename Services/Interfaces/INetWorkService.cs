namespace HyperVProxyManager.Services.Interfaces;

public interface INetworkService
{
    Task<string?> GetHyperVHostIpAsync();
}
