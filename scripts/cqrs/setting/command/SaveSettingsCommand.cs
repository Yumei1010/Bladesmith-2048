using GFramework.Core.Command;
using GFramework.Game.Abstractions.Setting;

namespace Bladesmith2048.scripts.cqrs.setting.command;

/// <summary>
///     把当前设置写回存储。
/// </summary>
/// <remarks>退出时由 GameEntryPoint._ExitTree 触发。</remarks>
public sealed class SaveSettingsCommand : AbstractAsyncCommand
{
    protected override async Task OnExecuteAsync()
    {
        await this.GetSystem<ISettingsSystem>()!.SaveAll().ConfigureAwait(false);
    }
}