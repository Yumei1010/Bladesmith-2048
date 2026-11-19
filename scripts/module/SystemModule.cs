using GFramework.Core.Abstractions.Architectures;
using GFramework.Core.Abstractions.Localization;
using GFramework.Core.Localization;
using GFramework.Game.Setting;
using Bladesmith2048.scripts.framework.localization;
using Bladesmith2048.scripts.core.scene;
using Bladesmith2048.scripts.core.ui;

namespace Bladesmith2048.scripts.module;

/// <summary>
///     注册路由、设置与本地化系统。
/// </summary>
public class SystemModule : IArchitectureModule
{
    public void Install(IArchitecture architecture)
    {
        architecture.RegisterSystem(new UiRouter());
        architecture.RegisterSystem(new SceneRouter());
        architecture.RegisterSystem(new SettingsSystem());

        // 必须先注册本地化系统：GodotLocalizationSettings 应用器要解析 ILocalizationManager 才能切语言
        architecture.RegisterSystem(new LocalizationManager(new LocalizationConfig
        {
            DefaultLanguage = "zhs",
            FallbackLanguage = "eng",
            LocalizationPath = LocalizationPathResolver.Resolve()
        }));
    }
}
