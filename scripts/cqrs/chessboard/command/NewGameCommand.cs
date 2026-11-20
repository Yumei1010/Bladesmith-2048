using GFramework.Core.Command;
using Bladesmith2048.scripts.constants;
using Bladesmith2048.scripts.cqrs.chessboard.events;
using Bladesmith2048.scripts.framework.events;
using Bladesmith2048.scripts.model;

namespace Bladesmith2048.scripts.cqrs.chessboard.command;

/// <summary>
///     开新局：清空棋盘、放两个初始方块，并广播一次变化事件。
/// </summary>
/// <remarks>必须是异步命令：Godot 侧禁止同步 CQRS 入口。</remarks>
public sealed class NewGameCommand : AbstractAsyncCommand
{
    protected override Task OnExecuteAsync()
    {
        var model = this.GetModel<ChessboardModel>()!;
        model.NewGame();
        this.SendEvent(ChannelConstants.Gameplay,
            new ChessboardChangedEvent(model.Snapshot(), []));
        return Task.CompletedTask;
    }
}
