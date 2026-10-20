using BaseLib.Extensions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MoeNegiMod.Aglaea.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MainFile = Aglaea.MainFile;

namespace MoeNegiMod.Aglaea.Character;

/// <summary>
/// 阿格莱雅的战斗编队：与【亡灵契约师】对齐。
///
/// 原版在 <c>NCombatRoom.PositionPlayersAndPets</c> 里给本地亡灵契约师写死了特殊处理 ——
/// 立绘整体左移 150px，Osty 摆在右侧；而其它角色的宠物走通用逻辑，直接叠在玩家身上。
/// 阿格莱雅的衣匠属于后者，会和本体重叠。
///
/// 这里复刻亡灵契约师的做法：本地玩家**首次召唤衣匠后**本体左移 150px（此后不再回弹，
/// 与亡灵契约师一致），第一个衣匠摆在与 Osty 相同的位置（右 180、上 75），
/// 之后的衣匠继续向右下错开，做出前后层次。多人下只调整「自己屏幕上的自己」。
/// </summary>
public static class AglaeaFormation
{
    /// <summary>本体左移量，与原版亡灵契约师一致。</summary>
    private const float PlayerShift = 150f;

    /// <summary>第一个衣匠的位置：对齐 Osty（原版 Osty 为右 150~250、上 75，取中值）。</summary>
    private const float FirstPetDx = 180f;

    private const float FirstPetDy = -75f;

    /// <summary>后续衣匠向右下错开的理想步长（数量少时用这个，保持层次感）。</summary>
    private const float MaxStepDx = 110f;

    private const float MaxStepDy = 45f;

    /// <summary>
    /// 后续衣匠总共占用的额外空间上限：数量多时步长按这个预算压缩。
    /// 衣匠上限 = 基础 3 + 【缠丝聚金】升级后 3 = 6 只，不限的话整排会铺进敌人区、
    /// 纵向垂到手牌上，所以压缩到「第一只 + 260px / +120px」以内。
    /// </summary>
    private const float ExtraBudgetX = 260f;

    private const float ExtraBudgetY = 120f;

    private static readonly ConditionalWeakTable<Player, FormationState> States = new();

    private sealed class FormationState
    {
        /// <summary>游戏摆好的本体原始 X（未左移时）。</summary>
        public float BaseX;

        public bool Shifted;
    }

    /// <summary>
    /// 重新摆放阿格莱雅与其衣匠。<paramref name="resetBase"/> = true 时把当前本体位置当作基准
    /// （战斗开始时游戏刚摆完位，需要重新记录）。
    /// </summary>
    public static void Refresh(Player player, bool resetBase = false)
    {
        NCreature? node = player.Creature.GetCreatureNode();
        if (node == null || node.Visuals == null)
        {
            return;
        }

        FormationState state = States.GetOrCreateValue(player);
        if (resetBase)
        {
            state.BaseX = node.Position.X;
            state.Shifted = false;
        }
        else if (!state.Shifted)
        {
            state.BaseX = node.Position.X;
        }

        List<NCreature> tailors = TailorCmd.AliveTailors(player)
            .Select(t => t.GetCreatureNode())
            .Where(n => n != null && n.Visuals != null)
            .Select(n => n!)
            .ToList();

        if (tailors.Count == 0)
        {
            // 衣匠全灭也不回弹：位置一旦让出来就保持（与亡灵契约师一致）。
            return;
        }

        state.Shifted = true;
        node.Position = new Vector2(state.BaseX - PlayerShift, node.Position.Y);

        // 第一个衣匠对齐 Osty 的位置，之后的向右下错开，形成从前到后的层次；
        // 数量多时按预算压缩步长，避免铺进敌人区 / 手牌区。
        int extra = tailors.Count - 1;
        float stepX = extra > 0 ? System.Math.Min(MaxStepDx, ExtraBudgetX / extra) : 0f;
        float stepY = extra > 0 ? System.Math.Min(MaxStepDy, ExtraBudgetY / extra) : 0f;

        float x = node.Position.X + FirstPetDx;
        float y = node.Position.Y + FirstPetDy;
        foreach (NCreature tailor in tailors)
        {
            tailor.Position = new Vector2(x, y);
            x += stepX;
            y += stepY;
        }
    }
}

/// <summary>
/// 战斗开始摆位后补一次编队：熔光手链等「战斗开始时召唤」的衣匠在这个时点已经出场，
/// 要等原版摆完位再调整；之后每次召唤/死亡由 <see cref="TailorCmd"/> 主动调用。
/// </summary>
[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.PositionPlayersAndPets))]
internal static class AglaeaFormationPatch
{
    [HarmonyPostfix]
    private static void Postfix(List<NCreature> creatureNodes)
    {
        foreach (NCreature node in creatureNodes)
        {
            if (!node.Entity.IsPlayer || !LocalContext.IsMe(node.Entity))
            {
                continue;
            }

            Player? player = node.Entity.Player;
            if (player != null && player.Character.Id.Entry.RemovePrefix() == "AGLAEA")
            {
                AglaeaFormation.Refresh(player, resetBase: true);
            }
        }
    }
}
