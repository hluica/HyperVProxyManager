using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HyperVProxyManager.Views;

public partial class DashboardPage : UserControl
{
    public DashboardPage()
        => InitializeComponent();

    private async void OnCopyContentButtonClick(object sender, RoutedEventArgs e)
    {
        // 确保 sender 是 Button，且 Content 是 SymbolIcon
        if (sender is Wpf.Ui.Controls.Button btn && btn.Content is Wpf.Ui.Controls.SymbolIcon icon)
        {
            // 从 Tag 中提取我们绑定的文本数据
            if (btn.Tag is not string textToCopy || string.IsNullOrWhiteSpace(textToCopy))
            {
                return;
            }

            // 1. 记录按钮的原始状态，以便稍后恢复
            var originalSymbol = icon.Symbol;
            var originalForeground = icon.Foreground;

            // 暂时禁用按钮，防止用户疯狂连击
            btn.IsEnabled = false;

            try
            {
                // 2. 尝试复制内容到剪贴板（包含重试机制）
                bool isSuccess = await TryCopyToClipboardAsync(textToCopy);

                // 3. 根据结果提供视觉反馈
                if (isSuccess)
                {
                    icon.Symbol = Wpf.Ui.Controls.SymbolRegular.Checkmark24; // 成功打勾
                    icon.Foreground = Brushes.MediumSeaGreen;
                }
                else
                {
                    icon.Symbol = Wpf.Ui.Controls.SymbolRegular.Warning24;   // 失败警告
                    icon.Foreground = Brushes.IndianRed;
                }

                // 4. 保持反馈状态 2 秒钟
                await Task.Delay(2000);
            }
            finally
            {
                // 5. 恢复原始状态
                icon.Symbol = originalSymbol;
                icon.Foreground = originalForeground;
                btn.IsEnabled = true;
            }
        }
    }

    private static async Task<bool> TryCopyToClipboardAsync(string text)
    {
        for (int i = 0; i < 5; i++)
        {
            try
            {
                Clipboard.SetText(text);
                return true;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                await Task.Delay(20);
            }
            catch (Exception)
            {
                break;
            }
        }
        return false;
    }
}
