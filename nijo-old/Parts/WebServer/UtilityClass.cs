using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Nijo.Core;
using Nijo.Util.CodeGenerating;

namespace Nijo.Parts.WebServer {
    internal class UtilityClass : ISummarizedFile {
        internal const string CLASSNAME = "Util";
        internal const string GET_JSONOPTION = "GetJsonSrializerOptions";
        internal const string MODIFY_JSONOPTION = "ModifyJsonSrializerOptions";
        internal const string TO_JSON = "ToJson";
        internal const string PARSE_JSON = "ParseJson";
        internal const string ENSURE_OBJECT_TYPE = "EnsureObjectType";
        internal const string TRY_PARSE_AS_OBJECT_TYPE = "TryParseAsObjectType";
        internal const string PARSE_JSON_AS_OBJARR = "ParseJsonAsObjectArray";

        internal const string CUSTOM_CONVERTER_NAMESPACE = "CustomJsonConverters";
        private const string INT_CONVERTER = "IntegerValueConverter";
        private const string DECIMAL_CONVERTER = "DecimalValueConverter";
        private const string DATETIME_CONVERTER = "DateTimeValueConverter";
        private const string ENUM_CONVERTER = "EnumConverterForDisplayNameFactory";
        int ISummarizedFile.RenderingOrder => 999;
        void ISummarizedFile.OnEndGenerating(CodeRenderingContext context) {
            context.CoreLibrary.UtilDir(dir => {
                dir.Generate(RenderJsonConversionMethods());
            });
        }

        private SourceFile RenderJsonConversionMethods() => new SourceFile {
            FileName = "JsonConversion.cs",
            RenderContent = context => {

                return $$"""
                    namespace {{context.Config.RootNamespace}} {
                        using System.Text.Json;
                        using System.Text.Json.Nodes;
                        using System.Text.Json.Serialization;

                        public static partial class {{CLASSNAME}} {
                            public static void {{MODIFY_JSONOPTION}}(JsonSerializerOptions option) {
                                // 日本語文字や記号がUnicode変換されるのを避ける
                                option.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

                                // json中のenumの値を名前で設定できるようにする
                                option.Converters.Add(new CustomJsonConverters.EnumConverterForDisplayNameFactory());

                                // 値がnullの場合はレンダリングしない
                                option.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

                                // カスタムコンバータ
                                option.Converters.Add(new {{CUSTOM_CONVERTER_NAMESPACE}}.{{INT_CONVERTER}}());
                                option.Converters.Add(new {{CUSTOM_CONVERTER_NAMESPACE}}.{{DECIMAL_CONVERTER}}());
                                option.Converters.Add(new {{CUSTOM_CONVERTER_NAMESPACE}}.{{DATETIME_CONVERTER}}());

                    {{_jsonConverters.SelectTextTemplate(converter => $$"""
                                option.Converters.Add(new {{converter.ConverterClassName}}());
                    """)}}
                            }
                            public static JsonSerializerOptions {{GET_JSONOPTION}}() {
                                var option = new System.Text.Json.JsonSerializerOptions();
                                {{MODIFY_JSONOPTION}}(option);
                                return option;
                            }

                            /// <summary>
                            /// 共通のJSONオプションを設定したうえでJSON化します。
                            /// </summary>
                            public static string {{TO_JSON}}<T>(this T obj) {
                                return JsonSerializer.Serialize(obj, {{GET_JSONOPTION}}());
                            }
                            /// <summary>
                            /// 共通のJSONオプションを設定したうえでJSON化します。
                            /// </summary>
                            /// <param name="writeIndented">改行するかどうか</param>
                            public static string {{TO_JSON}}<T>(this T obj, bool writeIndented) {
                                var options = {{GET_JSONOPTION}}();
                                options.WriteIndented = writeIndented;
                                return JsonSerializer.Serialize(obj, options);
                            }
                            public static T {{PARSE_JSON}}<T>(string? json) {
                                if (json == null) throw new ArgumentNullException(nameof(json));
                                return JsonSerializer.Deserialize<T>(json, {{GET_JSONOPTION}}())!;
                            }
                            public static object {{PARSE_JSON}}(string? json, Type type) {
                                if (json == null) throw new ArgumentNullException(nameof(json));
                                return JsonSerializer.Deserialize(json, type, {{GET_JSONOPTION}}())!;
                            }
                            /// <summary>
                            /// 単に <see cref="JsonSerializer.Deserialize(JsonElement, Type, JsonSerializerOptions?)"/> で object?[] を指定すると JsonElement[] 型になり各要素のキャストができないためその回避
                            /// </summary>
                            public static object?[] {{PARSE_JSON_AS_OBJARR}}(string? json) {
                                return ParseJson<JsonElement[]>(json)
                                    .Select(jsonElement => (object?)(jsonElement.ValueKind switch {
                                        JsonValueKind.Undefined => null,
                                        JsonValueKind.Null => null,
                                        JsonValueKind.True => true,
                                        JsonValueKind.False => false,
                                        JsonValueKind.String => jsonElement.GetString(),
                                        JsonValueKind.Number => jsonElement.GetDecimal(),
                                        _ => jsonElement,
                                    }))
                                    .ToArray();
                            }
                            /// <summary>
                            /// JSONから復元されたオブジェクトを事後的に特定の型として扱いたいときに用いる
                            /// </summary>
                            public static T {{ENSURE_OBJECT_TYPE}}<T>(object? obj) where T : new() {
                                return (T){{ENSURE_OBJECT_TYPE}}(obj, typeof(T));
                            }
                            /// <summary>
                            /// JSONから復元されたオブジェクトを事後的に特定の型として扱いたいときに用いる
                            /// </summary>
                            public static object {{ENSURE_OBJECT_TYPE}}(object? obj, Type type) {
                                if (obj == null) return Activator.CreateInstance(type) ?? throw new ArgumentException(nameof(type));
                                var json = obj as string ?? {{TO_JSON}}(obj);
                                return {{PARSE_JSON}}(json, type);
                            }
                            /// <summary>
                            /// JSONから復元されたオブジェクトを事後的に特定の型として扱いたいときに用いる
                            /// </summary>
                            public static bool {{TRY_PARSE_AS_OBJECT_TYPE}}<T>(object? obj, out T parsed) where T : new() {
                                try {
                                    var json = obj as string ?? {{TO_JSON}}(obj);
                                    parsed = {{PARSE_JSON}}<T>(json);
                                    return true;
                                } catch {
                                    parsed = new();
                                    return false;
                                }
                            }
                        }
                    }

                    namespace {{context.Config.RootNamespace}}.{{CUSTOM_CONVERTER_NAMESPACE}} {
                        using System.Text;
                        using System.Text.Json;
                        using System.Text.Json.Serialization;
                        using System.Reflection;
                        using System.ComponentModel.DataAnnotations;

                        {{WithIndent(RenderIntConverter(context), "    ")}}
                        {{WithIndent(RenderDecimalConverter(context), "    ")}}
                        {{WithIndent(RenderDateTimeConverter(context), "    ")}}
                        {{WithIndent(RenderEnumConverter(context), "    ")}}
                    {{_jsonConverters.SelectTextTemplate(converter => $$"""

                        {{WithIndent(converter.ConverterClassDeclaring, "    ")}}
                    """)}}
                    }
                    """;
            },
        };

        private static string RenderIntConverter(CodeRenderingContext ctx) {
            return $$"""
                public class {{INT_CONVERTER}} : JsonConverter<int?> {
                    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

                        if (reader.TokenType == JsonTokenType.String) {
                            if (string.IsNullOrWhiteSpace(reader.GetString())) {
                                return null;
                            }
                            return decimal.TryParse(reader.GetString(), out var str)
                                ? (int)str
                                : throw new InvalidOperationException($"文字列の数値への変換に失敗しました。:{reader.GetString()}");
                        }
                        
                        if (reader.TokenType == JsonTokenType.Number) {
                            return reader.TryGetDecimal(out var dec)
                                ? (int)dec
                                : null;
                        }
                        
                        if (reader.TokenType == JsonTokenType.Null) {
                            return null;
                        }
                        
                        throw new InvalidOperationException($"文字列または数値に変換できない値です。:{Encoding.UTF8.GetString(reader.ValueSpan)}");
                    }

                    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options) {
                        if (value == null) {
                            writer.WriteNullValue();
                        } else {
                            // Dpulsでは今のところ3桁カンマ区切りの数値しかないため、ここで一律カンマ区切りに変換している。
                            writer.WriteStringValue(value?.ToString("#,##0"));
                        }
                    }
                }
                """;
        }
        private static string RenderDecimalConverter(CodeRenderingContext ctx) {
            return $$"""
                public class {{DECIMAL_CONVERTER}} : JsonConverter<decimal?> {
                    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                        if (reader.TokenType == JsonTokenType.String) {
                            if (string.IsNullOrWhiteSpace(reader.GetString())) {
                                return null;
                            }
                            return decimal.TryParse(reader.GetString(), out var str)
                                ? str
                                : throw new InvalidOperationException($"文字列の数値への変換に失敗しました。:{reader.GetString()}");
                        }

                        if (reader.TokenType == JsonTokenType.Number) {
                            return reader.TryGetDecimal(out var dec)
                                ? dec
                                : null;
                        }

                        if (reader.TokenType == JsonTokenType.Null) {
                            return null;
                        }

                        throw new InvalidOperationException($"文字列または数値に変換できない値です。:{Encoding.UTF8.GetString(reader.ValueSpan)}");
                    }

                    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options) {
                        if (value == null) {
                            writer.WriteNullValue();
                        } else {
                            // Dpulsでは今のところ3桁カンマ区切りの数値しかないため、ここで一律カンマ区切りに変換している。
                            writer.WriteStringValue(value?.ToString("#,##0.############################"));
                        }
                    }
                }
                """;
        }
        private static string RenderDateTimeConverter(CodeRenderingContext ctx) {
            return $$"""
                public class {{DATETIME_CONVERTER}} : JsonConverter<DateTime?> {
                    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                        var strDateTime = reader.GetString();
                        return string.IsNullOrWhiteSpace(strDateTime)
                            ? null
                            : DateTime.Parse(strDateTime);
                    }

                    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options) {
                        if (value == null) {
                            writer.WriteNullValue();
                        } else {
                            writer.WriteStringValue(value.Value.ToString("yyyy/MM/dd HH:mm:ss"));
                        }
                    }
                }
                """;
        }

        private static string RenderEnumConverter(CodeRenderingContext ctx) {
            return $$"""
                public class {{ENUM_CONVERTER}} : JsonConverterFactory {
                    /// <summary>
                    /// 値の型がenumならこのクラスの処理対象である。
                    /// </summary>
                    public override bool CanConvert(Type typeToConvert) {
                        return typeToConvert.IsEnum;
                    }

                    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {

                        var conveterType = typeof(EnumConverterForDisplayName<>).MakeGenericType(typeToConvert);
                        return (JsonConverter)Activator.CreateInstance(conveterType)!;

                    }

                    private class EnumConverterForDisplayName<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum {

                        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                            if (reader.TokenType == JsonTokenType.Null) {
                                return default;
                            }

                            if (reader.TokenType == JsonTokenType.String) {
                                // [Display(Name = ...)] が指定されている場合はその名前で読み取る
                                var strValue = reader.GetString();
                                foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static)) {
                                    var display = field.GetCustomAttribute<DisplayAttribute>();
                                    if (display?.Name == strValue || field.Name == strValue) {
                                        return (TEnum)field.GetValue(null)!;
                                    }
                                }

                                // 一致するものが無い場合はenum値として解釈
                                if (int.TryParse(strValue, out var intValue)
                                 && Enum.IsDefined(typeof(TEnum), intValue)) {
                                    return (TEnum)Enum.ToObject(typeof(TEnum), intValue);
                                }
                            }

                            // 数値の場合はenum値として解釈
                            if (reader.TokenType == JsonTokenType.Number) {
                                var intValue = reader.GetInt32();
                                if (Enum.IsDefined(typeof(TEnum), intValue)) {
                                    return (TEnum)Enum.ToObject(typeof(TEnum), intValue);
                                }
                            }

                            throw new JsonException($"Cannot Convert '{reader.GetString()}' to {typeof(TEnum).Name}");
                        }

                        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) {
                            var field = typeof(TEnum).GetField(value.ToString());
                            var display = field?.GetCustomAttribute<DisplayAttribute>();

                            var output = display?.Name ?? value.ToString();
                            writer.WriteStringValue(output);
                        }
                    }
                }
                """;
        }

        #region JSONコンバータ
        internal class CustomJsonConverter {
            /// <summary>コンバータクラス名</summary>
            internal required string ConverterClassName { get; init; }
            /// <summary>コンバータクラス定義</summary>
            internal required string ConverterClassDeclaring { get; init; }
        }
        private readonly List<CustomJsonConverter> _jsonConverters = new();
        /// <summary>
        /// 特定の型をJSON変換する処理を登録します。
        /// </summary>
        internal void AddJsonConverter(CustomJsonConverter customJsonConverter) {
            _jsonConverters.Add(customJsonConverter);
        }
        #endregion JSONコンバータ
    }
}
