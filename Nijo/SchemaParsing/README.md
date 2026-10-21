# スキーマパース処理

このフォルダには、nijo.xmlのパース処理に関する実装が含まれています。

## 主な責務

- nijo.xmlの読み込みと解析
- スキーマ定義の妥当性検証
- スキーマ定義からオブジェクトモデルへの変換

## 処理フロー

1. XMLファイルの読み込み
2. スキーマ定義の構文解析
3. バリデーションチェック
4. 内部モデルへの変換

## ルート集約の種類
ルート集約の種類（[E_RootAggregateKind](./E_RootAggregateKind.cs)）によって、指定できる属性や検証内容が異なります。
種類ごとの検証は [RootAggregateValidation](./RootAggregateValidation/) フォルダにあります。

## 関連コンポーネント

- [`../Models/`](../Models/): 変換先の内部モデル定義
- [`../ImmutableSchema/`](../ImmutableSchema/): 不変なスキーマ定義
