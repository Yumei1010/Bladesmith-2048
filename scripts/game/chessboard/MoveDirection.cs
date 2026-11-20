namespace Bladesmith2048.scripts.game.chessboard;

/// <summary>
///     一次移动的方向：棋盘上的所有方块朝该方向挤压并合并。
/// </summary>
/// <remarks>
///     斜向也是「沿对角线挤压」：每条对角线上同值的方块合并，格子不会离开自己所在的那条对角线。
/// </remarks>
public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right,

    /// <summary>朝左上角。</summary>
    UpLeft,

    /// <summary>朝右上角。</summary>
    UpRight,

    /// <summary>朝左下角。</summary>
    DownLeft,

    /// <summary>朝右下角。</summary>
    DownRight
}
