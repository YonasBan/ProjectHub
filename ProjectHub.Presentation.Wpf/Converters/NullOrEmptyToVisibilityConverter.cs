using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProjectHub.Presentation.Wpf.Converters;

/// <summary>
/// 将 null 或空字符串转换为 Visibility 的转换器
/// null/空字符串 = Collapsed，有值 = Visible
/// </summary>
public class NullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isVisible = value switch
        {
            null => false,
            string s => !string.IsNullOrWhiteSpace(s),
            DateTime dt => true,
            _ => true
        };
        
        // 参数为 "Inverse" 时反转结果
        if (parameter?.ToString() == "Inverse")
            isVisible = !isVisible;
            
        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
