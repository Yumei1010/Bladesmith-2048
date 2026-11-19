using GFramework.Core.Command;
using GFramework.Game.Abstractions.Setting;
using GFramework.Game.Abstractions.Setting.Data;
using GFramework.Godot.Setting;
using Bladesmith2048.scripts.cqrs.audio.command.input;

namespace Bladesmith2048.scripts.cqrs.audio.command;

/// <summary>
///     改 BGM 音量：写模型后立即应用到音频总线。
/// </summary>
public sealed class ChangeBgmVolumeCommand(ChangeBgmVolumeCommandInput input)
    : AbstractAsyncCommand<ChangeBgmVolumeCommandInput>(input)
{
    protected override async Task OnExecuteAsync(ChangeBgmVolumeCommandInput input)
    {
        var model = this.GetModel<ISettingsModel>()!;
        model.GetData<AudioSettings>().BgmVolume = input.Volume;
        await this.GetSystem<ISettingsSystem>()!.Apply<GodotAudioSettings>().ConfigureAwait(false);
    }
}