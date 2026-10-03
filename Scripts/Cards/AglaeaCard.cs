using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using MoeNegiMod.Aglaea.Extensions;
using MoeNegiMod.Aglaea.Character;
using MoeNegiMod.Aglaea.Powers;

namespace MoeNegiMod.Aglaea.Cards;

[Pool(typeof(AglaeaCardPool))]
public abstract class AglaeaCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    // 核心机制的统一提示入口。卡牌只写 ExtraHoverTips => [TipTailor]，不再各自
    // 手写 HoverTipFactory.FromPower<...>() —— 散着写容易漏（曾经漏了 25 张）。
    //
    // 用属性而不是 static readonly 字段：字段的初始化发生在类型首次被访问时，
    // 那时 ModelDb 未必已经建好；属性每次求值，没有这个顺序问题。
    //
    // 若虫给的是**卡**不是 power —— 提示里能直接看到卡面，比一句 buff 说明有用。
    protected static IHoverTip TipTailor => HoverTipFactory.FromPower<TailorSwarmPower>();
    protected static IHoverTip TipWeaveLine => HoverTipFactory.FromPower<WeaveLinePower>();
    protected static IHoverTip TipGoldenThread => HoverTipFactory.FromPower<GoldenThreadPower>();
    protected static IHoverTip TipChrysalis => HoverTipFactory.FromCard<AglaeaChrysalis>();

    /// <summary>
    /// 至高之姿 —— **和金丝绑定成一对**。凡提及至高之姿的地方都必须同时给出金丝，
    /// 所以这里直接返回两条，卡牌用展开语法引用：<c>ExtraHoverTips => [.. TipPose]</c>。
    /// 别单独把其中一条拆出去用，否则就绕过了这条规则。
    /// </summary>
    protected static IEnumerable<IHoverTip> TipPose => [
        HoverTipFactory.FromPower<SupremePosePower>(),
        HoverTipFactory.FromPower<GoldenThreadPower>()
    ];

    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190

    /// <summary>
    /// 卡面路径，按优先级取：缩略图 → 正式大图 → 通用占位 card.png。
    ///
    ///   images/cards/&lt;name&gt;.png       250x190 / 250x350（渲染用，优先）
    ///   images/cards/big/&lt;name&gt;.png   普通卡 1000x760 / 全幅卡 606x852（原稿）
    ///
    /// 为什么优先缩略图：大图一张 1000x760 RGBA 约占 3 MB 显存，93 张就是 ~280 MB，
    /// 集成显卡和牌堆全览界面会吃不消。游戏本体也是只用缩略图渲染的。
    /// 大图留在包里当素材（以后做卡牌详情页可以直接切回来）。
    ///
    /// 大图兜底是为了容错：万一哪张卡只丢了大图、没生成缩略图，仍然能显示出来，
    /// 不会退成通用占位。正常流程两边都会生成，走不到这个分支。
    /// </summary>
    private static string ResolveCardArt(string entry)
    {
        var name = $"{entry}.png";
        var small = name.CardImagePath();
        var path = ResourceLoader.Exists(small) ? small : name.BigCardImagePath();
        if (!ResourceLoader.Exists(path))
        {
            path = "card.png".CardImagePath();
        }
        Log.Info(">>>[ManboMod]CardPath=" + path, 2);
        return path;
    }

    public override string CustomPortraitPath => ResolveCardArt(Id.Entry.RemovePrefix().ToLowerInvariant());

    public override string PortraitPath => ResolveCardArt(Id.Entry.RemovePrefix().ToLowerInvariant());

    //Optional and I'm not sure it's functional yet.
    public override string BetaPortraitPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
            Log.Info(">>>[ManboMod]CardPath=" + path, 2);
            return ResourceLoader.Exists(path) ? path : "card.png".CardImagePath();
        }
    }
}
