using Bladesmith2048.scripts.core.input;
using Bladesmith2048.scripts.enums.input;

namespace Bladesmith2048.global;

/// <summary>
///     全局输入：处理 Global 与 Paused 阶段。
/// </summary>
/// <remarks>autoload 挂的是 global_input_controller.tscn（process_mode=ALWAYS），否则暂停后收不到输入。</remarks>
[Log]
public partial class GlobalInputController : GameInputController
{
    protected override bool AcceptPhase(InputPhase phase)
    {
        return phase is InputPhase.Global or InputPhase.Paused;
    }

    protected override void Handle(InputPhase phase, InputEvent @event)
    {
        if (!@event.IsActionPressed("ui_cancel"))
            return;

        GetViewport().SetInputAsHandled();
    }
}
