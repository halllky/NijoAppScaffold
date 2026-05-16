using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.QueryModelModules;
using Nijo.Parts.CSharp;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.ValueMemberTypes;

/// <summary>
/// 年月型
/// </summary>
internal class YearMonthMember : IValueMemberType {
    string IValueMemberType.TypePhysicalName => "YearMonth";
    string IValueMemberType.SchemaTypeName => "yearmonth";
    string IValueMemberType.CsDomainTypeName => "YearMonth";
    string IValueMemberType.CsPrimitiveTypeName => "int"; // DBには yyyymm の6桁で保存する
    string IValueMemberType.TsTypeName => "string"; // TypeScriptでは yyyy/mm で扱う
    string IValueMemberType.DisplayName => "年月型";

    string IValueMemberType.RenderSpecificationMarkdown() {
        return $$"""
            西暦と月から成る年月（YYYY/MM形式）を格納する型です。
            契約期間、有効期限、統計期間などの年月データに適しています。
            日付は含まれません。
            検索時の挙動は期間検索（開始年月〜終了年月）が可能です。
            """;
    }

    void IValueMemberType.Validate(XElement element, SchemaParseContext context, Action<XElement, string> addError) {
        // 年月型の検証
        // 必要に応じて年月の範囲制約などを検証するコードをここに追加できます
    }

    ValueMemberSearchBehavior? IValueMemberType.SearchBehavior => new() {
        FilterCsTypeName = $"{FromTo.CS_CLASS_NAME}<YearMonth?>",
        FilterTsTypeName = "{ from?: string | null; to?: string | null }",
        RenderTsNewObjectFunctionValue = () => "{ from: '', to: '' }",
        RenderFiltering = ctx => RangeSearchRenderer.RenderRangeSearchFiltering(ctx),
    };

    void IValueMemberType.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
        // 特になし
    }

    string IValueMemberType.RenderCreateDummyDataValueBody(CodeRenderingContext ctx) {
        return $$"""
            var now = DateTime.Now;
            return member.IsKey
                ? new YearMonth(now.Year, Math.Max(1, Math.Min(12, (context.GetNextSequence() % 12) + 1)))
                : new YearMonth(now.Year, now.Month);
            """;
    }

    void IValueMemberType.RenderStaticSources(CodeRenderingContext ctx) {

        if (ctx.IsLegacyCompatibilityMode()) {
            var requiresLegacyYearMonthSource = ctx.Schema.GetRootAggregates().Any(rootAggregate => {
                var requiresLegacySource = rootAggregate.Model is Models.DataModel || rootAggregate.GenerateDefaultQueryModel;
                if (!requiresLegacySource) return false;

                return rootAggregate
                    .EnumerateThisAndDescendants()
                    .SelectMany(aggregate => aggregate.GetMembers())
                    .OfType<ValueMember>()
                    .Any(member => member.Type is YearMonthMember);
            });
            if (!requiresLegacyYearMonthSource) return;

            ctx.CoreLibrary(dir => {
                dir.Directory("Util", utilDir => {
                    utilDir.Generate(new SourceFile {
                        FileName = "YearMonth.cs",
                        Contents = $$"""
                            namespace {{ctx.Config.RootNamespace}};

                            /// <summary>
                            /// 年月。
                            /// 通常の <see cref="DateTime"/> を使った場合とで何か挙動が変わるといったことはない。
                            /// 通常の <see cref="DateTime"/> を使った場合、日・時・分・秒といった情報がノイズになるので、
                            /// わざわざそれを意識しなくても済むようにするためのクラス。
                            /// </summary>
                            public partial class YearMonth : IComparable<YearMonth>, IComparable {
                                public YearMonth(int year, int month) {
                                    // 年、月の範囲チェック
                                    if (year < 1 || month < 1 || month > 12) {
                                        throw new ArgumentOutOfRangeException(MSG.ERRC0044(year.ToString(), month.ToString()));
                                    }
                                    Year = year;
                                    Month = month;
                                }
                                public YearMonth(DateTime dateTime) {
                                    Year = dateTime.Year;
                                    Month = dateTime.Month;
                                }
                                public YearMonth(Date date) {
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
                                    if (obj is not YearMonth other) throw new InvalidOperationException("YearMonth型とそれ以外の大小を比較できません。");
                                    return CompareTo(other);
                                }
                                public int CompareTo(YearMonth? other) {
                                    if (other == null) return 1; // nullは常に小さい
                                    return ToDateTime().CompareTo(other.ToDateTime()); // DateTimeに変換して比較
                                }
                                #endregion IComparableインターフェースの実装

                                /// <summary>
                                /// Entity Framework Core 用のDBとC#の型変換定義
                                /// </summary>
                                public class EFCoreYearMonthConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<YearMonth, int> {
                                    public EFCoreYearMonthConverter() : base(
                                        yearMonth => (yearMonth.Year * 100) + yearMonth.Month,
                                        yyyymm => new YearMonth(yyyymm / 100, yyyymm % 100)) { }
                                }
                            }
                            """,
                    });
                });
            });

        } else {
            ctx.CoreLibrary(dir => {
                dir.Generate(new SourceFile {
                    FileName = "YearMonth.cs",
                    Contents = $$"""
                using System;
                using System.Globalization;
                using System.Text.Json;
                using System.Text.Json.Serialization;

                namespace {{ctx.Config.RootNamespace}};

                /// <summary>
                /// 年月を表す値オブジェクト
                /// </summary>
                public readonly struct YearMonth : IComparable<YearMonth>, IEquatable<YearMonth> {
                    /// <summary>
                    /// 数値表現（YYYYMM形式の6桁整数）
                    /// </summary>
                    private readonly int _value;

                    /// <summary>
                    /// 数値表現（YYYYMM形式の6桁整数）
                    /// </summary>
                    public int Value => _value;

                    /// <summary>
                    /// 年
                    /// </summary>
                    public int Year => _value / 100;

                    /// <summary>
                    /// 月（1〜12）
                    /// </summary>
                    public int Month => _value % 100;

                    /// <summary>
                    /// 指定した年月から値オブジェクトを生成します
                    /// </summary>
                    /// <param name="year">年（4桁）</param>
                    /// <param name="month">月（1〜12）</param>
                    public YearMonth(int year, int month) {
                        if (year < 1 || year > 9999) throw new ArgumentOutOfRangeException(nameof(year), "年は1〜9999の範囲で指定してください");
                        if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month), "月は1〜12の範囲で指定してください");
                        _value = year * 100 + month;
                    }

                    /// <summary>
                    /// YYYYMMの数値表現から値オブジェクトを生成します
                    /// </summary>
                    /// <param name="value">YYYYMMの6桁整数</param>
                    public YearMonth(int value) {
                        int year = value / 100;
                        int month = value % 100;

                        if (year < 1 || year > 9999) throw new ArgumentOutOfRangeException(nameof(value), "年は1〜9999の範囲である必要があります");
                        if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(value), "月は1〜12の範囲である必要があります");

                        _value = value;
                    }

                    /// <summary>
                    /// DateTimeから値オブジェクトを生成します
                    /// </summary>
                    public static YearMonth FromDateTime(DateTime dateTime) {
                        return new YearMonth(dateTime.Year, dateTime.Month);
                    }

                    /// <summary>
                    /// DateOnlyから値オブジェクトを生成します
                    /// </summary>
                    public static YearMonth FromDateOnly(DateOnly dateOnly) {
                        return new YearMonth(dateOnly.Year, dateOnly.Month);
                    }

                    /// <summary>
                    /// 年月をDateTime型に変換します（日付は1日として扱います）
                    /// </summary>
                    public DateTime ToDateTime() {
                        return new DateTime(Year, Month, 1);
                    }

                    /// <summary>
                    /// 現在の年月を取得します
                    /// </summary>
                    public static YearMonth Now => FromDateTime(DateTime.Now);

                    /// <summary>
                    /// UTC現在の年月を取得します
                    /// </summary>
                    public static YearMonth UtcNow => FromDateTime(DateTime.UtcNow);

                    /// <summary>
                    /// 年月をYYYY/MM形式の文字列に変換します
                    /// </summary>
                    public override string ToString() {
                        return $"{Year:0000}/{Month:00}";
                    }

                    /// <summary>
                    /// 2つの年月を比較します
                    /// </summary>
                    public int CompareTo(YearMonth other) {
                        return _value.CompareTo(other._value);
                    }

                    /// <summary>
                    /// 2つの年月が等しいかどうかを判定します
                    /// </summary>
                    public bool Equals(YearMonth other) {
                        return _value == other._value;
                    }

                    /// <summary>
                    /// オブジェクトが等しいかどうかを判定します
                    /// </summary>
                    public override bool Equals(object? obj) {
                        return obj is YearMonth yearMonth && Equals(yearMonth);
                    }

                    /// <summary>
                    /// ハッシュコードを取得します
                    /// </summary>
                    public override int GetHashCode() {
                        return _value.GetHashCode();
                    }

                    /// <summary>
                    /// 等価演算子
                    /// </summary>
                    public static bool operator ==(YearMonth left, YearMonth right) {
                        return left.Equals(right);
                    }

                    /// <summary>
                    /// 非等価演算子
                    /// </summary>
                    public static bool operator !=(YearMonth left, YearMonth right) {
                        return !left.Equals(right);
                    }

                    /// <summary>
                    /// 比較演算子（より大きい）
                    /// </summary>
                    public static bool operator >(YearMonth left, YearMonth right) {
                        return left.CompareTo(right) > 0;
                    }

                    /// <summary>
                    /// 比較演算子（より小さい）
                    /// </summary>
                    public static bool operator <(YearMonth left, YearMonth right) {
                        return left.CompareTo(right) < 0;
                    }

                    /// <summary>
                    /// 比較演算子（以上）
                    /// </summary>
                    public static bool operator >=(YearMonth left, YearMonth right) {
                        return left.CompareTo(right) >= 0;
                    }

                    /// <summary>
                    /// 比較演算子（以下）
                    /// </summary>
                    public static bool operator <=(YearMonth left, YearMonth right) {
                        return left.CompareTo(right) <= 0;
                    }

                    /// <summary>
                    /// 明示的な型変換演算子（int -> YearMonth）
                    /// </summary>
                    public static explicit operator YearMonth(int value) {
                        return new YearMonth(value);
                    }

                    /// <summary>
                    /// 明示的な型変換演算子（YearMonth -> int）
                    /// </summary>
                    public static explicit operator int(YearMonth yearMonth) {
                        return yearMonth._value;
                    }
                }
                """,
                });
            });
        }
    }
}
