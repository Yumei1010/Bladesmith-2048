using GFramework.Core.Abstractions.Command;
using GFramework.Core.Abstractions.Logging;
using GFramework.Core.Abstractions.Rule;

namespace Bladesmith2048.scripts.framework.command;

/// <summary>
///     命令分发的统一入口：Godot 信号/输入回调里没法 await，
///     直接 <c>_ = SendCommandAsync(...)</c> 会把命令内部的异常变成未观察异常，
///     日志里只剩一条沉默的失败。
/// </summary>
public static class CommandDispatchExtensions
{
    /// <summary>
    ///     发送命令但不阻塞调用方；命令抛异常时写日志，而不是无声吞掉。
    /// </summary>
    public static void SendCommandDetached(this IContextAware owner, IAsyncCommand command, ILogger logger)
    {
        _ = DispatchAsync(owner, command, logger);
    }

    private static async Task DispatchAsync(IContextAware owner, IAsyncCommand command, ILogger logger)
    {
        try
        {
            await owner.SendCommandAsync(command);
        }
        catch (Exception exception)
        {
            logger.Error($"命令执行失败：{command.GetType().Name}", exception);
        }
    }
}
