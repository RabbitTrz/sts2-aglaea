using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MoeNegiMod.Aglaea.Extensions;
using MoeNegiMod.Aglaea.Cards;
using MoeNegiMod.Aglaea.Relics;
using System;

namespace MoeNegiMod.Aglaea.Character;

public class Aglaea : PlaceholderCharacterModel
{
	public const string CharacterId = "Aglaea";

	public override string CustomCharacterSelectBg => "res://Aglaea/scene/Char_Select/char_select_bg_aglaea.tscn";

	public override string PlaceholderID => "necrobinder";

	public static readonly Color Color = new Color("c4278a");


	public override Color NameColor => Color;
	public override CharacterGender Gender => CharacterGender.Feminine;
	public override int StartingHp => 75;

	// 地图上涂鸦线的颜色。不覆写的话走 CharacterModel 的默认值——那是纯黑，
	// 画在地图上很不显眼。用金黄色，和能量球/卡组配色一致。
	// 原版各角色取值参考：铁甲战士 CB282B、静默猎手 2F6729、故障机器人 0D638C、亡约 AC0486。
	public override Color MapDrawingColor => new Color("ffcc00");

	public override IEnumerable<CardModel> StartingDeck => [

		ModelDb.Card<AglaeaAttack>(),
		ModelDb.Card<AglaeaAttack>(),
		ModelDb.Card<AglaeaAttack>(),
		ModelDb.Card<AglaeaAttack>(),
		ModelDb.Card<AglaeaDefend>(),
		ModelDb.Card<AglaeaDefend>(),
		ModelDb.Card<AglaeaDefend>(),
		ModelDb.Card<AglaeaDefend>(),
		ModelDb.Card<AglaeaTrim>(),
		ModelDb.Card<AglaeaWeave>(),

	];

	public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<AglaeaToy>()];

	public override CardPoolModel CardPool => ModelDb.CardPool<AglaeaCardPool>();
	public override RelicPoolModel RelicPool => ModelDb.RelicPool<AglaeaRelicPool>();
	public override PotionPoolModel PotionPool => ModelDb.PotionPool<SharedPotionPool>();

	/*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
		override all the other methods that define those assets.
		These are just some of the simplest assets, given some placeholders to differentiate your character with.
		You don't have to, but you're suggested to rename these images. */
	// 战斗中的角色立绘场景：静态图 + AnimatedSprite2D（没有 Spine 骨架）。
	// 动画触发由 AglaeaAnimationPatch 桥接，立绘由 AglaeaSpriteDriver 驱动。
	public override string CustomVisualPath => AglaeaVisuals.ScenePath;

	// 战斗左上角头像、角色选择图标、地图标记：都走 Aglaea/images/charui/ 下的 png。
	public override string CustomIconTexturePath => "character_icon_aglaea.png".CharacterUiPath();
	public override string CustomCharacterSelectIconPath => "char_select_aglaea.png".CharacterUiPath();
	public override string CustomCharacterSelectLockedIconPath => "char_select_aglaea_locked.png".CharacterUiPath();
	public override string CustomMapMarkerPath => "map_marker_aglaea.png".CharacterUiPath();

	// 火堆 / 商店立绘：静态图场景（不是原版那种 Spine 骨架）。
	// 场景本身在 Aglaea/scene/Rest|Merchant/ 下，节点结构要求写在场景文件的注释里。
	public override string CustomRestSiteAnimPath => AglaeaVisuals.RestScenePath;
	public override string CustomMerchantAnimPath => AglaeaVisuals.MerchantScenePath;

	// 多人联机猜拳的手臂（石头/剪刀/布/指向）。
	// 画布固定 422x1200 RGBA —— 和原版五个角色完全一致；界面按这个尺寸定位，
	// 改画布比例会让手臂在屏幕上对不齐。
	public override string CustomArmRockTexturePath => "ui/hands/multiplayer_hand_aglaea_rock.png".ImagePath();
	public override string CustomArmScissorsTexturePath => "ui/hands/multiplayer_hand_aglaea_scissors.png".ImagePath();
	public override string CustomArmPaperTexturePath => "ui/hands/multiplayer_hand_aglaea_paper.png".ImagePath();
	public override string CustomArmPointingTexturePath => "ui/hands/multiplayer_hand_aglaea_point.png".ImagePath();

	// 战斗左上角头像、角色数据界面用的是 CharacterModel.Icon，它读的是 .tscn 场景（IconPath），
	// 和上面 CustomIconTexturePath 那张 png 是两条独立的腿。
	// PlaceholderCharacterModel 默认给的是 necrobinder_icon.tscn，所以在这些位置一直是亡灵契约师。
	// 这里在运行时用 TextureRect 拼一个等效控件，布局照抄 necrobinder_icon.tscn。
	public override Control? CustomIcon => new TextureRect
	{
		AnchorRight = 1f,
		AnchorBottom = 1f,
		GrowHorizontal = Control.GrowDirection.Both,
		GrowVertical = Control.GrowDirection.Both,
		MouseFilter = Control.MouseFilterEnum.Ignore,
		Texture = PreloadManager.Cache.GetTexture2D("character_icon_aglaea.png".CharacterUiPath()),
		ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
		StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
	};

	// 战斗左下角能量球。
	// 游戏本体的能量球是从 .tscn 场景实例化的（默认 necrobinder_energy_counter.tscn），
	// BaseLib 用 CustomEnergyCounter 这条腿在运行时用图片拼出等效结构：
	// Layer1 底、Layer2/3 旋转、Layer4/5 顶，共 5 层。这里用同一张图填满 5 层。
	public override CustomEnergyCounter? CustomEnergyCounter => new(
		_ => "charui/big_energy.png".ImagePath(),
		new Color("e6a800"),
		new Color("ffcc00"));
}
