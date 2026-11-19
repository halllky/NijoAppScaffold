using System.Globalization;

namespace MyApp;

/// <summary>
/// スキーマ定義の属性による、項目の型ごとの入力チェック。
/// 自動生成された登録・更新コマンドの TryValidate から呼ばれる。
/// 以下はテンプレートの初期値なので、プロジェクトの事情に合わせて書き換えてよい。
/// </summary>
partial class OverridedDbContext {

    /// <summary>
    /// 属性 IsKey か IsNotNull が付いた項目の必須チェック。文字列は空白だけの値も未入力とみなす。
    /// </summary>
    public override string? ValidateRequired(object? value, string displayName) {
        var isEmpty = value is null
            || value is string str && string.IsNullOrWhiteSpace(str);

        return isEmpty
            ? MSG.RequiredError(displayName)
            : null;
    }

    /// <summary>
    /// 属性 MaxLength が付いた文字列の長さのチェック。
    /// 見た目の1文字を1と数える（サロゲートペアや結合文字も1文字）。
    /// DB の製品が長さを数える単位（UTF-16 の単位など）に合わせるときは、ここを書き換える。
    /// </summary>
    public override string? ValidateMaxLength(string? value, int maxLength, string displayName) {
        if (string.IsNullOrEmpty(value)) return null;

        return new StringInfo(value).LengthInTextElements > maxLength
            ? MSG.MaxLengthError(displayName, maxLength.ToString())
            : null;
    }

    /// <summary>
    /// 属性 TotalDigit と DecimalPlace が付いた数値の桁数のチェック。
    /// 桁数を超える値は、丸めも切り捨てもせずエラーにする。
    /// </summary>
    public override string? ValidateDigitsAndScales(decimal? value, int totalDigit, int decimalPlace, string displayName) {
        if (value == null) return null;

        var integerDigits = totalDigit - decimalPlace;
        var isIntegerPartOver = Math.Abs(Math.Truncate(value.Value)) >= Pow10(integerDigits);
        var isDecimalPartOver = Math.Round(value.Value, decimalPlace) != value.Value;

        return isIntegerPartOver || isDecimalPartOver
            ? MSG.DigitsError(displayName, integerDigits.ToString(), decimalPlace.ToString())
            : null;

        static decimal Pow10(int exponent) {
            var result = 1m;
            for (var i = 0; i < exponent; i++) result *= 10m;
            return result;
        }
    }

    /// <summary>
    /// 汎用参照テーブルを参照する項目に、テーブルに存在しない区分値が指定されたときのメッセージ。
    /// </summary>
    public override string GetGenericLookupValueNotFoundMessage(string displayName) {
        return MSG.GenericLookupValueNotFound(displayName);
    }
}
