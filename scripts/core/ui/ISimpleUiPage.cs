using GFramework.Game.Abstractions.UI;

namespace Bladesmith2048.scripts.core.ui;

/// <summary>
///     页面生命周期接口：把 IUiPage 的每个回调都给了空实现，页面只重写自己需要的那个。
/// </summary>
public interface ISimpleUiPage : IUiPage
{
    void IUiPage.OnExit() {}

    void IUiPage.OnPause() {}

    void IUiPage.OnResume() {}

    void IUiPage.OnShow() {}

    void IUiPage.OnHide() {}

    void IUiPage.OnEnter(IUiPageEnterParam? param) {}
}