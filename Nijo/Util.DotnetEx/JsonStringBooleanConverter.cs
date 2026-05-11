using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nijo.Util.DotnetEx;

/// <summary>
/// Json文字列をboolへデシリアライズするコンバーター。
/// "True" / "true" / "1" を true として扱い、"False" / "false" / "0" を false とする。
/// 通常のboolトークンもサポートする。
/// </summary>
public sealed class JsonStringBooleanConverter : JsonConverter<bool> {
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.True) return true;
        if (reader.TokenType == JsonTokenType.False) return false;
        if (reader.TokenType == JsonTokenType.Null) return false;

        if (reader.TokenType == JsonTokenType.String) {
            var raw = reader.GetString()?.Trim();
            if (string.IsNullOrEmpty(raw)) return false;

            return raw.Equals("true", StringComparison.OrdinalIgnoreCase)
                || raw == "1";
        }

        if (reader.TokenType == JsonTokenType.Number) {
            // 数値 0/1 を許容
            if (reader.TryGetInt64(out var number)) {
                return number != 0;
            }
        }

        throw new JsonException($"Unexpected token '{reader.TokenType}' when parsing boolean.");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) {
        if (value) {
            writer.WriteStringValue("True");
        } else {
            writer.WriteNullValue();
        }
    }
}
