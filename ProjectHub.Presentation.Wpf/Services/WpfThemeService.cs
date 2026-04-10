using System.Reactive.Subjects;
using System.Windows;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Presentation.Wpf.Services;

/// <summary>
/// WPF 主题服务实现
/// </summary>
public class WpfThemeService : IThemeService
{
    private readonly Subject<string> _themeChanged = new();
    private string _currentTheme = "Dark";

    public string CurrentTheme => _currentTheme;

    public IObservable<string> ThemeChanged => _themeChanged;

    public void SetTheme(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName))
            throw new ArgumentException("主题名称不能为空", nameof(themeName));

        if (_currentTheme == themeName)
            return;

        _currentTheme = themeName;
        ApplyTheme(themeName);
        _themeChanged.OnNext(themeName);
    }

    public void ToggleTheme()
    {
        var newTheme = _currentTheme == "Dark" ? "Light" : "Dark";
        SetTheme(newTheme);
    }

    /// <summary>
    /// 应用主题资源字典
    /// </summary>
    private void ApplyTheme(string themeName)
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;

        // 移除旧的主题资源
        var oldTheme = app.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.OriginalString.Contains("Theme.xaml") == true);
        if (oldTheme != null)
        {
            app.Resources.MergedDictionaries.Remove(oldTheme);
        }

        // 添加新的主题资源
        var themeUri = new Uri($"/Themes/{themeName}Theme.xaml", UriKind.Relative);
        var newTheme = new ResourceDictionary { Source = themeUri };
        app.Resources.MergedDictionaries.Add(newTheme);
    }
}
