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
    /// 初始化默认主题 - 应在 Application 启动完成后调用
    /// </summary>
    void InitializeTheme();
}
