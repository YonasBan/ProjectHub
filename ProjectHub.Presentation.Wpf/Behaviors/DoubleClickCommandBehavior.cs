using Microsoft.Xaml.Behaviors;
using ReactiveUI;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectHub.Presentation.Wpf.Behaviors;

/// <summary>
/// 双击执行命令的 Behavior
/// </summary>
public class DoubleClickCommandBehavior : Behavior<Border>
{
    /// <summary>
    /// 要执行的命令
    /// </summary>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ReactiveCommand<Unit, Unit>),
            typeof(DoubleClickCommandBehavior));

    public ReactiveCommand<Unit, Unit>? Command
    {
        get => (ReactiveCommand<Unit, Unit>?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        base.OnDetaching();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 检查是否是双击
        if (e.ClickCount == 2)
        {
            // 执行命令
            Command?.Execute().Subscribe();
            
            // 标记事件已处理，防止冒泡
            e.Handled = true;
        }
    }
}
