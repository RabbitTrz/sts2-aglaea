# Aglaea（阿格莱雅）—《杀戮尖塔 2》角色 Mod

一个《杀戮尖塔 2》(Slay the Spire 2) 的自制角色 Mod。角色主题是**裁缝与织线**，核心是召唤并指挥一群「衣匠」作战。

- **作者**：RaconTrz
- **版本**：0.1.4
- **依赖**：[BaseLib](https://steamcommunity.com/sharedfiles/filedetails/?id=3737335127) ≥ 3.4.7（**必须**，Mod 不会自己带）

---

## 内容规模

| | 数量 |
|---|---|
| 卡牌 | 93 张（基础 / 普通 / 罕见 / 稀有 / Token / 多人 / 先古） |
| 能力（Power） | 33 个 |
| 遗物 | 7 个 |
| 美术 | 全套卡面、能力图标、遗物图标、战斗立绘、火堆 / 商店立绘、多人猜拳手臂 |

## 核心机制

**衣匠**是召唤物，会攻击、也会替玩家承受伤害。围绕它展开的体系：

- **金丝** — 每层使衣匠攻击 +1
- **至高之姿** — 与金丝成对的核心资源（凡提及至高之姿处必同时提示金丝）
- **间隙织线** — 标记敌人，累计受衣匠攻击若干次后使其失去力量
- **若虫** — 放入抽牌堆、抽到即自动打出的衍生牌
- **半神形态 / 神性 / 诛线 / 锻泪匠躯** 等进阶机制

---

## 玩家：安装

下载 `AglaeaInstaller_Aglaea_v0.1.4.zip`，解压后：

1. 确认已安装 **BaseLib**（游戏 `mods` 目录下应有含 `BaseLib` 的文件夹）
2. **完全关闭游戏**
3. 双击 `AglaeaInstaller.exe`，按提示操作

安装位置：`<游戏目录>\mods\Aglaea\`。卸载 = 直接删掉这个文件夹。

安装器无代码签名，Windows 会弹「已保护你的电脑」，点**更多信息 → 仍要运行**即可。

---

## 开发者：构建与部署

### 环境

| | |
|---|---|
| Godot | 4.5.1 **Mono** 版 |
| .NET | 9.0 |
| 游戏 | 与 `Aglaea.json` 里 `min_game_version` 匹配 |

`SlaytheSpire2_mod_Aglaea.csproj` 里的引用路径**写死了本机的目录结构**（相对路径但带用户名层级），换机器需要改两处：

- 游戏 DLL → `..\..\..\..\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\`
- BaseLib → `..\..\Desktop\BaseLib.3.4.7\BaseLib.dll`

### 构建

```bash
# 1. 编译 C#（产物：.godot/mono/temp/bin/ExportRelease/Aglaea.dll）
dotnet build -c ExportRelease

# 2. 新增过图片 / 场景 / 本地化时，先让 Godot 导入并生成 .import
"<Godot>" --headless --path . --import

# 3. 导出资源包（产物：Aglaea.pck）
"<Godot>" --headless --path . --export-pack "Windows Desktop" Aglaea.pck
```

导出日志里出现 `Export .NET Project: ... assembly not found` 是**正常的**，可以忽略——DLL 是单独发布的。

### 部署

把这三样拷进 `<游戏目录>\mods\Aglaea\`：

```
.godot/mono/temp/bin/ExportRelease/Aglaea.dll
Aglaea.pck
Aglaea.json
```

仓库根目录的 `deploy.bat` 是一键脚本，**但它的 DLL 路径写的是 `ExportRelease\win-x64\`，那个副本不一定是新的**——用之前务必比对时间戳。

### ⚠️ 几个会踩的坑

1. **两个 `ExportRelease` DLL 会不同步。**
   `.godot/mono/temp/bin/ExportRelease/Aglaea.dll`（`dotnet build` 产物）和
   `.../ExportRelease/win-x64/Aglaea.dll`（win-x64 发布产物）**可能一新一旧，方向不定**。
   部署前用时间戳 / MD5 跟源码比对，别无脑信任何一个路径。

2. **改了本地化 / 图片 / 场景，必须重新导出 pck。**
   只改 `.cs` 的话重建 DLL 就够；但 `.json` 文案、`images/`、`scene/` 都在 pck 里。

3. **部署前必须关闭游戏**，否则 DLL / pck 被占用，拷贝会失败。

4. **卡面渲染用的是缩略图，大图只是素材。**
   `images/cards/big/`（1000×760 / 606×852）不参与渲染；渲染走 `images/cards/`（250×190 / 250×350）。
   93 张大图全上渲染路径会占约 280 MB 显存。新增卡面时两套都要生成。

### 打包安装器

```bash
bash tools/build_installer.sh     # 填充 payload + 发布自包含单文件 exe
```

之后把 `dist/AglaeaInstaller.exe` 和 `payload/README.txt` 压成 zip 即可分发。

---

## 目录结构

```
Scripts/                      C# 源码
├── Cards/                    卡牌（按稀有度分目录）
├── Power/                    能力
├── Relic/                    遗物
├── Characters/               角色模型、立绘驱动、动画桥接
├── Commands/                 衣匠召唤等公共命令
├── Monsters/                 衣匠（作为召唤物）
├── CardPools/                卡池
└── Extensions/               路径工具等

Aglaea/                       资源（打进 pck）
├── images/cards/             卡面缩略图（渲染用）
├── images/cards/big/         卡面大图（素材，不渲染）
├── images/powers/            能力图标
├── images/relics/            遗物图标
├── images/battle/            战斗立绘
├── images/charui/            头像 / 地图标记 / 能量球
├── images/ui/hands/          多人猜拳手臂
├── images/rest_site/         火堆立绘
├── images/merchant/          商店立绘
├── scene/                    Godot 场景（战斗 / 火堆 / 商店 / 选人背景）
└── localization/zhs/         中文本地化

tools/
├── installer/                一键安装器源码
├── build_installer.sh        安装器打包脚本
├── gen_card_placeholders.py  卡面占位图批量生成
├── 美术需求报告.txt            美术资产规格与缺口清单
└── hovertip_audit.md         卡牌 / 能力提示链路审计
docs/                         设计待办与待验证事项
```

---

## 开发笔记

- **多人联机是确定性锁步**：Mod 代码里**绝不能**用 `Random.Shared` / `System.Random` 选目标或做随机，
  否则各客户端算出不同结果 → 状态分歧 → 闪退。必须用游戏自带的同步随机流
  （`player.RunState.Rng.CombatTargets` 等）。
- **衣匠伤害公式**：基础 3，**不受力量 / 虚弱影响**，仍受易伤影响；每层金丝 +1。
- **提示统一**：卡牌用 `AglaeaCard` 基类的 `TipTailor / TipPose / TipWeaveLine / TipGoldenThread / TipChrysalis` 常量；
  能力用 `ExtraHoverTips` 声明它自己说明里引用到的其它机制。
- 更多设计规则见 `docs/` 与源码注释。

## 声明

本 Mod 为个人爱好作品，与 Mega Crit 无关。

角色「阿格莱雅」及部分美术素材来源于《崩坏：星穹铁道》，版权归 miHoYo 所有；
本仓库中的相关素材仅用于非商业的爱好者创作。
