using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using GFramework.Core.Abstractions.Command;
using GFramework.Core.SourceGenerators.Abstractions.Logging;
using GFramework.Core.SourceGenerators.Abstractions.Rule;

namespace Bladesmith2048.Tests;

/// <summary>
///     仓库一致性自检：守护规范、配置与本地化数据不随时间漂移。
///     不依赖 Godot 运行时，纯反射 + 文件系统检查。
/// </summary>
public class RepositoryConsistencyTests
{
    private static readonly Assembly TemplateAssembly = typeof(global::Bladesmith2048.global.GameEntryPoint).Assembly;

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is { Parent: not null })
        {
            // 用 project.godot 而不是 csproj 识别仓库根：csproj 名字会被
            // tools/rename-project.ps1 改掉，硬编码项目名会让守卫在改名后直接抛异常
            if (Directory.Exists(Path.Combine(dir.FullName, "resource", "schemas")) &&
                File.Exists(Path.Combine(dir.FullName, "project.godot")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("仓库根目录未找到");
    }

    // ── 规范守护 ──────────────────────────────────────────────

    [Fact]
    public void 命令类_必须全部为_sealed()
    {
        var commandTypes = TemplateAssembly.GetTypes()
            .Where(static t => t is { IsClass: true, IsAbstract: false, IsInterface: false })
            .Where(static t => typeof(ICommand).IsAssignableFrom(t) ||
                               typeof(IAsyncCommand).IsAssignableFrom(t) ||
                               t.GetInterfaces().Any(static i =>
                                   i.IsGenericType &&
                                   (i.GetGenericTypeDefinition() == typeof(ICommand<>) ||
                                    i.GetGenericTypeDefinition() == typeof(IAsyncCommand<>))))
            .ToArray();

        Assert.NotEmpty(commandTypes);
        Assert.All(commandTypes, static t => Assert.True(t.IsSealed, $"命令类 {t.Name} 必须声明为 sealed"));
    }

    [Fact]
    public void 命令输入类_必须为_class_禁止_struct()
    {
        // 先按接口挑出所有命令输入，再断言它们不是值类型：
        // 若沿用 IsClass 预筛，struct 会先被过滤掉，这条守卫就永远是绿的
        var inputTypes = TemplateAssembly.GetTypes()
            .Where(static t => !t.IsAbstract && !t.IsInterface)
            .Where(static t => typeof(global::GFramework.Cqrs.Abstractions.Cqrs.Command.ICommandInput).IsAssignableFrom(t))
            .ToArray();

        Assert.NotEmpty(inputTypes);
        Assert.All(inputTypes, static t =>
        {
            Assert.False(t.IsValueType, $"命令输入 {t.Name} 禁止用 struct");
            Assert.True(t.IsSealed, $"命令输入 {t.Name} 必须声明为 sealed");
        });
    }

    [Fact]
    public void 事件类_必须全部为_sealed()
    {
        // 事件位于 scripts/cqrs/<domain>/events/（命名空间 ...cqrs.<domain>.events）
        var eventTypes = TemplateAssembly.GetTypes()
            .Where(static t => t is { IsClass: true, IsAbstract: false, IsPublic: true })
            .Where(static t => t.Namespace is not null &&
                               t.Namespace.Contains(".cqrs.", StringComparison.Ordinal) &&
                               t.Namespace.EndsWith(".events", StringComparison.Ordinal))
            .ToArray();

        // 基线里没有事件类（事件随演示域一并移除），因此不能断言非空：
        // 新项目新增事件后，循环仍会强制它们 sealed
        Assert.All(eventTypes, static t => Assert.True(t.IsSealed, $"事件类 {t.Name} 必须声明为 sealed"));
    }

    [Fact]
    public void 语法糖注入_必须调用对应生成方法()
    {
        var root = FindRepoRoot();
        var checks = new (string Attribute, string Method)[]
        {
            ("GetNode", "__InjectGetNodes_Generated()"),
            ("BindNodeSignal", "__BindNodeSignals_Generated()"),
            ("GetSystem", "__InjectContextBindings_Generated()"),
            ("GetModel", "__InjectContextBindings_Generated()"),
            ("GetUtility", "__InjectContextBindings_Generated()")
        };

        var groups = new[] { "scripts", "global" }
            .Select(dir => Path.Combine(root, dir))
            .Where(Directory.Exists)
            .SelectMany(dir => Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .GroupBy(path =>
            {
                var name = Path.GetFileName(path);
                return new { Dir = Path.GetDirectoryName(path), Class = name[..name.IndexOf('.')] };
            });

        var violations = new List<string>();
        foreach (var group in groups)
        {
            // 去掉注释行，避免文档示例里的特性被当成真实用法
            var text = string.Join('\n', group.Select(path => string.Join('\n',
                File.ReadAllLines(path).Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)))));

            foreach (var (attribute, method) in checks)
            {
                var used = text.Contains($"[{attribute}]", StringComparison.Ordinal) ||
                           text.Contains($"[{attribute}(", StringComparison.Ordinal);
                if (used && !text.Contains(method, StringComparison.Ordinal))
                {
                    violations.Add($"{group.Key.Class}: 使用了 [{attribute}] 却未调用 {method}");
                }
            }
        }

        Assert.True(violations.Count == 0,
            $"以下类漏调生成方法，注入字段会是 null：{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void 命名空间_必须与目录一致()
    {
        var root = FindRepoRoot();
        var violations = new List<string>();

        foreach (var (dir, prefix) in new[]
                 {
                     (Path.Combine(root, "scripts"), "Bladesmith2048.scripts"),
                     (Path.Combine(root, "global"), "Bladesmith2048.global")
                 })
        {
            foreach (var path in Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories))
            {
                if (path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                {
                    continue;
                }

                var declaration = File.ReadLines(path)
                    .Select(static line => line.Trim())
                    .FirstOrDefault(static line => line.StartsWith("namespace ", StringComparison.Ordinal));
                if (declaration is null)
                {
                    continue; // GlobalUsings.cs 没有命名空间
                }

                var actual = declaration["namespace ".Length..].TrimEnd(';').Trim();
                var relativeDir = Path.GetRelativePath(root, Path.GetDirectoryName(path)!);
                var segments = relativeDir.Split(Path.DirectorySeparatorChar);
                var expected = prefix + (segments.Length > 1 ? "." + string.Join('.', segments.Skip(1)) : string.Empty);
                if (!string.Equals(actual, expected, StringComparison.Ordinal))
                {
                    violations.Add($"{relativeDir}: {actual} 应为 {expected}");
                }
            }
        }

        Assert.True(violations.Count == 0,
            $"命名空间必须与目录一一对应：{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void 脚本目录_必须为_snake_case()
    {
        var root = FindRepoRoot();
        var offenders = Directory.GetDirectories(Path.Combine(root, "scripts"), "*", SearchOption.AllDirectories)
            .Where(static dir => !dir.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Where(static dir => Path.GetFileName(dir).Any(char.IsUpper))
            .Select(dir => Path.GetRelativePath(root, dir))
            .OrderBy(static dir => dir, StringComparer.Ordinal)
            .ToArray();

        Assert.True(offenders.Length == 0,
            $"目录名必须为 snake_case：{Environment.NewLine}{string.Join(Environment.NewLine, offenders)}");
    }

    [Fact]
    public void 场景引用的脚本_必须存在且_uid_一致()
    {
        var root = FindRepoRoot();
        var violations = new List<string>();
        var assets = new[] { "scenes", "global", "resource" }
            .Select(dir => Path.Combine(root, dir))
            .Where(Directory.Exists)
            .SelectMany(dir => Directory.GetFiles(dir, "*.tscn", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(dir, "*.tres", SearchOption.AllDirectories)));

        foreach (var asset in assets)
        {
            foreach (var line in File.ReadLines(asset))
            {
                const string pathMarker = "path=\"res://scripts/";
                var pathAt = line.IndexOf(pathMarker, StringComparison.Ordinal);
                if (pathAt < 0)
                {
                    continue;
                }

                var pathStart = pathAt + "path=\"res://".Length;
                var pathEnd = line.IndexOf('"', pathStart);
                var resourcePath = line[pathStart..pathEnd];
                var scriptPath = Path.Combine(root, resourcePath.Replace('/', Path.DirectorySeparatorChar));
                var relative = Path.GetRelativePath(root, asset);

                if (!File.Exists(scriptPath))
                {
                    violations.Add($"{relative}: 脚本不存在 {resourcePath}");
                    continue;
                }

                const string uidMarker = "uid=\"uid://";
                var uidAt = line.IndexOf(uidMarker, StringComparison.Ordinal);
                if (uidAt < 0)
                {
                    continue;
                }

                var uidStart = uidAt + "uid=\"".Length;
                var uidEnd = line.IndexOf('"', uidStart);
                var expectedUid = line[uidStart..uidEnd];
                var uidFile = scriptPath + ".uid";
                if (!File.Exists(uidFile))
                {
                    violations.Add($"{relative}: 缺少 .uid {resourcePath}");
                    continue;
                }

                var actualUid = File.ReadAllText(uidFile).Trim();
                if (!string.Equals(actualUid, expectedUid, StringComparison.Ordinal))
                {
                    violations.Add($"{relative}: uid 不一致 {resourcePath}（场景 {expectedUid} / 文件 {actualUid}）");
                }
            }
        }

        Assert.True(violations.Count == 0,
            $"场景引用的脚本与 .uid 必须一致：{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    [Fact]
    public void 脚本模板_必须使用占位符且禁止_ConfigureAwait()
    {
        var root = FindRepoRoot();
        var nodeDir = Path.Combine(root, "script_templates", "Node");
        var templates = Directory.GetFiles(nodeDir, "*.cs");

        Assert.NotEmpty(templates);
        foreach (var file in templates)
        {
            var text = File.ReadAllText(file);
            Assert.StartsWith("// meta-name:", text, StringComparison.Ordinal);
            Assert.Contains("_CLASS_", text, StringComparison.Ordinal);
            Assert.Contains("_BASE_", text, StringComparison.Ordinal);

            var code = string.Join(Environment.NewLine, File.ReadAllLines(file)
                .Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)));
            Assert.DoesNotContain("ConfigureAwait(false)", code, StringComparison.Ordinal);
        }

        var uiPageDir = Path.Combine(root, "script_templates", "UiPage");
        foreach (var file in Directory.GetFiles(uiPageDir, "*.cs"))
        {
            var code = string.Join(Environment.NewLine, File.ReadAllLines(file)
                .Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)));
            Assert.DoesNotContain("ConfigureAwait(false)", code, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Godot节点_ContextAware与Log必须成对()
    {
        var contextAwareTypes = TemplateAssembly.GetTypes()
            .Where(static t => t is { IsClass: true, IsAbstract: false })
            .Where(static t => t.GetCustomAttribute<ContextAwareAttribute>() is not null)
            .ToArray();

        Assert.NotEmpty(contextAwareTypes);
        Assert.All(contextAwareTypes, static t => Assert.True(
            t.GetCustomAttribute<LogAttribute>() is not null,
            $"类 {t.Name} 标注了 [ContextAware] 却缺少配对的 [Log]"));
    }

    // ── 配置与本地化数据守护 ──────────────────────────────────

    [Fact]
    public void 每个schema_都有对应配置目录与数据()
    {
        var root = FindRepoRoot();
        var schemaFiles = Directory.GetFiles(Path.Combine(root, "resource", "schemas"), "*.schema.json");

        Assert.NotEmpty(schemaFiles);
        foreach (var schemaPath in schemaFiles)
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(schemaPath));
            var configPath = doc.RootElement.TryGetProperty("x-gframework-config-path", out var p)
                ? p.GetString()
                : Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(schemaPath));

            Assert.False(string.IsNullOrWhiteSpace(configPath), $"{Path.GetFileName(schemaPath)} 缺少配置目录声明");

            var dataDir = Path.Combine(root, "resource", configPath!.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(Directory.Exists(dataDir), $"schema 声明的配置目录不存在: {configPath}");
            Assert.NotEmpty(Directory.GetFiles(dataDir, "*.yaml"));
        }
    }

    [Fact]
    public void 本地化_各语言表结构一致()
    {
        var root = FindRepoRoot();
        var localizationRoot = Path.Combine(root, "resource", "localization");
        var languages = Directory.GetDirectories(localizationRoot);

        Assert.True(languages.Length >= 2, "至少应有两种语言用于演示/验证回退");

        // 各语言的表文件名集合应一致
        var tableSets = languages
            .Select(lang => (Lang: Path.GetFileName(lang),
                Tables: Directory.GetFiles(lang, "*.json")
                    .Select(Path.GetFileName)
                    .OrderBy(static n => n, StringComparer.Ordinal)
                    .ToArray()))
            .ToArray();

        var reference = tableSets[0];
        foreach (var (lang, tables) in tableSets.Skip(1))
        {
            Assert.True(reference.Tables.SequenceEqual(tables),
                $"语言 {lang} 的表文件与 {reference.Lang} 不一致：{string.Join(",", tables)}");
        }

        // 同表在不同语言下 key 集合应一致（防漏翻译）
        var tableNames = reference.Tables.Select(static n => Path.GetFileNameWithoutExtension(n)!).ToArray();
        foreach (var table in tableNames)
        {
            var keySets = languages.Select(lang =>
            {
                var path = Path.Combine(lang, $"{table}.json");
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                return (Lang: Path.GetFileName(lang),
                    Keys: doc.RootElement.EnumerateObject()
                        .Select(static kv => kv.Name)
                        .OrderBy(static k => k, StringComparer.Ordinal)
                        .ToArray());
            }).ToArray();

            var baseline = keySets[0];
            foreach (var (lang, keys) in keySets.Skip(1))
            {
                var missing = baseline.Keys.Except(keys, StringComparer.Ordinal).ToArray();
                var extra = keys.Except(baseline.Keys, StringComparer.Ordinal).ToArray();
                Assert.True(missing.Length == 0 && extra.Length == 0,
                    $"表 {table} 在 {baseline.Lang} 与 {lang} 的 key 集合不一致；缺失=[{string.Join(",", missing)}] 多余=[{string.Join(",", extra)}]");
            }
        }
    }

    [Fact]
    public void UiKey_枚举值数量与页面场景注册数一致()
    {
        var root = FindRepoRoot();
        var uiKeyCount = Enum.GetValues<global::Bladesmith2048.scripts.enums.ui.UiKey>().Length;

        var entryScene = File.ReadAllText(Path.Combine(root, "global", "game_entry_point.tscn"));

        // 只统计 UiPageConfigs 数组本身：每个已注册页面一个 SubResource。
        // 不能按「metadata/_custom_type_script 出现次数」算，那会把 GameSceneConfigs 也算进去。
        var assignment = Regex.Match(entryScene,
            @"UiPageConfigs\s*=\s*Array\[Object\]\(\[(?<list>.*?)\]\)",
            RegexOptions.Singleline | RegexOptions.ExplicitCapture,
            TimeSpan.FromSeconds(1));
        Assert.True(assignment.Success, "game_entry_point.tscn 缺少 UiPageConfigs 声明");

        var registeredCount = Regex.Matches(assignment.Groups["list"].Value, @"SubResource\(",
            RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1)).Count;

        Assert.True(uiKeyCount > 0, "UiKey 至少应有一个页面键");
        Assert.True(registeredCount > 0, "game_entry_point.tscn 应注册至少一个 UiPageConfig");
        Assert.Equal(uiKeyCount, registeredCount);
    }
}