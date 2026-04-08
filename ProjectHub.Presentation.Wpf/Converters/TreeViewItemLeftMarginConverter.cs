using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ProjectHub.Presentation.Wpf.Converters
{
    public class TreeViewItemLeftMarginConverter : IValueConverter
    {
        public double Indent { get; set; } = 12; // 每一层缩进的宽度

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as TreeViewItem;
            if (item == null) return new Thickness(0);

            // 递归计算当前项在树中的深度
            return new Thickness(GetDepth(item) * Indent, 0, 0, 0);
        }

        private int GetDepth(TreeViewItem item)
        {
            int depth = 0;
            DependencyObject parent = VisualTreeHelper.GetParent(item);

            while (parent != null && !(parent is TreeView))
            {
                if (parent is TreeViewItem)
                {
                    depth++;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return depth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
