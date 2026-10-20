using BaseLib.Config;

namespace MoeNegiMod.Aglaea;

/// <summary>
/// 模组的游戏内设置：在「设置 → 模组配置」（BaseLib 注入）里显示。
///
/// 写法对齐 BaseLib 自己的配置类：属性用 <c>static</c>，运行时直接读；
/// 标签文字走 <c>settings_ui</c> 表、key = <c>&lt;模组前缀&gt;&lt;属性名的蛇形大写&gt;.title</c>。
/// 注册在 <c>MainFile.Initialize</c> 里（ModConfigRegistry.Register）。
/// </summary>
[ConfigHoverTipsByDefault]
public class AglaeaConfig : SimpleModConfig
{
    [ConfigSection("VfxSection")]
    public static bool EnemyLongVfx { get; set; } = true;

    public static bool AllyLongVfx { get; set; } = true;

    [ConfigSection("AudioSection")]
    [ConfigSlider(0.0, 100.0, 5.0)]
    public static int BgmVolumePercent { get; set; } = 45;
}
