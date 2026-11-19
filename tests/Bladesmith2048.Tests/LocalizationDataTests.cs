using System;
using System.IO;
using GFramework.Core.Abstractions.Localization;
using GFramework.Core.Localization;

namespace Bladesmith2048.Tests;

/// <summary>
///     本地化数据验证：确认 localization/ 目录的语言表可被加载与切换。
///     使用仓库根真实路径（非 Godot 虚拟路径），不依赖 Godot 运行时。
/// </summary>
public class LocalizationDataTests
{
    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is { Parent: not null })
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "resource", "localization")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("resource/localization 目录未找到（仓库根）");
    }

    private static LocalizationManager CreateManager()
    {
        var manager = new LocalizationManager(new LocalizationConfig
        {
            DefaultLanguage = "zhs",
            FallbackLanguage = "eng",
            LocalizationPath = Path.Combine(FindRepoRoot(), "resource", "localization"),
            EnableHotReload = false,
            ValidateOnLoad = false
        });
        manager.Initialize();
        return manager;
    }

    [Fact]
    public void 本地化数据_中英文表均可加载()
    {
        var manager = CreateManager();

        Assert.Equal("Bladesmith 2048", manager.GetText("common", "app.title"));

        manager.SetLanguage("eng");
        Assert.Equal("Bladesmith 2048", manager.GetText("common", "app.title"));
    }

    [Fact]
    public void 本地化数据_两语言键一致()
    {
        var manager = CreateManager();

        Assert.True(manager.TryGetText("common", "app.title", out var zh));
        manager.SetLanguage("eng");
        Assert.True(manager.TryGetText("common", "app.title", out var en));

        // 产品名通常不翻译，这里只断言两种语言都取得到、且非空
        Assert.False(string.IsNullOrWhiteSpace(zh));
        Assert.False(string.IsNullOrWhiteSpace(en));
    }

    [Fact]
    public void 本地化数据_语言目录按框架语言码组织()
    {
        var root = Path.Combine(FindRepoRoot(), "resource", "localization");

        Assert.True(File.Exists(Path.Combine(root, "zhs", "common.json")));
        Assert.True(File.Exists(Path.Combine(root, "eng", "common.json")));
    }
}