
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : ConstantTemplateKind.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

namespace MyApp;

/// <summary>
/// テンプレート文字列の定数の種類ごとの、引数を埋め込む前の変換。
/// 種類は nijo.xml の TemplateKind 属性で指定したもの。
/// 変換の中身は、このプロジェクトの直下（自動生成されるフォルダの外）に、このクラスの partial として書くこと。
/// 自動生成されるコードから呼ぶため、同じプロジェクトに置く必要がある。
/// </summary>
public static partial class ConstantTemplateKind {
    /// <summary>種類「JSON」の引数を埋め込む前の変換</summary>
    public static partial string JSON(string value);
    /// <summary>種類「文言」の引数を埋め込む前の変換</summary>
    public static partial string 文言(string value);
}
