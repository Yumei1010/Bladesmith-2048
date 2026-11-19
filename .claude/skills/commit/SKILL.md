---
name: commit
description: 按逻辑单元分组并提交所有更改
---

# 提交技能

你的任务是将当前未提交的更改，按逻辑拆分为多次有意义的变更块。

1.  **分析逻辑单元**：运行 `git status` 和 `git diff`，完整审查所有更改并列出逻辑上独立的单元。
2.  **生成提交群组计划**：**严禁使用 `git add .` / `git add -A`**，为每个逻辑单元规划具体的文件列表。
    *   *群组 1（领域代码）*: `scripts/cqrs/<domain>/command/`, `scripts/cqrs/<domain>/events/`
    *   *群组 2（表现层）*: `scripts/ui/<kind>/<page>/`, `scenes/ui/<kind>/<page>/`
    *   *群组 3（资源/工程配置）*: `resource/`, `project.godot`, `*.csproj`
3.  **逐步校验与提交**：对每个计划，向用户展示并简要说明，经确认后，**仅暂存该组文件**并生成规范的提交信息。
    格式 `<type>(<scope>): <中文描述>`；一次提交只做一件逻辑上完整的事，提交前跑一次 `dotnet build`。
