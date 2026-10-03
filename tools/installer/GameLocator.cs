using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace AglaeaInstaller;

/// <summary>
/// 定位 Slay the Spire 2 的游戏根目录。
/// 依次尝试：注册表 SteamPath → libraryfolders.vdf 中各个库 → 常见默认路径。
/// </summary>
public static class GameLocator
{
    public const string GameFolderName = "Slay the Spire 2";
    public const string GameExeName = "SlayTheSpire2.exe";

    /// <summary>游戏可执行文件名（用于校验目录是否为游戏根目录）。</summary>
    public static bool IsValidGameDir(string? dir)
    {
        if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
        {
            return false;
        }
        // 以可执行文件为准；若可执行文件缺失但存在 mods 目录也容忍（少数手动安装场景）。
        return File.Exists(Path.Combine(dir, GameExeName))
            || Directory.Exists(Path.Combine(dir, "mods"));
    }

    /// <summary>尝试自动检测；找不到返回 null。</summary>
    public static string? AutoDetect()
    {
        foreach (var candidate in EnumerateCandidates())
        {
            if (IsValidGameDir(candidate))
            {
                return Path.GetFullPath(candidate);
            }
        }
        return null;
    }

    private static IEnumerable<string> EnumerateCandidates()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var steamRoot in EnumerateSteamRoots())
        {
            foreach (var lib in EnumerateLibraryFolders(steamRoot))
            {
                var path = Path.Combine(lib, "steamapps", "common", GameFolderName);
                if (seen.Add(path))
                {
                    yield return path;
                }
            }
        }

        // 兜底：常见的默认位置。
        var drives = new[] { "C:", "D:", "E:", "F:" };
        var suffixes = new[]
        {
            @"Program Files (x86)\Steam\steamapps\common",
            @"Program Files\Steam\steamapps\common",
            @"Steam\steamapps\common",
            @"SteamLibrary\steamapps\common",
            @"Games\Steam\steamapps\common",
        };
        foreach (var drive in drives)
        {
            foreach (var suffix in suffixes)
            {
                var path = Path.Combine(drive + Path.DirectorySeparatorChar, suffix, GameFolderName);
                if (seen.Add(path))
                {
                    yield return path;
                }
            }
        }
    }

    /// <summary>从注册表拿到 Steam 安装目录。</summary>
    public static IEnumerable<string> EnumerateSteamRoots()
    {
        string? steamPath = null;
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            steamPath = key?.GetValue("SteamPath") as string;
        }
        catch
        {
            // 忽略注册表读取失败，走兜底路径。
        }

        if (!string.IsNullOrWhiteSpace(steamPath))
        {
            yield return steamPath.Replace('/', '\\');
        }
    }

    /// <summary>解析 libraryfolders.vdf，列出所有 Steam 库根目录（含 Steam 自身）。</summary>
    public static IEnumerable<string> EnumerateLibraryFolders(string steamRoot)
    {
        var results = new List<string>();
        var vdf = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");

        if (File.Exists(vdf))
        {
            try
            {
                var text = File.ReadAllText(vdf);
                // 形如：  "path"      "D:\\SteamLibrary"
                foreach (Match m in Regex.Matches(text, "\"path\"\\s+\"([^\"]+)\""))
                {
                    var p = m.Groups[1].Value.Replace("\\\\", "\\");
                    if (!string.IsNullOrWhiteSpace(p))
                    {
                        results.Add(p);
                    }
                }
            }
            catch
            {
                // 解析失败则只回退到 steamRoot 本身。
            }
        }

        if (!results.Contains(steamRoot, StringComparer.OrdinalIgnoreCase))
        {
            results.Insert(0, steamRoot);
        }
        return results;
    }
}
