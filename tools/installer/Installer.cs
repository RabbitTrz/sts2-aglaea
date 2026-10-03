using System.Diagnostics;
using System.Reflection;

namespace AglaeaInstaller;

public sealed record PayloadFile(string ResourceName, string FileName);

public sealed record InstallResult(bool Success, bool NeedsAdmin, string Message, List<string> Log);

/// <summary>
/// 把内嵌的模组文件写入 &lt;游戏目录&gt;\mods\Aglaea\。
/// </summary>
public static class Installer
{
    public const string GameProcessName = "SlayTheSpire2";
    public const string ModFolderName = "Aglaea";

    /// <summary>内嵌载荷清单（LogicalName → 目标文件名）。</summary>
    public static readonly PayloadFile[] Payload =
    [
        new("payload.Aglaea.dll",  "Aglaea.dll"),
        new("payload.Aglaea.pck",  "Aglaea.pck"),
        new("payload.Aglaea.json", "Aglaea.json"),
        new("payload.README.txt",  "README.txt"),
    ];

    public static bool IsGameRunning() =>
        Process.GetProcessesByName(GameProcessName).Length > 0;

    /// <summary>检查目标 mods 目录下是否已有 BaseLib（模组的运行时依赖）。</summary>
    public static bool IsBaseLibPresent(string gameDir)
    {
        var modsDir = Path.Combine(gameDir, "mods");
        if (!Directory.Exists(modsDir))
        {
            return false;
        }
        try
        {
            return Directory.EnumerateFileSystemEntries(modsDir, "*BaseLib*").Any();
        }
        catch
        {
            return false;
        }
    }

    public static InstallResult Install(string gameDir, Action<string> log)
    {
        var lines = new List<string>();
        void Say(string msg)
        {
            lines.Add(msg);
            log(msg);
        }

        if (!GameLocator.IsValidGameDir(gameDir))
        {
            return new InstallResult(false, false,
                $"所选目录不是有效的游戏目录：\n{gameDir}\n\n请选择包含 {GameLocator.GameExeName} 的文件夹。", lines);
        }

        if (IsGameRunning())
        {
            return new InstallResult(false, false,
                "检测到《杀戮尖塔 2》正在运行。\n\n请先完全关闭游戏，再点击「安装」。", lines);
        }

        var targetDir = Path.Combine(gameDir, "mods", ModFolderName);
        Say($"游戏目录：{gameDir}");
        Say($"安装到：{targetDir}");

        try
        {
            Directory.CreateDirectory(targetDir);
            Say("已创建 mods\\Aglaea 目录。");
        }
        catch (UnauthorizedAccessException)
        {
            return new InstallResult(false, true,
                "没有权限写入游戏目录（游戏通常安装在 Program Files 下）。\n\n请以管理员身份重新运行本安装器。", lines);
        }
        catch (Exception ex)
        {
            return new InstallResult(false, false, $"创建目录失败：{ex.Message}", lines);
        }

        var asm = Assembly.GetExecutingAssembly();
        foreach (var item in Payload)
        {
            using var stream = asm.GetManifestResourceStream(item.ResourceName);
            if (stream == null)
            {
                return new InstallResult(false, false,
                    $"安装包损坏：内部缺少 {item.FileName} 资源。\n请重新下载安装器。", lines);
            }

            var dest = Path.Combine(targetDir, item.FileName);
            try
            {
                // 目标可能被占用或只读；先尽力清掉只读属性。
                if (File.Exists(dest))
                {
                    try { File.SetAttributes(dest, FileAttributes.Normal); } catch { }
                }

                using var outFs = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None);
                stream.CopyTo(outFs);
                Say($"已写入 {item.FileName}（{new FileInfo(dest).Length:N0} 字节）");
            }
            catch (UnauthorizedAccessException)
            {
                return new InstallResult(false, true,
                    "没有权限写入游戏目录。\n\n请以管理员身份重新运行本安装器。", lines);
            }
            catch (IOException ex)
            {
                return new InstallResult(false, false,
                    $"写入 {item.FileName} 失败：文件可能被占用。\n\n请确认游戏已完全关闭后重试。\n\n({ex.Message})", lines);
            }
        }

        if (!IsBaseLibPresent(gameDir))
        {
            Say("提示：未在 mods 目录发现 BaseLib。本模组依赖 BaseLib，若游戏内未加载模组，请先安装 BaseLib。");
        }

        Say("安装完成。");
        return new InstallResult(true, false,
            $"安装成功！\n\n模组已安装到：\n{targetDir}\n\n启动游戏即可使用。", lines);
    }

    /// <summary>以管理员身份重启自身（保留原参数）。</summary>
    public static bool RestartAsAdmin()
    {
        try
        {
            var exe = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exe))
            {
                return false;
            }
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = true,
                Verb = "runas",
            };
            Process.Start(psi);
            return true;
        }
        catch
        {
            // 用户在 UAC 弹窗点了「否」。
            return false;
        }
    }
}
