using GFramework.Core.Query;
using GFramework.Game.Abstractions.Setting;
using GFramework.Game.Abstractions.Setting.Data;
using Bladesmith2048.scripts.cqrs.setting.query.result;

namespace Bladesmith2048.scripts.cqrs.setting.query;

public sealed class GetCurrentSettingsQuery : AbstractQuery<SettingsView>
{
    protected override SettingsView OnDo()
    {
        var model = this.GetModel<ISettingsModel>()!;
        return new SettingsView
        {
            Audio = model.GetData<AudioSettings>(),
            Graphics = model.GetData<GraphicsSettings>(),
            Localization = model.GetData<LocalizationSettings>()
        };
    }
}