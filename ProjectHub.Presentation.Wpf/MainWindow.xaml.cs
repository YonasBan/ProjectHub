using System.Diagnostics;
using System.Windows;
using ProjectHub.Application.ViewModels;
using ReactiveUI;

namespace ProjectHub.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// Note: This is a code-behind file that only handles view-specific logic.
    /// All business logic should be in the ViewModel (MVVM pattern).
    /// </summary>
    public partial class MainWindow :  ReactiveWindow<MainViewModel>
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            // Set DataContext to ViewModel for data binding
            DataContext = viewModel;
        }
    }
}