namespace Bladesmith2048.scripts.game.chessboard;

/// <summary>
///     4×4 棋盘规则：挤压、合并、生成新方块、判定胜负。支持 4 正 + 4 斜共 8 个方向。
/// </summary>
/// <remarks>
///     纯逻辑，不引用 Godot，可以脱离引擎跑单元测试。
///     格子存成一维数组：<c>index = row * 4 + col</c>，0 表示空。
///     8 个方向共用同一套「线」抽象：一行、一列、一条对角线都是一条线。
/// </remarks>
public sealed class ChessboardState
{
    /// <summary>棋盘边长。</summary>
    public const int Size = 4;

    /// <summary>格子总数。</summary>
    public const int CellCount = Size * Size;

    /// <summary>达到该值即视为通关。</summary>
    public const int WinValue = 2048;

    /// <summary>
    ///     8 个方向的线表：<c>Lines[方向][第几条线][沿线第几格] = 格子下标</c>。
    /// </summary>
    /// <remarks>
    ///     offset 0 是该方向的尽头（方块要挤向的那一端）。表在类型初始化时算一次，
    ///     <see cref="Move" /> 里就只剩遍历线，没有任何方向判断。
    /// </remarks>
    private static readonly int[][][] Lines = BuildLines();

    private readonly int[] _cells = new int[CellCount];

    /// <summary>空格盘。</summary>
    public ChessboardState()
    {
    }

    /// <summary>按给定布局构造（测试与读档用），长度必须等于 <see cref="CellCount" />。</summary>
    public ChessboardState(ReadOnlySpan<int> cells)
    {
        if (cells.Length != CellCount)
            throw new ArgumentException($"需要 {CellCount} 个格子，实际 {cells.Length} 个", nameof(cells));

        cells.CopyTo(_cells);
    }

    /// <summary>当前得分（所有合并值之和）。</summary>
    public int Score { get; private set; }

    /// <summary>只读的格子视图，顺序与索引一致。</summary>
    public ReadOnlySpan<int> Cells => _cells;

    /// <summary>按索引取格子值，0 表示空。</summary>
    public int this[int index] => _cells[index];

    /// <summary>是否已出现 <see cref="WinValue" />。</summary>
    public bool HasWon => Array.IndexOf(_cells, WinValue) >= 0;

    /// <summary>
    ///     是否已无路可走：格子占满（满盘时「能移动」等价于「能合并」），且 8 个方向都没有相邻同值。
    /// </summary>
    public bool IsGameOver => Array.IndexOf(_cells, 0) < 0 && !CanMergeAnywhere();

    /// <summary>清空棋盘与分数。</summary>
    public void Reset()
    {
        Array.Clear(_cells);
        Score = 0;
    }

    /// <summary>
    ///     随机挑一个空格放新方块（90% 出 2，10% 出 4）。
    /// </summary>
    /// <returns>放成功返回 true；棋盘已满返回 false。</returns>
    public bool TrySpawn(Random random)
    {
        // 先收集空格下标再随机挑一个：空格通常很少，比反复重试稳定且不会抖
        Span<int> empties = stackalloc int[CellCount];
        var emptyCount = 0;
        for (var index = 0; index < CellCount; index++)
            if (_cells[index] == 0)
                empties[emptyCount++] = index;

        if (emptyCount == 0)
            return false;

        _cells[empties[random.Next(emptyCount)]] = random.NextDouble() < 0.9 ? 2 : 4;
        return true;
    }

    /// <summary>
    ///     朝指定方向移动一次。同一块方块在一次移动里最多合并一次。
    /// </summary>
    /// <remarks>每条线都做「读入压缩 → 合并 → 写回」，方向差异只在 <see cref="Lines" /> 里。</remarks>
    public MoveResult Move(MoveDirection direction)
    {
        var transitions = new List<TileTransition>();
        var moved = false;
        var gained = 0;

        foreach (var line in Lines[(int)direction])
            moved |= ApplyLine(line, transitions, ref gained);

        Score += gained;
        return moved ? new MoveResult(gained, transitions) : MoveResult.None;
    }

    /// <summary>
    ///     就地挤压并合并一条线：方块依次写回靠前的格子，遇到同值就翻倍合进上一格。
    /// </summary>
    /// <remarks>
    ///     写位置永远落在读位置之前（<c>write &lt;= 已读格数</c>），所以可以直接在原数组上覆盖，
    ///     不需要额外缓冲。<c>merged</c> 保证一块方块一回合只合并一次：
    ///     <c>[4,4,8]</c> 会变成 <c>[8,8]</c>，不会连锁成 16。
    /// </remarks>
    /// <returns>这条线上有没有方块动过。</returns>
    private bool ApplyLine(int[] line, List<TileTransition> transitions, ref int gained)
    {
        var write = 0;         // 下一个可写的格子
        var lastValue = 0;     // 上一格最终的值
        var lastIndex = -1;    // 上一格的格子下标
        var merged = false;    // 上一格这一回合已经合并过
        var moved = false;

        foreach (var index in line)
        {
            var value = _cells[index];
            if (value == 0)
                continue;

            if (!merged && value == lastValue)
            {
                // 和上一格同值：翻倍写回上一格，自己消失
                lastValue = value * 2;
                gained += lastValue;
                _cells[lastIndex] = lastValue;
                transitions.Add(new TileTransition(index, lastIndex, true));

                merged = true;
                moved = true;
                continue;
            }

            var target = line[write++];
            _cells[target] = value;
            transitions.Add(new TileTransition(index, target, false));

            lastValue = value;
            lastIndex = target;
            merged = false;
            moved |= target != index;
        }

        // 尾巴上的格子清空
        while (write < line.Length)
        {
            var index = line[write++];
            if (_cells[index] == 0)
                continue;

            _cells[index] = 0;
            moved = true;
        }

        return moved;
    }

    /// <summary>
    ///     生成 8 个方向的线表。
    /// </summary>
    /// <remarks>
    ///     每条线都从「尽头」出发：再沿方向走一步就出界的格子就是某条线的尽头。
    ///     从尽头逆着方向一路走回边界，就得到一条按「由近到远」排好序的线。
    ///     行、列、主副对角线都会被这同一条规则切出来，不需要按方向分别处理。
    /// </remarks>
    private static int[][][] BuildLines()
    {
        var directions = Enum.GetValues<MoveDirection>();
        var lines = new int[directions.Length][][];

        foreach (var direction in directions)
        {
            var (dr, dc) = VectorOf(direction);
            var perDirection = new List<int[]>();

            for (var row = 0; row < Size; row++)
                for (var col = 0; col < Size; col++)
                {
                    if (Inside(row + dr, col + dc))
                        continue;

                    var line = new List<int>();
                    var (r, c) = (row, col);
                    while (Inside(r, c))
                    {
                        line.Add(r * Size + c);
                        r -= dr;
                        c -= dc;
                    }

                    perDirection.Add(line.ToArray());
                }

            lines[(int)direction] = perDirection.ToArray();
        }

        return lines;
    }

    /// <summary>方向 → 单位向量（行向下为正，列向右为正）。</summary>
    private static (int Dr, int Dc) VectorOf(MoveDirection direction) => direction switch
    {
        MoveDirection.Up => (-1, 0),
        MoveDirection.Down => (1, 0),
        MoveDirection.Left => (0, -1),
        MoveDirection.Right => (0, 1),
        MoveDirection.UpLeft => (-1, -1),
        MoveDirection.UpRight => (-1, 1),
        MoveDirection.DownLeft => (1, -1),
        _ => (1, 1)
    };

    /// <summary>
    ///     是否还有任意方向能合并。
    /// </summary>
    /// <remarks>
    ///     满盘时线上相邻就是棋盘上相邻，所以「某条线上出现相邻同值」等价于「那个方向还能动」。
    ///     直接复用线表，不用再单独维护一套邻居偏移。
    /// </remarks>
    private bool CanMergeAnywhere()
    {
        foreach (var direction in Lines)
            foreach (var line in direction)
                for (var i = 1; i < line.Length; i++)
                    if (_cells[line[i]] == _cells[line[i - 1]])
                        return true;

        return false;
    }

    private static bool Inside(int row, int col) => row is >= 0 and < Size && col is >= 0 and < Size;
}
