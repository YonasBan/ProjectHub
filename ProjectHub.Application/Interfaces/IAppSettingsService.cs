namespace ProjectHub.Application.Interfaces;

/// <summary>
/// 应用配置服务接口 - 管理用户偏好设置（语言、主题等）
/// </summary>
public interface IAppSettingsService
{
    /// <summary>
    /// 加载应用配置
    /// </summary>
    AppSettings Load();

    /// <summary>
    /// 保存应用配置
    /// </summary>
    void Save(AppSettings settings);
}

/// <summary>
/// 应用配置模型
/// </summary>
public class AppSettings
{
    /// <summary>
    /// 支持的语言列表
    /// </summary>
    public static readonly IReadOnlyList<string> SupportedCultures = new[] { "zh-CN", "en-US" };

    /// <summary>
    /// 当前语言 (如 zh-CN, en-US)
    /// </summary>
    public string Language { get; set; } = "en-US";

    /// <summary>
    /// 当前主题 (Light / Dark)
    /// </summary>
    public string Theme { get; set; } = "Dark";
}
