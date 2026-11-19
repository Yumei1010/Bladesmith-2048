using GFramework.Core.Abstractions.Bases;
using Bladesmith2048.scripts.enums.scene;

namespace Bladesmith2048.scripts.core.resource;

/// <summary>
///     场景注册项：SceneKey → PackedScene，在入口场景里配置。
/// </summary>
[GlobalClass]
public partial class SceneConfig : Resource, IKeyValue<string, PackedScene>
{
    [Export]
    public SceneKey SceneKey { get; set; }

    [Export]
    public PackedScene Scene { get; set; } = null!;

    public string Key => SceneKey.ToString();

    public PackedScene Value => Scene;
}