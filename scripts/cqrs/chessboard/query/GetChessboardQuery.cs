using GFramework.Core.Query;
using Bladesmith2048.scripts.game.chessboard;
using Bladesmith2048.scripts.model;

namespace Bladesmith2048.scripts.cqrs.chessboard.query;

/// <summary>
///     读当前棋盘快照：页面进场时用它做首帧渲染（事件只覆盖进场之后的变化）。
/// </summary>
/// <remarks>
///     必须是异步查询：Godot 侧禁止同步 CQRS 入口（<c>GodotArchitectureContext</c> 会抛
///     <c>GuardSyncCqrs</c>），同步处理器可能落在池线程上碰节点。
/// </remarks>
public sealed class GetChessboardQuery : AbstractAsyncQuery<ChessboardSnapshot>
{
    protected override Task<ChessboardSnapshot> OnDoAsync() =>
        Task.FromResult(this.GetModel<ChessboardModel>()!.Snapshot());
}
