using GFramework.Core.Environment;
using Bladesmith2048.scripts.constants;

namespace Bladesmith2048.scripts.core.environment;

/// <summary>
///     开发环境：与主环境共用同一套模块，只换环境名（IsDev 时由 GameEntryPoint 选用）。
/// </summary>
public class GameDevEnvironment : EnvironmentBase
{
    public override string Name { get; } = GameConstants.Development;

    public override void Initialize()
    {
        // 需要测试替身时在这里从 DevBindingManager 取
    }
}