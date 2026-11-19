using GFramework.Core.Abstractions.Architectures;
using GFramework.Game.Abstractions.Data;
using GFramework.Game.Data;
using GFramework.Game.Serializer;
using GFramework.Godot.Scene;
using GFramework.Godot.Storage;
using GFramework.Godot.UI;
using Bladesmith2048.scripts.component.input_rebind;
using Bladesmith2048.scripts.framework.input;

namespace Bladesmith2048.scripts.module;

/// <summary>
///     注册注册表、序列化、存储与设置仓储。
/// </summary>
/// <remarks>必须最先安装：ModelModule 依赖这里的 ISettingsDataRepository。</remarks>
public class UtilityModule : IArchitectureModule
{
    public void Install(IArchitecture architecture)
    {
        var inputDeviceService = new InputDeviceService();
        architecture.RegisterUtility(inputDeviceService);

        // 复用设备服务的绑定存储；动作列表默认取 InputMap 里非 ui_ 前缀的动作
        architecture.RegisterUtility(new InputRebindService(inputDeviceService.BindingStore));

        architecture.RegisterUtility(new GodotUiRegistry());
        architecture.RegisterUtility(new GodotSceneRegistry());
        architecture.RegisterUtility(new GodotUiFactory());

        var jsonSerializer = new JsonSerializer();
        architecture.RegisterUtility(jsonSerializer);

        var storage = new GodotFileStorage(jsonSerializer);
        architecture.RegisterUtility(storage);

        architecture.RegisterUtility(new UnifiedSettingsDataRepository(storage, jsonSerializer,
            new DataRepositoryOptions
            {
                BasePath = ProjectSettings.GetSetting("application/config/save/setting_path").AsString(),
                AutoBackup = true
            }));
    }
}
