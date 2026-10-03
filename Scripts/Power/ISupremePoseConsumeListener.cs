namespace MoeNegiMod.Aglaea.Powers;

/// <summary>
/// 「至高之姿」消耗监听器。当玩家身上的「至高之姿」被消耗若干层时，
/// 通知所有实现该接口的 power（用于命运眼睑、蓄势绵延等）。
/// </summary>
public interface ISupremePoseConsumeListener
{
    /// <summary>至高之姿被消耗了 <paramref name="layers"/> 层。</summary>
    System.Threading.Tasks.Task OnSupremePoseConsumed(int layers);
}
