using GFramework.Core.Command;
using Bladesmith2048.scripts.constants;
using Bladesmith2048.scripts.cqrs.chessboard.command.input;
using Bladesmith2048.scripts.cqrs.chessboard.events;
using Bladesmith2048.scripts.framework.events;
using Bladesmith2048.scripts.model;

namespace Bladesmith2048.scripts.cqrs.chessboard.command;

/// <summary>
///     走一步：移动 → 生成新方块 → 广播 <see cref="ChessboardChangedEvent" />。
/// </summary>
/// <remarks>
///     被顶死（没有方块动）时只做判定，不发事件，避免 UI 白重绘。
///     必须是异步命令：Godot 侧禁止同步 CQRS 入口。
/// </remarks>
public sealed class MoveCommand(MoveCommandInput input) : AbstractAsyncCommand<MoveCommandInput>(input)
{
    protected override Task OnExecuteAsync(MoveCommandInput input)
    {
        var model = this.GetModel<ChessboardModel>()!;
        var result = model.Move(input.Direction);
        if (!result.Moved)
            return Task.CompletedTask;

        this.SendEvent(ChannelConstants.Gameplay,
            new ChessboardChangedEvent(model.Snapshot(), result.Transitions));
        return Task.CompletedTask;
    }
}
