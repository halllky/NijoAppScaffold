#!/bin/bash

# XMLスキーマ定義編集UIのデバッグ開始コマンド
# デバッグは Windows Forms ではなくホットリロードがある vite で行います。

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKEND_PROJECT="$SCRIPT_DIR/../Nijo"
FRONTEND_ROOT="$SCRIPT_DIR/../Nijo.GuiClient/package_schema-editor-v1"

# dotnet watch を新しいターミナルで実行
osascript -e "tell application \"Terminal\" to do script \"cd \\\"$BACKEND_PROJECT\\\" && dotnet watch --launch-profile DebugServe\""

# npm run dev を新しいターミナルで実行
osascript -e "tell application \"Terminal\" to do script \"cd \\\"$FRONTEND_ROOT\\\" && npm run dev\""
