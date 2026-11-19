using GFramework.Core.Abstractions.Controller;
using GFramework.Core.Abstractions.Localization;
using GFramework.Game.Abstractions.Enums;
using GFramework.Game.Abstractions.UI;
using GFramework.Godot.SourceGenerators.Abstractions;
using GFramework.Godot.SourceGenerators.Abstractions.UI;
using Bladesmith2048.global;
using Bladesmith2048.scripts.core.ui;
using Bladesmith2048.scripts.enums.ui;

namespace Bladesmith2048.scripts.ui.menu.main_menu;

/// <summary>
///     起始页面（极简占位）：只演示页面注入链与本地化取词，业务内容由新项目替换。
/// </summary>
/// <remarks>
///     保留它是因为 <c>UiRouter</c> 需要一个已注册页面：删掉它就要同时改
///     <c>AppState</c> 与 <c>game_entry_point.tscn</c> 的页面注册。
/// </remarks>
[Log]
[ContextAware]
[AutoUiPage(nameof(UiKey.MainMenu), nameof(UiLayer.Page))]
public partial class MainMenu : Control, IController, IUiPageBehaviorProvider, ISimpleUiPage
{
    [GetSystem] private ILocalizationManager _localization = null!;
    [GetNode] private Label TitleLabel = null!;

    /// <summary>
    ///     节点就绪：架构注入 → 节点注入 → 异步初始化。
    /// </summary>
    public override void _Ready()
    {
        __InjectContextBindings_Generated();
        __InjectGetNodes_Generated();
        _ = ReadyAsync();
    }

    /// <summary>
    ///     等待架构就绪后刷新页面内容。
    /// </summary>
    private async Task ReadyAsync()
    {
        try
        {
            // 先取树：await 之后可能已在池线程，那时连 GetTree() 都不能调
            var tree = GetTree();
            await GameEntryPoint.Architecture.WaitUntilReadyAsync();

            // 再等一帧把执行权交回主线程，之后的节点操作才安全
            await ToSignal(tree, SceneTree.SignalName.ProcessFrame);

            TitleLabel.Text = _localization.GetText("common", "app.title");
            _log.Debug("MainMenu Initialize Done");
        }
        catch (Exception ex)
        {
            _log.Error("MainMenu Initialize Failed", ex);
        }
    }
}
