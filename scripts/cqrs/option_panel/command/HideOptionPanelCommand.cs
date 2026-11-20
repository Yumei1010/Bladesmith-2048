using GFramework.Core.Command;
using GFramework.Game.Abstractions.Enums;
using GFramework.Game.Abstractions.UI;
using Bladesmith2048.scripts.enums.ui;

namespace Bladesmith2048.scripts.cqrs.option_panel.command;

/// <summary>
///     关闭设置弹窗并销毁实例（下次打开重新播入场动画）。
/// </summary>
public sealed class HideOptionPanelCommand : AbstractAsyncCommand
{
    protected override Task OnExecuteAsync()
    {
        this.GetSystem<IUiRouter>()!
            .HideByKey(UiKey.OptionPanel.ToString(), UiLayer.Overlay, destroy: true);
        return Task.CompletedTask;
    }
}
