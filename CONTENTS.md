# FBE 内容与原版改动清单

本文档基于当前 `FBE` 源码整理。对照的原版源码版本为 `STS2 0.106.0`。

## 新增内容

### 卡牌

- 铁甲战士：恶魔（`Demon`）、波动拳（`HaDouKen`）
- 静默猎手：工业！！！（`Industry`）、血清（`Serum`）
- 故障机器人：静电释放（`StaticDischarge`）
- 亡灵缚者：血债（`BloodFeud`）、死体组织（`DeadTissue`）、留了一手（`KeepingAHand`）、时间雨（`TimeFall`）、遣返者（`Repatriate`）、虚幻引擎（`UnrealEngine`）
- 摄政者：创世纪（`Genesis2`）、无限剑制（`UnlimitedBladeWorks`）
- 无色：汇集（`Aggregate`）、东征（`Crusade`）、巨人杀手（`GiantKiller`）、招商引资（`InvestmentPromotion`）、操控现实（`MasterReality`）
- 诅咒：岁月（`Years`）

### 遗物

- 通用遗物：高礼帽（`TopHat`）
- 事件遗物：洞见（`Insight`）、赎罪（`Redemption`）、被拔掉的路标（`RemovedRoadSign`）、石之剑MKⅡ（`SwordOfStoneMk2`）

### 事件

- 时间的回响（`Reflection`）
- 奇怪的路标（`StrangeRoadSign`）
- 恶魔房（`DevilRoom`）
- 无迹之塔（`QuantumTower`）

`TestEvent` 及其本地化文本存在于仓库中，但事件源码当前整体被注释掉，未作为实际内容启用。

### 能力

- 彩虹（`RainbowPower`）
- 冷却剂（`CoolantPower`）
- 恶魔（`DemonPower`）
- 额外回合用隐藏能力（`ExtraTurnPower`）
- 创世纪（`GenesisPower2`）
- 留了一手（`KeepingAHandPower`）
- 操控现实（`MasterRealityPower`）
- 遣返者（`RepatriatePower`）
- 静电释放（`StaticDischargePower`）
- 剑圣（`SwordSagePower2`）
- 时间雨（`TimeFallPower`）
- 无限剑制（`UnlimitedBladeWorksPower`）
- 虚幻引擎（`UnrealEnginePower`）

### 附魔

- 量子态（`Quantinized`）
- 无限剑制（`UbwEnchantment`）

### 资源与本地化

- 新增卡牌、遗物、事件、能力、附魔的图片资源。
- 新增事件音乐、事件音效、死亡音效、爆炸音效等音频资源。
- 新增简体中文本地化：卡牌、遗物、事件、能力、附魔。

## 对原版的改动

### 卡池与事件池

- 从亡灵缚者卡池中移除 `Afterlife`、`SentryMode`。
- 从摄政者卡池中移除 `Genesis`。
- 禁用原版事件 `LostWisp`。
- 向共享事件池追加 FBE 自定义事件。

### 原版事件

- 改写 `SunkenStatue` 的变量、初始选项和“拔剑”结果。
- `SunkenStatue` 的拔剑选项改为获得 `SwordOfStoneMk2`。

### 原版卡牌

- `Acrobatics`：稀有度改为普通。
- `Snakebite`：中毒数值改为 8。
- `EternalArmor`：`PlatingPower` 数值改为 10，卡牌类型改为技能。
- `Bolas`：基础伤害改为 7，升级时额外增加 3 点伤害。
- `RollingBoulder`：费用改为 2，`RollingBoulderPower` 数值改为 10，升级时额外增加 5。
- `Mayhem`：费用改为 1。
- `BladeDance`：升级效果改为获得保留。
- `Rainbow`：改为能力牌；移除原关键词；基础变量改为 `Repeat=4`；打出时获得并填充随机充能球栏位，同时施加 `RainbowPower`；升级时 `Repeat+1`。
- `Coolant`：打出时改为施加 `CoolantPower`；悬停提示改为充能球和冰霜；升级效果改为获得固有；构造器 patch 中设置费用为 1。
- `BouncingFlask`：基础变量改为中毒 2、重复 4；升级时重复次数增加 2。
- `SwordSage`：简体中文描述被覆盖为“君王之剑获得重放1”。

### 原版能力

- `EnragePower`：技能牌触发力量的概率改为 `1 / 玩家数`，并覆盖对应动态变量和简体中文描述。

### 战斗、选择与同步

- 在单选和单张升级选择界面中，选择 1 张牌后自动完成确认。
- 修复/规避玩家回合开始阶段过早调用 `PlayerCmd.EndTurn` 的问题：过早的结束回合请求会延后到更安全的时机执行，并在战斗结束、败北处理和战斗重置时清理缓存。
- `SovereignBlade` 多段攻击时，攻击动画延迟减半。
- 新增默认关闭的同步调试追踪 patch；`Entry.EnableSyncDebugTracePatches` 当前为 `false`，正常情况下不改变玩法。

### 音频、图标与事件显示

- `SfxCmd.Play` 支持播放 `res://` 自定义音频路径。
- 玩家死亡后播放 FBE 死亡音效。
- 持有 `TopHat` 时，`TheBombPower` 的爆炸结算播放 FBE 爆炸音效。
- FBE 自定义能力和附魔使用自定义图标路径。
- FBE 自定义事件支持自定义初始立绘、背景场景和 VFX 路径。

### 模型与本地化注册

- FBE 自定义模型的本地化 entry 自动加 `FBE-` 前缀。
- FBE 自定义卡牌、遗物通过 `PoolAttribute` 注册到指定原版池。
- 带 `SavedProperty` 的 FBE 自定义模型会注入保存属性类型缓存。
