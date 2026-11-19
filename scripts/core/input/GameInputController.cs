using GFramework.Core.Abstractions.Controller;
using Bladesmith2048.scripts.enums.input;
using Bladesmith2048.scripts.framework.input;

namespace Bladesmith2048.scripts.core.input;

/// <summary>
///     输入分发基类：先喂设备跟踪，再按「暂停与否」把事件派给 Global + Gameplay/Paused。
/// </summary>
/// <remarks>暂停时的输入依赖 autoload 的 process_mode=ALWAYS（见 global_input_controller.tscn）。</remarks>
[Log]
[ContextAware]
public abstract partial class GameInputController : Node, IController
{
    protected SceneTree Tree => GetTree();

    public override void _Input(InputEvent @event)
    {
        // 设备跟踪先于分发：任何输入都要更新「当前活跃设备」，UI 靠它切按键提示
        this.GetUtility<InputDeviceService>()?.Feed(@event);

        var pausedAtFrameStart = Tree.Paused;

        Dispatch(InputPhase.Global, @event);

        Dispatch(pausedAtFrameStart ? InputPhase.Paused : InputPhase.Gameplay, @event);
    }

    private void Dispatch(InputPhase phase, InputEvent @event)
    {
        if (!AcceptPhase(phase) || IsBlocked())
            return;

        Handle(phase, @event);
    }

    /// <summary>默认拒绝所有阶段，子类必须重写。</summary>
    protected virtual bool AcceptPhase(InputPhase phase)
    {
        return false;
    }

    protected abstract void Handle(InputPhase phase, InputEvent @event);

    /// <summary>默认不阻塞；需要「输入被模态 UI 吃掉」时重写。</summary>
    protected virtual bool IsBlocked()
    {
        return false;
    }
}