using Godot;
using GFramework.Core.Abstractions.Localization;
using GFramework.Godot.SourceGenerators.Abstractions;
using Bladesmith2048.global;

namespace Bladesmith2048.scripts.ui.menu.title_menu;

public partial class TitleMenu
{
    [GetSystem] private ILocalizationManager _localization = null!;

    [GetNode] private Panel BackgroundPanel = null!;
    [GetNode] private RichTextLabel TitleLabel = null!;
    [GetNode] private Button StartButton = null!;
    [GetNode] private Button OptionButton = null!;
    [GetNode] private Button QuitButton = null!;

    private async Task ReadyAsync()
    {
        try
        {
            await GameEntryPoint.Architecture.WaitUntilReadyAsync();

            // 交回主线程再碰节点（await 之后可能已在池线程）
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            TitleLabel.Text = $"[wave]{_localization.GetText("common", "app.title")}[/wave]";
            StartButton.Text = _localization.GetText("common", "title_menu.start");
            OptionButton.Text = _localization.GetText("common", "title_menu.option");
            QuitButton.Text = _localization.GetText("common", "title_menu.quit");
            _log.Debug("TitleMenu Initialize Done");
        }
        catch (Exception ex)
        {
            _log.Error("TitleMenu Initialize Failed", ex);
        }
    }
}
