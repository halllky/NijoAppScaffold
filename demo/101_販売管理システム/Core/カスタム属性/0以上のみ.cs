using System.Reflection;

namespace MyApp;

/// <summary>
/// カスタム属性「0以上のみ」のバリデーション。
/// この override 先のメソッドは、スキーマ定義でこのカスタム属性が付与された項目があるときだけ、項目の型ごとに自動生成される。
/// </summary>
partial class OverridedApplicationService {

    public override string? ValidateNotNegative(decimal? value, PropertyInfo propertyInfo) {
        if (value.HasValue && value.Value < 0m) {
            return "負の値は許可されていません。";
        }
        return null;
    }
}
