# 阿格莱雅 mod —— 卡牌 / Power 提示链路（2026-09-28 统一后）

规则：卡牌用 `TipXxx` 常量引用核心机制；power 用 `ExtraHoverTips` 声明它说明里引用的其它机制。嵌套只展开一层，不递归。

## 一、卡牌（96）

| 卡牌 | 文案提到 | 提示 |
|---|---|---|
| ? `AglaeaArchaicToothPatch` | — | — |
| 万丝齐舞 `AglaeaAllThreadsDance` | 衣匠 | 衣匠 |
| 万帷网 `AglaeaMyriadNet` | 衣匠 | 衣匠 |
| 三织 `AglaeaTripleWeave` | — | 三织 |
| 与我共舞 `AglaeaDanceWithMe` | 衣匠 | 衣匠 |
| 乱针 `AglaeaWildStitch` | — | — |
| 予你盛装 `AglaeaBestowGarb` | — | AglaeaLinkedS |
| 传线连绵 `AglaeaSpreadingThread` | 间隙织线 | 间隙织线 |
| 作茧自缚 `AglaeaCocoonSelfTrap` | 衣匠 | 衣匠 |
| 免织 `AglaeaFreeWeave` | — | 免织 |
| 决织 `AglaeaLastWeave` | — | — |
| 列阵 `AglaeaArray` | 衣匠 | 衣匠 |
| 刺纹之蜜 `AglaeaHoneyedStitch` | 间隙织线 | 间隙织线 |
| 刺纹之陷 `AglaeaStitchSnare` | 衣匠 | 衣匠 |
| 千针刺芒 `AglaeaThousandNeedles` | — | — |
| 半神形态 `AglaeaDemigodForm` | 衣匠、神性 | 神性、衣匠 |
| 华珠之礼 `AglaeaPearlGift` | 至高之姿 | 至高之姿 |
| 即织即战 `AglaeaWeaveAndFight` | 衣匠 | 衣匠 |
| 双唤衣匠 `AglaeaTwinSummon` | 衣匠 | 衣匠 |
| 叠姿 `AglaeaStackForm` | 至高之姿 | 至高之姿 |
| 叠茧 `AglaeaStackCocoon` | 若虫 | 若虫(卡) |
| 史诗终结 `AglaeaEpicFinale` | 至高之姿、金丝 | 金丝、至高之姿、Strength、Dexterity |
| 同衾 `AglaeaSameQuilt` | — | AglaeaLinkedS |
| 命定共舞 `AglaeaDestinedDance` | 至高之姿 | 至高之姿 |
| 命运眼睑 `AglaeaFateEyelid` | 至高之姿 | 至高之姿 |
| 回纺为守 `AglaeaSpinGuard` | 衣匠 | 衣匠 |
| 回缝 `AglaeaReSew` | — | — |
| 复缕 `AglaeaReThread` | — | — |
| 孤线独织 `AglaeaLoneThread` | 衣匠 | 衣匠 |
| 孤锋千吻 `AglaeaLoneKiss` | 间隙织线 | 间隙织线 |
| 密针 `AglaeaFineNeedle` | 衣匠 | 衣匠 |
| 密雨刺 `AglaeaDenseRain` | — | — |
| 广织成网 `AglaeaWideNet` | 衣匠 | 衣匠 |
| 打击 `AglaeaAttack` | — | — |
| 执行者 `AglaeaExecutor` | 间隙织线、衣匠 | 间隙织线、衣匠 |
| 护茧 `AglaeaWardCocoon` | 若虫 | 若虫(卡) |
| 披姿 `AglaeaCladForm` | 至高之姿 | 至高之姿 |
| 披星百裂 `AglaeaStarburst` | 至高之姿 | 至高之姿 |
| 抽丝剥茧 `AglaeaUnravel` | — | — |
| 捻丝 `AglaeaTwist` | 至高之姿、金丝 | 至高之姿、金丝 |
| 援织 `AglaeaAidWeave` | — | AglaeaLinkedS |
| 播种若虫 `AglaeaChrysalisSow` | 若虫 | 若虫(卡) |
| 政务处理 `AglaeaPaperwork` | 若虫 | 若虫(卡) |
| 散力 `AglaeaDisperse` | — | — |
| 断念 `AglaeaBreakThought` | — | Strength |
| 无常金线 `AglaeaFickleGoldThread` | 间隙织线 | 间隙织线 |
| 枕茧 `AglaeaPillowCocoon` | 若虫 | 若虫(卡) |
| 枯草之盈 `AglaeaWitheredPlenty` | — | — |
| 沐浴黄金 `AglaeaBatheGold` | 衣匠、金丝 | 金丝、衣匠 |
| 浪漫合奏 `AglaeaRomanticEnsemble` | — | — |
| 浴光回春 `AglaeaSunbath` | 衣匠 | 衣匠 |
| 火种归还 `AglaeaEmberReturn` | 至高之姿 | 至高之姿、Strength、Dexterity |
| 热狂圆舞 `AglaeaRiotDance` | 衣匠 | 衣匠 |
| 短视之惩 `AglaeaMyopia` | 衣匠 | Weak、衣匠 |
| 破线 `AglaeaBreakThread` | 衣匠 | 衣匠 |
| 筛缕 `AglaeaSiftThread` | — | — |
| 纤毫毕现 `AglaeaRevealFine` | 间隙织线 | 间隙织线、Vulnerable |
| 纬线 `AglaeaWeftLine` | — | AglaeaLinkedS |
| 细针密缕 `AglaeaFineStitch` | — | — |
| 织缕 `AglaeaWeft` | 至高之姿 | 至高之姿 |
| 织运之竭 `AglaeaThreadworn` | 至高之姿 | 至高之姿、Vulnerable |
| 织阵 `AglaeaWeaveLine` | 衣匠 | 衣匠 |
| 绞丝 `AglaeaWring` | 金丝 | 金丝 |
| 绞紧 `AglaeaKnotTight` | 间隙织线 | 间隙织线 |
| 缎舞 `AglaeaSatinDance` | 衣匠 | 衣匠 |
| 编织 `AglaeaWeave` | 衣匠 | 衣匠 |
| 缠丝聚金 `AglaeaEntwineGold` | 衣匠 | 衣匠 |
| 脆若游丝 `AglaeaGossamer` | 间隙织线 | Intangible、间隙织线 |
| 致命浪漫 `AglaeaFatalRomance` | 衣匠 | MonarchsGazeStrengthDown、衣匠 |
| 若虫 `AglaeaChrysalis` | — | — |
| 苦难织者 `AglaeaSorrowWeaver` | 至高之姿 | 至高之姿 |
| 蓄势绵延 `AglaeaEndlessCharge` | 至高之姿 | 至高之姿 |
| 蓄姿 `AglaeaChargeForm` | 至高之姿 | 至高之姿 |
| 虚无之舞 `AglaeaVoidDance` | — | Intangible |
| 蚀茧 `AglaeaCorrodeCocoon` | 若虫 | 若虫(卡) |
| 裁剪 `AglaeaTrim` | 间隙织线 | 间隙织线 |
| 裁断之时 `AglaeaSnipMoment` | 间隙织线 | 间隙织线 |
| 裂帛 `AglaeaTearSilk` | — | — |
| 触丝 `AglaeaTouchThread` | 衣匠 | 衣匠 |
| 诛线 `AglaeaExecuteThread` | 衣匠 | 衣匠强化、衣匠 |
| 连携打击 `AglaeaLinkedStrike` | 衣匠 | 衣匠 |
| 逐火之路 `AglaeaEmberPath` | — | — |
| 逮捕 `AglaeaArrest` | 间隙织线 | 间隙织线 |
| 金丝成障 `AglaeaGoldBarrier` | 至高之姿 | 至高之姿 |
| 金星行列 `AglaeaGoldStarRow` | 衣匠 | 衣匠强化、衣匠 |
| 金石闪烁 `AglaeaGiltSpark` | — | Strength |
| 金线降神 `AglaeaGoldThreadDescend` | 至高之姿 | 至高之姿 |
| 金血浇铸 `AglaeaGiltBloodForge` | 至高之姿 | 至高之姿 |
| 锐丝如刃 `AglaeaEdgeThread` | 衣匠 | 衣匠 |
| 锚定时刻 `AglaeaAnchorMoment` | — | MonarchsGazeStrengthDown |
| 锻泪匠躯 `AglaeaForgedTearBody` | 衣匠 | 衣匠 |
| 防御 `AglaeaDefend` | — | — |
| 静息蓄势 `AglaeaRestAccumulate` | 至高之姿 | 至高之姿 |
| 顺织 `AglaeaSmoothWeave` | — | — |
| 领袖魅力 `AglaeaLeadershipCharm` | 至高之姿 | 至高之姿 |
| 饲茧 `AglaeaFeedCocoon` | 若虫 | 若虫(卡) |

## 二、Power（37）

| 中文名 | Power | 嵌套提示 |
|---|---|---|
| 逮捕 | `ArrestPower` | 间隙织线 |
| 沐浴黄金 | `BatheGoldPower` | 金丝、衣匠 |
| 半神形态 | `DemigodFormPower` | 神性、衣匠 |
| 神性 | `DivinityPower` | 衣匠 |
| 锐丝如刃 | `EdgeThreadPower` | 衣匠 |
| 蓄势绵延 | `EndlessChargePower` | 至高之姿 |
| 致命浪漫 | `FatalRomancePower` | 衣匠 |
| 命运眼睑 | `FateEyelidPower` | 至高之姿 |
| 锻泪匠躯 | `ForgedTearBodyPower` | 衣匠 |
| 金血浇铸 | `GiltBloodForgePower` | 至高之姿 |
| 金丝成障 | `GoldBarrierPower` | 至高之姿 |
| 金丝 | `GoldenThreadPower` | 至高之姿、衣匠 |
| 脆若游丝 | `GossamerPower` | 间隙织线 |
| 领袖魅力 | `LeadershipCharmPower` | 至高之姿 |
| 静息蓄势 | `RestAccumulatePower` | 至高之姿 |
| 回纺为守 | `SpinGuardPower` | 衣匠 |
| 传线连绵 | `SpreadingThreadPower` | 间隙织线 |
| 浴光回春 | `SunbathPower` | 衣匠 |
| 至高之姿 | `SupremePosePower` | 金丝 |
| 缠丝聚金 | `TailorLimitPower` | 衣匠 |
| 衣匠强化 | `TailorMaxHpBonusPower` | 衣匠 |
| 绞紧 | `TwistTightPower` | 间隙织线 |
| 即织即战 | `WeaveAndFightPower` | 衣匠 |
| 间隙织线 | `WeaveLinePower` | MonarchsGazeStrengthDownPower、衣匠 |
| 广织成网 | `WideNetPower` | 衣匠 |
