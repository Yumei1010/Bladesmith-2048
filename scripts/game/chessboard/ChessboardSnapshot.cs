namespace Bladesmith2048.scripts.game.chessboard;

/// <summary>
///     棋盘对外的一次性快照：UI 只读它，不碰模型内部状态。
/// </summary>
/// <param name="Cells">16 个格子的值，0 表示空。</param>
/// <param name="Score">当前得分。</param>
/// <param name="Best">本局之前/之中的最高分。</param>
/// <param name="IsWon">是否已合成出 2048。</param>
/// <param name="IsGameOver">是否已无路可走。</param>
public readonly record struct ChessboardSnapshot(
    int[] Cells,
    int Score,
    int Best,
    bool IsWon,
    bool IsGameOver);
