---
sidebar_position: 2
---

# 開発ワークフロー

Nijoでの開発は、スキーマ定義ファイル `nijo.xml` を中心としたサイクルで進みます。

<img src="/NijoAppScaffold/img/workflow.drawio.svg" alt="開発ワークフロー図" style={{ width: '100%' }} />

## 1. プロジェクト作成

`nijo new` コマンドで新しいプロジェクトを作成します。

## 2. モデリング

`nijo serve` でGUIエディタを起動し、ブラウザ上でモデルを追加・編集します（図中の緑色の部分）。

* **[モデリングの基礎](./020000_modeling.md)**: モデリングの全体像
* **[DataModel](./020300_DataModel/index.md)**: データの保存形式
* **[QueryModel](./020400_QueryModel/index.md)**: データの検索・表示形式
* **[CommandModel](./020500_CommandModel/index.md)**: 処理・操作の定義

## 3. コード生成

更新された `nijo.xml` をもとに、ソースコードが自動生成されます（図中の橙色の部分）。

## 4. 実装

生成されたコードを利用しながら、UIや案件固有の業務ロジックを実装します。

## 5. 共有

`nijo.xml` をバージョン管理システムにコミットします。チームメンバーは常に最新のモデル図を参照できます。

---

2〜5の工程は一度きりではなく、開発期間を通じて繰り返されます。
