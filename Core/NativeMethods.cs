using System.Runtime.InteropServices;

namespace HyperVProxyManager.Core;

internal static class NativeMethods
{
    [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

    // 通知系统注册表设置已更改，需重新读取
    public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
    // 刷新代理数据
    public const int INTERNET_OPTION_REFRESH = 37;
}
