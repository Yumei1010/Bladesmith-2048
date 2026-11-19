using GFramework.Core.Command;
using GFramework.Game.Abstractions.Setting;
using GFramework.Game.Abstractions.Setting.Data;
using GFramework.Godot.Setting;
using Bladesmith2048.scripts.cqrs.graphics.command.input;

namespace Bladesmith2048.scripts.cqrs.graphics.command;

/// <summary>
///     切换全屏并立即应用。
/// </summary>
public sealed class ToggleFullscreenCommand(ToggleFullscreenCommandInput input)
    : AbstractAsyncCommand<ToggleFullscreenCommandInput>(input)
{
    protected override async Task OnExecuteAsync(ToggleFullscreenCommandInput input)
    {
        var model = this.GetModel<ISettingsModel>()!;
        model.GetData<GraphicsSettings>().Fullscreen = input.Fullscreen;
        await this.GetSystem<ISettingsSystem>()!.Apply<GodotGraphicsSettings>().ConfigureAwait(false);
    }
}