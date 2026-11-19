// 全局命名空间导入
// 规则：只收录「删掉就会编译失败」的框架级命名空间（已实测逐条必需）；项目命名空间（Bladesmith2048.*）禁止加入。
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;

global using Godot;

global using GFramework.Core.Extensions;
global using GFramework.Core.SourceGenerators.Abstractions.Logging;
global using GFramework.Core.SourceGenerators.Abstractions.Rule;
