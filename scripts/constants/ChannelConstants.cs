namespace Bladesmith2048.scripts.constants;

/// <summary>
///     事件总线频段名。频段本身没有语义，划分方式由订阅方约定。
/// </summary>
public static class ChannelConstants
{
    public const string Gameplay = "Gameplay";

    public const string Ui = "Ui";

    public const string Audio = "Audio";

    public const string Net = "Net";
}
