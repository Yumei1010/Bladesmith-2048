using GFramework.Core.Abstractions.Architectures;
using GFramework.Game.Abstractions.Config;
using GFramework.Game.Config;
using GFramework.Game.Config.Generated;
using GFramework.Godot.Config;
using Bladesmith2048.scripts.framework.config;

namespace Bladesmith2048.scripts.module;

/// <summary>
///     Config 系统接入：按生成器元数据加载 resource/schemas 下的全部配置表。
/// </summary>
/// <remarks>
///     用 <see cref="GodotYamlConfigLoader" />：编辑器态读 <c>res://resource</c>，
///     导出态把 YAML/schema 同步到 <c>user://config_cache</c>。
///     表清单来自源生成器（<c>GeneratedConfigCatalog</c>），不要手写表名与路径。
/// </remarks>
public sealed class ConfigModule : IArchitectureModule
{
    public IConfigRegistry? Registry { get; private set; }

    public void Install(IArchitecture architecture)
    {
        var tableSources = GeneratedConfigCatalog.Tables
            .Select(static metadata => new GodotYamlConfigTableSource(
                metadata.TableName,
                metadata.ConfigRelativePath,
                metadata.SchemaRelativePath))
            .ToArray();

        var loader = new GodotYamlConfigLoader(new GodotYamlConfigLoaderOptions
        {
            SourceRootPath = "res://resource",
            RuntimeCacheRootPath = "user://config_cache",
            TableSources = tableSources,
            ConfigureLoader = static yamlLoader => yamlLoader.RegisterAllGeneratedConfigTables()
        });

        var registry = new ConfigRegistry();
        // 装配期同步等待：模块安装是同步契约，加载失败会直接抛出而不是留到首次读取时
        loader.LoadAsync(registry).GetAwaiter().GetResult();
        Registry = registry;

        architecture.RegisterUtility(registry);
        architecture.RegisterUtility(new ConfigRuntime(registry));
    }
}