namespace Bladesmith2048.scripts.game.chessboard;

/// <summary>
///     一次移动的结果。
/// </summary>
/// <param name="GainedScore">本次合并得到的分数。</param>
/// <param name="Transitions">每块方块的去向（空 = 这次没动），表现层据此做滑动动画。</param>
public readonly record struct MoveResult(int GainedScore, IReadOnlyList<TileTransition> Transitions)
{
    /// <summary>什么都没发生（该方向被顶死）。</summary>
    public static MoveResult None => new(0, []);

    /// <summary>是否真的有方块动了。没有任何去向就说明被顶死，调用方不该生成新方块。</summary>
    public bool Moved => Transitions.Count > 0;
}
