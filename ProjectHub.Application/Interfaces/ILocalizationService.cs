using System.Globalization;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using ReactiveUI;

namespace ProjectHub.Application.Interfaces;

/// <summary>
/// 本地化服务接口 - 用于多语言支持
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// 索引器 - 通过 key 获取翻译文本
    /// </summary>
    string this[string key] { get; }

    /// <summary>
    /// 获取格式化的翻译文本 (支持占位符)
    /// </summary>
    string GetFormatted(string key, params object[] args);

    /// <summary>
    /// 当前文化信息
    /// </summary>
    CultureInfo CurrentCulture { get; }

    /// <summary>
    /// 异步设置文化
    /// </summary>
    Task SetCultureAsync(CultureInfo culture);

    /// <summary>
    /// 文化变更通知流 - ViewModel 订阅此流以在语言切换时刷新 UI
    /// </summary>
    IObservable<CultureInfo> CultureChanged { get; }
}

/// <summary>
/// 本地化服务扩展方法
/// </summary>
public static class LocalizationExtensions
{
    /// <summary>
    /// 格式化翻译文本的便捷方法
    /// </summary>
    public static string Fmt(this ILocalizationService loc, string key, params object[] args)
        => string.Format(loc[key], args);
}
