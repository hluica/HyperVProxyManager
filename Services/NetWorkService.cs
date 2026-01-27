using System.Net.NetworkInformation;
using System.Net.Sockets;

using HyperVProxyManager.Services.Interfaces;

namespace HyperVProxyManager.Services;

public class NetworkService : INetworkService
{
    public async Task<string?> GetHyperVHostIpAsync()
        // 使用 Task.Run 将网络扫描放入后台线程，防止阻塞 UI
        => await Task.Run(() =>
        {
            try
            {
                var hyperVInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(ni
                        => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback
                        && ni.OperationalStatus == OperationalStatus.Up
                        && ni.Description.Contains("Microsoft Hyper-V Network Adapter"));

                if (hyperVInterface == null)
                    return null;

                var ipProperties = hyperVInterface.GetIPProperties();
                var gateway = ipProperties?.GatewayAddresses
                    .FirstOrDefault(ga => ga.Address.AddressFamily == AddressFamily.InterNetwork);

                return gateway?.Address.ToString();
            }
            catch
            {
                return null;
            }
        });
}
