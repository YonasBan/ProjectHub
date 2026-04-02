using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Localization;
using ProjectHub.Application.ViewModels;
using Splat;

namespace ProjectHub.Presentation.Wpf.Controls;

public partial class HeaderView : UserControl
{
    private readonly LocalizedStrings _l = Locator.Current.GetService<LocalizedStrings>()!;
    private readonly ILocalizationService? _localizationService = Locator.Current.GetService<ILocalizationService>();
    public LocalizedStrings L => _l;

    public HeaderView()
    {
        InitializeComponent();
        
        // 订阅语言改变事件，刷新 UI
        _localizationService?.CultureChanged.Subscribe(_ => 
        {
            // 触发属性变更通知（通过重新设置 DataContext 或触发事件）
            // 由于 XAML 绑定使用的是 RelativeSource，需要手动刷新
            var binding = BindingOperations.GetBindingExpression(this, DataContextProperty);
            binding?.UpdateTarget();
        });
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
