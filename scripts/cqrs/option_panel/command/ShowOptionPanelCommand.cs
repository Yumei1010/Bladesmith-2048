using GFramework.Core.Command;
using GFramework.Game.Abstractions.Enums;
using GFramework.Game.Abstractions.UI;
using Bladesmith2048.scripts.enums.ui;

namespace Bladesmith2048.scripts.cqrs.option_panel.command;

/// <summary>
///     在 Overlay 层显示设置弹窗（幂等：已经显示就不再叠一层）。
/// </summary>
public sealed class ShowOptionPanelCommand : AbstractAsyncCommand
{
    protected override Task OnExecuteAsync()
    {
        var uiRouter = this.GetSystem<IUiRouter>()!;
        var key = UiKey.OptionPanel.ToString();

        // 同 key 的非重入页面在层内重复显示会抛异常，先做幂等判断
        if (uiRouter.GetAllFromLayer(key, UiLayer.Overlay).Count > 0)
            return Task.CompletedTask;

        uiRouter.Show(key, UiLayer.Overlay);
        return Task.CompletedTask;
    }
}
