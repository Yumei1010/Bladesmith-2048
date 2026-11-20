using Godot;
using GFramework.Godot.SourceGenerators.Abstractions;
using Bladesmith2048.scripts.cqrs.option_panel.command;
using Bladesmith2048.scripts.framework.command;

namespace Bladesmith2048.scripts.ui.menu.title_menu;

/// <remarks>
///     Godot 主线程禁止同步 CQRS 调用（框架 <c>GuardSyncCqrs</c>），命令统一走 <c>SendCommandDetached</c>。
/// </remarks>
public partial class TitleMenu
{
    /// <summary>TODO(game): 游戏页接入后改成 <c>_ = _uiRouter.PushAsync(UiKey.&lt;游戏页&gt;.ToString());</c>。</summary>
    [BindNodeSignal(nameof(StartButton), nameof(Button.Pressed))]
    private void OnStartButtonPressed() => _log.Info("游戏页面尚未接入，Start 目前只是占位。");

    [BindNodeSignal(nameof(OptionButton), nameof(Button.Pressed))]
    private void OnOptionButtonPressed() =>
        this.SendCommandDetached(new ShowOptionPanelCommand(), _log);

    [BindNodeSignal(nameof(QuitButton), nameof(Button.Pressed))]
    private void OnQuitButtonPressed() => GetTree().Quit();

    [BindNodeSignal(nameof(StartButton), nameof(Button.MouseEntered))]
    private void OnStartButtonMouseEntered() => PlayHover(StartButton);

    [BindNodeSignal(nameof(OptionButton), nameof(Button.MouseEntered))]
    private void OnOptionButtonMouseEntered() => PlayHover(OptionButton);

    [BindNodeSignal(nameof(QuitButton), nameof(Button.MouseEntered))]
    private void OnQuitButtonMouseEntered() => PlayHover(QuitButton);

    /// <summary>悬停：横向挤扁 + 随机歪一下，再弹回原状（模板里那套手感）。</summary>
    private void PlayHover(Button button)
    {
        var tween = CreateTween().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(button, "offset_transform_scale", new Vector2(0.8f, 1.2f), 0.1f);
        tween.Parallel().TweenProperty(button, "offset_transform_rotation", Mathf.DegToRad(GD.RandRange(-15f, 15f)), 0.1f);

        tween.TweenProperty(button, "offset_transform_scale", Vector2.One, 0.1f);
        tween.Parallel().TweenProperty(button, "offset_transform_rotation", 0.0, 0.1f);
    }
}
