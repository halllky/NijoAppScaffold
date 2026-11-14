
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : ValueObjectsSection.cs
// ※ このファイルを生成したメソッド: RenderCSharp

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace MyApp;

/// <summary>
/// 医療従事者ID型。
/// 誤って医療従事者ID型ではない文字列型の項目に代入してしまったときにエラーで気付けるよう、値オブジェクトで定義している。
/// stringと相互に変換するときは明示的に(string)や(医療従事者ID型)でキャストする。
/// </summary>
public partial class 医療従事者ID型 : IEquatable<医療従事者ID型> {
    private readonly string _value;

    public 医療従事者ID型(string value) {
        _value = value;
    }

    public override bool Equals(object? obj) {
        return obj is 医療従事者ID型 other && Equals(other);
    }

    public bool Equals(医療従事者ID型? other) {
        if (other is null) return false;
        return _value == other._value;
    }

    public override int GetHashCode() {
        return _value?.GetHashCode() ?? 0;
    }

    public static bool operator ==(医療従事者ID型? left, 医療従事者ID型? right) {
        return EqualityComparer<医療従事者ID型>.Default.Equals(left, right);
    }
    public static bool operator !=(医療従事者ID型? left, 医療従事者ID型? right) {
        return !(left == right);
    }

    // 医療従事者ID型型の変数を (string) でキャストできるようにする
    [return: NotNullIfNotNull(nameof(value))]
    public static explicit operator string?(医療従事者ID型? value) {
        return value?._value;
    }
    // string型の変数を (医療従事者ID型) でキャストできるようにする
    [return: NotNullIfNotNull(nameof(value))]
    public static explicit operator 医療従事者ID型?(string? value) {
        return value == null ? null : new 医療従事者ID型(value);
    }

    public override string ToString() {
        return _value;
    }

    #region JSONシリアライズ
    public class JsonConverter : System.Text.Json.Serialization.JsonConverter<医療従事者ID型?> {
        public override 医療従事者ID型? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var strValue = reader.GetString();
            if (strValue == null) {
                return null;
            } else {
                return new 医療従事者ID型(strValue);
            }
        }

        public override void Write(Utf8JsonWriter writer, 医療従事者ID型? value, JsonSerializerOptions options) {
            writer.WriteStringValue(value?._value);
        }
    }
    #endregion JSONシリアライズ
}
