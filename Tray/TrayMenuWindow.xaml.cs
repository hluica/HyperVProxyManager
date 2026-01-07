using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

using HyperVProxyManager.Core;
using HyperVProxyManager.ViewModels;

namespace HyperVProxyManager.Tray;

public partial class TrayMenuWindow : Window
{
    public TrayMenuWindow(TrayViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // 订阅 Deactivated 事件：失去焦点时隐藏窗口
        Deactivated += (s, e) => Hide();
    }

    public void ShowAtCursor()
    {
        // 1. Toggle 行为
        if (IsVisible)
        {
            Hide();
            return;
        }

        // 2. 获取鼠标物理坐标
        if (!NativeMethods.GetCursorPos(out var pt))
            return;

        // 3. 找到鼠标所在显示器
        nint hMonitor = NativeMethods.MonitorFromPoint(
            pt,
            NativeMethods.MONITOR_DEFAULTTONEAREST);

        var monitorInfo = new NativeMethods.MONITORINFO
        {
            cbSize = Marshal.SizeOf<NativeMethods.MONITORINFO>()
        };

        if (!NativeMethods.GetMonitorInfo(hMonitor, ref monitorInfo))
            return;

        // 4. 获取该显示器 DPI
        NativeMethods.GetDpiForMonitor(
            hMonitor,
            NativeMethods.MonitorDpiType.EffectiveDpi,
            out uint dpiX,
            out uint dpiY);

        // 5. 转换为 WPF 逻辑坐标
        double scaleX = dpiX / 96.0;
        double scaleY = dpiY / 96.0;

        Point cursor = new(
        pt.X / scaleX,
        pt.Y / scaleY);

        // 6. 显示窗口并完成布局
        ShowActivated = true;
        Show();
        UpdateLayout();

        Rect screenBounds = new(
            monitorInfo.rcMonitor.Left / scaleX,
            monitorInfo.rcMonitor.Top / scaleY,
            (monitorInfo.rcMonitor.Right - monitorInfo.rcMonitor.Left) / scaleX,
            (monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top) / scaleY);

        double winW = ActualWidth;
        double winH = ActualHeight;

        // 7. Anchor 模型：默认菜单在鼠标右上方
        double left = cursor.X;
        double top = cursor.Y - winH;

        // 水平方向：右侧空间不足 → 翻转到左侧
        if (left + winW > screenBounds.Right)
            left = cursor.X - winW;

        // 垂直方向：上方空间不足 → 翻转到下方
        if (top < screenBounds.Top)
            top = cursor.Y;

        // 8. Clamp 到工作区
        left = Math.Max(
            screenBounds.Left,
            Math.Min(
                left,
                screenBounds.Right - winW));

        top = Math.Max(
            screenBounds.Top,
            Math.Min(
                top,
                screenBounds.Bottom - winH));


        Left = left;
        Top = top;

        // 9. 强制前台与激活（托盘窗口必需）
        nint hwnd = new WindowInteropHelper(this).Handle;
        NativeMethods.SetForegroundWindow(hwnd);

        Activate();
        Focus();
    }

}
