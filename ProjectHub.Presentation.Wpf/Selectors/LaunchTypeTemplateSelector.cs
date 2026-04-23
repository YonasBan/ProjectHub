using System.Windows;
using System.Windows.Controls;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Presentation.Wpf.Selectors;

/// <summary>
/// 启动类型模板选择器
/// 根据 LaunchType 选择对应的 DataTemplate
/// </summary>
public class LaunchTypeTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// 打开文件模板
    /// </summary>
    public DataTemplate? OpenFileTemplate { get; set; }

    /// <summary>
    /// 打开 EXE 模板
    /// </summary>
    public DataTemplate? OpenExeTemplate { get; set; }

    /// <summary>
    /// 打开网页模板
    /// </summary>
    public DataTemplate? OpenWebUrlTemplate { get; set; }

    /// <summary>
    /// 运行 CMD 命令模板
    /// </summary>
    public DataTemplate? OpenCmdTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is LaunchType launchType)
        {
            return launchType switch
            {
                LaunchType.OpenFile => OpenFileTemplate,
                LaunchType.OpenExe => OpenExeTemplate,
                LaunchType.OpenWebUrl => OpenWebUrlTemplate,
                LaunchType.OpenCmd => OpenCmdTemplate,
                _ => OpenFileTemplate
            };
        }

        return OpenFileTemplate;
    }
}
