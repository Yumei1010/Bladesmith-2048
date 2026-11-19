using GFramework.Core.Abstractions.Architectures;
using GFramework.Core.Functional.Pipe;
using GFramework.Game.State;
using Bladesmith2048.scripts.core.state;

namespace Bladesmith2048.scripts.module;

/// <summary>
///     注册状态机系统与状态。
/// </summary>
public class StateModule : IArchitectureModule
{
    public void Install(IArchitecture architecture)
    {
        architecture.RegisterSystem(new GameStateMachineSystem().Also(it =>
        {
            it.Register(new AppState());
        }));
    }
}
