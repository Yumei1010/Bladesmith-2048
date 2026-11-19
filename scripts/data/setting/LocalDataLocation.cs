using GFramework.Game.Abstractions.Data;
using GFramework.Game.Abstractions.Enums;

namespace Bladesmith2048.scripts.data.setting;

/// <summary>
///     本地（user://）数据位置描述，供数据仓库做 location → key 映射。
/// </summary>
public sealed record LocalDataLocation : IDataLocation
{
    public string Key { get; init; } = "local";

    public StorageKinds Kinds { get; init; } = StorageKinds.Local;

    public string? Namespace { get; init; } = "";

    public IReadOnlyDictionary<string, string>? Metadata { get; init; } = null;
}
