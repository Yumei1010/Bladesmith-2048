using GFramework.Core.Abstractions.State;
using GFramework.Core.State;
using GFramework.Game.Abstractions.Scene;
using GFramework.Game.Abstractions.UI;
using Bladesmith2048.scripts.enums.ui;

namespace Bladesmith2048.scripts.core.state;

/// <summary>
///     应用默认状态：清空 UI 与场景后推入起始页面。
/// </summary>
/// <remarks>
///     不能加 <c>ConfigureAwait(false)</c>：<c>PushAsync</c> 会实例化页面节点，续体必须留在主线程。
/// </remarks>
public class AppState : AsyncContextAwareStateBase
{
    public override async Task OnEnterAsync(IState? from)
    {
        var uiRouter = this.GetSystem<IUiRouter>()!;
        await uiRouter.ClearAsync();
        await this.GetSystem<ISceneRouter>()!.ClearAsync();

        // 起始页面：标题菜单
        await uiRouter.PushAsync(UiKey.TitleMenu.ToString());
    }

    /// <inheritdoc />
    public override Task<bool> CanTransitionToAsync(IState target) => Task.FromResult(true);
}
