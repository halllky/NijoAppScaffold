namespace MyApp;

/// <summary>
/// nijo.xml の定数のうちテンプレート文字列のものについて、引数を埋め込む前の変換を種類ごとに定義する。
/// 画面側の client/src/ConstantTemplateKind.ts と同じ結果になるようにすること。
/// サーバーで組み立てた URL と画面で組み立てた URL が食い違わないようにするため。
/// </summary>
public static partial class ConstantTemplateKind {
    /// <summary>
    /// URL のパスやクエリ文字列に埋め込む値。"/" や "?" も含めてパーセントエンコードする。
    /// </summary>
    public static partial string URL(string value) => Uri.EscapeDataString(value);
}
