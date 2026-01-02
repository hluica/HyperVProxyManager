using System.Runtime.InteropServices;
using System.Security;

using HyperVProxyManager.Core;
using HyperVProxyManager.Models;

using Microsoft.Win32;

namespace HyperVProxyManager.Services;

public interface IProxyService
{
    ProxyState GetSystemProxy();
    OperationResult SetSystemProxy(string address);
    OperationResult DisableSystemProxy();
}

public class ProxyService : IProxyService
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

    public ProxyState GetSystemProxy()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
            if (key == null)
                return ProxyState.Empty;

            int enabled = (int?)key.GetValue("ProxyEnable", 0) ?? 0;
            string server = (string?)key.GetValue("ProxyServer", "") ?? "";

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

    public OperationResult SetSystemProxy(string address)
        => ApplyRegistrySettings(true, address);

    public OperationResult DisableSystemProxy()
        => ApplyRegistrySettings(false, "");

    private static OperationResult ApplyRegistrySettings(bool enable, string address)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            if (key == null)
                return new OperationResult(false, "无法创建或打开注册表项");

            key.SetValue("ProxyEnable", enable ? 1 : 0, RegistryValueKind.DWord);
            key.SetValue("ProxyServer", enable ? address : "", RegistryValueKind.String);

            // 刷新系统设置并收取返回值
            int result1 = NativeMethods.InternetSetOption(IntPtr.Zero, NativeMethods.INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            int result2 = NativeMethods.InternetSetOption(IntPtr.Zero, NativeMethods.INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);

            if (result1 == 0 || result2 == 0)
            {
                // 获取具体的 Win32 错误代码
                int errorCode = Marshal.GetLastPInvokeError();
                return new OperationResult(false, $"警告：Win32 操作异常，最后返回值 - {errorCode}");
            }

            string successMsg = enable
                ? $"操作完成：设置代理为 {address}"
                : "操作完成：禁用系统代理";

            return new OperationResult(true, successMsg);
        }
        catch (UnauthorizedAccessException)
        {
            return new OperationResult(false, "错误：权限不足。请尝试以管理员身份运行程序。");
        }
        catch (SecurityException)
        {
            return new OperationResult(false, "错误：安全异常，无法访问注册表。");
        }
        catch (Exception ex)
        {
            // 捕获所有其他未知错误，并包含异常信息以便调试
            return new OperationResult(false, $"错误：未预期的异常 - {ex.Message}");
        }
    }
}
