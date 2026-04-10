using ReactiveUI;

namespace ProjectHub.Application.Interfaces;

/// <summary>
/// 主题服务接口 - 用于管理应用程序主题
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// 当前主题名称 (Light/Dark)
    /// </summary>
    string CurrentTheme { get; }

    /// <summary>
    /// 主题变更通知流
    /// </summary>
    IObservable<string> ThemeChanged { get; }

    /// <summary>
    /// 设置主题
    /// </summary>
    /// <param name="themeName">主题名称 (Light/Dark)</param>
    void SetTheme(string themeName);

    /// <summary>
    /// 切换主题 (Light -> Dark, Dark -> Light)
    /// </summary>
    void ToggleTheme();
}
