#!/usr/bin/env python3
# 批量生成 Aglaea 卡牌占位图（解决图鉴闪烁/卡牌消失 bug）
# 文件名规则: class AglaeaXxxYyy -> aglaea_xxx_yyy.png
# 依据: CustomPortraitPath = Id.Entry.RemovePrefix().ToLowerInvariant() + ".png"
#       Id.Entry 含 "-AGLAEA_XXX_YYY", RemovePrefix 去掉首个 '-' 前部分 -> AGLAEA_XXX_YYY -> 小写
import json, os, re, glob
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUTDIR = os.path.join(ROOT, "Aglaea", "images", "cards")
os.makedirs(OUTDIR, exist_ok=True)

# ---- 1. 收集全部卡牌类名（去掉抽象基类 AglaeaCard）----
classes = []
for path in glob.glob(os.path.join(ROOT, "Scripts", "Cards", "**", "*.cs"), recursive=True):
    src = open(path, encoding="utf8").read()
    for m in re.finditer(r"class\s+(Aglaea[A-Za-z0-9]+)\b", src):
        c = m.group(1)
        if c != "AglaeaCard":
            classes.append(c)
classes = sorted(set(classes))

# ---- 2. 文件名辅助：AglaeaXxxYyy -> AGLAEA_XXX_YYY（Slugify）----
def class_to_snake(cname):
    return re.sub(r"(?<!^)(?=[A-Z])", "_", cname)  # AglaeaAttack -> Aglaea_Attack

def entry_key(cname):  # 本地化 key 段（大写下划线）
    return class_to_snake(cname).upper()  # AGLAEA_ATTACK

# ---- 3. 读中文牌名 ----
cards_json = os.path.join(ROOT, "Aglaea", "localization", "zhs", "cards.json")
loc = json.load(open(cards_json, encoding="utf8")) if os.path.exists(cards_json) else {}

def chinese_title(cname):
    # BaseLib LocKey 形如 MOENEGIMOD-AGLAEA_ATTACK.title；也兼容裸 AGLAEA_ATTACK.title
    k = entry_key(cname) + ".title"
    if k in loc:
        return loc[k]
    for full, val in loc.items():
        if full.endswith(k):
            return val
    return cname

# ---- 4. 字体 ----
def font(size):
    for cand in ["C:/Windows/Fonts/msyh.ttc", "C:/Windows/Fonts/msyhbd.ttc",
                 "C:/Windows/Fonts/simhei.ttf"]:
        if os.path.exists(cand):
            try:
                return ImageFont.truetype(cand, size)
            except Exception:
                pass
    return ImageFont.load_default()

# ---- 5. 生成 ----
W, H = 250, 190
fname_font = font(16)
title_font = font(40)
sub_font = font(18)

def distinct_color(i):
    import colorsys
    base = colorsys.hsv_to_rgb((i * 0.61803398875) % 1.0, 0.45, 0.62)
    return tuple(int(v * 255) for v in base)

existing = sorted(f for f in os.listdir(OUTDIR) if not f.endswith(".import"))
print(f"现有卡图: {len(existing)} 个")

made = skipped = 0
for i, cname in enumerate(classes):
    fname = class_to_snake(cname).lower() + ".png"  # aglaea_xxx_yyy.png
    fpath = os.path.join(OUTDIR, fname)
    title = chinese_title(cname)
    color = distinct_color(i)

    img = Image.new("RGB", (W, H), color)
    # 半透明浅色装饰圆，让每张视觉不同
    overlay = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    od = ImageDraw.Draw(overlay)
    for j in range(6):
        cx = int(((j * 57) % (W + 80)) - 40)
        cy = int(((j * 83+40) % (H + 60)) - 30)
        od.ellipse([cx-28, cy-28, cx+28, cy+28], fill=(255,255,255,int(14+ (j%3)*8)))
    img = Image.alpha_composite(img.convert("RGBA"), overlay).convert("RGBA")
    draw = ImageDraw.Draw(img)

    # 卡名(大) + 类名slug(中) + Aglaea(小)，带黑色描边保证可读
    def outlined_center(text, fnt, y, fill):
        bb = draw.textbbox((0, 0), text, font=fnt)
        tw = bb[2] - bb[0]
        x = (W - tw) / 2 - bb[0]
        for dx in (-1, 0, 1):
            for dy in (-1, 0, 1):
                draw.text((x + dx, y + dy), text, font=fnt, fill=(20, 20, 20))
        draw.text((x, y), text, font=fnt, fill=fill)

    outlined_center(title, title_font, 42, (255, 255, 255))
    outlined_center(entry_key(cname), fname_font, 96, (235, 235, 235))
    outlined_center("Aglaea", sub_font, 148, (255, 255, 255))
    img.convert("RGB").save(fpath)

    made += 1

print(f"应生成 {len(classes)} 张，本次生成 {made} 张 -> {OUTDIR}")

# ---- 6. 报告缺口（本地化无标题的卡，及文件名预览）----
missing_loc = [c for c in classes if not any(k.endswith(entry_key(c) + ".title") for k in loc)]
if missing_loc:
    print("[警告] 以下卡缺本地化标题:", missing_loc)
else:
    print("[OK] 所有卡均有本地化标题")
