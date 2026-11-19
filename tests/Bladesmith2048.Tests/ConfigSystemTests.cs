using System;
using System.IO;
using GFramework.Game.Config;
using GFramework.Game.Config.Generated;

namespace Bladesmith2048.Tests;

/// <summary>
///     GFramework Config 系统验证：加载本地 YAML + schema 校验 + 强类型读取。
///     使用非 Godot 的 YamlConfigLoader（普通文件系统路径），不依赖 Godot 运行时。
///     样例域为 resource/config/sample，覆盖加载数量、强类型字段、生成元数据与缺项行为。
/// </summary>
public class ConfigSystemTests
{
    /// <summary>
    ///     计算仓库根目录（tests 项目在 tests/Bladesmith2048.Tests/ 下两层）。
    /// </summary>
    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is { Parent: not null })
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "resource", "schemas")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("resource/schemas 目录未找到（仓库根）");
    }

    private static ConfigRegistry LoadRegistry()
    {
        var root = Path.Combine(FindRepoRoot(), "resource");
        var registry = new ConfigRegistry();
        var loader = new YamlConfigLoader(root)
            .RegisterTable<string, SampleConfig>(
                SampleConfigBindings.Metadata.TableName,
                SampleConfigBindings.Metadata.ConfigRelativePath,
                SampleConfigBindings.Metadata.SchemaRelativePath,
                static c => c.Id,
                StringComparer.Ordinal);
        loader.LoadAsync(registry).GetAwaiter().GetResult();
        return registry;
    }

    [Fact]
    public void 加载_样例数据全部读取()
    {
        var registry = LoadRegistry();
        var table = registry.GetSampleTable();

        Assert.Equal(2, table.Count);
        Assert.True(table.ContainsKey("sample_a"));
        Assert.True(table.ContainsKey("sample_b"));
    }

    [Fact]
    public void 强类型读取_字段值正确()
    {
        var registry = LoadRegistry();
        var sample = registry.GetSampleTable().Get("sample_a");

        Assert.Equal("Sample A", sample.Label);
        Assert.Equal(10, sample.Weight);
    }

    [Fact]
    public void 生成元数据_表名与路径正确()
    {
        Assert.Equal("sample", SampleConfigBindings.Metadata.TableName);
        Assert.EndsWith("sample.schema.json", SampleConfigBindings.Metadata.SchemaRelativePath,
            StringComparison.Ordinal);
    }

    [Fact]
    public void 目录_未提供的Key查不到()
    {
        var registry = LoadRegistry();
        var table = registry.GetSampleTable();

        Assert.False(table.ContainsKey("sample_c"));
    }
}
