using System.Runtime.InteropServices;

namespace HyperVProxyManager.Core;

// 封装操作结果
public record OperationResult(bool IsSuccess, string Message);

// 执行外部操作
internal static partial class NativeMethods
{
    [LibraryImport(
        "wininet.dll",
        EntryPoint = "InternetSetOptionW",
        SetLastError = true,
        StringMarshalling = StringMarshalling.Utf16)]
    public static partial int InternetSetOption(
        IntPtr hInternet,
        int dwOption,
        IntPtr lpBuffer,
        int dwBufferLength);

    // 通知系统注册表设置已更改，需重新读取
    public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
    // 刷新代理数据
    public const int INTERNET_OPTION_REFRESH = 37;
}
