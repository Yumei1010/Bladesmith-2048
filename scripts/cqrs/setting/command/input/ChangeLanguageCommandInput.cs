using GFramework.Cqrs.Abstractions.Cqrs.Command;

namespace Bladesmith2048.scripts.cqrs.setting.command.input;

public sealed class ChangeLanguageCommandInput : ICommandInput
{
    /// <summary>语言码，对应 resource/localization/ 下的目录名。</summary>
    public string Language { get; set; } = null!;
}