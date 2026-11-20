using GFramework.Godot.SourceGenerators.Abstractions;

namespace Bladesmith2048.scripts.ui.chessboard.widgets.tile;

public partial class Tile
{
    [GetNode] private Panel BackgroundPanel = null!;

    [GetNode] private RichTextLabel ScoreTextLabel = null!;
}