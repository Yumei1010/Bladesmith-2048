using Bladesmith2048.scripts.game.chessboard;

namespace Bladesmith2048.Tests;

/// <summary>
///     2048 棋盘规则单元测试：移动/合并/生成/终局判定都是纯逻辑，不需要 Godot 运行时。
/// </summary>
public class ChessboardStateTests
{
    [Fact]
    public void Move_Left_MergesEqualPair()
    {
        var board = CreateBoard(
            2, 2, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0);

        var result = board.Move(MoveDirection.Left);

        Assert.True(result.Moved);
        Assert.Equal(4, result.GainedScore);
        Assert.Equal(new[] { 4, 0, 0, 0 }, Row(board, 0));
        Assert.Equal(4, board.Score);
    }

    [Fact]
    public void Move_Left_MergesEachPairOnlyOnce()
    {
        var board = CreateBoard(
            2, 2, 2, 2,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0);

        var result = board.Move(MoveDirection.Left);

        Assert.Equal(new[] { 4, 4, 0, 0 }, Row(board, 0));
        Assert.Equal(8, result.GainedScore);
    }

    [Fact]
    public void Move_Left_DoesNotChainMerge()
    {
        var board = CreateBoard(
            4, 4, 8, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0);

        board.Move(MoveDirection.Left);

        Assert.Equal(new[] { 8, 8, 0, 0 }, Row(board, 0));
    }

    [Fact]
    public void Move_Right_PushesToFarSide()
    {
        var board = CreateBoard(
            2, 0, 0, 2,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0);

        board.Move(MoveDirection.Right);

        Assert.Equal(new[] { 0, 0, 0, 4 }, Row(board, 0));
    }

    [Fact]
    public void Move_Up_MergesColumn()
    {
        var board = CreateBoard(
            2, 0, 0, 0,
            2, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0);

        board.Move(MoveDirection.Up);

        Assert.Equal(4, board[0]);
        Assert.Equal(0, board[4]);
    }

    [Fact]
    public void Move_Down_MergesColumnTowardBottom()
    {
        var board = CreateBoard(
            2, 0, 0, 0,
            0, 0, 0, 0,
            2, 0, 0, 0,
            2, 0, 0, 0);

        board.Move(MoveDirection.Down);

        // 自下往上依次是 2、2、2：底格合出 4，剩下的 2 落到它上一格
        Assert.Equal(4, board[12]);
        Assert.Equal(2, board[8]);
        Assert.Equal(0, board[4]);
        Assert.Equal(0, board[0]);
    }

    [Fact]
    public void Move_BlockedBoard_ReportsNoMovement()
    {
        var board = CreateBoard(
            2, 4, 2, 4,
            4, 2, 4, 2,
            2, 4, 2, 4,
            4, 2, 4, 2);

        var result = board.Move(MoveDirection.Left);

        Assert.False(result.Moved);
        Assert.Equal(0, result.GainedScore);
        Assert.Equal(0, board.Score);
        Assert.Empty(result.Transitions);
    }

    [Fact]
    public void Move_TwiceInSameDirection_SecondMoveReportsNoMovement()
    {
        var board = CreateBoard(
            2, 2, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0);

        var first = board.Move(MoveDirection.Left);
        var second = board.Move(MoveDirection.Left);

        Assert.True(first.Moved);
        Assert.Equal(new[] { 4, 0, 0, 0 }, Row(board, 0));
        Assert.False(second.Moved);
    }

    [Fact]
    public void IsGameOver_FalseWhenOnlyDiagonalMatches()
    {
        // 经典反例：上下左右都没有同值邻居，但斜向还有一对 2。
        // 4 向版本里这盘是终局，8 向版本里还能动。
        var board = CreateBoard(
            2, 4, 2, 4,
            4, 2, 4, 2,
            2, 4, 2, 4,
            4, 2, 4, 2);

        Assert.False(board.IsGameOver);
        Assert.False(board.HasWon);
    }

    [Fact]
    public void IsGameOver_TrueWhenNoDirectionCanMerge()
    {
        // 每个格子与 8 个邻居都不同值：8 个方向都动不了
        var board = CreateBoard(
            2, 8, 2, 8,
            4, 16, 4, 16,
            8, 2, 8, 2,
            16, 4, 16, 4);

        Assert.True(board.IsGameOver);
    }

    [Fact]
    public void IsGameOver_FalseWhenEmptyCellExists()
    {
        var board = CreateBoard(
            2, 4, 2, 4,
            4, 2, 4, 2,
            2, 4, 2, 4,
            4, 2, 4, 0);

        Assert.False(board.IsGameOver);
    }

    [Fact]
    public void IsGameOver_FalseWhenNeighbourMatches()
    {
        var board = CreateBoard(
            2, 2, 4, 8,
            4, 8, 16, 32,
            2, 4, 8, 16,
            4, 8, 16, 32);

        Assert.False(board.IsGameOver);
    }

    [Fact]
    public void HasWon_TrueAfterMergingInto2048()
    {
        var board = CreateBoard(
            1024, 1024, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0);

        board.Move(MoveDirection.Left);

        Assert.True(board.HasWon);
    }

    [Fact]
    public void TrySpawn_FillsExactlyOneEmptyCell()
    {
        var board = new ChessboardState();
        board.Reset();

        var spawned = board.TrySpawn(new FixedRandom(next: 3, nextDouble: 0.0));

        Assert.True(spawned);
        Assert.Equal(1, board.Cells.ToArray().Count(value => value != 0));
        Assert.Equal(2, board[3]);
    }

    [Fact]
    public void TrySpawn_UsesFourOnHighRoll()
    {
        var board = new ChessboardState();
        board.Reset();

        board.TrySpawn(new FixedRandom(nextDouble: 0.99));

        Assert.Equal(4, board[0]);
    }

    [Fact]
    public void TrySpawn_ReturnsFalseOnFullBoard()
    {
        var board = CreateBoard(
            2, 4, 2, 4,
            4, 2, 4, 2,
            2, 4, 2, 4,
            4, 2, 4, 8);

        Assert.False(board.TrySpawn(new FixedRandom()));
    }

    [Fact]
    public void Move_UpLeft_MergesAlongMainDiagonal()
    {
        var board = CreateBoard(
            0, 0, 0, 0,
            0, 2, 0, 0,
            0, 0, 2, 0,
            0, 0, 0, 0);

        var result = board.Move(MoveDirection.UpLeft);

        Assert.True(result.Moved);
        Assert.Equal(4, result.GainedScore);
        Assert.Equal(4, Cell(board, 0, 0));
        Assert.Equal(0, Cell(board, 1, 1));
        Assert.Equal(0, Cell(board, 2, 2));
    }

    [Fact]
    public void Move_DownRight_PushesToBottomRightCorner()
    {
        // 主对角线 [2, 2, 0, 2] 朝 (3,3) 挤压：底角合出 4，剩下的 2 落在它前一格
        var board = CreateBoard(
            2, 0, 0, 0,
            0, 2, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 2);

        board.Move(MoveDirection.DownRight);

        Assert.Equal(4, Cell(board, 3, 3));
        Assert.Equal(2, Cell(board, 2, 2));
        Assert.Equal(0, Cell(board, 1, 1));
        Assert.Equal(0, Cell(board, 0, 0));
    }

    [Fact]
    public void Move_UpRight_MergesAlongAntiDiagonal()
    {
        // (1,2) 与 (2,1) 同在 r + c = 3 的副对角线上，朝右上挤压后落在外角 (0,3)
        var board = CreateBoard(
            0, 0, 0, 0,
            0, 0, 2, 0,
            0, 2, 0, 0,
            0, 0, 0, 0);

        board.Move(MoveDirection.UpRight);

        Assert.Equal(4, Cell(board, 0, 3));
        Assert.Equal(0, Cell(board, 1, 2));
        Assert.Equal(0, Cell(board, 2, 1));
    }

    [Fact]
    public void Move_DownLeft_MergesTowardBottomLeft()
    {
        var board = CreateBoard(
            0, 0, 0, 0,
            0, 0, 2, 0,
            0, 2, 0, 0,
            0, 0, 0, 0);

        board.Move(MoveDirection.DownLeft);

        Assert.Equal(4, Cell(board, 3, 0));
    }

    [Fact]
    public void Move_UpRight_HandlesShortDiagonal()
    {
        // r + c = 4 的副对角线只有 3 格：(1,3)、(2,2)、(3,1)
        var board = CreateBoard(
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 2, 0,
            0, 2, 0, 0);

        board.Move(MoveDirection.UpRight);

        Assert.Equal(4, Cell(board, 1, 3));
        Assert.Equal(0, Cell(board, 2, 2));
        Assert.Equal(0, Cell(board, 3, 1));
    }

    [Fact]
    public void Move_UpLeft_SingleCellDiagonalCannotMove()
    {
        // (0,3) 所在的 r - c = -3 那条线上只有它自己，怎么挤都不动
        var board = CreateBoard(
            0, 0, 0, 2,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0);

        var result = board.Move(MoveDirection.UpLeft);

        Assert.False(result.Moved);
        Assert.Equal(2, Cell(board, 0, 3));
    }

    [Fact]
    public void Move_UpLeft_MergesDiagonalPairOnlyOnce()
    {
        var board = CreateBoard(
            2, 0, 0, 0,
            0, 2, 0, 0,
            0, 0, 2, 0,
            0, 0, 0, 2);

        var result = board.Move(MoveDirection.UpLeft);

        Assert.Equal(new[] { 4, 0, 0, 0 }, Row(board, 0));
        Assert.Equal(4, Cell(board, 1, 1));
        Assert.Equal(8, result.GainedScore);
        Assert.Equal(0, Cell(board, 2, 2));
        Assert.Equal(0, Cell(board, 3, 3));
    }

    [Fact]
    public void Move_Left_ReportsSlideAndMergeTransitions()
    {
        var board = CreateBoard(
            2, 2, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0);

        var result = board.Move(MoveDirection.Left);

        // 留下的一块原地不动，被吃掉的一块滑进同一格
        Assert.Contains(new TileTransition(0, 0, false), result.Transitions);
        Assert.Contains(new TileTransition(1, 0, true), result.Transitions);
    }

    [Fact]
    public void Move_Left_ReportsPureSlideTransition()
    {
        var board = CreateBoard(
            0, 0, 0, 2,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0);

        var result = board.Move(MoveDirection.Left);

        Assert.Equal([new TileTransition(3, 0, false)], result.Transitions);
    }

    [Fact]
    public void Move_UpLeft_ReportsTransitionsAlongDiagonal()
    {
        var board = CreateBoard(
            0, 0, 0, 0,
            0, 2, 0, 0,
            0, 0, 2, 0,
            0, 0, 0, 0);

        var result = board.Move(MoveDirection.UpLeft);

        // (1,1) 是留下的那块，(2,2) 被它吃掉，两块都滑向 (0,0)
        Assert.Contains(new TileTransition(5, 0, false), result.Transitions);
        Assert.Contains(new TileTransition(10, 0, true), result.Transitions);
    }

    [Fact]
    public void Reset_ClearsCellsAndScore()
    {
        var board = CreateBoard(
            2, 2, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0);
        board.Move(MoveDirection.Left);

        board.Reset();

        Assert.Equal(0, board.Score);
        Assert.All(board.Cells.ToArray(), value => Assert.Equal(0, value));
    }

    private static ChessboardState CreateBoard(params int[] cells)
    {
        Assert.Equal(ChessboardState.CellCount, cells.Length);

        return new ChessboardState(cells);
    }

    private static int Cell(ChessboardState board, int row, int col) =>
        board[row * ChessboardState.Size + col];

    private static int[] Row(ChessboardState board, int row) =>
        board.Cells.Slice(row * ChessboardState.Size, ChessboardState.Size).ToArray();

    /// <summary>可预测的随机源：下标固定为 <paramref name="next" />，掷点固定为 <paramref name="nextDouble" />。</summary>
    private sealed class FixedRandom(int next = 0, double nextDouble = 0.0) : Random
    {
        public override int Next(int maxValue) => maxValue <= 0 ? 0 : next % maxValue;

        public override double NextDouble() => nextDouble;
    }
}
