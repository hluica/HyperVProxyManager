using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HyperVProxyManager.Utils;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value switch
        {
            true => Visibility.Visible,
            _ => Visibility.Collapsed
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Visible;
}
