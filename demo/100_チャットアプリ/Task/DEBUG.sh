#!/bin/bash

# Check required tools
if ! command -v dotnet &> /dev/null; then
    echo "'dotnet' がインストールされていません。公式サイトからインストールしてください。"
    exit 1
fi

if ! command -v npm &> /dev/null; then
    echo "'npm' がインストールされていません。公式サイトからインストールしてください。"
    exit 1
fi

if ! command -v "$NIJO_CLI_PATH" &> /dev/null; then
    echo "'nijo' がインストールされていないかパスが通っていません。公式サイトからインストールしてください。"
    exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR/.."

# ソースコード自動生成処理のかけなおし
pushd "$PROJECT_ROOT" > /dev/null
"$NIJO_CLI_PATH" generate
if [ $? -ne 0 ]; then
    echo "ソースコード自動生成処理でエラーが発生しました。ビルドを中断します。"
    exit 1
fi
popd > /dev/null

# Install Node.js packages if not installed
pushd "$PROJECT_ROOT/client" > /dev/null
if [ ! -d "node_modules" ]; then
    read -p "node_modules （このアプリで使用しているNode.jsの各種ライブラリ）がインストールされていません。インストールしますか？ (y/n): " yn
    case $yn in
        [Yy]* ) npm ci;;
        * ) exit 1;;
    esac
fi
popd > /dev/null

$BROWSER "http://localhost:5173"

# Start debugging
# dotnet watch & npm run dev
npx --yes concurrently \
    --names "API,CLIENT" \
    --prefix-colors "blue,green" \
    "cd \"$PROJECT_ROOT/WebApi\" && dotnet watch --launch-profile http" \
    "cd \"$PROJECT_ROOT/client\" && npm run dev"
