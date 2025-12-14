#!/bin/bash

# XMLスキーマ定義編集UIのデバッグ開始コマンド

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKEND_PROJECT="$SCRIPT_DIR/../Nijo"
FRONTEND_ROOT="$SCRIPT_DIR/../Nijo.GuiClient/package_schema-editor-v1"

# dotnet watch と npm run dev を並行実行
npx --yes concurrently \
  -n "BACKEND,FRONTEND" \
  -c "blue,green" \
  "cd \"$BACKEND_PROJECT\" && dotnet watch --launch-profile DebugServe" \
  "cd \"$FRONTEND_ROOT\" && npm run dev -- --host"
