using Bladesmith2048.scripts.game.chessboard;

namespace Bladesmith2048.scripts.cqrs.chessboard.events;

/// <summary>
///     棋盘状态变化事件：移动成功或开新局时发出，页面据此重绘。
/// </summary>
/// <param name="Snapshot">变化之后的完整状态（分数等都在里面）。</param>
/// <param name="Transitions">每块方块的去向，页面用来做滑动动画。</param>
public sealed record ChessboardChangedEvent(
    ChessboardSnapshot Snapshot,
    IReadOnlyList<TileTransition> Transitions);
