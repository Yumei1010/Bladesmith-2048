using GFramework.Core.Command;
using GFramework.Game.Abstractions.Setting;

namespace Bladesmith2048.scripts.cqrs.setting.command;

/// <summary>
///     恢复默认设置并立即应用。
/// </summary>
public sealed class ResetAllSettingsCommand(EmptyCommandInput input)
    : AbstractAsyncCommand<EmptyCommandInput>(input)
{
    protected override async Task OnExecuteAsync(EmptyCommandInput input)
    {
        await this.GetSystem<ISettingsSystem>()!.ResetAll().ConfigureAwait(false);
    }
}