// meta-name: 简单UI页面控制器类模板
// meta-description: UI 页面（Page 层）：AutoUiPage + 按需注入 + 事件/异步初始化
using System;
using System.Threading.Tasks;
using Godot;
using GFramework.Core.Abstractions.Controller;
using GFramework.Core.SourceGenerators.Abstractions.Logging;
using GFramework.Core.SourceGenerators.Abstractions.Rule;
using GFramework.Game.Abstractions.Enums;
using GFramework.Game.Abstractions.UI;
using GFramework.Godot.SourceGenerators.Abstractions;
using GFramework.Godot.SourceGenerators.Abstractions.UI;
using Bladesmith2048.global;
using Bladesmith2048.scripts.core.ui;
using Bladesmith2048.scripts.enums.ui;

/// <summary>
///     _CLASS_ UI 页面：<see cref="AutoUiPageAttribute" /> 生成 UiKeyStr 与 GetPage()。
///     生成后先补命名空间，并在 <see cref="UiKey" /> 中添加同名页面键。
/// </summary>
[Log]
[ContextAware]
[AutoUiPage(nameof(UiKey._CLASS_), nameof(UiLayer.Page))]
public partial class _CLASS_ : _BASE_, IController, IUiPageBehaviorProvider, ISimpleUiPage
{
    // 需要节点/系统时取消注释：
    // [GetNode]   private Button _startButton = null!;
    // [GetSystem] private IUiRouter _uiRouter = null!;

    // [BindNodeSignal(nameof(_startButton), nameof(Button.Pressed))]
    // private void OnStartPressed() { }

    public override void _Ready()
    {
        // 只调用实际用到的生成方法：
        // __InjectContextBindings_Generated();   // [GetSystem]/[GetModel]/[GetUtility]
        // __InjectGetNodes_Generated();          // [GetNode]
        // __BindNodeSignals_Generated();         // [BindNodeSignal]
        RegisterEvents();                         // 先订阅
        _ = ReadyAsync();                         // 再异步初始化
    }

    private async Task ReadyAsync()
    {
        try
        {
            await GameEntryPoint.Architecture.WaitUntilReadyAsync();
            _log.Debug("_CLASS_ ready");
        }
        catch (Exception ex)
        {
            _log.Error("_CLASS_ initialization failed", ex);
        }
    }

    private void RegisterEvents()
    {
        // this.RegisterEvent<SomeEvent>(e => { ... }).UnRegisterWhenNodeExitTree(this);
    }
}
