using GFramework.Game.Abstractions.Scene;
using GFramework.Game.Scene;
using GFramework.Game.Scene.Handler;

namespace Bladesmith2048.scripts.core.scene;

/// <summary>
///     场景路由：Root 由 SceneRoot 自动加载节点绑定。
/// </summary>
public class SceneRouter : SceneRouterBase
{
    public Node? SceneRoot => Root as Node;

    protected override void RegisterHandlers()
    {
        RegisterHandler(new LoggingTransitionHandler(), new SceneTransitionHandlerOptions());
    }

    /// <summary>
    ///     空实现是必须的：基类 OnInit 会解析 ISceneFactory，而本项目只注册场景注册表、没有场景工厂。
    /// </summary>
    protected override void OnInit()
    {
    }
}
