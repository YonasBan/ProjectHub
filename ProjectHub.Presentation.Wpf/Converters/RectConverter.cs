using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProjectHub.Presentation.Wpf.Converters
{
    /// <summary>
    /// 将宽度和高度转换为 Rect 的转换器，用于圆角裁剪
    /// </summary>
    public class RectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double width && values[1] is double height)
            {
                return new Rect(0, 0, width, height);
            }
            return Rect.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
