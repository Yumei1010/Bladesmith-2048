using GFramework.Core.Command;
using GFramework.Game.Abstractions.Setting;
using GFramework.Game.Abstractions.Setting.Data;
using GFramework.Godot.Setting;
using Bladesmith2048.scripts.cqrs.setting.command.input;

namespace Bladesmith2048.scripts.cqrs.setting.command;

/// <summary>
///     切换语言并立即应用。
/// </summary>
public sealed class ChangeLanguageCommand(ChangeLanguageCommandInput input)
    : AbstractAsyncCommand<ChangeLanguageCommandInput>(input)
{
    protected override async Task OnExecuteAsync(ChangeLanguageCommandInput input)
    {
        var model = this.GetModel<ISettingsModel>()!;
        var settings = model.GetData<LocalizationSettings>();
        settings.Language = input.Language;
        await this.GetSystem<ISettingsSystem>()!.Apply<GodotLocalizationSettings>().ConfigureAwait(false);
    }
}