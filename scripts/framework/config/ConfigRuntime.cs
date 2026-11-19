using GFramework.Core.Abstractions.Utility;
using GFramework.Game.Abstractions.Config;

namespace Bladesmith2048.scripts.framework.config;

/// <summary>
///     配置读取入口：持有初始化后的配置注册表，业务层不接触字符串表名。
/// </summary>
/// <remarks>
///     由 <c>ConfigModule</c> 初始化后注册进架构。源生成器会为每个 schema 生成强类型表扩展
///     （例如 <c>Registry.GetSampleTable()</c>），业务侧优先用这些扩展读取；
///     只有需要绕过强类型包装时才直接拿 <see cref="Registry" />。
/// </remarks>
public sealed class ConfigRuntime : IUtility
{
    private readonly IConfigRegistry _registry;

    /// <summary>
    ///     创建配置读取入口。
    /// </summary>
    public ConfigRuntime(IConfigRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    ///     获取底层配置注册表。
    /// </summary>
    /// <remarks>
    ///     可直接使用源生成器提供的强类型表扩展（如 <c>Registry.GetSampleTable()</c>）。
    /// </remarks>
    public IConfigRegistry Registry => _registry;
}
