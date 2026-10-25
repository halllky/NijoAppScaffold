# DataStructures セクション
nijo.xml の `DataStructures` セクションに定義されるデータ構造の、検証とソースコード生成を定義します。

データ構造から何が生成されるかは、ルート集約に指定された属性で決まります。
ルート集約に指定するとソースコードが増える属性は [FeatureOption](./FeatureOption.cs) を継承し、
その属性の指定可否・検証・ソースコード生成のすべてを1クラスで担います。
各属性のフォルダには、その属性のクラスと、その属性から生成されるモジュールが置かれます。

- [IsStored](./IsStored/) : データベースに永続化されます。EFCoreのエンティティ、新規登録・更新・削除処理、入力検証などが生成されます。
- [IsQueryable](./IsQueryable/) : 画面や帳票から検索・照会されます。検索条件、検索処理、画面表示用データなどが生成されます。
- [UseSoftDelete](./UseSoftDelete/) : IsStored の削除処理を論理削除にします。
- [IsGenericLookupTable](./IsGenericLookupTable/) : IsStored のテーブルを、システムの運用中に値が追加削除されうる区分値（動的な定数）を格納する汎用参照テーブルにします。
- [Structure](./Structure/) : IsStored も IsQueryable も指定されていないデータ構造は、Webサーバーとクライアントの間で共有される構造体になります。
- [Members](./Members/) : 特定の機能に属さない、データ構造のメンバーに指定する属性。
- [Aggregates](./Aggregates/) : スキーマ定義の解析が通った後の、データ構造の集約のツリー（ルート集約、Child, Children、値メンバー、外部参照）。
- [ValueMemberTypes](./ValueMemberTypes/README.md) : 値メンバーの型。単語型、整数型、日付型、静的区分、値オブジェクトなど。

このフォルダの直下には、複数の属性のフォルダから共通して使われるモジュール（生成されるソースコード上のインスタンス構造の抽象、画面編集用オブジェクト、ディープイコール関数、メタデータなど）も置かれます。

どの属性を持つルート集約に対してどの順番でソースコードが生成されるかは [DataStructuresSection](./DataStructuresSection.cs) にあります。
