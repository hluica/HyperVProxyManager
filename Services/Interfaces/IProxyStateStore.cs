using System.ComponentModel;

using HyperVProxyManager.Models;

namespace HyperVProxyManager.Services.Interfaces;

public interface IProxyStateStore : INotifyPropertyChanged
{
    string HostIpAddress { get; }
    ProxyState CurrentProxy { get; }
    bool IsLoading { get; }
    Task RefreshAsync();
}
