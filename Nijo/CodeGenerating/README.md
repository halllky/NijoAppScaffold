# コード生成のコンテキスト
このフォルダには、スキーマ定義からアプリケーションのソースコードを生成するときに共通して使われる型が含まれています。
ファイルの出力や共有ソースファイルの管理といった、入力の形式に依存しない仕組みは [CodeGenerator](../CodeGenerator/) にあります。

## 主な型

### コンテキスト
- **`CodeRenderingContext`**: スキーマ定義、プロジェクトの設定、C#（クラスライブラリ、ASP.NET Core WebAPI、ユニットテスト）と TypeScript（React.js）それぞれの自動生成フォルダを提供する
- **`GeneratedProjectOptions`**: 生成されるプロジェクトの設定オプション
- **`CodeRenderingOptions`**: コード生成処理のオプション

### ソースコード内部に登場する変数の抽象
- **`Instance API`**: 生成されるソースコード上のインスタンス構造を抽象化
- **`IPresentationLayerStructure`**: C#とTypeScriptの両方に登場する構造体
- **`ISchemaPathNode`**: スキーマ定義のパス構造を管理するインターフェース
- **`SchemaNodeIdentity`**: マッピング処理でスキーマノードを一意に識別

