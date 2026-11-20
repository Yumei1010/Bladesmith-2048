# 2048 棋盘规则（可抽离的纯逻辑模块）

零依赖：不引用 Godot，也不引用任何框架。整个目录可以原样拷进别的 .NET 项目
（控制台、Unity、Godot、纯单元测试），只需改命名空间。

> 想从零自己写一个？看 [`TUTORIAL.md`](TUTORIAL.md)：规则推导、算法推导、测试清单、
> 常见坑与扩展点，全部按“怎么做出来的”顺序写。

## 文件

| 文件 | 作用 | 代码行 |
|---|---|---|
| `ChessboardState.cs` | 全部规则：挤压、合并、生成、胜负判定 | ~138 |
| `MoveDirection.cs` | 8 个方向 | 8 |
| `MoveResult.cs` | 一次移动的结果（分数 + 每块方块去向） | 10 |
| `TileTransition.cs` | 单块方块的去向 `(From, To, Merged)` | 5 |
| `ChessboardSnapshot.cs` | 给表现层的只读快照 | 8 |

对外只有 `ChessboardState` 一个门面，其余是它的返回值类型。

## 用法

```csharp
var board = new ChessboardState();              // 空格盘；也可 new ChessboardState(cells) 读档
var random = new Random();

board.TrySpawn(random);                          // 生成方块：90% 出 2，10% 出 4
board.TrySpawn(random);

var result = board.Move(MoveDirection.Left);     // 走一步
if (result.Moved)                                // 被顶死时不要生成新方块
    board.TrySpawn(random);

Console.WriteLine($"{board.Score} {board.HasWon} {board.IsGameOver}");
foreach (var t in result.Transitions)
    Console.WriteLine($"{t.From} -> {t.To}{(t.Merged ? " (被合并)" : "")}");
```

## 数据表示

一维 `int[16]`，`index = row * 4 + col`，`0` 表示空。没有二维数组：行列对角线都能用同一个
下标公式表达，缓存也更好。

## 三个不变量（每条都有测试钉住）

1. **一次移动里一块方块最多合并一次**：`[4,4,8]` 左移得 `[8,8]`，不会连锁成 16。
   实现靠 `ApplyLine` 里的 `merged` 标记。
2. **无效移动不生成方块**：`MoveResult.Moved == false` 时调用方不要 spawn，
   否则疯狂按方向键就能白刷方块。
3. **满盘时「能移动」等价于「能合并」**：所以 `IsGameOver` 只需要判断“占满 + 任一方向
   线上出现相邻同值”。

## 8 个方向是怎么统一起来的

一行、一列、一条对角线都是「线」。把方向写成单位向量 `(dr, dc)`，然后：

> 再沿方向走一步就出界的格子，是某条线的**尽头**；从尽头逆着方向走回边界，就是一条线。

这条规则自动切出全部 8 个方向的线，不需要按方向写分支；线表在类型初始化时算一次，
`Move` 里只剩遍历线 + 就地合并。

```
Left (0,-1)      Up (-1,0)        UpLeft (-1,-1)     UpRight (-1,1)
每条线：行       每条线：列        每条线：主对角线   每条线：副对角线
尽头：最左       尽头：最上        尽头：左上         尽头：右上
```

`Lines[方向][第几条线][沿线第几格] = 格子下标`，`offset 0` 就是尽头（方块挤向的那一端）。

## 挤压算法

`ApplyLine` 一趟扫完一条线：方块依次写回靠前的格子，遇到同值就翻倍写进上一格。

- 写位置永远 ≤ 已读格数，所以能**直接在原数组上覆盖**，不需要缓冲数组；
- 合并后置 `merged = true`，保证本回合不再参与合并；
- 一趟结束把这条线尾巴上的格子清零。

## 给表现层的挂钩

`MoveResult.Transitions` 是渲染动画的唯一依据：

| From | To | Merged | 表现层该做什么 |
|---|---|---|---|
| A | B ≠ A | false | 方块从 A 滑到 B |
| A | A | false | 原地不动（也要发，UI 靠它判断“这格不是新生成的”） |
| A | B | true | 滑到 B 后消失，B 的值翻倍 |

## 测试

```bash
dotnet test --filter ChessboardStateTests
```

覆盖：四方向与四斜向的挤压/合并、只合一次、短对角线、单格对角线、被顶死、终局与胜利判定、
生成概率与空格选择、以及每条 transitions 的 From/To/Merged。

## 抽离步骤

1. 拷走本目录 5 个 `.cs`；
2. 全局替换命名空间 `Bladesmith2048.scripts.game.chessboard.` → 你的；
3. 拷走测试文件 `tests/Bladesmith2048.Tests/ChessboardStateTests.cs`，同样替换命名空间；
4. 表现层按上表消费 `Transitions` 即可，规则层不需要任何改动。
