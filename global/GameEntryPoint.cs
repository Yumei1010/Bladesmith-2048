using GFramework.Core.Abstractions.Architectures;
using GFramework.Core.Abstractions.Logging;
using GFramework.Core.Abstractions.Properties;
using GFramework.Core.Abstractions.State;
using GFramework.Core.Architectures;
using GFramework.Game.Abstractions.Setting;
using GFramework.Game.Setting.Events;
using GFramework.Godot.Coroutine;
using GFramework.Godot.Logging;
using GFramework.Godot.Scene;
using GFramework.Godot.UI;
using GFramework.Ecs.Arch.Abstractions;
using GFramework.Ecs.Arch.Extensions;
using Bladesmith2048.scripts.core;
using Bladesmith2048.scripts.core.environment;
using Bladesmith2048.scripts.core.resource;
using Bladesmith2048.scripts.core.state;
using Bladesmith2048.scripts.framework.command;
using Bladesmith2048.scripts.framework.logging;
using Bladesmith2048.scripts.enums.scene;
using Bladesmith2048.scripts.cqrs.setting.command;
using Godot.Collections;

namespace Bladesmith2048.global;

/// <summary>
///     入口点：建架构 → 注册资源 → 等 UiRoot 就绪 → 进 AppState；同时驱动 ECS 与退出存盘。
/// </summary>
[Log]
[ContextAware]
public partial class GameEntryPoint : Node
{
    [Export] public bool IsDev { get; set; } = true;

    [Export] public Array<UiPageConfig> UiPageConfigs { get; set; } = null!;

    [Export] public Array<SceneConfig> GameSceneConfigs { get; set; } = null!;

    private IGodotSceneRegistry _sceneRegistry = null!;
    private ISettingsModel _settingsModel = null!;
    private ISettingsSystem _settingsSystem = null!;
    private IGodotUiRegistry _uiRegistry = null!;

    /// <summary>架构初始化后从容器收集的 ECS 系统，每帧统一驱动（页面不要自己驱动域系统）。</summary>
    private IReadOnlyList<IArchSystemAdapter<float>> _ecsSystems = [];
    private bool _ecsReady;

    public static IArchitecture Architecture { get; private set; } = null!;

    public static SceneTree Tree { get; private set; } = null!;

    public static SessionLogFileInfo? SessionLogFile { get; private set; }

    private static SessionFileLoggerFactoryProvider _logProvider = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        Tree = GetTree();

        // 初始化会话日志文件（先于架构创建，供 LoggerProperties 引用）
        var logDirectory = ProjectSettings.GlobalizePath("user://logs/");
        SessionLogFileHelper.PruneOldSessions(logDirectory);   // 清理历史会话日志，只保留最近若干份
        SessionLogFile = SessionLogFileHelper.CreateNow(logDirectory);
        _logProvider = new SessionFileLoggerFactoryProvider(SessionLogFile.FullPath)
        {
            MinLevel = LogLevel.Debug
        };
        LogOpenHelper.PrintSessionLogPath(SessionLogFile.FullPath);

        var arch = new GameArchitecture(new ArchitectureConfiguration
        {
            LoggerProperties = new LoggerProperties
            {
                LoggerFactoryProvider = _logProvider
            }
        }, IsDev ? new GameDevEnvironment() : new GameMainEnvironment())
        .UseArch(); // 接入 Arch ECS（World 注册进容器，可选：UseArch(options => options.WorldCapacity = 2048)）
        Architecture = arch;
        arch.Initialize();

        // ECS 更新驱动：ArchEcsModule 在 service module 阶段就完成了系统收集，
        // 而业务系统是在 InstallModules() 里注册的（晚于该阶段），模块内部的列表会是空的。
        // 因此从容器取全部适配器统一驱动；系统本身仍由架构生命周期负责 Initialize/Destroy。
        _ecsSystems = arch.Context.GetSystems<IArchSystemAdapter<float>>();
        _ecsReady = true;
        try { GameContext.Bind(typeof(GameArchitecture), arch.Context); }
        catch (InvalidOperationException) { /* 上下文已绑定 */ }
        _settingsModel = this.GetModel<ISettingsModel>()!;

        // 先注册事件再初始化设置，避免异步发送错过订阅（0.7.1 时序变化）
        this.RegisterEvent<SettingsInitializedEvent>(e =>
        {
            _settingsSystem = this.GetSystem<ISettingsSystem>()!;
            _ = _settingsSystem.ApplyAll();
            _log.Info("设置已加载");
        });

        _ = _settingsModel.InitializeAsync();

        _sceneRegistry = this.GetUtility<IGodotSceneRegistry>()!;
        _uiRegistry = this.GetUtility<IGodotUiRegistry>()!;

        foreach (var gameSceneConfig in GameSceneConfigs) _sceneRegistry.Registry(gameSceneConfig);
        foreach (var uiPageConfig in UiPageConfigs) _uiRegistry.Registry(uiPageConfig);

        if (ShouldEnterAppState())
            this.RegisterEvent<UiRoot.UiRootReadyEvent>(_evt =>
            {
                _ = this.GetSystem<IStateMachineSystem>()!.ChangeToAsync<AppState>();
            });

        _log.Debug("框架入口点就绪.");
        _log.Debug($"ECS 更新驱动就绪，系统数 {_ecsSystems.Count}.");
        CallDeferred(nameof(CallDeferredInit));
    }

    private static void CallDeferredInit()
    {
        Timing.Prewarm();
    }

    private bool ShouldEnterAppState()
    {
        var tree = GetTree();
        var currentScene = tree.CurrentScene;
        if (currentScene == null)
            return false;
        var scenePath = currentScene.SceneFilePath;
        return string.Equals(scenePath, _sceneRegistry.Get(nameof(SceneKey.Main)).GetPath(),
            StringComparison.Ordinal);
    }

    /// <summary>每帧驱动 ECS 系统（delta 以秒计）。</summary>
    public override void _Process(double delta)
    {
        if (!_ecsReady)
            return;

        var deltaTime = (float)delta;
        foreach (var system in _ecsSystems)
            system.Update(in deltaTime);
    }

    /// <summary>打开本次会话日志所在目录（F12 用）。</summary>
    public static void OpenSessionLogDirectory()
    {
        if (SessionLogFile == null)
        {
            GD.PrintErr("[Log] 会话日志尚未初始化。");
            return;
        }

        LogOpenHelper.OpenLogDirectory(SessionLogFile.DirectoryPath);
    }

    /// <summary>IsDev 时按 F12 打开日志目录。</summary>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsDev)
        {
            return;
        }

        if (@event is InputEventKey { Pressed: true, Keycode: Key.F12 })
        {
            OpenSessionLogDirectory();
            GetViewport().SetInputAsHandled();
        }
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        _logProvider?.Flush();
        // 退出时不能 await：用扩展方法兜住异常并写日志，而不是把 Task 丢掉
        this.SendCommandDetached(new SaveSettingsCommand(), _log);
    }
}
