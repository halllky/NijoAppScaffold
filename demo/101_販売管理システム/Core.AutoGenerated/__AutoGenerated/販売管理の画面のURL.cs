
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : ConstantsSection.cs
// ※ このファイルを生成したメソッド: RenderRootAggregate
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

namespace MyApp;

/// <summary>
/// 販売管理の画面のうち、サーバーで URL を組み立てるもの。値の決まりは「画面のURL」と同じ。
/// </summary>
public static class 販売管理の画面のURL {
    /// <summary>
    /// P101 売上詳細
    /// </summary>
    public static string 売上詳細(int 売上SEQ) => $"/uriage/edit/{ConstantTemplateKind.URL(売上SEQ.ToString(System.Globalization.CultureInfo.InvariantCulture))}";
    /// <summary>
    /// P301 商品詳細
    /// </summary>
    public static string 商品詳細(int 商品SEQ) => $"/shohin/{ConstantTemplateKind.URL(商品SEQ.ToString(System.Globalization.CultureInfo.InvariantCulture))}";
}
