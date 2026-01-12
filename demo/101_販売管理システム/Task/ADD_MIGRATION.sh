#!/bin/bash

TARGET_PROJECT="Core"
STARTUP_PROJECT="Core"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR/.."
MIGRATION_DIR="$PROJECT_ROOT/Core/Migrations"
MIGRATION_SCRIPT_DIR="$PROJECT_ROOT/Core/MigrationsScript"

# Check required tools
if ! command -v dotnet-ef &> /dev/null; then
    echo "'dotnet-ef' がインストールされていません。公式サイトからインストールしてください。"
    exit 1
fi

if ! command -v "$NIJO_CLI_PATH" &> /dev/null; then
    echo "'nijo' がインストールされていないかパスが通っていません。公式サイトからインストールしてください。"
    exit 1
fi

# ソースコード自動生成処理のかけなおし
pushd "$PROJECT_ROOT" > /dev/null
"$NIJO_CLI_PATH" generate
if [ $? -ne 0 ]; then
    echo "ソースコード自動生成処理でエラーが発生しました。ビルドを中断します。"
    exit 1
fi

# ビルドしなおし
dotnet build "$STARTUP_PROJECT" -c Debug
if [ $? -ne 0 ]; then
    echo "ビルドに失敗しました。ビルドを中断します。"
    exit 1
fi

# DB定義に変更があるかを調べ、なければマイグレーションを続行するか確認
echo "DB定義の変更を確認中..."
dotnet ef migrations has-pending-model-changes --project "$TARGET_PROJECT" --startup-project "$STARTUP_PROJECT" --no-build
HAS_CHANGES_EXIT_CODE=$?

# has-pending-model-changes: 変更がある場合は 1, ない場合は 0 を返す
if [ $HAS_CHANGES_EXIT_CODE -eq 0 ]; then
    echo "変更が検出されませんでした。"
    read -p "それでもマイグレーションを作成しますか？ (y/N): " CONTINUE
    if [[ ! "$CONTINUE" =~ ^[yY] ]]; then
        echo "中断しました。"
        exit 0
    fi
fi

# マイグレーション名を入力
read -p "マイグレーション名を入力してください: " MIGRATION_NAME
if [ -z "$MIGRATION_NAME" ]; then
    echo "マイグレーション名が入力されませんでした。"
    exit 1
fi

# 現在の最新のマイグレーションを取得（これが直前のマイグレーションになる）
RAW_LIST=$(dotnet ef migrations list --project "$TARGET_PROJECT" --startup-project "$STARTUP_PROJECT" --no-build)
# タイムスタンプ(数字)で始まる行のみ抽出して配列化
# (Pending) などが含まれる場合があるため、awk で最初のトークンのみ取得する
MIGRATIONS_LIST=$(echo "$RAW_LIST" | grep -E '^[0-9]+_' | awk '{print $1}')
MIGRATIONS_ARRAY=($MIGRATIONS_LIST)
COUNT=${#MIGRATIONS_ARRAY[@]}

PREV_MIGRATION=""
if [ $COUNT -gt 0 ]; then
    PREV_MIGRATION=${MIGRATIONS_ARRAY[$((COUNT-1))]}
fi

# マイグレーション作成
dotnet ef migrations add "$MIGRATION_NAME" --project "$TARGET_PROJECT" --startup-project "$STARTUP_PROJECT" --output-dir Migrations
if [ $? -ne 0 ]; then
    echo "マイグレーションの作成に失敗しました。"
    exit 1
fi

# SQLスクリプト生成
# 直近のマイグレーションからの差分のみを作成する
if [ -n "$PREV_MIGRATION" ]; then
    echo "直前のマイグレーション: $PREV_MIGRATION からの差分スクリプトを生成します。"
    dotnet ef migrations script "$PREV_MIGRATION" --project "$TARGET_PROJECT" --startup-project "$STARTUP_PROJECT" --output "$MIGRATION_SCRIPT_DIR/$MIGRATION_NAME.sql"
else
    echo "前回のマイグレーションが見つからないため、全量のスクリプトを生成します。"
    dotnet ef migrations script --project "$TARGET_PROJECT" --startup-project "$STARTUP_PROJECT" --output "$MIGRATION_SCRIPT_DIR/$MIGRATION_NAME.sql"
fi

if [ $? -ne 0 ]; then
    echo "SQLスクリプトの生成に失敗しました。"
    exit 1
fi

echo "マイグレーションの作成が完了しました。"
popd > /dev/null
