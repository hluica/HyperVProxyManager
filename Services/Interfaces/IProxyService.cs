using HyperVProxyManager.Utils;
using HyperVProxyManager.Models;

namespace HyperVProxyManager.Services.Interfaces;

public interface IProxyService
{
    ProxyState GetSystemProxy();
    OperationResult SetSystemProxy(string address);
    OperationResult DisableSystemProxy();
}
