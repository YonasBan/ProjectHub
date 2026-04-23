using System.Globalization;
using System.IO;
using System.Text.Json;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Infrastructure.Services;

/// <summary>
/// JSON 配置文件实现 - 保存到 %LocalAppData%/ProjectHub/settings.json
/// </summary>
public class JsonAppSettingsService : IAppSettingsService
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ProjectHub");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private AppSettings? _cachedSettings;

    public AppSettings Load()
    {
        if (_cachedSettings != null)
        {
            return _cachedSettings;
        }

        if (!File.Exists(ConfigPath))
        {
            _cachedSettings = CreateDefaultSettings();
            return _cachedSettings;
        }

        try
        {
            var json = File.ReadAllText(ConfigPath);
            _cachedSettings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? CreateDefaultSettings();
            return _cachedSettings;
        }
        catch
        {
            _cachedSettings = CreateDefaultSettings();
            return _cachedSettings;
        }
    }

    public void Save(AppSettings settings)
    {
        if (!Directory.Exists(ConfigDir))
        {
            Directory.CreateDirectory(ConfigDir);
        }

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(ConfigPath, json);
        _cachedSettings = settings;
    }

    /// <summary>
    /// 创建默认配置：主题 Dark，语言优先使用系统语言
    /// </summary>
    private static AppSettings CreateDefaultSettings()
    {
        var systemCulture = CultureInfo.CurrentUICulture.Name;
        var language = AppSettings.SupportedCultures.Contains(systemCulture, StringComparer.OrdinalIgnoreCase)
            ? systemCulture
            : "en-US";

        return new AppSettings
        {
            Language = language,
            Theme = "Dark"
        };
    }
}
