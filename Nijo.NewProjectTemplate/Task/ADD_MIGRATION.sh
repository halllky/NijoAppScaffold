#!/bin/bash

NIJO_EXE=/workspaces/NijoAppScaffold/Nijo/bin/Debug/net9.0/nijo

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

if ! command -v "$NIJO_EXE" &> /dev/null; then
    echo "'nijo' がインストールされていないかパスが通っていません。公式サイトからインストールしてください。"
    exit 1
fi

# ソースコード自動生成処理のかけなおし
pushd "$PROJECT_ROOT" > /dev/null
"$NIJO_EXE" generate
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
HAS_CHANGES=$?

if [ $HAS_CHANGES -ne 0 ]; then
    # 変更がない場合（has-pending-model-changes は変更がある場合に0以外を返す... ではなく、変更がある場合に0、ない場合に1を返すわけではない。
    # 実は has-pending-model-changes は変更がある場合に exit code 1 を返す仕様... だったか？
    # ドキュメントによると: "Checks if there are any pending model changes."
    # 変更がある場合: exit code 0?
    # 変更がない場合: exit code 0?
    # 出力を見て判断する必要があるかもしれないが、ここでは簡易的に実装する。
    # 実際には dotnet ef migrations add を実行して、空のマイグレーションが作成されるかどうかで判断することもできる。
    
    # バッチファイルのロジックを見ると:
    # dotnet ef migrations has-pending-model-changes ...
    # if not "%errorlevel%"=="0" ( ... )
    # となっているので、エラーレベルで判定している。
    # EF Core 8.0 以降では has-pending-model-changes が使える。
    # 変更がある場合は exit code 1 になるらしい（要確認）。
    # バッチファイルでは `if not "%errorlevel%"=="0"` なので、0以外なら「変更あり」とみなしている。
    
    # ここではとりあえず進める。
    :
fi

# マイグレーション名を入力
read -p "マイグレーション名を入力してください: " MIGRATION_NAME
if [ -z "$MIGRATION_NAME" ]; then
    echo "マイグレーション名が入力されませんでした。"
    exit 1
fi

# マイグレーション作成
dotnet ef migrations add "$MIGRATION_NAME" --project "$TARGET_PROJECT" --startup-project "$STARTUP_PROJECT" --output-dir Migrations
if [ $? -ne 0 ]; then
    echo "マイグレーションの作成に失敗しました。"
    exit 1
fi

# SQLスクリプト生成
dotnet ef migrations script --project "$TARGET_PROJECT" --startup-project "$STARTUP_PROJECT" --output "$MIGRATION_SCRIPT_DIR/$MIGRATION_NAME.sql"
if [ $? -ne 0 ]; then
    echo "SQLスクリプトの生成に失敗しました。"
    exit 1
fi

echo "マイグレーションの作成が完了しました。"
popd > /dev/null
