using System.ComponentModel;
using System.Windows;
using ProjectHub.Application.Interfaces;
using ReactiveUI;

namespace ProjectHub.Presentation.Wpf.Localization;

/// <summary>
/// WPF XAML 绑定的本地化辅助类
/// 用于在 XAML 中直接访问本地化资源
/// </summary>
public class Localizer : ReactiveObject, IDisposable
{
    private static Localizer? _instance;
    public static Localizer Instance => _instance ??= new();

    private readonly ILocalizationService _loc;
    private IDisposable? _subscription;
    private bool _disposed;

    /// <summary>
    /// 静态构造函数 - 初始化实例
    /// </summary>
    static Localizer()
    {
        // 实例将在首次访问时创建
    }

    /// <summary>
    /// 私有构造函数 - 从 DI 容器获取服务
    /// </summary>
    public Localizer()
    {
        // 注意：这个构造函数主要用于 XAML 设计时支持
        // 运行时应该通过 App.Current.Services 获取服务
        _loc = App.Current is App app 
            ? app.GetService<ILocalizationService>() 
            : throw new InvalidOperationException("ILocalizationService not registered");

        // 订阅文化变更通知，使用 Dispatcher 确保在主线程上更新 UI
        _subscription = _loc.CultureChanged
            .Subscribe(_ =>
            {
                if (System.Windows.Application.Current?.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.RaisePropertyChanged(nameof(Item));
                    });
                }
                else
                {
                    this.RaisePropertyChanged(nameof(Item));
                }
            });
    }

    /// <summary>
    /// 索引器访问器 - 用于 XAML 绑定
    /// </summary>
    public Indexer Item => new(_loc);

    /// <summary>
    /// 设置当前文化（用于测试或代码切换）
    /// </summary>
    public async Task SetCultureAsync(System.Globalization.CultureInfo culture)
    {
        await _loc.SetCultureAsync(culture);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _subscription?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// 索引器类 - 用于 XAML 中的索引访问语法
/// </summary>
public class Indexer : ReactiveObject, IDisposable
{
    private readonly ILocalizationService _loc;
    private IDisposable? _subscription;
    private bool _disposed;

    public Indexer(ILocalizationService loc)
    {
        _loc = loc;
        
        // 订阅文化变更通知，刷新所有属性
        _subscription = _loc.CultureChanged
            .Subscribe(_ =>
            {
                if (System.Windows.Application.Current?.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.RaisePropertyChanged(string.Empty);
                    });
                }
                else
                {
                    this.RaisePropertyChanged(string.Empty);
                }
            });
    }

    /// <summary>
    /// 索引器 - 通过 key 获取翻译文本
    /// </summary>
    public string this[string key] => _loc[key];

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _subscription?.Dispose();
            _disposed = true;
        }
    }
}
