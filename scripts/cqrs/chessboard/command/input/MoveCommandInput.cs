using Bladesmith2048.scripts.game.chessboard;
using GFramework.Cqrs.Abstractions.Cqrs.Command;

namespace Bladesmith2048.scripts.cqrs.chessboard.command.input;

/// <summary>移动命令入参。</summary>
public sealed class MoveCommandInput : ICommandInput
{
    /// <summary>移动方向。</summary>
    public MoveDirection Direction { get; set; }
}
