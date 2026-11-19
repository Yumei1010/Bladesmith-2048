using GFramework.Game.Abstractions.Scene;
using GFramework.Godot.Extensions;

namespace Bladesmith2048.global;

/// <summary>
///     场景根：SceneRouter 通过 BindRoot 绑定它，切场景时调用 AddScene / RemoveScene。
/// </summary>
[Log]
[ContextAware]
public partial class SceneRoot : Node2D, ISceneRoot
{
    private ISceneBehavior? _current;

    public void AddScene(ISceneBehavior scene)
    {
        if (scene.Original is not Node node)
            throw new InvalidOperationException("SceneBehavior.Original must be a Godot Node");

        RemoveSceneInternal();
        AddChild(node);
        _current = scene;
    }

    public void RemoveScene(ISceneBehavior scene)
    {
        if (scene.Original is not Node node)
            return;

        node.QueueFreeX();
        _current = null;
    }

    private void RemoveSceneInternal()
    {
        if (_current is null)
            return;

        if (_current.Original is Node node)
            node.QueueFreeX();
        _current = null;
    }

    public override void _Ready()
    {
        var router = this.GetSystem<ISceneRouter>()!;
        router.BindRoot(this);
        this.SendEvent<SceneRootReadyEvent>();
    }

    public sealed class SceneRootReadyEvent;
}
