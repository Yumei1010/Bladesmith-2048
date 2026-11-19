using GFramework.Game.UI;
using GFramework.Game.UI.Handler;

namespace Bladesmith2048.scripts.core.ui;

/// <summary>
///     UI 路由：在框架 UiRouterBase 上只挂一个日志过渡处理器。
/// </summary>
[Log]
public partial class UiRouter : UiRouterBase
{
    protected override void RegisterHandlers()
    {
        RegisterHandler(new LoggingTransitionHandler());
    }
}