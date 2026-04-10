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
    /// 初始化默认主题 - 应在 Application 启动完成后调用
    /// </summary>
    public void InitializeTheme()
    {
        ApplyTheme(_currentTheme);
    }

    /// <summary>
    /// 应用主题资源字典
    /// </summary>
    private void ApplyTheme(string themeName)
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;

        // 在 UI 线程执行
        app.Dispatcher.Invoke(() =>
        {
            // 移除旧的主题资源（匹配 DarkTheme.xaml 或 LightTheme.xaml）
            var oldTheme = app.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains("Theme.xaml") == true);
            if (oldTheme != null)
            {
                app.Resources.MergedDictionaries.Remove(oldTheme);
            }

            // 添加新的主题资源 - 使用 Pack URI 格式
            var assemblyName = typeof(WpfThemeService).Assembly.GetName().Name;
            var themeUri = new Uri($"pack://application:,,,/{assemblyName};component/Themes/{themeName}Theme.xaml");
            var newTheme = new ResourceDictionary { Source = themeUri };
            app.Resources.MergedDictionaries.Insert(0, newTheme);
        });
    }
}
