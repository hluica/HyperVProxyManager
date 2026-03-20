using System.Runtime.InteropServices;
using System.Security;

using HyperVProxyManager.Models;
using HyperVProxyManager.Services.Interfaces;
using HyperVProxyManager.Utils;

using Microsoft.Win32;

using Windows.Win32;

namespace HyperVProxyManager.Services;


public class ProxyService : IProxyService
{
    private const string REGISTRY_KEY_PATH = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

    public ProxyState GetSystemProxy()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY_PATH, false);
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
            using var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH);
            if (key == null)
                return new OperationResult(false, "无法创建或打开注册表项");

            key.SetValue("ProxyEnable", enable ? 1 : 0, RegistryValueKind.DWord);
            key.SetValue("ProxyServer", enable ? address : "", RegistryValueKind.String);

            // 刷新系统设置并收取返回值
            bool result1;
            bool result2;

            unsafe
            {
                // default 作为 HINTERNET 句柄
                // null 作为 void* 缓冲区指针
                result1 = PInvoke.InternetSetOption(
                    default,
                    PInvoke.INTERNET_OPTION_SETTINGS_CHANGED,
                    null,
                    0);

                result2 = PInvoke.InternetSetOption(
                    default,
                    PInvoke.INTERNET_OPTION_REFRESH,
                    null,
                    0);
            }

            if (result1 == false || result2 == false)
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
