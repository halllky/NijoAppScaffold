#!/bin/bash
set -e

# unzip はロケールがUTF-8でないと、日本語などマルチバイト文字を含むファイル名を正しく復元できない。
# （zipアーカイブ自体にはUTF-8のファイル名が正しく格納されているが、非UTF-8ロケールの環境では
# unzipがそれを別の文字コードとして解釈してしまい、ファイル名が文字化けする。）
if locale -a 2>/dev/null | grep -qi "^en_US\.utf-\?8$"; then
  export LANG=en_US.UTF-8
elif locale -a 2>/dev/null | grep -qi "^C\.utf-\?8$"; then
  export LANG=C.UTF-8
fi

# デモ101（/demo/101_販売管理システム）から、GUI・ログ出力・ログイン認証など
# 基本的な機能があらかじめそろった新規プロジェクトテンプレート（demo101-template.zip）を作成する。
#
# 処理内容:
#   - git archive でデモ101のソース一式を展開する
#   - 販売管理業務固有の部分（nijo.xml の定義と、それに依存するフォルダ・ファイル）を削除し、
#     デモ101固有の名前を既定の名前に戻す（Demo101TemplateBuilder）
#   - 残ったデータ構造の初期マイグレーションと、client のロックファイルを作成する
#   - 作業で作られた一時ファイルを取り除き、フォルダをzip化する
#
# 出力される demo101-template.zip のリソース名は、
# Nijo.AppScaffold/GeneratedProject.cs の Templates ディクショナリで定義されているものと対応している。

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIJO_ROOT="$SCRIPT_DIR/.."
DEMO_DIR="demo/101_販売管理システム"

NIJO_EXE_PATH="$(cd "$NIJO_ROOT" && pwd)/Nijo.AppScaffold/bin/Debug/net10.0/nijo"

OUT_DIR="$NIJO_ROOT/temp_release"
ARCHIVE_ZIP="$OUT_DIR/demo101-template-src.zip"
WORK_DIR="$OUT_DIR/demo101-template-work"
FINAL_ZIP="$OUT_DIR/demo101-template.zip"

echo "出力先を初期化します。"
mkdir -p "$OUT_DIR"
rm -f "$ARCHIVE_ZIP" "$FINAL_ZIP"
rm -rf "$WORK_DIR"

echo "初期マイグレーションの作成に使う nijo をビルドします。"
dotnet build "$NIJO_ROOT/Nijo.AppScaffold" -c Debug

echo "デモ101のソース一式を圧縮します: $DEMO_DIR"
git -C "$NIJO_ROOT" archive HEAD:"$DEMO_DIR" --format=zip -o "$ARCHIVE_ZIP"
if [ ! -f "$ARCHIVE_ZIP" ]; then
  echo "デモ101の圧縮に失敗しました。"
  exit 1
fi

echo "解凍します: $WORK_DIR"
mkdir -p "$WORK_DIR"
# unzip (Info-ZIP) はファイル名をNFC(合成済み文字)のままファイル作成システムコールに渡すが、
# macOSのファイルシステム(APFS/HFS+)はNFD(濁点などを分解した形式)を要求するため、
# 日本語ファイル名（特に濁点・半濁点を含むもの）で "Illegal byte sequence" エラーになることがある。
# ditto はこの正規化を自動的に行うため、macOSではunzipの代わりにdittoを使う。
if command -v ditto > /dev/null 2>&1; then
  ditto -x -k "$ARCHIVE_ZIP" "$WORK_DIR"
else
  unzip -q "$ARCHIVE_ZIP" -d "$WORK_DIR"
fi

echo "販売管理業務固有の部分を削除し、名前を既定の名前に戻します。"
dotnet run --project "$SCRIPT_DIR/Demo101TemplateBuilder/Demo101TemplateBuilder.csproj" -- "$WORK_DIR"
if [ $? -ne 0 ]; then
  echo "販売管理業務固有の部分の削除に失敗しました。"
  exit 1
fi

echo "初期マイグレーションを作成します。"
printf '00000_Init\n' | NIJO_CLI_PATH="$NIJO_EXE_PATH" bash "$WORK_DIR/Task/ADD_MIGRATION.sh"
if ! ls "$WORK_DIR/Core/Migrations/"*_00000_Init.cs > /dev/null 2>&1; then
  echo "初期マイグレーションの作成に失敗しました。"
  exit 1
fi

echo "client のロックファイルを作成します。"
(cd "$WORK_DIR/client" && npm install --package-lock-only)

echo "一時ファイルを取り除きます。"
# git init するのは、テンプレートの .gitignore に従ってビルド結果や自動生成されたソースを消すためだけである
git -C "$WORK_DIR" init -q
git -C "$WORK_DIR" clean -fdXq
rm -rf "$WORK_DIR/.git"

echo "テンプレートを圧縮します: $FINAL_ZIP"
(cd "$WORK_DIR" && zip -r -q "$FINAL_ZIP" .)
if [ ! -f "$FINAL_ZIP" ]; then
  echo "テンプレートの圧縮に失敗しました。"
  exit 1
fi

echo "後片付けをします。"
rm -f "$ARCHIVE_ZIP"
rm -rf "$WORK_DIR"

echo "demo101-template.zip を作成しました: $FINAL_ZIP"

exit 0
