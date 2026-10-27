using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;

namespace MoeNegiMod.Aglaea.Acts;

/// <summary>
/// 第四章的单线地图：事件（往昔的涟漪）→ 商店 → Boss。
///
/// 起点类型是 <see cref="MapPointType.Unknown"/>（不是原版的 Ancient），
/// 配合 <c>Act4EventRoomPatch</c> 必定解析为「往昔的涟漪」事件。
/// 地图本身不经过 <c>StandardActMap</c> 的随机生成/剪枝，
/// 只建立一条直线路径（参考原版 <c>GoldenPathActMap</c> 的写法）。
/// </summary>
public sealed class AglaeaZandarActMap : ActMap
{
    private const int Width = 7;
    private const int Middle = 3;

    public override MapPoint BossMapPoint { get; }

    public override MapPoint StartingMapPoint { get; }

    protected override MapPoint?[,] Grid { get; }

    public AglaeaZandarActMap(IRunState runState)
    {
        // 本章固定两段路：起点事件 → 商店 → Boss。
        // （多人也用同一条，不压缩 —— 本来就只有两间房。）
        //
        // 关键：起点【不能】放进 Grid！NMapScreen.SetMap 会先遍历 Grid 里的点画路径，
        // 再单独从起点画一次；起点若在 Grid 里，同一条边会被添加两次，
        // 抛 "An item with the same key has already been added"（实测表现为 act 4 黑屏）。
        // 原版 GoldenPathActMap 同样把起点留在 Grid 之外。
        List<MapPointType> rooms = new() { MapPointType.Shop };

        // 第 0 行留给起点（不在 Grid 内），第 1..rooms.Count 行是普通房间。
        Grid = new MapPoint?[Width, rooms.Count + 1];

        StartingMapPoint = new MapPoint(Middle, 0) { PointType = MapPointType.Unknown };

        MapPoint previous = StartingMapPoint;
        for (int i = 0; i < rooms.Count; i++)
        {
            MapPoint point = new MapPoint(Middle, i + 1) { PointType = rooms[i] };
            Grid[Middle, i + 1] = point;
            previous.AddChildPoint(point);
            previous = point;
        }

        BossMapPoint = new MapPoint(Middle, GetRowCount()) { PointType = MapPointType.Boss };
        previous.AddChildPoint(BossMapPoint);

        startMapPoints.Add(Grid[Middle, 1]);
    }
}
