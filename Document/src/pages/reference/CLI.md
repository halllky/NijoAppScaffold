---
title: コマンドライン API
outline: [2, 2]  # `##` の見出しをページ内ナビゲーションに表示
---

# コマンドライン API

このページでは nijo コマンドラインツールの使用方法について説明します。

## 概要

`nijo` は、データベース設計からWebアプリケーションのコードを自動生成するためのツールです。
プロジェクトの新規作成から開発、デバッグまでの各種操作をコマンドラインから実行できます。

### 基本的な使用方法

```bash
nijo <command> [options] [arguments]
```

## コマンド一覧

- [`generate`](#generate) - ソースコードの自動生成を実行します。
- [`generate-reference`](#generatereference) - 各モデルで利用可能なNodeOptionのHelpTextを.mdファイルとして出力します。
- [`new`](#new) - 新規プロジェクトを作成します。
- [`run`](#run) - プロジェクトのデバッグを開始します。
- [`serve`](#serve) - GUI用のサービスを展開します。
- [`validate`](#validate) - スキーマ定義の検証を行ないます。

## `generate`

ソースコードの自動生成を実行します。

### 使用法

```bash
nijo generate [project path] [options]
```

### 引数

#### `project path`

カレントディレクトリから操作対象のnijoプロジェクトへの相対パス

**デフォルト値**: 空文字列またはnull

**必須**: いいえ

### オプション

#### `-a`

QueryModelのデータ構造定義などの必ず実装しなければならないメソッドは通常abstractでレンダリングされるが、コンパイルエラーの確認などのためにあえてvirtualでレンダリングする。

**デフォルト値**: `false` (フラグオプション)

### 使用例

```bash
# 基本的な使用方法
nijo generate

# 引数を指定した使用方法
nijo generate my-project

# オプションを使用した例
nijo generate my-project -a
```

## `generate-reference`

各モデルで利用可能なNodeOptionのHelpTextを.mdファイルとして出力します。

### 使用法

```bash
nijo generate-reference [options]
```

### オプション

#### `-o`

出力先ディレクトリのパス

**型**: `string`

**必須**: はい

### 使用例

```bash
# 基本的な使用方法
nijo generate-reference

# オプションを使用した例
nijo generate-reference -o ./docs
```

## `new`

新規プロジェクトを作成します。

### 使用法

```bash
nijo new [project path]
```

### 引数

#### `project path`

カレントディレクトリから操作対象のnijoプロジェクトへの相対パス

**デフォルト値**: 空文字列またはnull

**必須**: いいえ

### 使用例

```bash
# 基本的な使用方法
nijo new

# 引数を指定した使用方法
nijo new my-project
```

## `run`

プロジェクトのデバッグを開始します。

### 使用法

```bash
nijo run [project path] [options]
```

### 引数

#### `project path`

カレントディレクトリから操作対象のnijoプロジェクトへの相対パス

**デフォルト値**: 空文字列またはnull

**必須**: いいえ

### オプション

#### `-a`

QueryModelのデータ構造定義などの必ず実装しなければならないメソッドは通常abstractでレンダリングされるが、コンパイルエラーの確認などのためにあえてvirtualでレンダリングする。

**デフォルト値**: `false` (フラグオプション)

#### `-c`

デバッグ実行の終了のトリガーは、通常はユーザーからのキー入力ですが、これを指定したときはこのファイルが存在したら終了と判定します。

**型**: `string`

#### `-b`

デバッグ開始時にブラウザを立ち上げません。

**デフォルト値**: `false` (フラグオプション)

#### `-n`

デバッグ開始時にコード自動生成をせず、アプリケーションの起動のみ行います。

**デフォルト値**: `false` (フラグオプション)

### 使用例

```bash
# 基本的な使用方法
nijo run

# 引数を指定した使用方法
nijo run my-project

# オプションを使用した例
nijo run my-project -n
nijo run my-project -b
nijo run my-project -a
```

## `serve`

GUI用のサービスを展開します。

### 使用法

```bash
nijo serve [project path] [options]
```

### 引数

#### `project path`

カレントディレクトリから操作対象のnijoプロジェクトへの相対パス

**デフォルト値**: 空文字列またはnull

**必須**: いいえ

### オプション

#### `-b`

デバッグ開始時にブラウザを立ち上げません。

**デフォルト値**: `false` (フラグオプション)

#### `-u`

GUI用のサービスが実行されるURLを明示的に指定します。

**型**: `string`

### 使用例

```bash
# 基本的な使用方法
nijo serve

# 引数を指定した使用方法
nijo serve my-project

# オプションを使用した例
nijo serve my-project -u value
nijo serve my-project -b
```

## `validate`

スキーマ定義の検証を行ないます。

### 使用法

```bash
nijo validate [project path]
```

### 引数

#### `project path`

カレントディレクトリから操作対象のnijoプロジェクトへの相対パス

**デフォルト値**: 空文字列またはnull

**必須**: いいえ

### 使用例

```bash
# 基本的な使用方法
nijo validate

# 引数を指定した使用方法
nijo validate my-project
```

