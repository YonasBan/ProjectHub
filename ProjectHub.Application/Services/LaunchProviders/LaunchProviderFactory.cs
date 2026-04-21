using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Services.LaunchProviders;

/// <summary>
/// 启动提供者工厂
/// 根据启动类型获取对应的提供者实现
/// </summary>
public class LaunchProviderFactory
{
    private readonly IEnumerable<ILaunchProvider> _providers;

    public LaunchProviderFactory(IEnumerable<ILaunchProvider> providers)
    {
        _providers = providers;
    }

    /// <summary>
    /// 获取指定类型的启动提供者
    /// </summary>
    /// <param name="type">启动类型</param>
    /// <returns>对应的启动提供者</returns>
    /// <exception cref="NotSupportedException">当找不到对应提供者时抛出</exception>
    public ILaunchProvider GetProvider(LaunchType type)
    {
        var provider = _providers.FirstOrDefault(p => p.Type == type);
        
        if (provider == null)
            throw new NotSupportedException($"不支持的启动类型: {type}");
            
        return provider;
    }
}
