using GFramework.Cqrs.Abstractions.Cqrs.Command;

namespace Bladesmith2048.scripts.cqrs.graphics.command.input;

public sealed class ToggleFullscreenCommandInput : ICommandInput
{
    public bool Fullscreen { get; set; }
}