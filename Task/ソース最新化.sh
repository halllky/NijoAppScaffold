#!/bin/bash
set -e

# テンプレートと全デモのソースコード自動生成をかけなおし、
# テンプレート配布対象.txt に載っているファイルをテンプレートから各デモへコピーする。
#
# --check を付けると何も変更せず、配布対象のファイルがテンプレートと全デモで一致しているかだけを調べる。

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SOURCE_ROOT="$SCRIPT_DIR/../Nijo.NewProjectTemplate"
NIJO_EXE_PATH="$SCRIPT_DIR/../Nijo.AppScaffold/bin/Debug/net10.0/nijo"
DISTRIBUTION_LIST="$SCRIPT_DIR/テンプレート配布対象.txt"

# 配布対象の一覧からコメントと空行を除いたものを出力する
list_distribution_targets() {
    grep -vE '^(#|$)' "$DISTRIBUTION_LIST"
}

# 指定されたデモの配布対象がテンプレートと一致していなければ、その内容を出力して失敗を返す
check_demo() {
    local target_root="$1"
    local result=0
    while IFS= read -r path; do
        diff -rq --exclude=".*" "$SOURCE_ROOT/$path" "$target_root/$path" || result=1
    done < <(list_distribution_targets)
    return $result
}

# 指定されたデモへ配布対象をコピーする
distribute_to_demo() {
    local target_root="$1"
    while IFS= read -r path; do
        if [[ "$path" == */ ]]; then
            rsync -a --delete-excluded --exclude=".*" "$SOURCE_ROOT/$path" "$target_root/$path"
        else
            mkdir -p "$(dirname "$target_root/$path")"
            cp "$SOURCE_ROOT/$path" "$target_root/$path"
        fi
    done < <(list_distribution_targets)
}

if [ "$1" == "--check" ]; then
    RESULT=0
    for d in "$SCRIPT_DIR/../demo/"*/; do
        check_demo "$d" || RESULT=1
    done
    if [ $RESULT -ne 0 ]; then
        echo "配布対象のファイルがテンプレートとデモで一致していません。Task/ソース最新化.sh を実行してください。"
    fi
    exit $RESULT
fi

# nijo.exe リビルド
dotnet build "$SCRIPT_DIR/../Nijo.AppScaffold" -c Debug

# テンプレートプロジェクトのソースコード自動生成かけなおし
"$NIJO_EXE_PATH" generate "$SOURCE_ROOT"

# デモフォルダをループし全デモフォルダに対して処理を行う
for d in "$SCRIPT_DIR/../demo/"*/; do
    echo "Processing $d"

    # ソースコード自動生成かけなおし
    "$NIJO_EXE_PATH" generate "$d"

    distribute_to_demo "$d"
done

echo "コピー完了しました"
exit 0
