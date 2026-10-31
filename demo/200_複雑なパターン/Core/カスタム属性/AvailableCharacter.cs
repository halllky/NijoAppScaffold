using System.Reflection;

namespace MyApp;

/// <summary>
/// カスタム属性「AvailableCharacter」のバリデーション。
/// この override 先のメソッドは、スキーマ定義でこのカスタム属性が付与された項目があるときだけ、項目の型ごとに自動生成される。
/// ここに1回書けば、このカスタム属性が付与されたすべての集約の項目に適用される。
/// </summary>
partial class OverridedDbContext {

    public override string? ValidateAvailableCharacter(string? value, string attributeValue, PropertyInfo propertyInfo) {
        if (attributeValue == "半角数字および半角ハイフンのみ") {
            if (string.IsNullOrEmpty(value)) {
                return null; // エラーなし
            }
            foreach (char c in value) {
                if ((c < '0' || c > '9') && c != '-') {
                    return "半角数字および半角ハイフンのみで入力してください。";
                }
            }
            return null; // エラーなし
        }
        return null; // エラーなし
    }
}
