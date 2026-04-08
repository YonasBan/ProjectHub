using ProjectHub.Application.Interfaces;
using ProjectHub.Application.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
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
public partial class InputDialog : Window, IViewFor<CreateFolderDialogViewModel>
{
    public InputDialog()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            // ViewModel → DataContext
            this.WhenAnyValue(x => x.ViewModel)
                .BindTo(this, x => x.DataContext)
                .DisposeWith(d);

            // 等 ViewModel 赋值后再订阅命令
            this.WhenAnyValue(x => x.ViewModel)
                .WhereNotNull()
                .Subscribe(vm =>
                {
                    vm.CancelCommand
                        .Subscribe(_ => this.Close())
                        .DisposeWith(d);
                    vm.ConfirmCommand
                        .Subscribe(_ => this.Close())
                        .DisposeWith(d);
                })
                .DisposeWith(d);
        });
    }
    public CreateFolderDialogViewModel? ViewModel
    {
        get => (CreateFolderDialogViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (CreateFolderDialogViewModel?)value; }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(CreateFolderDialogViewModel),
            typeof(InputDialog)
           );

}
