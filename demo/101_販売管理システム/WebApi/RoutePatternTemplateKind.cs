namespace MyApp.WebApi;

/// <summary>
/// テンプレート文字列の引数の位置に、渡された文字列を変換せずにそのまま置く変換。
/// テンプレート文字列の URL から、ルート定義のパターン（"/api/job/{ジョブID}" のように、引数の位置にルートパラメーターを置いたもの）を組み立てるときに使う。
/// 画面とサーバーで同じ URL を使うため、URL をテンプレート文字列にだけ定義し、ルート定義もそこから組み立てる。
/// 引数にはルートパラメーターの文字列を渡すので、文字列以外の型の引数をもつテンプレート文字列には使えない。
/// 変換しないので、利用者が入力した値など、任意の値を渡す用途には使わないこと。
/// </summary>
public sealed class RoutePatternTemplateKind : ITemplateKind {
    public static string URL(string value) => value;
    public static string URL(int value) => throw new NotSupportedException(INT_NOT_SUPPORTED);
    public static string 文言(string value) => value;
    public static string 文言(int value) => throw new NotSupportedException(INT_NOT_SUPPORTED);

    private const string INT_NOT_SUPPORTED = "整数の引数をもつテンプレート文字列からは、ルート定義のパターンを組み立てられません。";
}
