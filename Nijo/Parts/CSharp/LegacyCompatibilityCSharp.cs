using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Parts.CSharp {
    /// <summary>
    /// 旧版互換モードで生成する C# 関連ファイル。
    /// </summary>
    internal static class LegacyCompatibilityCSharp {

        internal static void RenderCore(CodeRenderingContext ctx) {
            ctx.CoreLibrary(dir => {
                dir.Directory("Util", utilDir => {
                    utilDir.Generate(RenderCoreDotnetExtensions(ctx));
                    utilDir.Generate(RenderFromTo(ctx));
                    utilDir.Generate(RenderPresentationContextCSharp(ctx));
                    utilDir.Generate(RenderRuntimeSetting(ctx));
                    utilDir.Generate(RenderJsonConversion(ctx));
                    utilDir.Generate(RenderRegexCache(ctx));
                    utilDir.Generate(RenderSpaceFinderConstCSharp(ctx));
                });
            });
        }

        internal static void RenderWebApi(CodeRenderingContext ctx) {
            ctx.WebapiProject(dir => {
                dir.Directory("Util", utilDir => {
                    utilDir.Generate(RenderAttachmentFileRepositoryWeb(ctx));
                    utilDir.Generate(RenderComplexPost(ctx));
                    utilDir.Generate(RenderWebApiDotnetExtensions(ctx));
                    utilDir.Generate(RenderSavingUploadedFilesFilter(ctx));
                });
            });
        }

        private static SourceFile RenderRuntimeSetting(CodeRenderingContext ctx) {
            return new SourceFile {
                FileName = "IRuntimeSetting.cs",
                Contents = $$"""
                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// 自動生成されたソースの中で実行時設定に依存する箇所があるところ、その実行時設定のデータ構造。
                    /// </summary>
                    public interface IRuntimeSetting {
                        /// <summary>
                        /// 現在接続中のDBの名前。 <see cref="DbProfiles"/> のいずれかのキーと一致
                        /// </summary>
                        string? CurrentDb { get; }

                        /// <summary>
                        /// 接続可能なデータベースの接続情報の一覧。
                        /// 開発時はこのリストの中から随時接続先を切り替えながら開発していく。
                        /// </summary>
                        List<DbProfile> DbProfiles { get; }

                        /// <summary>
                        /// <see cref="CurrentDb"/> で設定されている設定から接続文字列を返します。
                        /// </summary>
                        DbProfile? GetCurrentDbProfile() {
                            if (string.IsNullOrWhiteSpace(CurrentDb)) return null;
                            return DbProfiles.FirstOrDefault(profile => profile.Name == CurrentDb);
                        }
                    }

                    /// <summary>
                    /// <see cref="IRuntimeSetting"/> の中のDB接続情報
                    /// </summary>
                    public class DbProfile {
                        public string Name { get; set; } = string.Empty;
                        public string ConnStr { get; set; } = string.Empty;
                        public E_RDB? RDBMS { get; set; }
                        public string DbName { get; set; } = string.Empty;
                    }
                    public enum E_RDB {
                        SQLite,
                        Oracle,
                    }
                    """,
            };
        }

        private static SourceFile RenderCoreDotnetExtensions(CodeRenderingContext ctx) {
            return new SourceFile {
                FileName = "DotnetExtensions.cs",
                Contents = $$"""
                    namespace {{ctx.Config.RootNamespace}} {
                        using System;
                        using System.Collections;
                        using System.Collections.Generic;
                        using System.Linq;
                        using System.Text.Json;

                        public static class DotnetExtensions {

                            /// <summary>
                            /// 例外オブジェクトのメッセージを列挙します。InnerExceptionsも考慮します。
                            /// </summary>
                            public static IEnumerable<string> GetMessagesRecursively(this Exception ex, string indent = "") {
                                yield return indent + ex.Message;

                                if (ex is AggregateException aggregateException) {
                                    var innerExceptions = aggregateException.InnerExceptions
                                        .SelectMany(inner => inner.GetMessagesRecursively(indent + "  "));
                                    foreach (var inner in innerExceptions) {
                                        yield return inner;
                                    }
                                }

                                if (ex.InnerException != null) {
                                    foreach (var inner in ex.InnerException.GetMessagesRecursively(indent + "  ")) {
                                        yield return inner;
                                    }
                                }
                            }


                            /// <summary>
                            /// 引数の文字列の中から、見かけ上は1文字であるものの、
                            /// Unicoceのコードポイント換算だと複数とみなされる文字をピックアップします。
                            /// </summary>
                            public static IEnumerable<string> PickupMultipleCodeUnitCharacters(string str) {
                                var stringInfo = new System.Globalization.StringInfo(str);
                                for (int i = 0; i < stringInfo.LengthInTextElements; i++) {

                                    // 見かけ上の文字数基準で1文字ピックアップ
                                    var character = stringInfo.SubstringByTextElements(i, 1);

                                    // コードユニットが2以上なら該当
                                    if (character.Length > 1) yield return character;
                                }
                            }

                            /// <summary>
                            /// decimal型の変数の整数部分の桁数を取得します。
                            /// </summary>
                            public static int GetDigitsOfIntegerPart(this decimal value) {
                                // 文字列に変換して文字列の長さで判定
                                var splitted = Math.Abs(value).ToString().Split('.');
                                return splitted.Length == 0
                                    ? 0
                                    : splitted[0].Length;
                            }

                            /// <summary>
                            /// decimal型の変数の小数部分の桁数を取得します。
                            /// </summary>
                            public static int GetDigitsOfDecimalPart(this decimal value) {
                                // 文字列に変換して末尾のゼロを除去して文字列の長さを調べる
                                var splitted = value.ToString().Split('.');
                                return splitted.Length <= 1
                                    ? 0
                                    : splitted[1].TrimEnd('0').Length;
                            }
                        }
                    }
                    """,
            };
        }

        private static SourceFile RenderFromTo(CodeRenderingContext ctx) {
            return new SourceFile {
                FileName = "FromTo.cs",
                Contents = $$"""
                    using System;
                    using System.Text.Json.Serialization;

                    namespace {{ctx.Config.RootNamespace}} {
                        public partial class FromTo {
                            [JsonPropertyName("from")]
                            public virtual object? From { get; set; }
                            [JsonPropertyName("to")]
                            public virtual object? To { get; set; }
                        }
                        public partial class FromTo<T> : FromTo {
                            [JsonPropertyName("from")]
                            public new T From {
                                get => (T)base.From!;
                                set => base.From = value;
                            }
                            [JsonPropertyName("to")]
                            public new T To {
                                get => (T)base.To!;
                                set => base.To = value;
                            }
                        }
                    }
                    """,
            };
        }

        private static SourceFile RenderPresentationContextCSharp(CodeRenderingContext ctx) {
            return new SourceFile {
                FileName = "IPresentationContext.cs",
                Contents = $$"""
                    using System.Text.Json.Nodes;

                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// 更新系処理結果後のプレゼンテーション側の状態操作を提供します。
                    /// </summary>
                    public interface IPresentationContext : IDisplayMessageContainer {

                        /// <summary>
                        /// 処理実行時オプション
                        /// </summary>
                        SaveOptions Options { get; }

                        /// <summary>
                        /// エラーメッセージのコンテナを返します。
                        /// </summary>
                        T GetMessageContainerAs<T>() where T : IDisplayMessageContainer;
                        /// <summary>
                        /// 処理内部で明示的にメッセージコンテナの型を変更したい場合に使用。
                        /// 通常は <see cref="GetMessageContainerAs" /> で足りるはずなので使用機会は少ないはず。
                        /// </summary>
                        IDisplayMessageContainer MessageContainerRoot { get; set; }

                        /// <summary>
                        /// プレゼンテーション側に返す任意の戻り値。
                        /// </summary>
                        object? ReturnValue { get; set; }

                        /// <summary>
                        /// 処理が成功した旨のみをユーザーに伝えます。
                        /// </summary>
                        /// <param name="text">メッセージ</param>
                        ICommandResult Ok(string? text = null);

                        /// <summary>
                        /// 処理の途中でエラーが発生した旨をユーザーに伝えます。
                        /// </summary>
                        /// <param name="error">エラー内容。未指定の場合は既にこのインスタンスが保持しているエラーが表示されます。</param>
                        ICommandResult Error(string? error = null);

                        /// <summary>
                        /// 処理を続行してもよいかどうかユーザー側が確認し了承する必要がある旨を返します。
                        /// </summary>
                        /// <param name="message">確認メッセージ</param>
                        void AddConfirm(string message);

                        /// <summary>
                        /// 処理を続行してもよいかどうかユーザー側が確認し了承する必要がある旨を返します。
                        /// </summary>
                        /// <param name="confirm">確認メッセージ。未指定の場合は標準の確定確認メッセージが表示されます。</param>
                        ICommandResult Confirm(string? confirm = null);

                        /// <inheritdoc cref="AddConfirm">
                        ICommandResult Confirm(IEnumerable<string> confirms);

                        /// <summary>
                        /// この処理の中で処理続行の是非をユーザー側に確認するメッセージがあるかどうかを返します。
                        /// </summary>
                        bool HasConfirm();
                    }

                    /// <summary>
                    /// 更新系処理結果後のプレゼンテーション側の状態。
                    /// </summary>
                    public class PresentationContextResult {
                        /// <summary>処理全体の成否</summary>
                        public required bool Ok { get; set; }
                        /// <summary>処理結果の概要。「処理成功しました」など</summary>
                        public string? Summary { get; set; }
                        /// <summary>処理結果の詳細。項目ごとにメッセージが格納される。</summary>
                        public DisplayMessageContainerBase? Detail { get; set; }
                        /// <summary>確認メッセージ</summary>
                        public List<string> Confirms { get; set; } = [];
                        /// <summary>アプリケーション側からプレゼンテーション側に返す任意の値</summary>
                        public object? ReturnValue { get; set; }

                        /// <summary>
                        /// Web用。
                        /// 処理結果を表すJSONにして返します。
                        /// </summary>
                        public JsonObject ToJsonObject() {
                            var confirms = new JsonArray();
                            foreach (var conf in Confirms) confirms.Add(conf);

                            var detail = Detail == null ? null : new JsonArray(Detail.ToReactHookFormErrors().ToArray());

                            var returnValue = ReturnValue == null ? null : JsonNode.Parse(ReturnValue.ToJson());

                            // ここの戻り値のオブジェクトのプロパティ名や型は React hook 側と合わせる必要がある
                            return new JsonObject {
                                ["ok"] = Ok,
                                ["summary"] = Summary,
                                ["confirms"] = confirms,
                                ["detail"] = detail,
                                ["returnValue"] = returnValue,
                            };
                        }
                    }
                    """,
            };
        }

        private static SourceFile RenderJsonConversion(CodeRenderingContext ctx) {
            var valueObjectClassNames = ctx.GetMultiAggregateSourceFiles()
                .OfType<LegacyDbContextClass>()
                .SelectMany(sourceFile => sourceFile.GetValueObjectClassNames())
                .Concat(ctx.GetMultiAggregateSourceFiles()
                    .OfType<LegacyDefaultConfiguration>()
                    .SelectMany(sourceFile => sourceFile.GetValueObjectClassNames()))
                .Distinct()
                .ToArray();
            var writeModel2Roots = ctx.Schema.GetRootAggregates()
                .Where(root => root.Model is Models.WriteModel2)
                .OrderByDataFlow()
                .ToArray();
            var readModel2Roots = ctx.Schema.GetRootAggregates()
                .Where(root => root.Model is Models.ReadModel2)
                .OrderByDataFlow()
                .ToArray();
            var hasYearMonth = ctx.Schema.GetRootAggregates()
                .SelectMany(root => root.EnumerateThisAndDescendants())
                .SelectMany(aggregate => aggregate.GetMembers())
                .OfType<Nijo.ImmutableSchema.ValueMember>()
                .Any(member => member.Type is Nijo.ValueMemberTypes.YearMonthMember);
            var hasFileAttachment = System.Xml.Linq.XDocument.Load(ctx.Project.SchemaXmlPath).Descendants()
                .Any(element => element.Attribute(SchemaParsing.SchemaParseContext.ATTR_NODE_TYPE)?.Value == "file");
            var jsonValueObjectClassNames = valueObjectClassNames
                .Where(className => !(readModel2Roots.Length > 0 && className == "Date"))
                .Where(className => className != "YearMonth")
                .Where(className => className != "FileAttachmentMetadata")
                .ToArray();
            var contents = $$"""
                    namespace {{ctx.Config.RootNamespace}} {
                        using System.Text.Json;
                        using System.Text.Json.Nodes;
                        using System.Text.Json.Serialization;

                        public static partial class Util {
                            public static void ModifyJsonSrializerOptions(JsonSerializerOptions option) {
                                // 日本語文字や記号がUnicode変換されるのを避ける
                                option.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

                                // json中のenumの値を名前で設定できるようにする
                                option.Converters.Add(new CustomJsonConverters.EnumConverterForDisplayNameFactory());

                                // 値がnullの場合はレンダリングしない
                                option.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

                                // カスタムコンバータ
                                option.Converters.Add(new CustomJsonConverters.IntegerValueConverter());
                                option.Converters.Add(new CustomJsonConverters.DecimalValueConverter());
                                option.Converters.Add(new CustomJsonConverters.DateTimeValueConverter());

                    {{If(writeModel2Roots.Length > 0, () => $$"""
                                option.Converters.Add(new CustomJsonConverters.SaveCommandBaseConverter());
                    """)}}
                    {{jsonValueObjectClassNames.SelectTextTemplate(className => $$"""
                                option.Converters.Add(new {{className}}.JsonValueConverter());
                    """)}}
                    {{If(readModel2Roots.Length > 0, () => $$"""
                                option.Converters.Add(new CustomJsonConverters.DateJsonValueConverter());
                    {{If(hasYearMonth, () => $$"""
                                option.Converters.Add(new CustomJsonConverters.YearMonthJsonValueConverter());
                    """)}}
                    {{If(hasFileAttachment, () => $$"""
                                option.Converters.Add(new FileAttachmentMetadata.JsonValueConverter());
                    """)}}
                                option.Converters.Add(new CustomJsonConverters.DisplayDataBatchUpdateCommandConverter());
                    """)}}
                            }
                            public static JsonSerializerOptions GetJsonSrializerOptions() {
                                var option = new System.Text.Json.JsonSerializerOptions();
                                ModifyJsonSrializerOptions(option);
                                return option;
                            }

                            /// <summary>
                            /// 共通のJSONオプションを設定したうえでJSON化します。
                            /// </summary>
                            public static string ToJson<T>(this T obj) {
                                return JsonSerializer.Serialize(obj, GetJsonSrializerOptions());
                            }
                            /// <summary>
                            /// 共通のJSONオプションを設定したうえでJSON化します。
                            /// </summary>
                            /// <param name="writeIndented">改行するかどうか</param>
                            public static string ToJson<T>(this T obj, bool writeIndented) {
                                var options = GetJsonSrializerOptions();
                                options.WriteIndented = writeIndented;
                                return JsonSerializer.Serialize(obj, options);
                            }
                            public static T ParseJson<T>(string? json) {
                                if (json == null) throw new ArgumentNullException(nameof(json));
                                return JsonSerializer.Deserialize<T>(json, GetJsonSrializerOptions())!;
                            }
                            public static object ParseJson(string? json, Type type) {
                                if (json == null) throw new ArgumentNullException(nameof(json));
                                return JsonSerializer.Deserialize(json, type, GetJsonSrializerOptions())!;
                            }
                            /// <summary>
                            /// 単に <see cref="JsonSerializer.Deserialize(JsonElement, Type, JsonSerializerOptions?)"/> で object?[] を指定すると JsonElement[] 型になり各要素のキャストができないためその回避
                            /// </summary>
                            public static object?[] ParseJsonAsObjectArray(string? json) {
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
                            public static T EnsureObjectType<T>(object? obj) where T : new() {
                                return (T)EnsureObjectType(obj, typeof(T));
                            }
                            /// <summary>
                            /// JSONから復元されたオブジェクトを事後的に特定の型として扱いたいときに用いる
                            /// </summary>
                            public static object EnsureObjectType(object? obj, Type type) {
                                if (obj == null) return Activator.CreateInstance(type) ?? throw new ArgumentException(nameof(type));
                                var json = obj as string ?? ToJson(obj);
                                return ParseJson(json, type);
                            }
                            /// <summary>
                            /// JSONから復元されたオブジェクトを事後的に特定の型として扱いたいときに用いる
                            /// </summary>
                            public static bool TryParseAsObjectType<T>(object? obj, out T parsed) where T : new() {
                                try {
                                    var json = obj as string ?? ToJson(obj);
                                    parsed = ParseJson<T>(json);
                                    return true;
                                } catch {
                                    parsed = new();
                                    return false;
                                }
                            }
                        }
                    }

                    namespace {{ctx.Config.RootNamespace}}.CustomJsonConverters {
                        using System.Text;
                        using System.Text.Json;
                        using System.Text.Json.Serialization;
                        using System.Reflection;
                        using System.ComponentModel.DataAnnotations;

                        public class IntegerValueConverter : JsonConverter<int?> {
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
                        public class DecimalValueConverter : JsonConverter<decimal?> {
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
                        public class DateTimeValueConverter : JsonConverter<DateTime?> {
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
                        public class EnumConverterForDisplayNameFactory : JsonConverterFactory {
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
                        {{If(writeModel2Roots.Length > 0, () => $$"""

                        {{RenderSaveCommandBaseConverter(writeModel2Roots)}}
                        """)}}
                        {{If(readModel2Roots.Length > 0, () => $$"""

                        {{RenderReadModel2JsonConverters(readModel2Roots)}}
                        """)}}
                        {{If(jsonValueObjectClassNames.Length > 0, () => $$"""


                        """)}}
                    }
                    """;
            contents = contents
                .Replace("options) {\n\n            if", "options) {\n    \n            if")
                .Replace("}\n\n            if", "}\n    \n            if")
                .Replace("}\n\n            throw new InvalidOperationException", "}\n    \n            throw new InvalidOperationException")
                .Replace("}\n\n        public override void Write", "}\n    \n        public override void Write")
                .Replace("}\n\n    public override JsonConverter? CreateConverter", "}\n    \n    public override JsonConverter? CreateConverter")
                .Replace("options) {\n\n        var conveterType", "options) {\n    \n        var conveterType")
                .Replace("Activator.CreateInstance(conveterType)!;\n\n    }", "Activator.CreateInstance(conveterType)!;\n    \n    }")
                .Replace("}\n\n    private class EnumConverterForDisplayName<TEnum>", "}\n    \n    private class EnumConverterForDisplayName<TEnum>")
                .Replace("Enum {\n\n        public override TEnum Read", "Enum {\n    \n        public override TEnum Read")
                .Replace("}\n\n        if (reader.TokenType == JsonTokenType.String)", "}\n    \n        if (reader.TokenType == JsonTokenType.String)")
                .Replace("}\n\n        // 数値の場合はenum値として解釈", "}\n    \n        // 数値の場合はenum値として解釈")
                .Replace("}\n\n        throw new JsonException", "}\n    \n        throw new JsonException")
                .Replace("}\n\n    public override void Write(Utf8JsonWriter", "}\n    \n    public override void Write(Utf8JsonWriter")
                .Replace("display = field?.GetCustomAttribute<DisplayAttribute>();\n\n        var output", "display = field?.GetCustomAttribute<DisplayAttribute>();\n    \n        var output");
            return new SourceFile {
                FileName = "JsonConversion.cs",
                Contents = contents,
            };
        }

        private static string RenderReadModel2JsonConverters(RootAggregate[] readModel2Roots) {
            return "    " + WithIndent($$"""
                public class DateJsonValueConverter : JsonConverter<Date?> {
                    public override Date? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                        var strDateTime = reader.GetString();
                        return string.IsNullOrWhiteSpace(strDateTime)
                            ? null
                            : new Date(DateTime.Parse(strDateTime));
                    }

                    public override void Write(Utf8JsonWriter writer, Date? value, JsonSerializerOptions options) {
                        if (value == null) {
                            writer.WriteNullValue();
                        } else {
                            writer.WriteStringValue(value.ToString());
                        }
                    }
                }

                public class YearMonthJsonValueConverter : JsonConverter<YearMonth?> {
                    public override YearMonth? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                        var strYearMonth = reader.GetString();
                        return string.IsNullOrWhiteSpace(strYearMonth)
                            ? null
                            : new YearMonth(DateTime.Parse($"{strYearMonth}/01"));
                    }

                    public override void Write(Utf8JsonWriter writer, YearMonth? value, JsonSerializerOptions options) {
                        if (value == null) {
                            writer.WriteNullValue();
                        } else {
                            writer.WriteStringValue(value.ToString());
                        }
                    }
                }

                class DisplayDataBatchUpdateCommandConverter : JsonConverter<DisplayDataClassBase> {
                    public override DisplayDataClassBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                        using var jsonDocument = JsonDocument.ParseValue(ref reader);
                        var dataType = jsonDocument.RootElement.GetProperty("dataType").GetString();
                        var value = jsonDocument.RootElement.GetProperty("values");

                {{readModel2Roots.SelectTextTemplate(root => $$"""
                        {{WithIndent(RenderReadModel2JsonConverterBranch(root), "        ")}}
                """)}}

                        throw new InvalidOperationException(MSG.ERRC0026(dataType!));
                    }

                    public override void Write(Utf8JsonWriter writer, DisplayDataClassBase? value, JsonSerializerOptions options) {
                        JsonSerializer.Serialize(writer, value, options);
                    }
                }
                """, "    ");
        }

        private static string RenderReadModel2JsonConverterBranch(RootAggregate root) {
            var displayData = new Models.ReadModel2Modules.DisplayData(root);

            return $$"""
                if (dataType == "{{root.DisplayName}}") {
                    return JsonSerializer.Deserialize<{{displayData.CsClassName}}>(value.GetRawText(), options)
                        ?? throw new InvalidOperationException(MSG.ERRC0025("{{displayData.CsClassName}}",value.GetRawText()));
                }
                """;
        }

        private static string RenderSaveCommandBaseConverter(RootAggregate[] writeModel2Roots) {
            return "    " + WithIndent($$"""
                class SaveCommandBaseConverter : JsonConverter<SaveCommandBase> {
                    public override SaveCommandBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                        using var jsonDocument = JsonDocument.ParseValue(ref reader);
                        var dataType = jsonDocument.RootElement.GetProperty("dataType").GetString();
                        var addOrModOrDel = jsonDocument.RootElement.GetProperty("addOrModOrDel").GetString();
                        var value = jsonDocument.RootElement.GetProperty("values");

                {{writeModel2Roots.SelectTextTemplate(root => $$"""
                        {{WithIndent(RenderSaveCommandBaseConverterBranch(root), "        ")}}
                """)}}

                        throw new InvalidOperationException(MSG.ERRC0037(jsonDocument.RootElement.GetRawText()));
                    }

                    public override void Write(Utf8JsonWriter writer, SaveCommandBase? value, JsonSerializerOptions options) {
                        JsonSerializer.Serialize(writer, value, options);
                    }
                }
                """, "    ");
        }

        private static string RenderSaveCommandBaseConverterBranch(RootAggregate root) {
            var create = new Models.WriteModel2Modules.DataClassForSave(root, Models.WriteModel2Modules.DataClassForSave.E_Type.Create);
            var save = new Models.WriteModel2Modules.DataClassForSave(root, Models.WriteModel2Modules.DataClassForSave.E_Type.UpdateOrDelete);

            return $$"""
                if (dataType == "{{root.PhysicalName}}") {
                    if (addOrModOrDel == "ADD") {
                        return new CreateCommand<{{create.CsClassName}}> {
                            Values = JsonSerializer.Deserialize<{{create.CsClassName}}>(value.GetRawText(), options)
                                ?? throw new InvalidOperationException(MSG.ERRC0025("{{create.CsClassName}}",value.GetRawText())),
                        };
                    } else if (addOrModOrDel == "MOD") {
                        return new UpdateCommand<{{save.CsClassName}}> {
                            Values = JsonSerializer.Deserialize<{{save.CsClassName}}>(value.GetRawText(), options)
                                ?? throw new InvalidOperationException(MSG.ERRC0025("{{save.CsClassName}}",value.GetRawText())),
                            Version = jsonDocument.RootElement.GetProperty("version").GetInt32(),
                        };
                    } else if (addOrModOrDel == "DEL") {
                        return new DeleteCommand<{{save.CsClassName}}> {
                            Values = JsonSerializer.Deserialize<{{save.CsClassName}}>(value.GetRawText(), options)
                                ?? throw new InvalidOperationException(MSG.ERRC0025("{{save.CsClassName}}",value.GetRawText())),
                            Version = jsonDocument.RootElement.GetProperty("version").GetInt32(),
                        };
                    } else if (addOrModOrDel == "NONE") {
                        return new NoOperation<{{save.CsClassName}}> {
                            Values = JsonSerializer.Deserialize<{{save.CsClassName}}>(value.GetRawText(), options)
                                ?? throw new InvalidOperationException(MSG.ERRC0025("{{save.CsClassName}}",value.GetRawText())),
                            Version = jsonDocument.RootElement.GetProperty("version").GetInt32(),
                        };
                    }
                }
                """;
        }

        private static SourceFile RenderRegexCache(CodeRenderingContext ctx) {
            return new SourceFile {
                FileName = "RegexCache.cs",
                Contents = $$"""
                    using System;
                    using System.Collections.Generic;
                    using System.Linq;
                    using System.Text;
                    using System.Threading.Tasks;

                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// 正規表現のキャッシュのようなもの。
                    /// ソースコード中で直接Regexのインスタンスを生成する場合と比較して実行時のパフォーマンスに優れる。
                    /// </summary>
                    public static partial class RegexCache {

                        /// <summary>
                        /// 半角英数字のみ、スペースも記号も含まない文字列か否かを判定します。
                        /// </summary>
                        [System.Text.RegularExpressions.GeneratedRegex("^[a-zA-Z0-9]+$")]
                        public static partial System.Text.RegularExpressions.Regex OnlyAlphaNumeric();
                    }
                    """,
            };
        }

        private static SourceFile RenderSpaceFinderConstCSharp(CodeRenderingContext ctx) {
            var members = new List<string>();
            if (ctx.Config.MaxFileSizeMB.HasValue) {
                members.Add($$"""
                    /// <summary>添付可能なファイルの上限サイズ（メガバイト）</summary>
                    public const int MAX_FILE_SIZE_MB = {{ctx.Config.MaxFileSizeMB.Value}};
                    """);
            }
            if (ctx.Config.MaxTotalFileSizeMB.HasValue) {
                members.Add($$"""
                    /// <summary>一度に複数ファイル添付する際のトータルの上限サイズ（メガバイト）</summary>
                    public const int MAX_TOTAL_FILE_SIZE_MB = {{ctx.Config.MaxTotalFileSizeMB.Value}};
                    """);
            }
            if (!string.IsNullOrWhiteSpace(ctx.Config.AttachmentFileExtensions)) {
                var extensions = string.Join(", ", ctx.Config.AttachmentFileExtensions.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(ext => $"\".{ext}\""));
                members.Add($$"""
                    /// <summary>添付可能なファイルの拡張子</summary>
                    public static IEnumerable<string> ATTACHMENT_FILE_EXTENSIONS => [{{extensions}}];
                    """);
            }

            return new SourceFile {
                FileName = "SpaceFinderConst.cs",
                Contents = $$"""
                    using System.Collections.Generic;

                    namespace {{ctx.Config.RootNamespace}} {
                        public class SpaceFinderConst {
                    {{members.SelectTextTemplate(source => $$"""
                            {{WithIndent(source, "        ")}}
                    """)}}
                        }
                    }
                    """,
            };
        }

        private static SourceFile RenderAttachmentFileRepositoryWeb(CodeRenderingContext ctx) {
            return new SourceFile {
                FileName = "AttachmentFileRepositoryWeb.cs",
                Contents = $$"""
                    namespace {{ctx.Config.RootNamespace}} {
                        /// <summary>
                        /// 添付ファイル保存処理のWebアプリケーション側の実装。
                        /// </summary>
                        public class AttachmentFileRepositoryWeb : IFileAttachmentRepository {

                            public AttachmentFileRepositoryWeb(RuntimeSettings.Server settings) {
                                _settings = settings;
                            }
                            private readonly RuntimeSettings.Server _settings;


                            public FileInfo? FindFile(FileAttachmentId id) {
                                var dirName = Path.Combine(GetStorageDirectory(), id.ToString());
                                if (!Directory.Exists(dirName)) {
                                    return null;
                                }

                                var files = Directory.GetFiles(dirName);
                                if (files.Length == 0) {
                                    return null;
                                }
                                if (files.Length >= 2) {
                                    throw new InvalidOperationException($"1つの添付ファイル保存ディレクトリ内に複数のファイルが存在します: {dirName}");
                                }

                                return new FileInfo(files[0]);
                            }


                            public async Task SaveFileAsync(FileAttachmentId id, string fileName, Stream stream, ICollection<string> errors) {
                                // 入力エラーチェック
                                var invalidChar = _invalidChar ??= Path.GetInvalidFileNameChars();
                                if (id.ToString().Any(c => invalidChar.Contains(c))) {
                                    errors.Add($"ファイルID '{id}' にファイル名として無効な文字が含まれています。");
                                }
                                if (fileName.Any(c => invalidChar.Contains(c))) {
                                    errors.Add($"ファイル '{fileName}' のファイル名に無効な文字が含まれています。");
                                }
                                if (errors.Count > 0) return;

                                /// 格納先フォルダを作成
                                var dirName = Path.Combine(GetStorageDirectory(), id.ToString());
                                Directory.CreateDirectory(dirName);

                                // 保存
                                var path = Path.Combine(dirName, fileName);
                                using var sw = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                                await stream.CopyToAsync(sw);
                            }


                            /// <summary>
                            /// 添付ファイル保存先ディレクトリを返します。
                            /// </summary>
                            private string GetStorageDirectory() {
                                if (_cache == null) {
                                    // 添付ファイルが保存されるディレクトリを決定
                                    if (string.IsNullOrWhiteSpace(_settings.UploadedFileDir))
                                        throw new InvalidOperationException("添付ファイル保存先ディレクトリが設定されていません。");

                                    _cache = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), _settings.UploadedFileDir));
                                }
                                return _cache;
                            }
                            private string? _cache;


                            // ファイル名に使用できない文字の一覧
                            private static char[]? _invalidChar;
                        }
                    }
                    """,
            };
        }

        private static SourceFile RenderComplexPost(CodeRenderingContext ctx) {
            return new SourceFile {
                FileName = "ComplexPost.cs",
                Contents = $$"""
                    using Microsoft.AspNetCore.Mvc;
                    using Microsoft.AspNetCore.Mvc.ModelBinding;
                    using System.Text.Json;
                    using System.Text.Json.Nodes;

                    namespace {{ctx.Config.RootNamespace}} {

                        /// <summary>
                        /// Reactフック側の処理と組み合わせて複雑な挙動を実現するPOSTリクエスト。
                        /// 例えば後述のような挙動を実現する。
                        /// 詳細な挙動を調べる場合はReact側のcomplexPost関連のソースも併せて参照のこと。
                        ///{{" "}}
                        /// <list type="bullet">
                        /// <item>ブラウザからサーバーへのリクエストで入力フォームの内容とファイル内容を同時に送信し（multipart/form-data）、サーバー側ではそれを意識せず利用できるようにする</item>
                        /// <item>「～ですがよろしいですか？」の確認ダイアログの表示と、それがOKされたときに同じ内容のリクエストを再送信する</item>
                        /// <item>POSTレスポンスの結果を React hook forms のsetErrorを利用して画面上の各項目の脇に表示</item>
                        /// <item>POSTレスポンスで返されたファイルのダウンロードを自動的に開始する</item>
                        /// <item>POSTレスポンスのタイミングで React Router を使った別画面へのリダイレクト</item>
                        /// </list>
                        /// </summary>
                        [ModelBinder(BinderType = typeof(GenericComplexPostRequestBinder))]
                        public class ComplexPostRequest<T> : ComplexPostRequest {
                    #pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
                            /// <summary>
                            /// 入力フォームの内容
                            /// </summary>
                            public T Data { get; set; }
                    #pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
                        }


                        /// <inheritdoc cref="ComplexPostRequest{T}"/>
                        [ModelBinder(BinderType = typeof(ComplexPostRequestBinder))]
                        public class ComplexPostRequest {
                            /// <summary>
                            /// 「～ですがよろしいですか？」の確認を無視します。
                            /// </summary>
                            public bool IgnoreConfirm { get; set; }


                            #region HTTPリクエストとC#クラスの変換
                            /// <summary>
                            /// HTTPリクエストの内容をパースして <see cref="ComplexPostRequest{T}"/> クラスのインスタンスを作成します。
                            /// </summary>
                            protected class GenericComplexPostRequestBinder : IModelBinder {

                                public Task BindModelAsync(ModelBindingContext bindingContext) {
                                    try {
                                        // data
                                        var dataJson = bindingContext.HttpContext.Request.Form[PARAM_DATA];
                                        var dataType = bindingContext.ModelType.GenericTypeArguments[0];
                                        var parsedData = JsonSerializer.Deserialize(dataJson!, dataType, Util.GetJsonSrializerOptions());

                                        // ignoreConfirm
                                        var ignoreConfirm = bindingContext.HttpContext.Request.Form.TryGetValue(PARAM_IGNORE_CONFIRM, out var strIgnoreConfirm)
                                            ? (bool.TryParse(strIgnoreConfirm, out var b) ? b : false)
                                            : false;

                                        // パラメータクラスのインスタンスを作成
                                        var instance = Activator.CreateInstance(bindingContext.ModelType) ?? throw new NullReferenceException();
                                        bindingContext.ModelType.GetProperty(nameof(ComplexPostRequest<object>.Data))!.SetValue(instance, parsedData);
                                        bindingContext.ModelType.GetProperty(nameof(IgnoreConfirm))!.SetValue(instance, ignoreConfirm);

                                        bindingContext.Result = ModelBindingResult.Success(instance);
                                        return Task.CompletedTask;

                                    } catch (Exception) {
                                        bindingContext.Result = ModelBindingResult.Failed();
                                        return Task.CompletedTask;
                                    }
                                }
                            }

                            /// <summary>
                            /// HTTPリクエストの内容をパースして <see cref="ComplexPostRequest"/> クラスのインスタンスを作成します。
                            /// </summary>
                            protected class ComplexPostRequestBinder : IModelBinder {
                                public Task BindModelAsync(ModelBindingContext bindingContext) {

                                    var instance = new ComplexPostRequest();

                                    instance.IgnoreConfirm = bindingContext.HttpContext.Request.Form.TryGetValue(PARAM_IGNORE_CONFIRM, out var strIgnoreConfirm)
                                        ? (bool.TryParse(strIgnoreConfirm, out var b) ? b : false)
                                        : false;

                                    bindingContext.Result = ModelBindingResult.Success(instance);

                                    return Task.CompletedTask;
                                }
                            }

                            /// <summary>
                            /// multipart/form-data 内の入力内容データJSONの項目のキー。
                            /// この名前はReact側の処理と一致させておく必要がある。
                            /// </summary>
                            internal const string PARAM_DATA = "data";
                            /// <summary>
                            /// この名前はReact側の処理と一致させておく必要がある。
                            /// </summary>
                            private const string PARAM_IGNORE_CONFIRM = "ignoreConfirm";
                            #endregion HTTPリクエストとC#クラスの変換
                        }
                    }
                    """,
            };
        }

        private static SourceFile RenderWebApiDotnetExtensions(CodeRenderingContext ctx) {
            return new SourceFile {
                FileName = "DotnetExtensions.cs",
                Contents = $$"""
                    namespace {{ctx.Config.RootNamespace}} {
                        using System;
                        using System.Collections;
                        using System.Collections.Generic;
                        using System.Linq;
                        using System.Text.Json;
                        using Microsoft.AspNetCore.Mvc;

                        public static class DotnetExtensionsInWebApi {
                            public static IActionResult JsonContent<T>(this ControllerBase controller, T obj, int? httpStatusCode = null) {
                                var json = Util.ToJson(obj);
                                var result = controller.Content(json, "application/json");
                                result.StatusCode = httpStatusCode ?? (int?)System.Net.HttpStatusCode.OK;
                                return result;
                            }
                        }
                    }
                    """,
            };
        }

        private static SourceFile RenderSavingUploadedFilesFilter(CodeRenderingContext ctx) {
            return new SourceFile {
                FileName = "SavingUploadedFilesFilter.cs",
                Contents = $$"""
                    using Microsoft.AspNetCore.Mvc;
                    using Microsoft.AspNetCore.Mvc.Filters;

                    namespace {{ctx.Config.RootNamespace}} {
                        /// <summary>
                        /// クライアント側からアップロードされたファイルをサーバー側のストレージに保存します。
                        /// </summary>
                        public class SavingUploadedFilesFilter : IAsyncActionFilter {
                            public SavingUploadedFilesFilter(IFileAttachmentRepository attachmentRepository) {
                                _attachmentRepository = attachmentRepository;
                            }
                            private readonly IFileAttachmentRepository _attachmentRepository;

                            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {

                                // ファイルを添付することができるContent-Typeの場合
                                if (context.HttpContext.Request.HasFormContentType) {

                                    foreach (var file in context.HttpContext.Request.Form.Files) {

                                        // 入力エラーチェック
                                        if (string.IsNullOrWhiteSpace(file.Name)) {
                                            context.Result = new BadRequestObjectResult($"ファイル '{file.FileName}' のName属性が指定されていません。");
                                            return;
                                        }

                                        var errors = new List<string>();
                                        var id = new FileAttachmentId(file.Name);
                                        using var stream = file.OpenReadStream();
                                        await _attachmentRepository.SaveFileAsync(id, file.FileName, stream, errors);

                                        if (errors.Count > 0) {
                                            context.Result = new BadRequestObjectResult(string.Join(Environment.NewLine, errors));
                                            return;
                                        }
                                    }
                                }

                                await next();

                            }
                        }
                    }
                    """,
            };
        }
    }
}
