# コード生成のコンテキスト
このフォルダには、スキーマ定義からアプリケーションのソースコードを生成するときに共通して使われる型が含まれています。
ファイルの出力や共有ソースファイルの管理といった、入力の形式に依存しない仕組みは [Nijo.CodeGenerator](../../Nijo.CodeGenerator/) にあります。

## 主な型

### コンテキスト
- **`CodeRenderingContext`**: スキーマ定義、プロジェクトの設定、C#（クラスライブラリ、ASP.NET Core WebAPI、ユニットテスト）と TypeScript（React.js）それぞれの自動生成フォルダを提供する
- **`GeneratedProjectOptions`**: 生成されるプロジェクトの設定オプション
- **`CodeRenderingOptions`**: コード生成処理のオプション

### ソースコードのレンダリングの補助
- **`XmlCommentRendering`**: nijo.xml に書かれたコメントを、生成されるソースコード上のコメントとしてレンダリングする

データ構造から生成されるソースコード上のインスタンス構造の抽象（Instance API、IPresentationLayerStructure、ISchemaPathNode、SchemaNodeIdentity）は [DataStructures](../DataStructures/) にあります。

