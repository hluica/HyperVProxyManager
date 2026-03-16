using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

using HyperVProxyManager.Utils;
using HyperVProxyManager.ViewModels;

using Wpf.Ui.Appearance;

namespace HyperVProxyManager.Tray;

public partial class TrayMenuWindow : Window
{
    public TrayMenuWindow(TrayViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();

        SystemThemeWatcher.Watch(this);

        // 订阅 Deactivated 事件：失去焦点时隐藏窗口
        Deactivated += (s, e) => Hide();
    }

    public void ShowAtCursor()
    {
        // 1. Toggle 逻辑
        if (IsVisible && Opacity > 0)
        {
            Hide();
            return;
        }

        // 2. 预备工作
        // 必须先 Show，WPF 才能创建 HWND 并获取当前屏幕的 DPI 上下文
        Opacity = 0;
        Show();
        // 强制触发布局更新，确保 ActualWidth/Height 是基于当前 DPI 计算出的正确逻辑值
        UpdateLayout();

        // --- 物理计算阶段开始 ---

        // 3. 获取鼠标物理坐标
        if (!NativeMethods.GetCursorPos(out var pt))
            return;

        // 4. 获取屏幕信息
        nint hMonitor = NativeMethods.MonitorFromPoint(pt, NativeMethods.MONITOR_DEFAULTTONEAREST);
        var monitorInfo = new NativeMethods.MONITORINFO
        {
            cbSize = Marshal.SizeOf<NativeMethods.MONITORINFO>()
        };

        if (!NativeMethods.GetMonitorInfo(hMonitor, ref monitorInfo))
            return;

        // 5. 获取精确 DPI
        _ = NativeMethods.GetDpiForMonitor(
            hMonitor,
            NativeMethods.MonitorDpiType.EffectiveDpi,
            out uint dpiX,
            out uint dpiY);

        double scaleX = dpiX / 96.0;
        double scaleY = dpiY / 96.0;

        // 6. 将 WPF 逻辑尺寸转换为 物理像素尺寸 (并在此时取整)
        // 使用 Round 配合 UseLayoutRounding 应该能得到非常接近整数的值
        int winWidthPhys = (int)Math.Round(ActualWidth * scaleX);
        int winHeightPhys = (int)Math.Round(ActualHeight * scaleY);

        // 7. 计算目标的物理坐标 (Anchor Logic)
        int cursorX = pt.X;
        int cursorY = pt.Y;

        // 屏幕工作区物理矩形
        var mRect = monitorInfo.rcMonitor;

        // 默认定位：鼠标右上方
        int targetLeft = cursorX;
        int targetTop = cursorY - winHeightPhys;

        // --- 翻转与限制逻辑 ---

        // 水平翻转：如果右侧超出屏幕，放左边
        if (targetLeft + winWidthPhys > mRect.Right)
            targetLeft = cursorX - winWidthPhys;

        // 垂直翻转：如果上方超出屏幕，放下面
        if (targetTop < mRect.Top)
            targetTop = cursorY;

        // Clamp 限制 (确保不跑出屏幕)
        // 左边界
        targetLeft = Math.Max(mRect.Left, targetLeft);
        // 右边界
        if (targetLeft + winWidthPhys > mRect.Right)
            targetLeft = mRect.Right - winWidthPhys;

        // 上边界
        targetTop = Math.Max(mRect.Top, targetTop);

        // 下边界
        if (targetTop + winHeightPhys > mRect.Bottom)
            targetTop = mRect.Bottom - winHeightPhys;

        // --- 应用位置阶段 ---

        // 8. 使用 SetWindowPos 进行原子化移动
        var helper = new WindowInteropHelper(this);

        _ = NativeMethods.SetWindowPos(
            helper.Handle,
            NativeMethods.HWND_TOP, // Z序：放在顶部
            targetLeft,
            targetTop,
            0, 0, // 忽略宽高设置，因为设置了 SWP_NOSIZE
            NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_NOZORDER
        );

        // 9. 显形与激活
        // 位置已经对齐，现在显示出来，视觉上不会有跳动
        Opacity = 1.0;

        // 强制前台激活
        _ = NativeMethods.SetForegroundWindow(helper.Handle);
        _ = Activate();
        _ = Focus();
    }
}
