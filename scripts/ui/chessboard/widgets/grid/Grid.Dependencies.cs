using GFramework.Godot.SourceGenerators.Abstractions;
using Bladesmith2048.scripts.ui.chessboard.widgets.grid_cell;

namespace Bladesmith2048.scripts.ui.chessboard.widgets.grid;

public partial class Grid
{
    [GetNode] private Panel BackgroundPanel = null!;

    [GetNode] private GridCell GridCell11 = null!;
    [GetNode] private GridCell GridCell12 = null!;
    [GetNode] private GridCell GridCell13 = null!;
    [GetNode] private GridCell GridCell14 = null!;

    [GetNode] private GridCell GridCell21 = null!;
    [GetNode] private GridCell GridCell22 = null!;
    [GetNode] private GridCell GridCell23 = null!;
    [GetNode] private GridCell GridCell24 = null!;

    [GetNode] private GridCell GridCell31 = null!;
    [GetNode] private GridCell GridCell32 = null!;
    [GetNode] private GridCell GridCell33 = null!;
    [GetNode] private GridCell GridCell34 = null!;

    [GetNode] private GridCell GridCell41 = null!;
    [GetNode] private GridCell GridCell42 = null!;
    [GetNode] private GridCell GridCell43 = null!;
    [GetNode] private GridCell GridCell44 = null!;
}