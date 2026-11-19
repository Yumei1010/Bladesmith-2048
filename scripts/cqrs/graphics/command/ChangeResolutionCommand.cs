using GFramework.Core.Command;
using GFramework.Game.Abstractions.Setting;
using GFramework.Game.Abstractions.Setting.Data;
using GFramework.Godot.Setting;
using Bladesmith2048.scripts.cqrs.graphics.command.input;

namespace Bladesmith2048.scripts.cqrs.graphics.command;

/// <summary>
///     改分辨率：宽高以像素计，写模型后立即应用。
/// </summary>
public sealed class ChangeResolutionCommand(ChangeResolutionCommandInput input)
    : AbstractAsyncCommand<ChangeResolutionCommandInput>(input)
{
    protected override async Task OnExecuteAsync(ChangeResolutionCommandInput input)
    {
        var model = this.GetModel<ISettingsModel>()!;
        var settings = model.GetData<GraphicsSettings>();
        settings.ResolutionWidth = input.Width;
        settings.ResolutionHeight = input.Height;
        await this.GetSystem<ISettingsSystem>()!.Apply<GodotGraphicsSettings>().ConfigureAwait(false);
    }
}