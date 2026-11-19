using GFramework.Game.Abstractions.Enums;
using GFramework.Game.Abstractions.UI;
using GFramework.Godot.Extensions;
using Bladesmith2048.scripts.constants;

namespace Bladesmith2048.global;

/// <summary>
///     UI 根：每个 UiLayer 一个全屏 Control 容器，页面挂在对应容器下。
/// </summary>
/// <remarks>ZIndex = 层序号 * 100 + 层内序号，同一算式在 AddUiPage 与 SetZOrder 里必须一致。</remarks>
[Log]
[ContextAware]
public partial class UiRoot : CanvasLayer, IUiRoot
{
    private readonly Dictionary<UiLayer, Control> _containers = new();
    private readonly List<IUiPageBehavior> _pages = new();

    public void AddUiPage(IUiPageBehavior child)
    {
        AddUiPage(child, UiLayer.Page);
    }

    public void AddUiPage(IUiPageBehavior child, UiLayer layer, int orderInLayer = 0)
    {
        if (child.View is not CanvasItem item)
            throw new InvalidOperationException("UIPage View must be a Godot Node");

        if (!_containers.TryGetValue(layer, out var container))
            throw new InvalidOperationException($"UiLayer not found: {layer}");

        if (item.GetParent() == null)
            container.AddChild(item);
        else if (item.GetParent() != container) item.Reparent(container);

        item.ZIndex = (int)layer * 100 + orderInLayer;
        item.ZAsRelative = false;

        if (!_pages.Contains(child))
            _pages.Add(child);

        _log.Debug($"Add UI [{child.Key}] Layer={layer} Order={orderInLayer}");
    }


    public void RemoveUiPage(IUiPageBehavior child)
    {
        if (child.View is not Node node)
            return;

        node.GetParent()?.RemoveChild(node);
        _pages.Remove(child);
        node.QueueFreeX();
        _log.Debug($"Remove UI [{child.Key}]");
    }

    public void SetZOrder(IUiPageBehavior page, int zOrder)
    {
        if (page.View is not CanvasItem item)
            return;

        var layer = _containers
            .FirstOrDefault(p => item.GetParent() == p.Value)
            .Key;

        item.ZIndex = (int)layer * 100 + zOrder;
        item.ZAsRelative = false;
    }

    public IReadOnlyList<IUiPageBehavior> GetVisiblePages()
    {
        return _pages
            .Where(p => p.View is CanvasItem { Visible: true })
            .ToList();
    }

    public override void _Ready()
    {
        Layer = UiLayers.UiRoot;
        InitLayers();
        var router = this.GetSystem<IUiRouter>()!;
        router.BindRoot(this);
        this.SendEvent<UiRootReadyEvent>();
        _log.Debug($"[UiRoot] Ready. Path={GetPath()} InTree={IsInsideTree()}");
    }

    private void InitLayers()
    {
        foreach (var layer in Enum.GetValues<UiLayer>())
        {
            var container = new Control
            {
                Name = layer.ToString(),
                AnchorLeft = 0,
                AnchorTop = 0,
                AnchorRight = 1,
                AnchorBottom = 1,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };

            AddChild(container);
            _containers[layer] = container;
        }
    }


    /// <summary>容器建好后发出，GameEntryPoint 收到才切 AppState。</summary>
    public sealed class UiRootReadyEvent;
}