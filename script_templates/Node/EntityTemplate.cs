// meta-name: 实体节点
// meta-description: 游戏实体：节点注入、信号桥接、CQRS 事件与池化钩子
using Godot;
using GFramework.Core.SourceGenerators.Abstractions.Logging;
using GFramework.Core.SourceGenerators.Abstractions.Rule;
using GFramework.Godot.SourceGenerators.Abstractions;

/// <summary>
///     _CLASS_ 实体节点：视图生命周期、CQRS 事件与可选的池化钩子。
///     生成后先补命名空间：<c>namespace Bladesmith2048.scripts.&lt;domain&gt;;</c>
/// </summary>
[Log]
[ContextAware]
public partial class _CLASS_ : _BASE_
{
    // 需要子节点/系统时取消注释：
    // [GetNode]   private Panel _visual = null!;
    // [GetSystem] private SomePool _pool = null!;

    public override void _Ready()
    {
        // __InjectContextBindings_Generated();   // [GetSystem]/[GetModel]/[GetUtility]
        // __InjectGetNodes_Generated();          // [GetNode]
        // __BindNodeSignals_Generated();         // [BindNodeSignal]
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        // this.RegisterEvent<SomeEvent>(e => { ... }).UnRegisterWhenNodeExitTree(this);
    }

    // 对象池钩子（实现 IPoolableNode 时启用）：
    // public void OnAcquire() { }
    // public void OnRelease() { }
    // public void OnPoolDestroy() => QueueFree();
}
