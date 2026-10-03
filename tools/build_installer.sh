#!/usr/bin/env bash
# 一键打包 Aglaea 模组安装器。
# 用前需先完成模组的正常构建（dotnet build -c ExportRelease + Godot 导出 pck）。
set -e

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
INSTALLER_DIR="$ROOT/tools/installer"
PAYLOAD_DIR="$INSTALLER_DIR/payload"
DLL_SRC="$ROOT/.godot/mono/temp/bin/ExportRelease/Aglaea.dll"

echo "==> 检查模组产物"
for f in "$ROOT/Aglaea.pck" "$ROOT/Aglaea.json" "$DLL_SRC"; do
    if [ ! -f "$f" ]; then
        echo "缺少文件: $f" >&2
        echo "请先完成模组构建（dotnet build -c ExportRelease 并导出 pck）。" >&2
        exit 1
    fi
done

echo "==> 拷贝产物到 payload"
mkdir -p "$PAYLOAD_DIR"
cp "$DLL_SRC"           "$PAYLOAD_DIR/Aglaea.dll"
cp "$ROOT/Aglaea.pck"   "$PAYLOAD_DIR/Aglaea.pck"
cp "$ROOT/Aglaea.json"  "$PAYLOAD_DIR/Aglaea.json"
# README.txt 由源码目录维护，不覆盖
if [ ! -f "$PAYLOAD_DIR/README.txt" ]; then
    echo "缺少 $PAYLOAD_DIR/README.txt" >&2
    exit 1
fi

echo "==> 发布自包含单文件 exe"
dotnet publish "$INSTALLER_DIR/AglaeaInstaller.csproj" \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true \
    -o "$INSTALLER_DIR/dist"

EXE="$INSTALLER_DIR/dist/AglaeaInstaller.exe"
if [ ! -f "$EXE" ]; then
    echo "发布失败：未生成 $EXE" >&2
    exit 1
fi

echo
echo "完成：$EXE"
ls -la "$EXE"
