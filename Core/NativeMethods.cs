using System.Runtime.InteropServices;

namespace HyperVProxyManager.Core;

// 封装操作结果
public record OperationResult(bool IsSuccess, string Message);

// 执行外部操作
internal static partial class NativeMethods
{
    // --- WinINet.dll ---

    // 通知系统注册表设置已更改，需重新读取
    public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
    // 刷新代理数据
    public const int INTERNET_OPTION_REFRESH = 37;

    [LibraryImport("wininet.dll", EntryPoint = "InternetSetOptionW", SetLastError = true)]
    public static partial int InternetSetOption(
         IntPtr hInternet,
         int dwOption,
         IntPtr lpBuffer,
         int dwBufferLength);

    // --- User32.dll ---

    public const uint MONITOR_DEFAULTTONEAREST = 2;

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetCursorPos(out POINT lpPoint);

    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetForegroundWindow(IntPtr hWnd);

    // --- SHCore.dll ---

    public enum MonitorDpiType
    {
        EffectiveDpi = 0,
        AngularDpi = 1,
        RawDpi = 2,
        Default = EffectiveDpi
    }

    [LibraryImport("shcore.dll", SetLastError = true)]
    public static partial int GetDpiForMonitor(
        IntPtr hmonitor,
        MonitorDpiType dpiType,
        out uint dpiX,
        out uint dpiY);
}
