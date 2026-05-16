---
applyTo: "Nijo/**/*.cs"
description: "Nijo の全コード生成で raw string, SelectTextTemplate, If, WithIndent を強制する"
---

## Render系メソッドの規約

ここでいう Render 系メソッドとは、生成コード文字列を返すメソッドと、その直下のテンプレート組み立て箇所を指す。非 Render 系の通常ロジックには適用しない。

- コードブロックの反復は `SelectTextTemplate`、条件分岐は `If` / `ElseIf` / `Else` を raw string の補間式として使うこと。詳細な使い方は [Nijo/CodeGenerating/TemplateTextHelper.cs](../../Nijo/CodeGenerating/TemplateTextHelper.cs) の XML コメントを参照。
- インデント
  - Render系メソッドが return するコードブロックはインデント 0 で組み立てること。
    インデントの制御はそのRender系メソッドを呼ぶ側の責務とする。
  - インデント制御は `WithIndent` を使うこと。2 引数版と 1 引数版があるが、1 引数版を優先すること。詳細な使い方は [Nijo/CodeGenerating/TemplateTextHelper.cs](../../Nijo/CodeGenerating/TemplateTextHelper.cs) の XML コメントを参照。
  - `SelectTextTemplate` / `If` / `ElseIf` / `Else` はそれを含む raw string の左端から開始すること。
  - `WithIndent` はインデントしたい列位置から開始すること。
  - TypeScript を生成する raw string の編集は `apply_patch` で行わず、 `sed` などの行単位の置換コマンドで修正すること。 `apply_patch` に不具合があり、C#の4スペースインデントとTypeScriptの2スペースインデントが混在する箇所で、インデントを崩す誤動作が発生するため。
- 禁止
  - 生成コードは raw string を起点に記述すること。崩れたからといって `string.Join`、後置換、後連結、`Trim` 逃げしてはならない。
  - 迷ったら既存の Render 実装を踏襲し、新しい流儀を持ち込まないこと。

## スキーマ定義と対応する構造体のレンダリング処理の規約

スキーマ定義（nijo.xml）と対応する構造体のレンダリング処理が頻出する。

* 子要素、子配列、外部参照といった複雑な構造体定義のレンダリング
* 同じ集約からレンダリングされるとあるオブジェクトから別のオブジェクトへの変換処理
* EFCore のエンティティ、検索条件オブジェクト、画面表示用オブジェクトなど生成後のクラス特有の事情

これは基本的に [Instance API](../../Nijo/CodeGenerating/Instance API.cs) を用いて生成物に対応するメタデータ構造を先に作る必要がある。

基本的な考え方は以下。

1. レンダリングされるクラス・構造体・フィールド・プロパティと1対1対応するクラスを定義し、それぞれに以下のインターフェースを実装する。
  * IInstancePropertyOwnerMetadata : フィールドを包含する構造体
  * IInstanceValuePropertyMetadata : これ自身がフィールドであり、かつ他のフィールドを包含する構造体ではないもの。具体的には int, string など。
  * IInstanceStructurePropertyMetadata : これ自身がフィールドであり、かつ他のフィールドを包含する構造体
  * これらメタデータクラスは、C#名、プロパティ名、多重度、子メンバー列挙規則を返す責務を持つ。
2. 処理のレンダリング箇所で `Variable` クラスのインスタンスを宣言し、この変数を起点としてその子孫フィールドの情報を生成しレンダリングに用いる。
  * フィールドの列挙には以下を使う。
    * CreateProperties : 構造体直下のフィールドを列挙
    * CreatePropertiesRecursively : 構造体直下のフィールドと、子構造体のフィールドを再帰的に列挙
    * Create1To1PropertiesRecursively : CreatePropertiesRecursively のうち元の変数との多重度が1:1のものを列挙。具体的にはネストされた配列のフィールドとその子孫が除外される。
   * 何階層もネストされた構造体のレンダリングが基本になるので、多くの場合は再起処理になる。
   * 「配列のときはこういうソースをレンダリングする」「外部参照(RefTo)のときはこう」のように処理を分岐することが多くなるはずだが、これには基本的に型スイッチを用いて上記1で定義したクラスを条件にして分岐をおこなう
   * Variable のルートからそのフィールドまでのパスのレンダリングには基本的に `GetJoinedPathFromInstance` を使う
   * 同じ集約からレンダリングされるとあるオブジェクトから別のオブジェクトへの変換処理において、右辺と左辺でどのフィールドが対応しているかの判定には `ISchemaPathNode.ToMappingKey` を用いる。
3. クラス名やプロパティ名の命名規則は、そのメタデータクラスの中に閉じ込めること。
  * 特定の構造体専用の命名規則を、別の `RenderXXX` メソッドや `GetName` 系の補助メソッド群に分散させないこと。
  * 再起関数の引数でエントリーからのパス名の履歴を持ち回って名前を組み立てるような実装は避けること。

既存実装例は多いが、主に以下を参照。

* [SaveCommand](../../Nijo/Models/DataModelModules/SaveCommand.cs)
* [EFCoreEntity](../../Nijo/Parts/CSharp/EFCoreEntity.cs)
* [SearchCondition](../../Nijo/Models/QueryModelModules/SearchCondition.cs)
