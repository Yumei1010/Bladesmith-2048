using System;
using System.Threading.Tasks;
using Godot;
using GFramework.Core.Abstractions.Controller;
using GFramework.Core.SourceGenerators.Abstractions.Logging;
using GFramework.Core.SourceGenerators.Abstractions.Rule;
using GFramework.Godot.SourceGenerators.Abstractions;
using Bladesmith2048.global;
using Bladesmith2048.scripts.ui.chessboard.widgets.grid_cell;

namespace Bladesmith2048.scripts.ui.chessboard.widgets.grid;

[Log]
[ContextAware]
public partial class Grid : Control, IController
{
	// 需要架构组件时取消注释（用哪个开哪个）：
	// [GetSystem] private IUiRouter _uiRouter = null!;
	// [GetNode]   private Label _statusLabel = null!;

	public override void _Ready()
	{
		// 只调用实际用到的生成方法，顺序不能乱：
		// __InjectContextBindings_Generated();   // [GetSystem]/[GetModel]/[GetUtility]
		__InjectGetNodes_Generated();          // [GetNode]
		// __BindNodeSignals_Generated();         // [BindNodeSignal]
		RegisterEvents();                         // 先订阅，避免初始化期间漏事件
		_ = ReadyAsync();
	}

	private async Task ReadyAsync()
	{
		try
		{
			await GameEntryPoint.Architecture.WaitUntilReadyAsync();
			_log.Debug("Grid ready");
		}
		catch (Exception ex)
		{
			_log.Error("Grid initialization failed", ex);
		}
	}

	private void RegisterEvents()
	{
		// this.RegisterEvent<SomeEvent>(e => { ... }).UnRegisterWhenNodeExitTree(this);
	}
}
