using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;

namespace MoeNegiMod.Aglaea.Acts;

/// <summary>
/// 第四章的单线地图：休息点 → 商店 → Boss。
///
/// 起点类型是 <see cref="MapPointType.RestSite"/>（不是原版的 Ancient），
/// 所以这一章不需要古神事件。地图本身不经过 <c>StandardActMap</c> 的随机生成/剪枝，
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
        // 本章固定两段路：起点休息点 → 商店 → Boss。
        // （多人也用同一条，不压缩 —— 本来就只有两间房。）
        List<MapPointType> route = new() { MapPointType.RestSite, MapPointType.Shop };

        Grid = new MapPoint?[Width, route.Count];

        MapPoint previous = StartingMapPoint = new MapPoint(Middle, 0) { PointType = route[0] };
        Grid[Middle, 0] = previous;

        for (int i = 1; i < route.Count; i++)
        {
            MapPoint point = new MapPoint(Middle, i) { PointType = route[i] };
            Grid[Middle, i] = point;
            previous.AddChildPoint(point);
            previous = point;
        }

        BossMapPoint = new MapPoint(Middle, route.Count) { PointType = MapPointType.Boss };
        previous.AddChildPoint(BossMapPoint);
    }
}
