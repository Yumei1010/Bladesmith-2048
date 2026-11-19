using GFramework.Game.Abstractions.Setting.Data;

namespace Bladesmith2048.scripts.cqrs.setting.query.result;

/// <summary>
///     设置查询结果。
/// </summary>
/// <remarks>字段直接引用模型里的设置对象：改它们等于绕过命令写状态。</remarks>
public sealed class SettingsView
{
    public AudioSettings Audio { get; set; } = new();

    public GraphicsSettings Graphics { get; set; } = new();

    public LocalizationSettings Localization { get; set; } = new();
}