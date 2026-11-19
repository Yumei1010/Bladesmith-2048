// meta-name: 控制器节点
// meta-description: 通用节点：生命周期 + CQRS 交互（语法糖按需注入）
using System;
using System.Threading.Tasks;
using Godot;
using GFramework.Core.Abstractions.Controller;
using GFramework.Core.SourceGenerators.Abstractions.Logging;
using GFramework.Core.SourceGenerators.Abstractions.Rule;
using GFramework.Godot.SourceGenerators.Abstractions;
using Bladesmith2048.global;

/// <summary>
///     _CLASS_ 节点：负责生命周期与 CQRS 交互。
///     生成后先补命名空间：<c>namespace Bladesmith2048.scripts.&lt;domain&gt;;</c>
/// </summary>
[Log]
[ContextAware]
public partial class _CLASS_ : _BASE_, IController
{
    // 需要架构组件时取消注释（用哪个开哪个）：
    // [GetSystem] private IUiRouter _uiRouter = null!;
    // [GetNode]   private Label _statusLabel = null!;

    public override void _Ready()
    {
        // 只调用实际用到的生成方法，顺序不能乱：
        // __InjectContextBindings_Generated();   // [GetSystem]/[GetModel]/[GetUtility]
        // __InjectGetNodes_Generated();          // [GetNode]
        // __BindNodeSignals_Generated();         // [BindNodeSignal]
        RegisterEvents();                         // 先订阅，避免初始化期间漏事件
        _ = ReadyAsync();
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
