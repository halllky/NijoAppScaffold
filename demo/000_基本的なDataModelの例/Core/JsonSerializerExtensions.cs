using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyApp;

/// <summary>
/// JSON変換設定。
/// WebAPI が HTTP リクエスト/レスポンスをC#のクラスに変換する際や、ログ出力用のJSON変換で使われる。
/// </summary>
public static class JsonSerializerExtensions {

    /// <summary>
    /// 引数で渡された JsonSerializerOptions を編集し、同じインスタンスを返す。
    /// </summary>
    public static JsonSerializerOptions EditDefaultJsonSerializerOptions(this JsonSerializerOptions option) {

        // 型ごとのシリアライズ設定
        option.Converters.Add(new BooleanConverter());
        option.Converters.Add(new Int32Converter());
        option.Converters.Add(new DecimalConverter());
        option.Converters.Add(new DateTimeConverter());
        option.Converters.Add(new DateOnlyConverter());
        option.Converters.Add(new YearMonthJsonConverter());
        option.Converters.Add(new EnumDisplayNameConverterFactory());

        foreach (var converter in ValueObjectJsonConverters.GetConverters()) {
            option.Converters.Add(converter);
        }

        // 日本語などがUnicodeエスケープされるのを防ぐ
        option.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

        // nullは省略せずそのまま出力する。
        // * HTMLでは項目自体が存在しない（undefined）よりは null の方が扱いやすいため
        // * react-hook-form などのフォームライブラリで、プロパティが欠落していると正しく動作しない場合があるため
        option.DefaultIgnoreCondition = JsonIgnoreCondition.Never;

        return option;
    }
    /// <summary>
    /// 真偽値。通常のtrue/falseだけでなく、"TRUE" "True" "true" といった文字列と、1を真として変換する。
    /// </summary>
    private class BooleanConverter : JsonConverterFactory {
        public override bool CanConvert(Type typeToConvert) {
            return typeToConvert == typeof(bool) || typeToConvert == typeof(bool?);
        }
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
            return (JsonConverter)Activator.CreateInstance(
                typeof(BooleanConverterInner<>).MakeGenericType(typeToConvert))!;
        }
        private class BooleanConverterInner<T> : JsonConverter<T> {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                bool? b;
                switch (reader.TokenType) {
                    case JsonTokenType.Null:
                        b = null;
                        break;
                    case JsonTokenType.True:
                        b = true;
                        break;
                    case JsonTokenType.False:
                        b = false;
                        break;
                    case JsonTokenType.String:
                        var str = reader.GetString();
                        if (string.IsNullOrWhiteSpace(str)) {
                            b = null;
                        } else if (bool.TryParse(str, out var p)) {
                            b = p;
                        } else if (str == "1") {
                            b = true;
                        } else if (str == "0") {
                            b = false;
                        } else {
                            throw new JsonException($"不正な真偽値形式です: {str}");
                        }
                        break;
                    case JsonTokenType.Number:
                        if (reader.TryGetInt32(out var i)) {
                            if (i == 1) b = true;
                            else if (i == 0) b = false;
                            else throw new JsonException($"不正な真偽値形式です: {i}");
                        } else {
                            throw new JsonException($"不正な真偽値形式です");
                        }
                        break;
                    default:
                        throw new JsonException($"不正な真偽値形式です: {reader.TokenType}");
                }

                if (b == null) {
                    if (default(T) is null) return default!;
                    throw new JsonException("null は許可されていません。");
                }
                return (T)(object)b.Value;
            }
            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
                if (value is null) {
                    writer.WriteNullValue();
                } else {
                    writer.WriteBooleanValue((bool)(object)value);
                }
            }
        }
    }
    /// <summary>
    /// 整数。クライアント側ではstring型。
    /// </summary>
    private class Int32Converter : JsonConverter<int?> {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null) {
                return null;

            } else if (reader.TokenType == JsonTokenType.String) {
                // カンマ区切りになった数値形式を考慮
                var strValue = reader.GetString()?.Trim().Replace(",", "");
                if (string.IsNullOrWhiteSpace(strValue)) {
                    return null;
                }
                return int.Parse(strValue);
            } else if (reader.TokenType == JsonTokenType.Number) {
                return reader.GetInt32();
            }

            throw new JsonException($"不正な整数形式です: {reader.GetString()}");
        }
        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options) {
            if (value == null) {
                writer.WriteNullValue();
            } else {
                writer.WriteStringValue(value.Value.ToString());
            }
        }
    }
    /// <summary>
    /// 実数。クライアント側ではstring型。
    /// </summary>
    private class DecimalConverter : JsonConverter<decimal?> {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null) {
                return null;

            } else if (reader.TokenType == JsonTokenType.String) {
                // カンマ区切りになった数値形式を考慮
                var strValue = reader.GetString()?.Trim().Replace(",", "");
                if (string.IsNullOrWhiteSpace(strValue)) {
                    return null;
                }
                return decimal.Parse(strValue);

            } else if (reader.TokenType == JsonTokenType.Number) {
                return reader.GetDecimal();
            }

            throw new JsonException($"不正な実数形式です: {reader.GetString()}");
        }
        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options) {
            if (value == null) {
                writer.WriteNullValue();
            } else {
                writer.WriteStringValue(value.Value.ToString("#.#"));
            }
        }
    }
    /// <summary>
    /// 日付時刻。 yyyy-MM-dd HH:mm:ss 形式でシリアライズする。空文字はnullとみなす
    /// </summary>
    private class DateTimeConverter : JsonConverter<DateTime?> {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var strValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(strValue)) {
                return null;
            }
            return DateTime.Parse(strValue);
        }
        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options) {
            if (value == null) {
                writer.WriteNullValue();
            } else {
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
    }
    /// <summary>
    /// 日付。yyyy-MM-dd形式でシリアライズする。空文字はnullとみなす
    /// </summary>
    private class DateOnlyConverter : JsonConverter<DateOnly?> {
        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var strValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(strValue)) {
                return null;
            }
            return DateOnly.Parse(strValue);
        }
        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options) {
            if (value == null) {
                writer.WriteNullValue();
            } else {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
    /// <summary>
    /// 年月。yyyy-MM形式でシリアライズする。空文字はnullとみなす
    /// </summary>
    public class YearMonthJsonConverter : JsonConverter<YearMonth?> {
        public override YearMonth? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Number) {
                return new YearMonth(reader.GetInt32());

            } else if (reader.TokenType == JsonTokenType.String) {
                string value = reader.GetString() ?? throw new JsonException();

                if (string.IsNullOrWhiteSpace(value)) {
                    return null;
                }

                if (int.TryParse(value, out int result)) {
                    return new YearMonth(result);
                }

                // YYYY-MM形式の場合
                if (value.Length == 7 && value[4] == '-') {
                    int year = int.Parse(value.Substring(0, 4));
                    int month = int.Parse(value.Substring(5, 2));
                    return new YearMonth(year, month);
                }

                throw new JsonException($"不正な年月形式です: {value}");
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, YearMonth? value, JsonSerializerOptions options) {
            if (value == null) {
                writer.WriteNullValue();
            } else {
                writer.WriteStringValue($"{value.Value.Year:0000}-{value.Value.Month:00}");
            }
        }
    }
    /// <summary>
    /// enumのJSONシリアライズ設定。Display属性が指定されている場合はそれが優先。指定なしの場合は物理名でシリアライズ
    /// </summary>
    private class EnumDisplayNameConverterFactory : JsonConverterFactory {
        public override bool CanConvert(Type typeToConvert) {
            // 通常のenum型、またはNullable<enum>型をサポート
            return typeToConvert.IsEnum || (Nullable.GetUnderlyingType(typeToConvert)?.IsEnum ?? false);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
            // typeToConvert が Nullable<TEnum> の場合は TEnum を、
            // そうでなければそのままの型 (TEnum) を取得する。
            var underlyingType = Nullable.GetUnderlyingType(typeToConvert);
            var enumTypeToConvert = underlyingType ?? typeToConvert;

            // EnumDisplayNameConverter<TEnum> を作成 (TEnum は非nullableなenum型)
            var converterType = typeof(EnumDisplayNameConverter<>).MakeGenericType(enumTypeToConvert);

            // EnumDisplayNameConverter のコンストラクタに引数がないことをユーザーが修正済みのため、引数なしでインスタンス化
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    }
    /// <summary>
    /// enumのJSONシリアライズ設定。[Display(Name = "...")]属性が指定されている場合はNameの値でシリアライズされる。指定なしの場合は物理名でシリアライズ
    /// </summary>
    private class EnumDisplayNameConverter<T> : JsonConverter<T?> where T : struct, Enum {

        // Display属性の値とenumの値のマッピングのキャッシュ
        private Dictionary<string, T>? _enumDisplayNameMap;
        private Dictionary<string, T> GetEnumDisplayNameMap() {
            _enumDisplayNameMap ??= Enum
                .GetValues<T>()
                .ToDictionary(e => e.GetType().GetField(e.ToString())?.GetCustomAttribute<DisplayAttribute>()?.Name ?? e.ToString(), e => e);
            return _enumDisplayNameMap;
        }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            // [Display(Name = "...")]属性が指定されている場合はNameの値でシリアライズされるため strValueは Name の値。
            // GetCustomAttribute で取得した Name 属性の値と突き合わせ、一致するものを探す。
            var strValue = reader.GetString();

            // 空文字はnull
            if (string.IsNullOrWhiteSpace(strValue)) {
                return default;
            }

            // GetCustomAttribute で取得した Name 属性の値と突き合わせ、一致するものを探す。
            var dict = GetEnumDisplayNameMap();
            if (dict.TryGetValue(strValue, out var enumValue)) {
                return enumValue;
            }

            // 一致するものがない場合は例外
            throw new JsonException($"不正なenum値です: {strValue}");
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options) {
            if (value == null) {
                writer.WriteNullValue();
            } else {
                // [Display(Name = "...")]属性が指定されている場合はNameの値でシリアライズ
                var displayName = GetEnumDisplayNameMap().Single(e => e.Value.Equals(value.Value)).Key;
                writer.WriteStringValue(displayName);
            }
        }
    }
}
