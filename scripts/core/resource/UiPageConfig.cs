using GFramework.Core.Abstractions.Bases;
using Bladesmith2048.scripts.enums.ui;

namespace Bladesmith2048.scripts.core.resource;

/// <summary>
///     页面注册项：UiKey → PackedScene，在入口场景里配置。
/// </summary>
[GlobalClass]
public partial class UiPageConfig : Resource, IKeyValue<string, PackedScene>
{
    [Export]
    public UiKey UiKey { get; set; }

    [Export]
    public PackedScene Scene { get; set; } = null!;

    public string Key => UiKey.ToString();

    public PackedScene Value => Scene;
}