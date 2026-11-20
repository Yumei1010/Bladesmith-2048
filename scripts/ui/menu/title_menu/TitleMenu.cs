using Godot;
using GFramework.Core.Abstractions.Controller;
using GFramework.Game.Abstractions.Enums;
using GFramework.Game.Abstractions.UI;
using GFramework.Godot.SourceGenerators.Abstractions;
using GFramework.Godot.SourceGenerators.Abstractions.UI;
using Bladesmith2048.scripts.core.ui;
using Bladesmith2048.scripts.enums.ui;

namespace Bladesmith2048.scripts.ui.menu.title_menu;

/// <summary>
///     标题菜单：入场时按钮从左侧依次滑入，悬停时晃动放大。
/// </summary>
/// <remarks>搬自 Template-Studio 的 MainMenu，按钮动作换成本项目的（开始→棋盘 / 选项→全屏 / 退出→结束进程）。</remarks>
[Log]
[ContextAware]
[AutoUiPage(nameof(UiKey.TitleMenu), nameof(UiLayer.Page))]
public partial class TitleMenu : Control, IController, IUiPageBehaviorProvider, ISimpleUiPage
{
    /// <summary>
    ///     节点就绪：架构注入 → 节点注入 → 信号绑定 → 事件注册 → 异步初始化。
    /// </summary>
    public override void _Ready()
    {
        __InjectContextBindings_Generated();   // [GetSystem] / [GetModel] 等架构组件字段注入
        __InjectGetNodes_Generated();          // [GetNode] 节点字段注入
        __BindNodeSignals_Generated();         // [BindNodeSignal] 信号绑定
        RegisterEvents();                      // 先订阅事件，避免错过初始化期间发出的事件
        _ = ReadyAsync();                      // 再做异步初始化（等待架构就绪后刷文案）

        PlayIntro();
    }

    /// <summary>入场动画：所有按钮先挪到屏幕左侧，再依次滑回原位。</summary>
    private void PlayIntro()
    {
        var buttons = GetTree().GetNodesInGroup("Buttons");

        foreach (var node in buttons)
            if (node is Button button)
                button.OffsetTransformPosition = new Vector2(-1920f, 0f);

        _introTween = CreateTween().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        foreach (var node in buttons)
            if (node is Button button)
                _introTween.TweenProperty(button, "offset_transform_position", Vector2.Zero, 0.25f);
    }

    /// <summary>退出场景树：解绑生成器绑定的信号（防泄漏）。</summary>
    public override void _ExitTree() => __UnbindNodeSignals_Generated();
}
