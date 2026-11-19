using GFramework.Cqrs.Abstractions.Cqrs.Command;

namespace Bladesmith2048.scripts.cqrs.graphics.command.input;

public sealed class ChangeResolutionCommandInput : ICommandInput
{
    public int Width { get; set; }

    public int Height { get; set; }
}