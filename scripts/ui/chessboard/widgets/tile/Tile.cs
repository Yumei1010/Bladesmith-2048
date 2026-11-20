using Godot;
using GFramework.Core.SourceGenerators.Abstractions.Logging;
using GFramework.Core.SourceGenerators.Abstractions.Rule;
using GFramework.Godot.SourceGenerators.Abstractions;

namespace Bladesmith2048.scripts.ui.chessboard.widgets.tile;

/// <summary>
///     一个方块：背景色与数字随数值变化。
/// </summary>
/// <remarks>
///     由棋盘页面复用实例（空格只隐藏、不销毁），所以颜色必须每帧可覆盖。
/// </remarks>
[Log]
[ContextAware]
public partial class Tile : Control
{
    private StyleBoxFlat? _styleCache;

    public override void _Ready()
    {
        __InjectGetNodes_Generated();
    }

    /// <summary>
    ///     设置显示数值：文本、背景色、字号一起换。
    /// </summary>
    public void SetValue(int value)
    {
        ScoreTextLabel.Text = value.ToString();
        ScoreTextLabel.AddThemeFontSizeOverride("normal_font_size", value >= 1024 ? 40 : 64);
        ScoreTextLabel.AddThemeColorOverride("default_color", TextColorFor(value));

        BackgroundPanel.AddThemeStyleboxOverride("panel", StyleFor(value));
    }

    /// <summary>数值 → 背景样式（复用同一份 style 副本，避免每次新建）。</summary>
    private StyleBoxFlat StyleFor(int value)
    {
        if (_styleCache == null)
        {
            _styleCache = (StyleBoxFlat)BackgroundPanel.GetThemeStylebox("panel").Duplicate();
            _styleCache.CornerRadiusTopLeft = 16;
            _styleCache.CornerRadiusTopRight = 16;
            _styleCache.CornerRadiusBottomLeft = 16;
            _styleCache.CornerRadiusBottomRight = 16;
            _styleCache.CornerDetail = 20;
        }

        _styleCache.BgColor = ColorFor(value);
        return _styleCache;
    }

    /// <summary>经典 2048 配色，按数值分段。</summary>
    private static Color ColorFor(int value) => value switch
    {
        2 => Color.FromHtml("3b4a5a"),
        4 => Color.FromHtml("43607a"),
        8 => Color.FromHtml("c98a5b"),
        16 => Color.FromHtml("c9744a"),
        32 => Color.FromHtml("c95c4a"),
        64 => Color.FromHtml("c9423b"),
        128 => Color.FromHtml("d4ae43"),
        256 => Color.FromHtml("d49a3a"),
        512 => Color.FromHtml("d48231"),
        1024 => Color.FromHtml("7c9f5a"),
        2048 => Color.FromHtml("5a9f7f"),
        _ => Color.FromHtml("8f5bd4")
    };

    /// <summary>浅色底用深字，深色底用浅字。</summary>
    private static Color TextColorFor(int value) =>
        value is >= 128 and < 1024 ? Color.FromHtml("2a2f36") : Color.FromHtml("f2f5f8");
}
