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
                ServerAddress = string.IsNullOrEmpty(server) ? l10n.ProxyNotSet : server
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
                return new OperationResult(false, l10n.ResultErrOpenRegisty);

            key.SetValue("ProxyEnable", enable ? 1 : 0, RegistryValueKind.DWord);
            key.SetValue("ProxyServer", enable ? address : "", RegistryValueKind.String);

            // 刷新系统设置并收取返回值
            bool result1, result2;
            int error1 = 0, error2 = 0;

            unsafe
            {
                // default 作为 HINTERNET 句柄；null 作为 void* 缓冲区指针
                // INTERNET_OPTION_SETTINGS_CHANGED - 通知系统注册表配置已更改
                result1 = PInvoke.InternetSetOption(default, PInvoke.INTERNET_OPTION_SETTINGS_CHANGED, null, 0);
                if (!result1)
                    error1 = Marshal.GetLastPInvokeError();

                // INTERNET_OPTION_REFRESH - 立即刷新代理设置
                result2 = PInvoke.InternetSetOption(default, PInvoke.INTERNET_OPTION_REFRESH, null, 0);
                if (!result2)
                    error2 = Marshal.GetLastPInvokeError();
            }

            return (result1, result2, enable) switch
            {
                (true, true, true) => new OperationResult(true, string.Format(l10n.ResultSccEnable, address)),
                (true, true, false) => new OperationResult(true, l10n.ResultSccDisable),
                (false, true, _) => new OperationResult(false, string.Format(l10n.ResultErrSettingChanged, error1)),
                (true, false, _) => new OperationResult(false, string.Format(l10n.ResultErrRefresh, error2)),
                (false, false, _) => new OperationResult(false, string.Format(l10n.ResultErrBoth, error1, error2))
            };
        }
        catch (UnauthorizedAccessException)
        {
            return new OperationResult(false, l10n.ResultErrUnauthorized);
        }
        catch (SecurityException)
        {
            return new OperationResult(false, l10n.ResultErrSecurity);
        }
        catch (Exception ex)
        {
            // 捕获所有其他未知错误，并包含异常信息以便调试
            return new OperationResult(false, string.Format(l10n.ResultErrUnexpected, ex.Message));
        }
    }
}
