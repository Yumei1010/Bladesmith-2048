using Godot;
using GFramework.Core.Command;
using GFramework.Godot.SourceGenerators.Abstractions;
using Bladesmith2048.scripts.cqrs.setting.command;
using Bladesmith2048.scripts.framework.command;

namespace Bladesmith2048.scripts.ui.panel.option_panel;

/// <remarks>
///     Godot 主线程禁止同步 CQRS 调用（框架 <c>GuardSyncCqrs</c>），命令统一走 <c>SendCommandDetached</c>。
/// </remarks>
public partial class OptionPanel
{
    [BindNodeSignal(nameof(DoneButton), nameof(Button.Pressed))]
    private void OnDoneButtonPressed() => PlayExit();

    /// <summary>恢复默认设置（模板里 Undo 是空实现，这里接到已有的重置命令上）。</summary>
    [BindNodeSignal(nameof(UndoButton), nameof(Button.Pressed))]
    private void OnUndoButtonPressed() =>
        this.SendCommandDetached(new ResetAllSettingsCommand(new EmptyCommandInput()), _log);

    [BindNodeSignal(nameof(CancelButton), nameof(Button.Pressed))]
    private void OnCancelButtonPressed() => PlayExit();

    [BindNodeSignal(nameof(DoneButton), nameof(Button.MouseEntered))]
    private void OnDoneButtonMouseEntered() => PlayHover(DoneButton);

    [BindNodeSignal(nameof(UndoButton), nameof(Button.MouseEntered))]
    private void OnUndoButtonMouseEntered() => PlayHover(UndoButton);

    [BindNodeSignal(nameof(CancelButton), nameof(Button.MouseEntered))]
    private void OnCancelButtonMouseEntered() => PlayHover(CancelButton);

    /// <summary>悬停：横向挤扁再弹回（和标题菜单同一套手感）。</summary>
    private void PlayHover(Button button)
    {
        var tween = CreateTween().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(button, "offset_transform_scale", new Vector2(0.8f, 1.2f), 0.1f);
        tween.TweenProperty(button, "offset_transform_scale", Vector2.One, 0.1f);
    }
}
