using BaseLib;
using Aglaea;
using MainFile = Aglaea.MainFile;

namespace MoeNegiMod.Aglaea.Extensions;

//Mostly utilities to get asset paths.
//
// 所有方法都必须返回 Godot 资源路径：res:// 前缀 + 正斜杠分隔。
// 不能用 Path.Join —— Windows 上它会拼出反斜杠（Aglaea\images\...），
// 而 PreloadManager.Cache / ResourceLoader 只认 res:// 正斜杠路径，
// 反斜杠会导致 "Asset not cached"，图片静默回落到占位素材（necrobinder）。
public static class StringExtensions
{
    public static string ImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/{path}";
    }

    public static string CardImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/cards/{path}";
    }

    public static string BigCardImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/cards/big/{path}";
    }

    public static string PowerImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/powers/{path}";
    }
    public static string BigPowerImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/powers/big/{path}";
    }

    public static string RelicImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/relics/{path}";
    }

    public static string BigRelicImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/relics/big/{path}";
    }

    public static string CharacterUiPath(this string path)
    {
        return $"res://{MainFile.ModId}/images/charui/{path}";
    }
}
