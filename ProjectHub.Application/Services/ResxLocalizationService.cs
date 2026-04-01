using System.Globalization;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Resources;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Application.Services;

/// <summary>
/// 基于 .resx 文件的本地化服务实现
/// 使用 ResourceManager 读取嵌入式资源文件
/// </summary>
public class ResxLocalizationService : ILocalizationService
{
    private readonly ResourceManager _resourceManager;
    private readonly Subject<CultureInfo> _cultureChanged = new();
    private readonly IScheduler mainThreadScheduler;
    private CultureInfo _currentCulture;

    /// <summary>
    /// 初始化本地化服务
    /// </summary>
    /// <param name="baseName">资源文件的基本名称（不含文化信息和扩展名）</param>
    /// <param name="assembly">包含资源文件的程序集</param>
    public ResxLocalizationService(string baseName, System.Reflection.Assembly assembly, IScheduler mainThreadScheduler)
    {
        _resourceManager = new ResourceManager(baseName, assembly);
        _currentCulture = CultureInfo.CurrentUICulture;
        this.mainThreadScheduler = mainThreadScheduler;
    }

    /// <summary>
    /// 索引器 - 通过 key 获取翻译文本
    /// </summary>
    public string this[string key]
    {
        get
        {
            try
            {
                var value = _resourceManager.GetString(key, _currentCulture);
                // 找不到 key 时返回 [key] 方便调试
                return value ?? $"[{key}]";
            }
            catch (MissingManifestResourceException)
            {
                // 资源文件未找到时返回 [key]
                return $"[{key}]";
            }
        }
    }

    /// <summary>
    /// 获取格式化的翻译文本 (支持占位符)
    /// </summary>
    public string GetFormatted(string key, params object[] args)
    {
        var value = this[key];
        try
        {
            return string.Format(_currentCulture, value, args);
        }
        catch (FormatException)
        {
            // 格式化失败时返回原始值
            return value;
        }
    }

    /// <summary>
    /// 当前文化信息
    /// </summary>
    public CultureInfo CurrentCulture => _currentCulture;

    /// <summary>
    /// 文化变更通知流
    /// </summary>
    public IObservable<CultureInfo> CultureChanged => _cultureChanged;

    /// <summary>
    /// 异步设置文化
    /// </summary>
    public Task SetCultureAsync(CultureInfo culture)
    {
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));

        _currentCulture = culture;
        // 设置线程的文化信息
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Resources.Strings.Strings.Culture = culture;
        // 通知观察者文化已变更
        _cultureChanged.OnNext(culture);
        return Task.CompletedTask;
    }
}
