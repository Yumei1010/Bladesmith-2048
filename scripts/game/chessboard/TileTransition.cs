namespace Bladesmith2048.scripts.game.chessboard;

/// <summary>
///     一次移动里某块方块的去向，供表现层做位移动画。
/// </summary>
/// <param name="From">出发格下标。</param>
/// <param name="To">终点格下标；合并时两块的目的地相同。</param>
/// <param name="Merged">true 表示这块被吃掉：滑到终点后消失，值并入留下的那块。</param>
public readonly record struct TileTransition(int From, int To, bool Merged);
