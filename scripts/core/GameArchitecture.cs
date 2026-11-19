using GFramework.Core.Abstractions.Architectures;
using GFramework.Core.Abstractions.Events;
using GFramework.Core.Abstractions.Environment;
using GFramework.Godot.Architectures;
using Bladesmith2048.scripts.module;
using Bladesmith2048.scripts.framework.events;
using Microsoft.Extensions.DependencyInjection;

namespace Bladesmith2048.scripts.core;

/// <summary>
///     架构装配：模块安装顺序 + 用频段版事件总线覆盖框架默认实现。
/// </summary>
public sealed class GameArchitecture(IArchitectureConfiguration configuration, IEnvironment environment) : AbstractArchitecture(configuration, environment)
{
    public IArchitectureConfiguration Configuration { get; } = configuration;

    /// <summary>
    ///     用 <see cref="ChannelEventBus" /> 替换框架的 IEventBus，让原有 RegisterEvent/SendEvent 支持频段。
    /// </summary>
    public override Action<IServiceCollection>? Configurator =>
        services =>
        {
            var bus = new ChannelEventBus();
            // 具体类型与接口都注册，两种解析方式都能拿到同一个实例
            services.AddSingleton(bus);
            services.AddSingleton(typeof(IEventBus), bus);
        };

    /// <summary>
    ///     模块安装顺序是硬依赖，不要调换（Utility → System → Model → Config → State）。
    /// </summary>
    protected override void InstallModules()
    {
        InstallModule(new UtilityModule());
        InstallModule(new SystemModule());
        InstallModule(new ModelModule());
        InstallModule(new ConfigModule());
        InstallModule(new StateModule());
    }
}
