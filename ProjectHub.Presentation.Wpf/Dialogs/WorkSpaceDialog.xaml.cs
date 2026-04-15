using ProjectHub.Application.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Windows;

namespace ProjectHub.Presentation.Wpf.Dialogs;

/// <summary>
/// 工作空间对话框 - 支持添加和编辑工作空间
/// </summary>
public partial class WorkSpaceDialog : Window, IViewFor<WorkSpaceDialogViewModel>
{
    public WorkSpaceDialog()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            // ViewModel → DataContext
            this.WhenAnyValue(x => x.ViewModel)
                .BindTo(this, x => x.DataContext)
                .DisposeWith(d);
        });
    }

    public WorkSpaceDialogViewModel? ViewModel
    {
        get => (WorkSpaceDialogViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (WorkSpaceDialogViewModel?)value; }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(WorkSpaceDialogViewModel),
            typeof(WorkSpaceDialog)
        );
}
