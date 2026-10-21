# モデル定義
このフォルダには、スキーマ定義から生成される各種モデルの実装が含まれています。
nijo.xmlのルート要素直下のXML要素は、その種類によりどういったソースコードが生成されるかが異なります。例えば、永続化されるべきデータを表したモデル(DataModel)からは、RDBMSのテーブル定義や、Entity Framework Core のDbContextの設定が生成されます。一方、画面や帳票に表示されるべきデータを表したモデル(QueryModel)からは、検索条件のオブジェクト定義や、検索処理が生成されます。このフォルダにはモデル毎にそのモデルからどういったソースコードが生成されるべきかを定義する処理が書かれています。

このフォルダのルートに置かれるファイルは必ず `...Model.cs` という名前を持ちます。
そこで定義される静的クラスは、それと対応する `...ModelModules` フォルダの各モジュールのレンダリングメソッドを呼び出し、生成後のプロジェクトにどういったファイルが生成されるべきかを定義します。
どのルート集約に対してどのモデルのレンダリングメソッドが呼ばれるかは [AppScaffold](../AppScaffold.cs) で定義されています。

## 主な責務
- スキーマ定義の内部表現モデル
- コード生成のための中間モデル
- モデル間の変換ロジック

## スキーマ定義の外部仕様

### モデルタイプ
詳細は各ファイルを参照してください。

DataModel, QueryModel, StructureModel は `DataStructures` セクションに定義され、ルート集約の `IsStored`, `IsQueryable` 属性の組み合わせで決まります（[E_RootAggregateKind](../SchemaParsing/E_RootAggregateKind.cs)）。
`IsStored` が指定されていれば DataModel（`IsQueryable` も指定されていれば QueryModel と全く同じ型のモジュールも生成される）、`IsQueryable` のみなら QueryModel、どちらも無ければ StructureModel です。
それ以外のモデルは各セクションのルート集約の Type 属性で指定します。

- [DataModel](./DataModel.cs) : 永続化されるデータモデルを表します。RDBMSのテーブル定義やEntity Framework Coreの設定が生成されます。
- [QueryModel](./QueryModel.cs) : 画面や帳票に表示されるデータを表します。検索条件のオブジェクト定義や検索処理が生成されます。
- [CommandModel](./CommandModel.cs) : ユーザーの操作などによって実行されるコマンドを表します。APIエンドポイントやバリデーション処理が生成されます。
- [StaticEnumModel](./StaticEnumModel.cs) : 静的な列挙型を表します。C#の列挙型やTypeScriptの定数定義などが生成されます。
- [ValueObjectModel](./ValueObjectModel.cs) : 値オブジェクトを表します。複数のプリミティブ値をひとまとめにした不変オブジェクトが生成されます。
- [StructureModel](./StructureModel.cs) : 単純な構造体を表します。C#クラスとTypeScript型を生成し、サーバー/クライアント間で共有します。
- [ConstantModel](./ConstantModel.cs) : 定数モデルを表します。C#とJavaScript/TypeScriptで常に値が同期される定数のソースコードが生成されます。
