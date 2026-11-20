using Bladesmith2048.scripts.game.chessboard;
using GFramework.Core.Model;

namespace Bladesmith2048.scripts.model;

/// <summary>
///     棋盘模型：持有棋盘状态与最高分。
/// </summary>
/// <remarks>
///     只有命令能改它（<c>MoveCommand</c> / <c>NewGameCommand</c>）；
///     UI 通过 <c>GetChessboardQuery</c> 读数、通过事件收到变化通知。
///     构造时可传 <see cref="Random" />，方便复现一局。
/// </remarks>
public sealed class ChessboardModel(Random? random = null) : AbstractModel
{
    private readonly Random _random = random ?? Random.Shared;

    /// <summary>棋盘规则状态。</summary>
    public ChessboardState State { get; } = new();

    /// <summary>本会话最高分（原型阶段不落盘）。</summary>
    public int BestScore { get; private set; }

    /// <inheritdoc />
    protected override void OnInit() => NewGame();

    /// <summary>开新局：清空棋盘并放两个初始方块。</summary>
    public void NewGame()
    {
        State.Reset();
        State.TrySpawn(_random);
        State.TrySpawn(_random);
    }

    /// <summary>
    ///     朝指定方向移动；真的动了才生成新方块并刷新最高分。
    /// </summary>
    public MoveResult Move(MoveDirection direction)
    {
        var result = State.Move(direction);
        if (!result.Moved)
            return result;

        State.TrySpawn(_random);
        BestScore = Math.Max(BestScore, State.Score);
        return result;
    }

    /// <summary>导出当前状态的只读快照。</summary>
    public ChessboardSnapshot Snapshot() =>
        new(State.Cells.ToArray(), State.Score, BestScore, State.HasWon, State.IsGameOver);
}
