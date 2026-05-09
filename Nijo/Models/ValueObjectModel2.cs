using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.Models {
    /// <summary>
    /// 旧版互換の値オブジェクト。
    /// </summary>
    internal class ValueObjectModel2 : IModel {
        internal const string SCHEMA_NAME = "value-object-2";

        public string SchemaName => SCHEMA_NAME;

        public void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            // 特に検証ロジックなし
        }

        public void GenerateCode(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            ctx.CoreLibrary(dir => {
                dir.Directory("Util", utilDir => {
                    utilDir.Generate(RenderCSharp(rootAggregate, ctx));
                });
            });
        }

        private static SourceFile RenderCSharp(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            var className = rootAggregate.PhysicalName;

            return new SourceFile {
                FileName = $"{className.ToFileNameSafe()}.cs",
                Contents = $$"""
                    using Microsoft.EntityFrameworkCore;
                    using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
                    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
                    using System.Linq.Expressions;
                    using System.Diagnostics.CodeAnalysis;
                    using System.Text.Json;
                    using System.Text.Json.Serialization;

                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// {{rootAggregate.DisplayName}}。
                    /// partial宣言されているため、このクラスに独自の処理を追加する場合は別ファイルで定義してください。
                    /// </summary>
                    public sealed partial class {{className}} {
                        public {{className}}(string value) {
                            _value = value;
                        }

                        private readonly string _value;

                        public override bool Equals(object? obj) {
                            if (obj is not {{className}} other) return false;
                            if (other._value != _value) return false;
                            return true;
                        }
                        public override int GetHashCode() {
                            return _value.GetHashCode();
                        }
                        public override string ToString() {
                            return _value;
                        }


                        public static bool operator ==({{className}}? left, {{className}}? right) {
                            if (left is null ^ right is null) return false;
                            return ReferenceEquals(left, right) || left!.Equals(right);
                        }
                        public static bool operator !=({{className}}? left, {{className}}? right) {
                            return !(left == right);
                        }
                        [return: NotNullIfNotNull(nameof(vo))]
                        public static explicit operator string?({{className}}? vo) => vo?._value;
                        [return: NotNullIfNotNull(nameof(value))]
                        public static explicit operator {{className}}?(string? value) => value == null ? null : new {{className}}(value);


                        /// <summary>
                        /// Entity Frameword Core 関連の処理で使用される、
                        /// <see cref="{{className}}"/> 型のプロパティと、DBのカラムの型変換。
                        /// </summary>
                        public class EFCoreValueConverter : ValueConverter<{{className}}, string> {
                            public EFCoreValueConverter() : base(
                                csValue => csValue._value,
                                dbValue => new {{className}}(dbValue),
                                new ConverterMappingHints(size: 255)) { }
                        }
                        /// <summary>
                        /// HTTPリクエスト・レスポンスの処理で使用される、
                        /// <see cref="{{className}}"/> 型のプロパティと、JSONプロパティの型変換。
                        /// </summary>
                        public class JsonValueConverter : JsonConverter<{{className}}> {
                            public override {{className}}? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                                var clrValue = reader.GetString();
                                return string.IsNullOrWhiteSpace(clrValue)
                                    ? null
                                    : new {{className}}(clrValue);
                            }
                            public override void Write(Utf8JsonWriter writer, {{className}}? value, JsonSerializerOptions options) {
                                if (value == null) {
                                    writer.WriteNullValue();
                                } else {
                                    writer.WriteStringValue(value.ToString());
                                }
                            }
                        }
                    }
                    """,
            };
        }

        public void GenerateCode(CodeRenderingContext ctx) {
            // 特になし
        }
    }
}
