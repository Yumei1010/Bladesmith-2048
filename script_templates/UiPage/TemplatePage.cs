using GFramework.Core.Abstractions.Controller;
using GFramework.Core.SourceGenerators.Abstractions.Logging;
using GFramework.Core.SourceGenerators.Abstractions.Rule;
using GFramework.Game.Abstractions.Enums;
using GFramework.Game.Abstractions.UI;
using GFramework.Godot.SourceGenerators.Abstractions;
using GFramework.Godot.SourceGenerators.Abstractions.UI;
using Godot;
using Bladesmith2048.scripts.core.ui;
using Bladesmith2048.scripts.enums.ui;

namespace Bladesmith2048.scripts.ui.menu;

/// <summary>
///     模板页面——语法糖版 UI 页面示例（partial class 模式）
///     <see cref="AutoUiPageAttribute" /> 自动生成 <c>UiKeyStr</c> + <c>GetPage()</c> + 缓存字段样板
/// </summary>
[Log]
[ContextAware]
[AutoUiPage(nameof(UiKey.MainMenu), nameof(UiLayer.Page))]   // 复制后替换为你的 UiKey
public partial class TemplatePage : Control, IController, IUiPageBehaviorProvider, ISimpleUiPage
{
    /// <summary>
    ///     Godot 节点就绪回调：架构注入 → 节点注入 → 信号绑定 → 事件订阅 → 异步初始化。
    /// </summary>
    /// <remarks>
    ///     两个关键点：
    ///     1. 只调用实际用到的生成方法：本页用了 [GetSystem]/[GetNode]/[BindNodeSignal]，所以三个都需要；
    ///        漏调对应方法会让注入字段为 null（有一致性测试守卫）；
    ///     2. <c>RegisterEvents()</c> 必须在异步初始化之前，否则初始化期间发出的事件会因尚未订阅而丢失。
    /// </remarks>
    public override void _Ready()
    {
        __InjectContextBindings_Generated();   // [GetSystem]/[GetModel]/[GetUtility] 字段注入
        __InjectGetNodes_Generated();          // [GetNode] 节点字段注入
        __BindNodeSignals_Generated();         // [BindNodeSignal] 信号绑定
        RegisterEvents();                      // 先订阅事件，避免错过初始化期间的事件
        _ = ReadyAsync();                      // 再做异步初始化
    }

    /// <summary>
    ///     节点退出场景树：解绑 [BindNodeSignal] 信号（防泄漏）
    /// </summary>
    public override void _ExitTree()
    {
        __UnbindNodeSignals_Generated();
    }
}
