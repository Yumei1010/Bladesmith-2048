using GFramework.Cqrs.Abstractions.Cqrs.Command;

namespace Bladesmith2048.scripts.cqrs.audio.command.input;

public sealed class ChangeSfxVolumeCommandInput : ICommandInput
{
    /// <summary>0–1；不在此处裁剪。</summary>
    public float Volume { get; set; }
}