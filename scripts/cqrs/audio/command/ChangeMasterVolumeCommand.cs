using GFramework.Core.Command;
using GFramework.Game.Abstractions.Setting;
using GFramework.Game.Abstractions.Setting.Data;
using GFramework.Godot.Setting;
using Bladesmith2048.scripts.cqrs.audio.command.input;

namespace Bladesmith2048.scripts.cqrs.audio.command;

/// <summary>
///     改主音量：写模型后立即应用到音频总线。
/// </summary>
public sealed class ChangeMasterVolumeCommand(ChangeMasterVolumeCommandInput input)
    : AbstractAsyncCommand<ChangeMasterVolumeCommandInput>(input)
{
    protected override async Task OnExecuteAsync(ChangeMasterVolumeCommandInput input)
    {
        var model = this.GetModel<ISettingsModel>()!;
        model.GetData<AudioSettings>().MasterVolume = input.Volume;
        await this.GetSystem<ISettingsSystem>()!.Apply<GodotAudioSettings>().ConfigureAwait(false);
    }
}