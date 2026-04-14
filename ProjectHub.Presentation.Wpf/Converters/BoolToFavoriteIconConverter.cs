using System.Globalization;
using System.Windows.Data;

namespace ProjectHub.Presentation.Wpf.Converters;

/// <summary>
/// Bool 转 Favorite 图标路径转换器
/// True -> like.svg (已收藏), False -> unlike.svg (未收藏)
/// </summary>
public class BoolToFavoriteIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? "/Icons/like.svg" : "/Icons/unlike.svg";
        return "/Icons/unlike.svg";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
