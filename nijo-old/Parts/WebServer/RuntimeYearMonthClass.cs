using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.WebServer {
    /// <summary>
    /// 自動生成される側のプロジェクトで定義される年月クラス
    /// </summary>
    internal class RuntimeYearMonthClass {
        internal const string CLASS_NAME = "YearMonth";

        internal static string EFCoreConverterClassFullName => $"{CLASS_NAME}.{EFCORE_CONVERTER}";
        private const string EFCORE_CONVERTER = "EFCoreYearMonthConverter";

        internal static SourceFile RenderDeclaring() => new() {
            FileName = "YearMonth.cs",
            RenderContent = ctx => {
                return $$"""
                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// 年月。
                    /// 通常の <see cref="DateTime"/> を使った場合とで何か挙動が変わるといったことはない。
                    /// 通常の <see cref="DateTime"/> を使った場合、日・時・分・秒といった情報がノイズになるので、
                    /// わざわざそれを意識しなくても済むようにするためのクラス。
                    /// </summary>
                    public partial class {{CLASS_NAME}} : IComparable<{{CLASS_NAME}}>, IComparable {
                        public YearMonth(int year, int month) {
                            // 年、月の範囲チェック
                            if (year < 1 || month < 1 || month > 12) {
                                throw new ArgumentOutOfRangeException({{MessageConst.CS_CLASS_NAME}}.{{MessageConst.C_INF0054}}(year.ToString(), month.ToString()));
                            }
                            Year = year;
                            Month = month;
                        }
                        public YearMonth(DateTime dateTime) {
                            Year = dateTime.Year;
                            Month = dateTime.Month;
                        }
                        public YearMonth({{RuntimeDateClass.CLASS_NAME}} date) {
                            Year = date.Year;
                            Month = date.Month;
                        }

                        public int Year { get; }
                        public int Month { get; }

                        public DateTime ToDateTime() {
                            return new DateTime(Year, Month, 1); // 日は1日固定
                        }

                        public bool Contains(DateTime dateTime) {
                            return dateTime.Year == Year && dateTime.Month == Month;
                        }
                        public bool Contains(Date date) {
                            return date.Year == Year && date.Month == Month;
                        }

                        /// <summary>
                        /// 日本の会計年度の開始月である4月を始まりとした年度を返します。
                        /// </summary>
                        public int GetNendo() {
                            return Month >= 1 && Month <= 3
                                ? Year - 1
                                : Year;
                        }

                        public override bool Equals(object? obj) {
                            if (obj is YearMonth other) {
                                return Year == other.Year && Month == other.Month;
                            }
                            return false;
                        }
                        public override int GetHashCode() {
                            return HashCode.Combine(Year, Month);
                        }
                        public override string ToString() {
                            return $"{Year:0000}/{Month:00}";
                        }
                        public string ToString(string format) {
                            return ToDateTime().ToString(format);
                        }

                        [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(ym))]
                        public static explicit operator DateTime?(YearMonth? ym) => ym == null ? null : new DateTime(ym.Year, ym.Month, 1);
                        [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(datetime))]
                        public static explicit operator YearMonth?(DateTime? datetime) => datetime == null ? null : new YearMonth(datetime.Value);

                        public static bool operator ==(YearMonth? left, YearMonth? right) {
                            return Equals(left, right);
                        }
                        public static bool operator !=(YearMonth? left, YearMonth? right) {
                            return !Equals(left, right);
                        }
                        public static bool operator <(YearMonth? left, YearMonth? right) {
                            if (left == null || right == null)
                                return false;
                            if (left.Year != right.Year)
                                return left.Year < right.Year;
                            return left.Month < right.Month;
                        }
                        public static bool operator >(YearMonth? left, YearMonth? right) {
                            if (left == null || right == null)
                                return false;
                            if (left.Year != right.Year)
                                return left.Year > right.Year;
                            return left.Month > right.Month;
                        }
                        public static bool operator <=(YearMonth? left, YearMonth? right) {
                            return left < right || left == right;
                        }
                        public static bool operator >=(YearMonth? left, YearMonth? right) {
                            return left > right || left == right;
                        }

                        #region IComparableインターフェースの実装
                        public int CompareTo(object? obj) {
                            if (obj == null) return 1; // nullは常に小さい
                            if (obj is not {{CLASS_NAME}} other) throw new InvalidOperationException("{{CLASS_NAME}}型とそれ以外の大小を比較できません。");
                            return CompareTo(other);
                        }
                        public int CompareTo({{CLASS_NAME}}? other) {
                            if (other == null) return 1; // nullは常に小さい
                            return ToDateTime().CompareTo(other.ToDateTime()); // DateTimeに変換して比較
                        }
                        #endregion IComparableインターフェースの実装

                        /// <summary>
                        /// Entity Framework Core 用のDBとC#の型変換定義
                        /// </summary>
                        public class {{EFCORE_CONVERTER}} : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<{{CLASS_NAME}}, int> {
                            public {{EFCORE_CONVERTER}}() : base(
                                yearMonth => (yearMonth.Year * 100) + yearMonth.Month,
                                yyyymm => new {{CLASS_NAME}}(yyyymm / 100, yyyymm % 100)) { }
                        }
                    }
                    """;
            },
        };

        internal static UtilityClass.CustomJsonConverter GetCustomJsonConverter(CodeRenderingContext ctx) => new() {
            ConverterClassName = $"{UtilityClass.CUSTOM_CONVERTER_NAMESPACE}.YearMonthJsonValueConverter",
            ConverterClassDeclaring = $$"""
                public class YearMonthJsonValueConverter : JsonConverter<{{CLASS_NAME}}?> {
                    public override {{CLASS_NAME}}? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                        if (reader.TokenType == JsonTokenType.Null) {
                            return null;
                        } else {
                {{If(ctx.Config.UseWijmo, () => $$"""
                            // WijmoのUIコントロールではDate型で扱われるので
                            return DateTime.TryParse(reader.GetString(), out var dateTime)
                                ? new {{CLASS_NAME}}(dateTime)
                                : null;
                """).Else(() => $$"""
                            var yyyymm = reader.GetInt32();
                            var year = yyyymm / 100;
                            var month = yyyymm % 100;
                            return new {{CLASS_NAME}}(year, month);
                """)}}
                        }
                    }

                    public override void Write(Utf8JsonWriter writer, {{CLASS_NAME}}? value, JsonSerializerOptions options) {
                        if (value == null) {
                            writer.WriteNullValue();
                        } else {
                {{If(ctx.Config.UseWijmo, () => $$"""
                            // WijmoのUIコントロールではDate型で扱われるので
                            writer.WriteStringValue(value.ToDateTime().ToString("yyyy/MM"));
                """).Else(() => $$"""
                            writer.WriteNumberValue((value.Year * 100) + value.Month);
                """)}}
                        }
                    }
                }
                """,
        };

        internal static Func<string, string> RenderEFCoreConversion() {
            return modelBuilder => $$"""
                foreach (var entityType in {{modelBuilder}}.Model.GetEntityTypes()) {
                    foreach (var property in entityType.GetProperties()) {
                        if (property.ClrType == typeof({{CLASS_NAME}})) {
                            property.SetValueConverter(new {{CLASS_NAME}}.{{EFCORE_CONVERTER}}()); // {{CLASS_NAME}}型のDBとC#の間の変換処理を定義
                        }
                    }
                }
                """;
        }
    }
}
