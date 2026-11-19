using GFramework.Core.Abstractions.Architectures;
using GFramework.Core.Functional.Pipe;
using GFramework.Game.Abstractions.Data;
using GFramework.Game.Setting;
using GFramework.Godot.Setting;
using GFramework.Godot.Setting.Data;
using Bladesmith2048.scripts.data.setting;

namespace Bladesmith2048.scripts.module;

/// <summary>
///     注册设置模型与三个应用器（音频/图形/本地化）。
/// </summary>
/// <remarks>应用器负责把 Model 里的值落到引擎；没有它们，命令写了值也不会生效。</remarks>
public class ModelModule : IArchitectureModule
{
    public void Install(IArchitecture architecture)
    {
        var settingsDataRepository = architecture.Context.GetUtility<ISettingsDataRepository>()!;

        architecture.RegisterModel(
            new SettingsModel<ISettingsDataRepository>(new SettingDataLocationProvider(), settingsDataRepository)
                .Also(it =>
                {
                    it.RegisterApplicator(new GodotAudioSettings(it, new AudioBusMap()));
                    it.RegisterApplicator(new GodotGraphicsSettings(it));
                    it.RegisterApplicator(new GodotLocalizationSettings(it, new LocalizationMap()));
                })
        );
    }
}
