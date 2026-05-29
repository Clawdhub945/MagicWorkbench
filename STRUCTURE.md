---
name: magic-workbench-structure
description: 魔法工作台Mod项目结构规范 - 目录布局、文件职责、添加新建筑步骤
metadata: 
  node_type: memory
  type: project
  originSessionId: 42404f7d-a34a-4d97-99a9-89e69c25affa
---

## 项目结构规范

项目路径: `C:\AI\mod\MagicWorkbench`

```
MagicWorkbench/
├── About/About.xml
├── Defs/           ← 游戏数据 JSON（自动加载，无需代码）
│   ├── stuff.json      物品/建筑定义
│   ├── build.json      建筑配置
│   ├── blueprint.json  配方
│   ├── tech.json       科技解锁
│   └── tech_tree.json  科技树
├── Textures/       ← 贴图 PNG + textures.xml（自动加载）
├── Sounds/         ← 音频 .wav/.mp3（自动加载）
├── Languages/      ← 翻译文本（自动加载）
├── Plugin/         ← C# 代码（仅限框架不支持的功能）
│   ├── MagicWorkbenchPlugin.cs   纯入口，调用 Register + Patch
│   ├── Core/                      可复用基础设施
│   │   ├── BuildingRegistry.cs    建筑注册表
│   │   ├── SpriteHelper.cs        精灵替换
│   │   └── WindowNullGuard.cs     UI空保护
│   ├── Buildings/                 每个建筑一个文件（~20行）
│   │   └── MagicWorkbench.cs
│   └── Patches/                   Harmony补丁（按功能分组）
│       └── BuildFacilityPatch.cs
└── .gitignore
```

## 核心原则

- **Defs/ 数据驱动**：能用 JSON 配置的绝不写代码
- **Plugin/ 最小化**：只用于预制体替换、运行时贴图替换、UI空保护
- **每个 .cs 文件不超过 800 行**
- **新建筑只在 Buildings/ 加一个注册文件 + 入口加一行调用**

## 添加新建筑步骤

1. `Defs/stuff.json` — 添加 stuff 定义
2. `Defs/build.json` — 添加 build 配置
3. `Defs/blueprint.json` — 添加配方（可选）
4. `Defs/tech.json` — 添加科技解锁（可选）
5. `Textures/` — 添加贴图 PNG + 更新 textures.xml
6. `Plugin/Buildings/NewBuilding.cs` — `BuildingRegistry.Register(id, prefab, sprite)`
7. `Plugin/MagicWorkbenchPlugin.cs` — 加一行 `NewBuilding.Register()`

## Git 信息

- 用户名: Clawdhub945
- 代理: http://127.0.0.1:7890
- 仓库: https://github.com/Clawdhub945/MagicWorkbench.git

**Why:** 后续会添加几十个建筑和新玩法，必须保持结构一致。
**How to apply:** 每次添加新建筑或功能时，严格按此结构放置文件，不要在 Core/ 之外写通用工具代码。
