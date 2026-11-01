#!/bin/bash

# XMLスキーマ定義編集UIのデバッグ開始コマンド
# 第1引数: 編集対象フォルダのパス

if [ -z "$1" ]; then
  echo "編集対象フォルダのパスを指定してください。"
  exit 1
fi
PROJECT_PATH="$(realpath -m "$1")"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKEND_PROJECT="$SCRIPT_DIR/../Nijo.AppScaffold"
FRONTEND_ROOT="$SCRIPT_DIR/../Nijo.GuiClient/package_schema-editor-v1"

# dotnet watch と npm run dev を並行実行
npx --yes concurrently \
  -n "BACKEND,FRONTEND" \
  -c "blue,green" \
  "cd \"$BACKEND_PROJECT\" && dotnet watch run --no-launch-profile -- serve \"$PROJECT_PATH\" --url http://0.0.0.0:5001 --no-browser" \
  "cd \"$FRONTEND_ROOT\" && npm run dev -- --host"
