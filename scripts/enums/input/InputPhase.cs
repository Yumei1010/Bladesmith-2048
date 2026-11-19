namespace Bladesmith2048.scripts.enums.input;

/// <summary>
///     输入阶段：GameInputController 先派发 Global，再按暂停状态派发 Gameplay 或 Paused。
/// </summary>
public enum InputPhase
{
    Global,
    Gameplay,
    Paused
}
