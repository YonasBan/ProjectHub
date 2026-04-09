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

        this.WhenActivated(d =>
        {
            // ViewModel → DataContext
            this.WhenAnyValue(x => x.ViewModel)
                .BindTo(this, x => x.DataContext)
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
