using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

using HyperVProxyManager.ViewModels;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;
using Windows.Win32.UI.WindowsAndMessaging;

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace HyperVProxyManager.Tray;

public partial class TrayMenuWindow : Window
{
    public TrayMenuWindow(TrayViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();

        SystemThemeWatcher.Watch(this);

        // 订阅 Deactivated 事件：失去焦点时隐藏窗口
        Deactivated += (_, _) => Hide();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        _ = WindowBackdrop.ApplyBackdrop(this, WindowBackdropType.Acrylic);
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
        if (!PInvoke.GetCursorPos(out var pt))
            return;

        // 4. 获取屏幕信息
        var hMonitor = PInvoke.MonitorFromPoint(pt, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        var monitorInfo = new MONITORINFO
        {
            cbSize = (uint)Marshal.SizeOf<MONITORINFO>()
        };

        if (!PInvoke.GetMonitorInfo(hMonitor, ref monitorInfo))
            return;

# pragma warning disable CA1416

        // 5. 获取精确 DPI
        // 由于本程序已验证无法在 Windows 11 以外的系统上运行，因此此处可以忽略平台兼容性警告 (CA1416)
        _ = PInvoke.GetDpiForMonitor(
            hMonitor,
            MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI,
            out uint dpiX,
            out uint dpiY);

# pragma warning restore CA1416

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
        if (targetLeft + winWidthPhys > mRect.right)
            targetLeft = cursorX - winWidthPhys;

        // 垂直翻转：如果上方超出屏幕，放下面
        if (targetTop < mRect.top)
            targetTop = cursorY;

        // Clamp 限制 (确保不跑出屏幕)
        // 左边界与右边界
        targetLeft = Math.Max(mRect.left, targetLeft);
        if (targetLeft + winWidthPhys > mRect.right)
            targetLeft = mRect.right - winWidthPhys;

        // 上边界与下边界
        targetTop = Math.Max(mRect.top, targetTop);
        if (targetTop + winHeightPhys > mRect.bottom)
            targetTop = mRect.bottom - winHeightPhys;

        // --- 应用位置阶段 ---

        // 8. 使用 SetWindowPos 进行原子化移动
        var helper = new WindowInteropHelper(this);

        _ = PInvoke.SetWindowPos(
            new HWND(helper.Handle),
            new HWND(0), // Z序：放在顶部
            targetLeft,
            targetTop,
            0, 0, // 忽略宽高设置，因为设置了 SWP_NOSIZE
            SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER
        );

        // 9. 显形与激活
        // 位置已经对齐，现在显示出来，视觉上不会有跳动
        Opacity = 1.0;

        // 强制前台激活
        _ = PInvoke.SetForegroundWindow(new HWND(helper.Handle));
        _ = Activate();
        _ = Focus();
    }
}
