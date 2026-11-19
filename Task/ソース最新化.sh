#!/bin/bash
set -e

# nijo をビルドし、テンプレートと全デモのソースコード自動生成をかけなおす。

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SOURCE_ROOT="$SCRIPT_DIR/../Nijo.NewProjectTemplate"
NIJO_EXE_PATH="$SCRIPT_DIR/../Nijo.AppScaffold/bin/Debug/net10.0/nijo"

# nijo.exe リビルド
dotnet build "$SCRIPT_DIR/../Nijo.AppScaffold" -c Debug

# テンプレートプロジェクトのソースコード自動生成かけなおし
"$NIJO_EXE_PATH" generate "$SOURCE_ROOT"

# デモフォルダをループし全デモフォルダに対して処理を行う
for d in "$SCRIPT_DIR/../demo/"*/; do
    echo "Processing $d"
    "$NIJO_EXE_PATH" generate "$d"
done

echo "自動生成のかけなおしが完了しました"
exit 0
