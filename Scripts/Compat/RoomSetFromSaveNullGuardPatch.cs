using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using System.Collections.Generic;
using MainFile = Aglaea.MainFile;

namespace MoeNegiMod.Aglaea.Compat;

/// <summary>
/// 原版存档序列化在「空房间池」上的 bug。
///
/// <see cref="SerializableRoomSet"/> 的 EventIds / NormalEncounterIds / EliteEncounterIds 都标了
/// <c>[JsonSerializeCondition(SaveIfNotCollectionEmptyOrNull)]</c>——空列表不会写进存档；
/// 而这三个属性没有初始化器，读档时缺失字段就是 null。
/// <see cref="RoomSet.FromSave"/> 随后直接对这些列表调用 <c>.Select(...)</c>，
/// 抛 <c>ArgumentNullException: Value cannot be null. (Parameter 'source')</c>。
///
/// 原版章节永远有事件 / 普通 / 精英遭遇，所以踩不到；第四章「创世涡心」没有普通和精英遭遇，
/// 存档里正好缺这两个字段，导致「继续游戏」崩溃。
///
/// 这里在读档前把 null 补成空列表。
/// </summary>
[HarmonyPatch(typeof(RoomSet), nameof(RoomSet.FromSave))]
internal static class RoomSetFromSaveNullGuardPatch
{
    [HarmonyPrefix]
    private static void Prefix(SerializableRoomSet save)
    {
        save.EventIds ??= new List<ModelId>();
        save.NormalEncounterIds ??= new List<ModelId>();
        save.EliteEncounterIds ??= new List<ModelId>();

        MainFile.Logger.Info(
            $"[Aglaea] RoomSet.FromSave：补空列表 events={save.EventIds.Count} " +
            $"normal={save.NormalEncounterIds.Count} elite={save.EliteEncounterIds.Count}");
    }
}
