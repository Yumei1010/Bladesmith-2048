using GFramework.Core.Environment;
using Bladesmith2048.scripts.constants;

namespace Bladesmith2048.scripts.core.environment;

/// <summary>
///     主环境：发布时使用。
/// </summary>
public class GameMainEnvironment : EnvironmentBase
{
    public override string Name { get; } = GameConstants.Main;

    public override void Initialize()
    {
    }
}