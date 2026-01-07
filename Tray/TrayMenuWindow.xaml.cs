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

        // 关键：失去焦点时隐藏窗口，模拟菜单行为
        Deactivated += (s, e) => Hide();
    }

    public void ShowAtCursor()
    {
        // 1. 获取鼠标物理位置
        if (NativeMethods.GetCursorPos(out var point))
        {
            // 2. 考虑 DPI 缩放 (这里简化处理，假设为 100% 或 WPF 自动适配)
            // 如果需要精确适配多屏 DPI，需使用 PresentationSource 获取转换矩阵

            // 基础定位：鼠标左上方
            double targetLeft = point.X - ActualWidth;
            double targetTop = point.Y - ActualHeight;

            // 边缘检测：防止超出屏幕左侧或顶侧
            if (targetLeft < 0)
                targetLeft = point.X;
            if (targetTop < 0)
                targetTop = point.Y;

            // 简单防溢出 (右/下边界可以使用 SystemParameters.WorkArea 判断)

            Left = targetLeft;
            Top = targetTop;
        }

        // 3. 显示窗口
        Show();

        // 4. 强制前台并获取焦点，确保能触发 Deactivated
        var helper = new WindowInteropHelper(this);
        NativeMethods.SetForegroundWindow(helper.Handle);
        Activate();
        Focus();
    }
}
