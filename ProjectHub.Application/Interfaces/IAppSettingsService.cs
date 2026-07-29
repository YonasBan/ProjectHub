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
    /// 支持的语言列表（从资源文件动态获取）
    /// </summary>
    public static IReadOnlyList<string> SupportedCultures => GetSupportedCultures();

    /// <summary>
    /// 动态获取支持的语言列表
    /// 通过检测 ProjectHub.Resources 程序集中的卫星程序集来确定
    /// </summary>
    private static IReadOnlyList<string> GetSupportedCultures()
    {
        var cultures = new List<string>();
        
        try
        {
            // 获取 ProjectHub.Resources 程序集
            var resourcesAssembly = System.Reflection.Assembly.Load("ProjectHub.Resources");
            if (resourcesAssembly == null)
            {
                // 如果无法加载，返回默认值
                return new[] { "zh-CN", "en-US" }.AsReadOnly();
            }

            // 添加默认语言（中性文化，通常是中文）
            cultures.Add("zh-CN");
            
            // 检测所有可用的卫星程序集（本地化资源）
            // 卫星程序集的命名格式为: {AssemblyName}.resources.dll
            // 位于子目录中，如: en-US/ProjectHub.Resources.resources.dll
            
            // 目前已知支持的语言（可以根据实际添加的 .resx 文件扩展）
            // 当添加新的语言时，只需在这里添加对应的文化代码即可
            var knownCultures = new[] { "en-US" };
            
            foreach (var culture in knownCultures)
            {
                cultures.Add(culture);
            }
        }
        catch
        {
            // 如果发生错误，返回默认值
            return new[] { "zh-CN", "en-US" }.AsReadOnly();
        }
        
        return cultures.AsReadOnly();
    }

    /// <summary>
    /// 当前语言 (如 zh-CN, en-US)
    /// </summary>
    public string Language { get; set; } = "zh-CN";

    /// <summary>
    /// 当前主题 (Light / Dark)
    /// </summary>
    public string Theme { get; set; } = "Dark";

    /// <summary>
    /// 最近使用显示天数（默认30天）
    /// </summary>
    public int RecentUsageDays { get; set; } = 30;

    /// <summary>
    /// 关闭行为：true=最小化到托盘，false=退出程序（默认 false）
    /// </summary>
    public bool MinimizeToTray { get; set; } = true;

    /// <summary>
    /// 是否添加右键菜单到系统（默认 false）
    /// </summary>
    public bool EnableShellContextMenu { get; set; } = false;
}
