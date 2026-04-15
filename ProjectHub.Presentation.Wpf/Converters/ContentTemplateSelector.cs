using ProjectHub.Application.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ProjectHub.Presentation.Wpf.Converters;

/// <summary>
/// 内容模板选择器 - 根据数据类型选择不同的 DataTemplate
/// </summary>
public class ContentTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// 项目卡片模板
    /// </summary>
    public DataTemplate? ProjectTemplate { get; set; }

    /// <summary>
    /// 工作空间卡片模板
    /// </summary>
    public DataTemplate? WorkSpaceTemplate { get; set; }

    /// <summary>
    /// 默认模板
    /// </summary>
    public DataTemplate? DefaultTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            ProjectViewModel => ProjectTemplate ?? DefaultTemplate,
            WorkSpaceViewModel => WorkSpaceTemplate ?? DefaultTemplate,
            _ => DefaultTemplate
        };
    }
}
