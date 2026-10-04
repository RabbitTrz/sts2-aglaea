#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
从当前代码 + 本地化重新生成「卡牌设计表」xlsx。

用法：
    python tools/gen_design_sheet.py <输出路径.xlsx>

数据来源（全部以代码为准）：
  · 文案        —— Aglaea/localization/zhs/cards.json（占位符按 CanonicalVars / OnUpgrade 求值）
  · 费用/类型/稀有度 —— 卡牌构造器 AglaeaCard(cost, CardType.X, CardRarity.Y, TargetType.Z)
  · 数值        —— CanonicalVars；升级增量从 OnUpgrade 的 UpgradeValueBy 读取
  · 关键字      —— CanonicalKeywords + OnUpgrade 的 Add/RemoveKeyword（不在 json 里）

编号规则：沿用旧设计表的 1~81；旧表没有的新卡从 82 起。
           新卡编号表见 NEW_CARD_IDS（半神形态 = 82 起）。
"""
import json
import os
import re
import sys
import glob

from openpyxl import Workbook
from openpyxl.styles import Font, Alignment, PatternFill

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# 旧设计表里 1~81 的编号（编号 -> 卡名），新卡不在此表内
OLD_IDS = {
    1: "打击",
    2: "防御",
    3: "裁剪",
    4: "编织",
    5: "若虫",
    6: "连携打击",
    7: "刺纹之蜜",
    8: "织运之竭",
    9: "短视之惩",
    10: "刺纹之陷",
    11: "孤锋千吻",
    12: "乱针",
    13: "裂帛",
    14: "披星百裂",
    15: "作茧自缚",
    16: "抽丝剥茧",
    17: "孤线独织",
    18: "细针密缕",
    19: "无常金线",
    20: "命定共舞",
    21: "千针刺芒",
    22: "蚀茧",
    23: "枯草之盈",
    24: "散力",
    25: "破线",
    26: "回缝",
    27: "密针",
    28: "复缕",
    29: "筛缕",
    30: "枕茧",
    31: "诛线",
    32: "万丝齐舞",
    33: "金线降神",
    34: "虚无之舞",
    35: "密雨刺",
    36: "护茧",
    37: "华珠之礼",
    38: "顺织",
    39: "双唤衣匠",
    40: "捻丝",
    41: "织缕",
    42: "叠茧",
    43: "触丝",
    44: "热狂圆舞",
    45: "苦难织者",
    46: "披姿",
    47: "绞丝",
    48: "织阵",
    49: "与我共舞",
    50: "饲茧",
    51: "纤毫毕现",
    52: "裁断之时",
    53: "免织",
    54: "同衾",
    55: "予你盛装",
    56: "援织",
    57: "叠姿",
    58: "沐浴黄金",
    59: "绞紧",
    60: "命运眼睑",
    61: "蓄姿",
    62: "缎舞",
    63: "列阵",
    64: "决织",
    65: "三织",
    66: "断念",
    67: "纬线",
    68: "金星行列",
    69: "回纺为守",
    70: "静息蓄势",
    71: "锐丝如刃",
    72: "金丝成障",
    73: "即织即战",
    74: "缠丝聚金",
    75: "传线连绵",
    76: "浴光回春",
    77: "广织成网",
    78: "脆若游丝",
    79: "金石闪烁",
    80: "蓄势绵延",
    81: "锻泪匠躯",
}

OLD_NAME_TO_ID = {v: k for k, v in OLD_IDS.items()}

RARITY_CN = {
    "Basic": "初始", "Common": "普通", "Uncommon": "罕见", "Rare": "稀有",
    "Token": "普通", "Ancient": "先古",
}
TYPE_CN = {"Attack": "攻击", "Skill": "技能", "Power": "能力"}
# 前缀关键字（写在描述前）与后缀关键字
PREFIX_KEYWORDS = [("Ethereal", "虚无"), ("Innate", "固有"), ("Retain", "保留")]
SUFFIX_KEYWORDS = [("Exhaust", "消耗")]

# 新卡编号（旧表没有的）。以设计表为准：82 半神形态 … 94 金血浇铸
NEW_CARD_IDS = {
    "AglaeaDemigodForm": 82,
    "AglaeaFatalRomance": 83,
    "AglaeaEmberPath": 84,
    "AglaeaLeadershipCharm": 85,
    "AglaeaAnchorMoment": 86,
    "AglaeaEmberReturn": 87,
    "AglaeaEpicFinale": 88,
    "AglaeaRomanticEnsemble": 89,
    "AglaeaMyriadNet": 90,
    "AglaeaPaperwork": 91,
    "AglaeaArrest": 92,
    "AglaeaExecutor": 93,
    "AglaeaGiltBloodForge": 94,
}


def strip_comments(src):
    return re.sub(r"//[^\n]*", "", re.sub(r"/\*.*?\*/", "", src, flags=re.S))


def parse_vars(src):
    m = re.search(r"CanonicalVars\s*=>\s*\[(.*?)\];", src, re.S)
    if not m:
        return {}
    out = {}
    for mm in re.finditer(r"new\s+(\w+Var)\s*(?:<[^>]*>)?\s*\(([^;]*?)\)\s*(?:,|\]|$)", m.group(1), re.S):
        cls, args = mm.group(1), mm.group(2)
        nm = re.search(r'"(\w+)"', args)
        name = nm.group(1) if nm else {
            "DamageVar": "Damage", "BlockVar": "Block", "RepeatVar": "Repeat",
            "CardsVar": "Cards", "EnergyVar": "Energy",
        }.get(cls, cls.replace("Var", ""))
        val = None
        for a in re.split(r",(?![^<]*>)", args):
            vm = re.match(r"^(-?\d+(?:\.\d+)?)m?$", a.strip())
            if vm:
                val = float(vm.group(1))
                break
        out[name] = val
    return out


def parse_keywords(src):
    m = re.search(r"CanonicalKeywords\s*=>\s*\[(.*?)\];", src, re.S)
    return sorted(set(re.findall(r"CardKeyword\.(\w+)", m.group(1)))) if m else []


def parse_upgrade(src):
    ups, kws, cost = {}, [], None
    m = re.search(r"OnUpgrade\(\)\s*\{(.*?)\n    \}", src, re.S)
    body = m.group(1) if m else ""
    for mm in re.finditer(r'DynamicVars(?:\.(\w+)|\["(\w+)"\])\.UpgradeValueBy\((-?[\d.]+)m?\)', body):
        ups[mm.group(1) or mm.group(2)] = float(mm.group(3))
    for mm in re.finditer(r"EnergyCost\.UpgradeBy\((-?\d+)\)", body):
        cost = int(mm.group(1))
    for mm in re.finditer(r"(Add|Remove)Keyword\(CardKeyword\.(\w+)\)", body):
        kws.append(("+" if mm.group(1) == "Add" else "-") + mm.group(2))
    return ups, kws, cost


def fmt(v):
    if v is None:
        return "?"
    return str(int(v)) if float(v) == int(v) else str(v)


def render(card, vals, upgraded):
    s = card["desc"]
    s = re.sub(
        r"\{(IfUpgraded:show:[^}]*)\}",
        lambda m: (lambda p: p[0] if upgraded else (p[1] if len(p) > 1 else ""))(
            m.group(1)[len("IfUpgraded:show:"):].split("|")),
        s)
    s = re.sub(r"\{Energy:energyIcons\(\)\}", lambda m: "%s费" % fmt(vals.get("Energy")), s)
    s = s.replace("{CalculatedDamage:diff()}",
                  "（%s+移除层数×%s）" % (fmt(vals.get("CalculationBase")), fmt(vals.get("ExtraDamage"))))
    s = re.sub(r"\{(\w+)(?::diff\(\))?\}", lambda m: fmt(vals.get(m.group(1))), s)
    s = re.sub(r"\[/?[a-zA-Z_]+(?:=[^\]]*)?\]", "", s)

    kws = set(card["kws"])
    if upgraded:
        for k in card["upkws"]:
            kws.add(k[1:]) if k[0] == "+" else kws.discard(k[1:])
    s = "".join(w + "。" for k, w in PREFIX_KEYWORDS if k in kws) + s
    s += "".join(w + "。" for k, w in SUFFIX_KEYWORDS if k in kws)
    if s and s[-1] not in "。！？":
        s += "。"
    return s


def collect():
    loc = json.load(open(os.path.join(ROOT, "Aglaea/localization/zhs/cards.json"), encoding="utf-8"))
    cards = []
    for p in sorted(glob.glob(os.path.join(ROOT, "Scripts/Cards/**/*.cs"), recursive=True)):
        cls = os.path.basename(p)[:-3]
        if cls in ("AglaeaCard", "AglaeaArchaicToothPatch"):
            continue
        raw = open(p, encoding="utf-8").read()
        if not re.search(r"\bclass\s+%s\b" % cls, strip_comments(raw)):
            continue  # 已被整体注释停用

        cn = cid = None
        for k in loc:
            if k.endswith(".title") and k[:-6].replace("MOENEGIMOD-", "").title().replace("_", "") == cls:
                cn, cid = loc[k], k[:-6].replace("MOENEGIMOD-", "").lower()
                break
        if cid is None:
            continue

        ctor = re.search(
            r":\s*AglaeaCard\(\s*(-?\d+)\s*,\s*CardType\.(\w+)\s*,\s*CardRarity\.(\w+)\s*,\s*TargetType\.(\w+)",
            raw, re.S)
        cost, ctype, rarity = (int(ctor.group(1)), ctor.group(2), ctor.group(3)) if ctor else (None, None, None)
        xcost = bool(re.search(r"HasEnergyCostX\s*=>\s*true", raw))
        ups, upkws, costup = parse_upgrade(raw)
        cards.append(dict(
            cls=cls, cn=cn, cid=cid, cost=cost, type=ctype, rarity=rarity, xcost=xcost,
            desc=loc.get("MOENEGIMOD-%s.description" % cid.upper(), ""),
            vars=parse_vars(raw), kws=parse_keywords(raw), ups=ups, upkws=upkws, costup=costup))
    return cards


def main():
    out_path = sys.argv[1] if len(sys.argv) > 1 else os.path.join(ROOT, "dos/卡牌设计.xlsx")
    cards = collect()

    rows = []
    for c in cards:
        bv, uv = dict(c["vars"]), dict(c["vars"])
        for k, d in c["ups"].items():
            if uv.get(k) is not None:
                uv[k] = uv[k] + d
        upcost = c["cost"] if c["costup"] is None else c["cost"] + c["costup"]
        cs = (lambda v: "X" if c["xcost"] else str(v))
        cid = OLD_NAME_TO_ID.get(c["cn"]) or NEW_CARD_IDS.get(c["cls"])
        rows.append(dict(
            id=cid, cn=c["cn"], en=c["cls"][6:] if c["cls"].startswith("Aglaea") else c["cls"],
            rar=RARITY_CN.get(c["rarity"], c["rarity"]), typ=TYPE_CN.get(c["type"], c["type"]),
            cost=cs(c["cost"]), costu=cs(upcost),
            desc=render(c, bv, False), descu=render(c, uv, True), new=cid is not None))

    # 旧卡编号来自旧设计表；此处按「未在 NEW_CARD_IDS 里的一律暂缺」处理，需人工补。
    missing = [r for r in rows if r["id"] is None]
    rows.sort(key=lambda r: (r["id"] is None, r["id"] or 0, r["cn"]))

    wb = Workbook()
    HEAD = ["id", "name", "稀有度", "类型", "费用", "描述"]
    HF, FILL, NEWF = Font(bold=True), PatternFill("solid", fgColor="EFEFEF"), PatternFill("solid", fgColor="FFF7E0")
    for title, up, suffix in [("Sheet1", False, ""), ("Sheet1+", True, "+")]:
        ws = wb.create_sheet(title) if wb.sheetnames != ["Sheet"] else wb.active
        ws.title = title
        for j, h in enumerate(HEAD, 1):
            cc = ws.cell(1, j, h); cc.font = HF; cc.fill = FILL
        for j, (t, r) in enumerate(zip(["攻击", "技能", "能力"], ["初始", "普通", "罕见", "稀有"]), 7):
            ws.cell(1, j, "类型" if j == 7 else "稀有度").font = HF
            ws.cell(2, j, t); ws.cell(2, j + 1, r)
        for i, r in enumerate(rows, 2):
            nm = r["cn"] + suffix
            for j, v in enumerate([r["id"], nm, r["rar"], r["typ"],
                                   r["costu"] if up else r["cost"], r["descu"] if up else r["desc"]], 1):
                cell = ws.cell(i, j, v)
                cell.alignment = Alignment(vertical="center", wrap_text=(j == 6))
                if r["new"]:
                    cell.fill = NEWF
        for col, wd in zip("ABCDEF", [6, 14, 8, 8, 6, 74]):
            ws.column_dimensions[col].width = wd
        ws.freeze_panes = "A2"

    ws = wb.create_sheet("中英对照")
    for j, h in enumerate(["id", "中文名", "英文名"], 1):
        cc = ws.cell(1, j, h); cc.font = HF; cc.fill = FILL
    for i, r in enumerate(rows, 2):
        ws.cell(i, 1, r["id"]); ws.cell(i, 2, r["cn"]); ws.cell(i, 3, r["en"])
    for col, wd in zip("ABC", [6, 16, 30]):
        ws.column_dimensions[col].width = wd
    ws.freeze_panes = "A2"

    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    wb.save(out_path)
    print("已生成:", out_path)
    print("卡牌行数:", len(rows))
    if missing:
        print("!! 以下卡牌没有编号（不在 NEW_CARD_IDS 里，需补）：")
        for r in missing:
            print("   ", r["cn"], r["cls"])


if __name__ == "__main__":
    main()
