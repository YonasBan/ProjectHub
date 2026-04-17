using System.Windows;

namespace ProjectHub.Presentation.Wpf.Converters;

/// <summary>
/// 绑定代理类
/// 用于解决 ContextMenu 等脱离视觉树元素的绑定问题
/// </summary>
public class BindingProxy : Freezable
{
    /// <summary>
    /// 数据依赖属性
    /// </summary>
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(
            nameof(Data),
            typeof(object),
            typeof(BindingProxy),
            new PropertyMetadata(null));

    /// <summary>
    /// 数据对象
    /// </summary>
    public object? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <summary>
    /// 创建实例（Freezable 必需）
    /// </summary>
    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }
}
