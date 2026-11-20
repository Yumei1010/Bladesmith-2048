using Godot;
using GFramework.Core.Abstractions.Controller;
using GFramework.Game.Abstractions.Enums;
using GFramework.Game.Abstractions.UI;
using GFramework.Godot.SourceGenerators.Abstractions;
using GFramework.Godot.SourceGenerators.Abstractions.UI;
using Bladesmith2048.scripts.core.ui;
using Bladesmith2048.scripts.cqrs.option_panel.command;
using Bladesmith2048.scripts.framework.command;
using Bladesmith2048.scripts.enums.ui;

namespace Bladesmith2048.scripts.ui.panel.option_panel;

/// <summary>
///     设置弹窗：从屏幕下方弹到正中，Done/Cancel 收起，Undo 恢复默认设置。
/// </summary>
/// <remarks>
///     搬自 Template-Studio。位置动画改在 <c>offset_top/offset_bottom</c> 上做，不依赖布局是否已算好，
///     也不写死 1920×1080 的坐标。
/// </remarks>
[Log]
[ContextAware]
[AutoUiPage(nameof(UiKey.OptionPanel), nameof(UiLayer.Overlay))]
public partial class OptionPanel : Control, IController, IUiPageBehaviorProvider, ISimpleUiPage
{
    /// <summary>弹入时长。Elastic 的弹簧感随距离放大，长一点更稳。</summary>
    private const double EnterDuration = 0.8;

    /// <summary>收起时长（要短，别让玩家等）。</summary>
    private const double ExitDuration = 0.15;

    public override void _Ready()
    {
        __InjectContextBindings_Generated();   // [GetSystem] / [GetModel] 注入
        __InjectGetNodes_Generated();          // [GetNode] 注入
        __BindNodeSignals_Generated();         // [BindNodeSignal] 信号绑定
        RegisterEvents();                      // 先订阅事件
        _ = ReadyAsync();                      // 架构就绪后再刷文案

        PlayEnter();
    }

    /// <summary>Esc 收起弹窗。</summary>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!@event.IsActionPressed("ui_cancel"))
            return;

        GetViewport().SetInputAsHandled();
        PlayExit();
    }

    /// <summary>退出场景树：解绑生成器绑定的信号（防泄漏）。</summary>
    public override void _ExitTree() => __UnbindNodeSignals_Generated();

    /// <summary>从屏幕下方弹到场景里摆好的居中位置。</summary>
    private void PlayEnter()
    {
        var targetTop = BackgroundPanel.OffsetTop;
        var height = BackgroundPanel.OffsetBottom - targetTop;

        BackgroundPanel.OffsetTop = GetViewportRect().Size.Y;   // 先挪到屏幕外
        BackgroundPanel.OffsetBottom = BackgroundPanel.OffsetTop + height;

        _tween = CreateTween().SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
        _tween.TweenProperty(BackgroundPanel, "offset_top", targetTop, EnterDuration);
        _tween.Parallel().TweenProperty(BackgroundPanel, "offset_bottom", targetTop + height, EnterDuration);
    }

    /// <summary>滑出屏幕底部后再让路由销毁本页面。</summary>
    private void PlayExit()
    {
        _tween?.Kill();

        var height = BackgroundPanel.OffsetBottom - BackgroundPanel.OffsetTop;
        var bottom = GetViewportRect().Size.Y;

        _tween = CreateTween().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
        _tween.TweenProperty(BackgroundPanel, "offset_top", bottom, ExitDuration);
        _tween.Parallel().TweenProperty(BackgroundPanel, "offset_bottom", bottom + height, ExitDuration);
        _tween.Finished += () => this.SendCommandDetached(new HideOptionPanelCommand(), _log);
    }
}
