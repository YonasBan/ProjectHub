using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProjectHub.Presentation.Wpf.Converters;

/// <summary>
/// Bool 转 Visibility 转换器
/// True -> Visible, False -> Collapsed
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility v)
            return v == Visibility.Visible;
        return false;
    }
}
