using Godot;
using GFramework.Core.Abstractions.Controller;
using GFramework.Core.Abstractions.Localization;
using GFramework.Core.Extensions;
using GFramework.Game.Abstractions.Enums;
using GFramework.Game.Abstractions.UI;
using GFramework.Godot.Extensions;
using GFramework.Godot.SourceGenerators.Abstractions;
using GFramework.Godot.SourceGenerators.Abstractions.UI;
using Bladesmith2048.global;
using Bladesmith2048.scripts.constants;
using Bladesmith2048.scripts.core.ui;
using Bladesmith2048.scripts.cqrs.chessboard.command;
using Bladesmith2048.scripts.cqrs.chessboard.command.input;
using Bladesmith2048.scripts.cqrs.chessboard.events;
using Bladesmith2048.scripts.cqrs.chessboard.query;
using Bladesmith2048.scripts.enums.ui;
using Bladesmith2048.scripts.framework.command;
using Bladesmith2048.scripts.framework.events;
using Bladesmith2048.scripts.game.chessboard;
using Bladesmith2048.scripts.ui.chessboard.widgets.grid;
using Bladesmith2048.scripts.ui.chessboard.widgets.tile;

namespace Bladesmith2048.scripts.ui.chessboard;

/// <summary>
///     棋盘页面：把输入翻译成命令，把事件翻成方块渲染。
/// </summary>
/// <remarks>
///     页面不持有棋盘状态：读数走 <c>GetChessboardQuery</c>，变化靠 <see cref="ChessboardChangedEvent" />。
///     输入同时支持方向键/WASD 与鼠标拖拽（触屏走同一套拖拽逻辑）。
/// </remarks>
[Log]
[ContextAware]
[AutoUiPage(nameof(UiKey.Chessboard), nameof(UiLayer.Page))]
public partial class ChessboardPage : Control, IController, IUiPageBehaviorProvider, ISimpleUiPage
{
    /// <summary>拖拽超过这个距离（像素）才算一次滑动，避免误触。</summary>
    private const float SwipeThreshold = 48f;

    /// <summary>方块模板场景。</summary>
    private const string TileScenePath = "res://scenes/ui/chessboard/base/tile.tscn";

    /// <summary>滑动时长（秒）。短一点才不拖手。</summary>
    private const double SlideDuration = 0.12;

    /// <summary>合并/生成时的缩放时长。</summary>
    private const double PopDuration = 0.09;

    [GetSystem] private IUiRouter _uiRouter = null!;
    [GetSystem] private ILocalizationManager _localization = null!;

    [GetNode] private Grid Board = null!;

    /// <summary>方块层：方块不挂在格子里，而是这层上按像素定位，才能滑过去。</summary>
    [GetNode] private Control TileLayer = null!;
    [GetNode] private Label ScoreLabel = null!;
    [GetNode] private Label BestLabel = null!;
    [GetNode] private Button RestartButton = null!;
    [GetNode] private Button BackButton = null!;
    [GetNode] private Control GameOverOverlay = null!;
    [GetNode] private Label GameOverLabel = null!;
    [GetNode] private Button RetryButton = null!;

    private readonly Tile?[] _tiles = new Tile?[ChessboardState.CellCount];

    /// <summary>每格的像素坐标（方块左上角），随窗口尺寸重算。</summary>
    private readonly Vector2[] _positions = new Vector2[ChessboardState.CellCount];

    private PackedScene? _tileScene;
    private ChessboardChangedEvent? _pending;
    private Vector2? _dragStart;
    private bool _animating;

    public override void _Ready()
    {
        __InjectContextBindings_Generated();
        __InjectGetNodes_Generated();

        RestartButton.Pressed += OnNewGame;
        RetryButton.Pressed += OnNewGame;
        BackButton.Pressed += OnBack;
        GameOverOverlay.Visible = false;

        // 状态就绪前不接输入：_Ready 里先关掉，ReadyAsync 渲染完再开
        SetProcessInput(false);

        // 窗口尺寸变了要把格子坐标重算一遍，否则方块会错位
        Resized += OnResized;

        // 页面被 router 移除时自动退订，避免事件打到已释放的节点
        this.RegisterEvent(ChannelConstants.Gameplay, (ChessboardChangedEvent e) => QueueRender(e))
            .UnRegisterWhenNodeExitTree(this);

        _ = ReadyAsync();
    }

    /// <summary>方向键/WASD 走正方向，Q/E/Z/C 走斜向，R 重开，鼠标拖拽滑动（8 向）。</summary>
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Echo: false } key)
        {
            var direction = key.Keycode switch
            {
                Key.Left or Key.A => MoveDirection.Left,
                Key.Right or Key.D => MoveDirection.Right,
                Key.Up or Key.W => MoveDirection.Up,
                Key.Down or Key.S => MoveDirection.Down,
                Key.Q => MoveDirection.UpLeft,
                Key.E => MoveDirection.UpRight,
                Key.Z => MoveDirection.DownLeft,
                Key.C => MoveDirection.DownRight,
                _ => (MoveDirection?)null
            };

            if (direction is { } value)
                TryMove(value);
            else if (key.Keycode == Key.R)
                OnNewGame();
            return;
        }

        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left } button)
        {
            if (button.Pressed)
                _dragStart = button.Position;
            else if (_dragStart is { } start)
            {
                _dragStart = null;
                ResolveSwipe(start, button.Position);
            }
        }
    }

    /// <summary>架构就绪后取首帧快照；事件只覆盖进场之后的变化。</summary>
    private async Task ReadyAsync()
    {
        try
        {
            await GameEntryPoint.Architecture.WaitUntilReadyAsync();

            // 交回主线程再碰节点（await 之后可能已在池线程）
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            BackButton.Text = _localization.GetText("common", "chessboard.back");
            RestartButton.Text = _localization.GetText("common", "chessboard.restart");
            RetryButton.Text = _localization.GetText("common", "chessboard.restart");

            var snapshot = await this.SendQueryAsync(new GetChessboardQuery());

            // 异步查询的续体可能不在主线程，再等一帧把执行权交回主线程
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            _tileScene = GD.Load<PackedScene>(TileScenePath);
            LayoutPositions();
            Settle(snapshot, []);
            SetProcessInput(true);
            _log.Debug("Chessboard page ready");
        }
        catch (Exception ex)
        {
            _log.Error("Chessboard page initialize failed", ex);
        }
    }

    /// <summary>格子像素坐标：直接问 Grid 的 16 个格子，不写死布局数字。</summary>
    private void LayoutPositions()
    {
        var cells = Board.Cells;
        for (var index = 0; index < ChessboardState.CellCount; index++)
            _positions[index] = cells[index].GlobalPosition - TileLayer.GlobalPosition;
    }

    /// <summary>窗口变化时重算坐标，并把现有方块瞬间贴回格子（不播动画）。</summary>
    private void OnResized()
    {
        LayoutPositions();
        for (var index = 0; index < ChessboardState.CellCount; index++)
            if (_tiles[index] is { } tile)
                tile.Position = _positions[index];
    }

    /// <summary>
    ///     播一次移动动画：每块方块从出发格滑到终点格，滑完再按快照重建。
    /// </summary>
    /// <remarks>
    ///     合并的两块会同时滑到同一格，随后被合并结果替换——所以位移结束时统一重建，
    ///     不试图在动画中维护「哪个实例代表哪一格」，避免链式移动时身份错乱。
    /// </remarks>
    private void PlayMove(ChessboardSnapshot snapshot, IReadOnlyList<TileTransition> transitions)
    {
        var slides = new List<(Tile Tile, int To)>();
        foreach (var transition in transitions)
        {
            if (transition.From == transition.To)
                continue;
            if (_tiles[transition.From] is not { } tile)
                continue;

            slides.Add((tile, transition.To));
        }

        if (slides.Count == 0)
        {
            Settle(snapshot, transitions);
            return;
        }

        _animating = true;
        var tween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        foreach (var (tile, to) in slides)
            tween.TweenProperty(tile, "position", _positions[to], SlideDuration);

        tween.Finished += () =>
        {
            _animating = false;
            Settle(snapshot, transitions);
        };
    }

    /// <summary>
    ///     按快照重建方块：合并结果弹一下、新生成的方块从 0 放大。
    /// </summary>
    private void Settle(ChessboardSnapshot snapshot, IReadOnlyList<TileTransition> transitions)
    {
        foreach (var tile in _tiles)
            tile?.QueueFree();

        Array.Clear(_tiles);

        for (var index = 0; index < ChessboardState.CellCount; index++)
        {
            var value = snapshot.Cells[index];
            if (value <= 0)
                continue;

            var tile = CreateTile(index, value);
            var merged = false;
            var targeted = false;
            foreach (var transition in transitions)
            {
                if (transition.To != index)
                    continue;

                targeted = true;
                merged |= transition.Merged;
            }

            if (merged)
                Pop(tile);
            else if (!targeted)
                Spawn(tile);
        }

        UpdateLabels(snapshot);
    }

    private Tile CreateTile(int index, int value)
    {
        var tile = _tileScene!.Instantiate<Tile>();
        TileLayer.AddChild(tile);

        tile.Size = Board.Cells[index].Size;
        tile.PivotOffset = tile.Size / 2f;   // 缩放围绕中心，而不是左上角
        tile.Position = _positions[index];
        tile.SetValue(value);

        _tiles[index] = tile;
        return tile;
    }

    /// <summary>新生成的方块：从 0 缩放出来。</summary>
    private void Spawn(Tile tile)
    {
        tile.Scale = Vector2.Zero;
        CreateTween()
            .TweenProperty(tile, "scale", Vector2.One, SlideDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }

    /// <summary>合并结果：先弹大一点再回到原尺寸。</summary>
    private void Pop(Tile tile)
    {
        var tween = CreateTween();
        tween.TweenProperty(tile, "scale", new Vector2(1.15f, 1.15f), PopDuration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        tween.TweenProperty(tile, "scale", Vector2.One, PopDuration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);
    }

    private void UpdateLabels(ChessboardSnapshot snapshot)
    {
        ScoreLabel.Text = $"{_localization.GetText("common", "chessboard.score")} {snapshot.Score}";
        BestLabel.Text = $"{_localization.GetText("common", "chessboard.best")} {snapshot.Best}";
        GameOverLabel.Text = _localization.GetText("common",
            snapshot.IsWon ? "chessboard.win" : "chessboard.game_over");
        GameOverOverlay.Visible = snapshot.IsGameOver;
    }

    /// <summary>
    ///     事件可能来自命令的异步续体（池线程），节点操作统一回主线程执行。
    /// </summary>
    /// <remarks>多次事件只保留最新一帧，避免同一帧内重复重绘。</remarks>
    private void QueueRender(ChessboardChangedEvent @event)
    {
        _pending = @event;
        Callable.From(FlushRender).CallDeferred();
    }

    private void FlushRender()
    {
        if (_pending is not { } @event)
            return;

        _pending = null;
        PlayMove(@event.Snapshot, @event.Transitions);
    }

    private void TryMove(MoveDirection direction)
    {
        if (_animating)
            return;

        this.SendCommandDetached(new MoveCommand(new MoveCommandInput { Direction = direction }), _log);
        GetViewport().SetInputAsHandled();
    }

    /// <summary>
    ///     把拖拽方向映射成 8 向：与主轴的夹角小于 22.5°（tan ≈ 0.414）算正向，否则算斜向。
    /// </summary>
    private void ResolveSwipe(Vector2 start, Vector2 end)
    {
        var delta = end - start;
        if (delta.Length() < SwipeThreshold)
            return;

        var horizontal = delta.X > 0 ? MoveDirection.Right : MoveDirection.Left;
        var vertical = delta.Y > 0 ? MoveDirection.Down : MoveDirection.Up;

        var minor = Mathf.Min(Mathf.Abs(delta.X), Mathf.Abs(delta.Y));
        var major = Mathf.Max(Mathf.Abs(delta.X), Mathf.Abs(delta.Y));
        TryMove(minor / major < 0.414f ? (Mathf.Abs(delta.X) > Mathf.Abs(delta.Y) ? horizontal : vertical)
            : Diagonal(horizontal, vertical));
    }

    private static MoveDirection Diagonal(MoveDirection horizontal, MoveDirection vertical) =>
        (horizontal, vertical) switch
        {
            (MoveDirection.Right, MoveDirection.Down) => MoveDirection.DownRight,
            (MoveDirection.Right, _) => MoveDirection.UpRight,
            (_, MoveDirection.Down) => MoveDirection.DownLeft,
            _ => MoveDirection.UpLeft
        };

    private void OnNewGame()
    {
        if (_animating)
            return;

        this.SendCommandDetached(new NewGameCommand(), _log);
        GameOverOverlay.Visible = false;
    }

    private void OnBack() => _ = BackAsync();

    private async Task BackAsync()
    {
        try
        {
            await _uiRouter.PopAsync();
        }
        catch (Exception ex)
        {
            _log.Error("返回主菜单失败", ex);
        }
    }
}
