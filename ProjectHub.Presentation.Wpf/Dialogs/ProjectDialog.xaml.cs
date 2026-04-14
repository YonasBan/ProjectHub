using ProjectHub.Application.Interfaces;
using ProjectHub.Application.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Windows;

namespace ProjectHub.Presentation.Wpf.Dialogs;

/// <summary>
/// 项目对话框 - 支持添加和编辑项目
/// </summary>
public partial class ProjectDialog : Window, IViewFor<ProjectDialogViewModel>
{
    public ProjectDialog()
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

    public ProjectDialogViewModel? ViewModel
    {
        get => (ProjectDialogViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (ProjectDialogViewModel?)value; }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(ProjectDialogViewModel),
            typeof(ProjectDialog)
        );
}
