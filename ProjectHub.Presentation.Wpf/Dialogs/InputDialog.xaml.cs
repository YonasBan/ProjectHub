using ProjectHub.Application.Interfaces;
using System.Windows;

namespace ProjectHub.Presentation.Wpf.Dialogs;

/// <summary>
/// 输入对话框
/// 支持创建文件夹、编辑名称等多种场景
/// 
/// DDD 设计要点:
/// - 视图层代码，与平台相关
/// - ViewModel 由外部传入
/// - 支持键盘快捷键（Enter 确认，Escape 取消）
/// </summary>
public partial class InputDialog : Window
{
    public InputDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// ViewModel 属性（用于 DataContext 绑定）
    /// </summary>
    public object? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(object),
            typeof(InputDialog),
            new PropertyMetadata(null, OnViewModelChanged));

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InputDialog dialog)
        {
            dialog.DataContext = e.NewValue;
        }
    }

    /// <summary>
    /// 显示对话框并返回结果（通用版本）
    /// </summary>
    public async Task<DialogResult<object>> ShowDialogAsync(Window owner)
    {
        Owner = owner;
        ShowDialog();

        if (DataContext is IDialogViewModel<object> vm)
        {
            var result =  vm.Result;
            return new DialogResult<object>(result.Confirmed, result.Value);
        }

        return new DialogResult<object>(false);
    }
}
