using System.Net.NetworkInformation;

namespace HyperVProxyManager.Services;

public interface INetworkService
{
    Task<string?> GetHyperVHostIpAsync();
}

public class NetworkService : INetworkService
{
    public async Task<string?> GetHyperVHostIpAsync()
        // 使用 Task.Run 将网络扫描放入后台线程，防止阻塞 UI
        => await Task.Run(() =>
        {
            try
            {
                var hyperVInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(ni =>
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback
                        && ni.OperationalStatus == OperationalStatus.Up
                        && (ni.Description.Contains("Microsoft Hyper-V Network Adapter")
                            || ni.Description.Contains("Hyper-V Virtual Ethernet Adapter")));

                if (hyperVInterface == null)
                    return null;

                var ipProperties = hyperVInterface.GetIPProperties();
                var gateway = ipProperties?.GatewayAddresses
                    .FirstOrDefault(ga =>
                        ga.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                return gateway?.Address.ToString();
            }
            catch
            {
                return null;
            }
        });
}
