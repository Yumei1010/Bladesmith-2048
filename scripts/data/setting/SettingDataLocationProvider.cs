using GFramework.Game.Abstractions.Data;

namespace Bladesmith2048.scripts.data.setting;

/// <summary>
///     设置数据落盘位置：按设置类型名分键，统一放在 settings 命名空间。
/// </summary>
public class SettingDataLocationProvider : IDataLocationProvider
{
    public IDataLocation GetLocation(Type type)
    {
        return new LocalDataLocation
        {
            Key = type.Name,
            Namespace = "settings"
        };
    }
}
