using Godot;
using GFramework.Core.SourceGenerators.Abstractions.Logging;
using GFramework.Core.SourceGenerators.Abstractions.Rule;
using GFramework.Godot.SourceGenerators.Abstractions;

namespace Bladesmith2048.scripts.ui.chessboard.widgets.tile;

[Log]
[ContextAware]
public partial class Tile : Control
{
	// 需要子节点/系统时取消注释：
	// [GetNode]   private Panel _visual = null!;
	// [GetSystem] private SomePool _pool = null!;

	public override void _Ready()
	{
		// __InjectContextBindings_Generated();   // [GetSystem]/[GetModel]/[GetUtility]
		__InjectGetNodes_Generated();             // [GetNode]
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
