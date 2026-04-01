using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProjectHub.Application.Localization;
using ProjectHub.Application.ViewModels;
using Splat;

namespace ProjectHub.Presentation.Wpf.Controls;

public partial class HeaderView : UserControl
{
    private readonly LocalizedStrings _l = Locator.Current.GetService<LocalizedStrings>()!;
    public LocalizedStrings L => _l;

    public HeaderView()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty SearchKeywordProperty =
        DependencyProperty.Register(nameof(SearchKeyword), typeof(string),
            typeof(HeaderView), new FrameworkPropertyMetadata(string.Empty));

    public string SearchKeyword
    {
        get => (string)GetValue(SearchKeywordProperty);
        set => SetValue(SearchKeywordProperty, value);
    }

    private void SearchInput_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.SearchKeyword = SearchInput.Text;
        }
    }
}
