using ProjectHub.Application.Interfaces;
using ProjectHub.Application.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Windows;

namespace ProjectHub.Presentation.Wpf.Dialogs;

/// <summary>
/// 添加项目对话框
/// </summary>
public partial class AddProjectDialog : Window, IViewFor<AddProjectDialogViewModel>
{
    public AddProjectDialog()
    {
        InitializeComponent();

        // 处理窗口关闭按钮（X）
        Closing += (_, _) =>
        {
            if (ViewModel != null)
            {
                ViewModel.Cancel();
            }
        };

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

    public AddProjectDialogViewModel? ViewModel
    {
        get => (AddProjectDialogViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (AddProjectDialogViewModel?)value; }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(AddProjectDialogViewModel),
            typeof(AddProjectDialog)
        );
}
