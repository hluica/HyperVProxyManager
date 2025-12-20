using Microsoft.Win32;

using HyperVProxyManager.Core;
using HyperVProxyManager.Models;

namespace HyperVProxyManager.Services;

public interface IProxyService
{
    ProxyState GetSystemProxy();
    bool SetSystemProxy(string address);
    bool DisableSystemProxy();
}

public class ProxyService : IProxyService
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

    public ProxyState GetSystemProxy()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
            if (key == null) return ProxyState.Empty;

            var enabled = (int?)key.GetValue("ProxyEnable", 0) ?? 0;
            var server = (string?)key.GetValue("ProxyServer", "") ?? "";

            return new ProxyState
            {
                IsEnabled = enabled == 1,
                ServerAddress = string.IsNullOrEmpty(server) ? "未设置" : server
            };
        }
        catch
        {
            return ProxyState.Empty;
        }
    }

    public bool SetSystemProxy(string address) => ApplyRegistrySettings(true, address);

    public bool DisableSystemProxy() => ApplyRegistrySettings(false, "");

    private static bool ApplyRegistrySettings(bool enable, string address)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            if (key == null) return false;

            key.SetValue("ProxyEnable", enable ? 1 : 0, RegistryValueKind.DWord);

            // 仅在启用时设置地址，禁用时清空以保持整洁
            if (enable)
            {
                key.SetValue("ProxyServer", address, RegistryValueKind.String);
            }
            else
            {
                key.SetValue("ProxyServer", "", RegistryValueKind.String);
            }

            // 刷新系统设置
            NativeMethods.InternetSetOption(IntPtr.Zero, NativeMethods.INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            NativeMethods.InternetSetOption(IntPtr.Zero, NativeMethods.INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
