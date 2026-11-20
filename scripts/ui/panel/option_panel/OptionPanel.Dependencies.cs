using Godot;
using GFramework.Core.Abstractions.Localization;
using GFramework.Godot.SourceGenerators.Abstractions;
using Bladesmith2048.global;

namespace Bladesmith2048.scripts.ui.panel.option_panel;

public partial class OptionPanel
{
    [GetSystem] private ILocalizationManager _localization = null!;

    [GetNode] private Panel BackgroundPanel = null!;
    [GetNode] private Label TitleLabel = null!;
    [GetNode] private Button DoneButton = null!;
    [GetNode] private Button UndoButton = null!;
    [GetNode] private Button CancelButton = null!;

    private async Task ReadyAsync()
    {
        try
        {
            await GameEntryPoint.Architecture.WaitUntilReadyAsync();

            // 交回主线程再碰节点（await 之后可能已在池线程）
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            TitleLabel.Text = _localization.GetText("common", "option_panel.title");
            DoneButton.Text = _localization.GetText("common", "option_panel.done");
            UndoButton.Text = _localization.GetText("common", "option_panel.undo");
            CancelButton.Text = _localization.GetText("common", "option_panel.cancel");
            _log.Debug("OptionPanel Initialize Done");
        }
        catch (Exception ex)
        {
            _log.Error("OptionPanel Initialize Failed", ex);
        }
    }
}
