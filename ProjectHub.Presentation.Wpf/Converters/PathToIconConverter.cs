using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ProjectHub.Presentation.Wpf.Converters;

/// <summary>
/// 将文件路径转换为图标 ImageSource
/// </summary>
public class PathToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string filePath || string.IsNullOrWhiteSpace(filePath))
            return null;

        try
        {
            var ext = System.IO.Path.GetExtension(filePath).ToLowerInvariant();

            if (ext == ".ico")
            {
                // ICO 文件直接加载
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            else if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".bmp")
            {
                // 图片文件直接加载（支持本地物理路径和相对路径）
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                
                // 判断是绝对路径还是相对路径
                if (Path.IsPathRooted(filePath) && File.Exists(filePath))
                {
                    // 绝对路径
                    bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                }
                else
                {
                    // 相对路径（如 Images/explorer.png）
                    bitmap.UriSource = new Uri(filePath, UriKind.Relative);
                }
                
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            else if (ext == ".exe" || ext == ".dll" || System.IO.File.Exists(filePath))
            {
                // 从可执行文件或 DLL 提取图标
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                if (icon != null)
                {
                    return Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
            else if (System.IO.Directory.Exists(filePath))
            {
                // 文件夹：使用 shell32.dll 中的默认文件夹图标（索引 4）
                try
                {
                    var shell32Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
                    var icon = System.Drawing.Icon.ExtractIcon(shell32Path, 4);
                    if (icon != null)
                    {
                        return Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                    }
                }
                catch
                {
                    // 如果提取失败，返回 null
                }
            }
        }
        catch
        {
            // 忽略错误，返回 null
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
