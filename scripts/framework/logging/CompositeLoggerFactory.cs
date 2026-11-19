using GFramework.Core.Abstractions.Logging;
using GFramework.Core.Logging;

namespace Bladesmith2048.scripts.framework.logging;

/// <summary>
///     组合日志工厂，为每个日志器创建共享一组 Appender 的组合日志器。
/// </summary>
internal sealed class CompositeLoggerFactory : ILoggerFactory
{
    private readonly ILogAppender[] _appenders;

    /// <summary>
    ///     创建组合日志工厂。
    /// </summary>
    public CompositeLoggerFactory(ILogAppender[] appenders)
    {
        _appenders = appenders;
    }

    /// <summary>
    ///     获取或创建指定名称的组合日志器。
    /// </summary>
    /// <returns>组合日志器实例。</returns>
    public ILogger GetLogger(string name, LogLevel minLevel = LogLevel.Info)
    {
        return new CompositeLogger(name, minLevel, _appenders);
    }
}
