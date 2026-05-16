using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.ReadModel2Modules;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.Models {
    /// <summary>
    /// 旧版互換の read-model-2。
    /// 実処理は ReadModel2Modules 配下へ段階的に移植する。
    /// </summary>
    internal class ReadModel2 : IModel {
        internal const string SCHEMA_NAME = "read-model-2";

        public string SchemaName => SCHEMA_NAME;

        public void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            foreach (var aggregate in rootAggregateElement
                .DescendantsAndSelf()
                .Where(el => el.Parent?.Parent == el.Document?.Root
                          || el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILDREN)) {
                var hasKey = aggregate.Elements().Any(member => member.Attribute(BasicNodeOptions.IsKey.AttributeName) != null);
                if (!hasKey) {
                    addError(aggregate, $"{aggregate.GetDisplayName()}にキーが1つもありません。");
                }
            }
        }

        public void GenerateCode(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            if (!ctx.IsLegacyCompatibilityMode()) throw new InvalidOperationException("旧版互換モードでのみ利用可能");

            ctx.Use<Parts.CSharp.MessageContainer.BaseClass>();

            var aggregateFile = new SourceFileByAggregate(rootAggregate);
            var rootDisplayData = new DisplayData(rootAggregate);

            var searchCondition = new SearchCondition.Entry(rootAggregate);
            aggregateFile.AddCSharpClass(SearchCondition.Entry.RenderCSharpRecursively(rootAggregate, ctx), "Class_SearchCondition");
            aggregateFile.AddTypeScriptTypeDef(SearchCondition.Entry.RenderTypeScriptRecursively(rootAggregate, ctx));
            aggregateFile.AddTypeScriptFunction(searchCondition.RenderNewObjectFunction());
            aggregateFile.AddTypeScriptFunction(searchCondition.RenderParseQueryParameterFunction());
            aggregateFile.AddTypeScriptTypeDef(searchCondition.RenderTypeScriptSortableMemberType());

            foreach (var aggregate in rootAggregate.EnumerateThisAndDescendants()) {
                var searchResult = new SearchResult(aggregate);
                aggregateFile.AddCSharpClass(searchResult.RenderCSharpDeclaring(ctx), "Class_SearchResult");

                var displayData = new DisplayData(aggregate);
                aggregateFile.AddCSharpClass(displayData.RenderCSharpDeclaring(ctx), "Class_DisplayData");
                aggregateFile.AddTypeScriptTypeDef(ctx.IsLegacyCompatibilityMode() ? displayData.RenderLegacyTypeScriptType() : displayData.RenderTypeScriptType(ctx));
                aggregateFile.AddTypeScriptFunction(displayData.RenderTsNewObjectFunction(ctx));
            }

            var singleView = new SingleView(rootAggregate);
            var loadMethod = new LoadMethod(rootAggregate);
            aggregateFile.AddTypeScriptFunction(loadMethod.RenderReactHook(ctx));
            aggregateFile.AddWebapiControllerAction(loadMethod.RenderControllerAction(ctx));
            aggregateFile.AddAppSrvMethod($$"""
                {{loadMethod.RenderAppSrvBaseMethod(ctx).TrimEnd()}}
                {{loadMethod.RenderAppSrvAbstractMethod(ctx).TrimEnd()}}
                {{rootDisplayData.RenderSetKeysReadOnly(ctx).TrimEnd()}}
                {{singleView.RenderSetSingleViewDisplayDataFn(ctx).TrimEnd()}}
                """);

            if (rootAggregate.GenerateBatchUpdateCommand) {
                aggregateFile.AddTypeScriptFunction(new BatchUpdateReadModel().RenderFunction(ctx, rootAggregate));
                aggregateFile.AddWebapiControllerAction(BatchUpdateReadModel.RenderControllerActionVersion2(ctx, rootAggregate));
                aggregateFile.AddAppSrvMethod(BatchUpdateReadModel.RenderAppSrvMethodVersion2(ctx, rootAggregate));
            }

            aggregateFile.AddTypeScriptFunction(rootDisplayData.RenderDeepEqualFunctionRecursively(ctx));
            aggregateFile.AddTypeScriptFunction(rootDisplayData.RenderCheckChangesFunction(ctx));

            ctx.Use<DisplayDataTypeList>().Add(rootDisplayData);
            ctx.Use<UiConstraintTypes>().Add(rootDisplayData);

            var multiView = new MultiView(rootAggregate);
            aggregateFile.AddTypeScriptFunction(multiView.RenderNavigationHook(ctx));
            aggregateFile.AddTypeScriptFunction(multiView.RenderExcelDownloadHook());
            if (ctx.IsLegacyCompatibilityMode()) {
                ctx.Use<Parts.CSharp.ApplicationService>().Add(multiView.RenderAppSrvGetUrlMethod());
            } else {
                aggregateFile.AddAppSrvMethod(multiView.RenderAppSrvGetUrlMethod());
            }

            aggregateFile.AddTypeScriptTypeDef(rootDisplayData.RenderUiConstraintType(ctx));
            aggregateFile.AddTypeScriptFunction(rootDisplayData.RenderUiConstraintValue(ctx));

            aggregateFile.AddTypeScriptFunction(singleView.RenderPageFrameComponent(ctx));
            aggregateFile.AddWebapiControllerAction(singleView.RenderSetSingleViewDisplayData(ctx));
            aggregateFile.AddTypeScriptFunction(singleView.RenderNavigateFn(ctx, SingleView.E_Type.New));
            aggregateFile.AddTypeScriptFunction(singleView.RenderNavigateFn(ctx, SingleView.E_Type.Edit));
            if (ctx.IsLegacyCompatibilityMode()) {
                ctx.Use<Parts.CSharp.ApplicationService>().Add(singleView.RenderAppSrvGetUrlMethod());
            } else {
                aggregateFile.AddAppSrvMethod(singleView.RenderAppSrvGetUrlMethod());
            }

            var aggregates = rootAggregate.EnumerateThisAndDescendants().ToArray();

            foreach (var aggregate in aggregates) {
                var refEntry = (AggregateBase)aggregate.GetEntry();
                var refSearchCondition = new RefSearchCondition(aggregate, refEntry);
                var refSearchResult = new RefSearchResult(aggregate, refEntry);
                var refDisplayData = new RefDisplayData(aggregate, refEntry);

                if (aggregate == refEntry) {
                    aggregateFile.AddCSharpClass(refSearchCondition.RenderCSharpDeclaringRecursively(ctx), "Class_RefSearchCondition");
                }
                if (!ctx.IsLegacyCompatibilityMode()) {
                    aggregateFile.AddCSharpClass(refSearchResult.RenderCSharp(ctx), $"Class_RefSearchResult_{refSearchResult.CsClassName}");
                    aggregateFile.AddCSharpClass(refDisplayData.RenderCSharp(ctx), $"Class_RefDisplayData_{refDisplayData.CsClassName}");
                }
                if (aggregate == refEntry) {
                    aggregateFile.AddTypeScriptTypeDef(refSearchCondition.RenderTypeScriptDeclaringRecursively(ctx));
                    aggregateFile.AddTypeScriptFunction(refSearchCondition.RenderCreateNewObjectFn(ctx));
                }
                aggregateFile.AddTypeScriptTypeDef(refDisplayData.RenderTypeScript(ctx));
                aggregateFile.AddTypeScriptFunction(refDisplayData.RenderTsNewObjectFunction(ctx));

                if (aggregate != refEntry) continue;

                var refSearchMethod = new RefSearchMethod(aggregate, refEntry);
                aggregateFile.AddTypeScriptFunction(refSearchMethod.RenderHook(ctx));
                aggregateFile.AddWebapiControllerAction(refSearchMethod.RenderController(ctx));
                aggregateFile.AddAppSrvMethod(refSearchMethod.RenderAppSrvMethodOfReadModel(ctx));
            }

            if (ctx.IsLegacyCompatibilityMode()) {
                aggregateFile.AddWebapiControllerAction(loadMethod.RenderLegacyExcelControllerAction());
            }

            if (ctx.IsLegacyCompatibilityMode()) {
                aggregateFile.AddCSharpClass(aggregates.SelectTextTemplate(aggregate => {
                    var refEntry = (AggregateBase)aggregate.GetEntry();
                    var refSearchResult = new RefSearchResult(aggregate, refEntry);
                    return refSearchResult.RenderCSharp(ctx);
                }), $"Class_RefSearchResults_{rootAggregate.PhysicalName}");

                aggregateFile.AddCSharpClass(aggregates.SelectTextTemplate(aggregate => {
                    var refEntry = (AggregateBase)aggregate.GetEntry();
                    var refDisplayData = new RefDisplayData(aggregate, refEntry);
                    return refDisplayData.RenderCSharp(ctx);
                }), $"Class_RefDisplayData_{rootAggregate.PhysicalName}");
            }

            ctx.Use<AuthorizedAction>().Register(rootAggregate);

            aggregateFile.ExecuteRendering(ctx);
        }

        public void GenerateCode(CodeRenderingContext ctx) {
            var applicationService = ctx.Use<Parts.CSharp.ApplicationService>();
            ctx.Use<Parts.CSharp.LegacyDbContextClass>();
            ctx.Use<Parts.CSharp.LegacyDefaultConfiguration>().AddValueObject("Date");
            ctx.Use<EnumFile2>().AddSourceCode(SingleView.RenderSingleViewNavigationEnums());

            ctx.CoreLibrary(dir => {
                dir.Directory("Util", utilDir => {
                    utilDir.Generate(new SourceFile {
                        FileName = "Date.cs",
                        Contents = $$"""
                            namespace {{ctx.Config.RootNamespace}};

                            /// <summary>
                            /// <see cref="DateTime"/> から時分秒を除いた日付型。
                            /// 通常の <see cref="DateTime"/> を使った場合とで何か挙動が変わるといったことはない。
                            /// 例えば範囲検索で時分秒を除外し忘れるなどといった実装ミスを減らすことを目的としている。
                            /// </summary>
                            public partial class Date : IComparable<Date>, IComparable {
                                public Date(int year, int month, int day) {
                                    // 年、月、日の範囲チェック
                                    if (year < 1 || month < 1 || month > 12 || day < 1 || day > DateTime.DaysInMonth(year, month)) {
                                        throw new ArgumentOutOfRangeException(MSG.ERRC0043(year.ToString(),month.ToString(),day.ToString()));
                                    }
                                    Year = year;
                                    Month = month;
                                    Day = day;
                                }
                                public Date(DateTime dateTime) {
                                    Year = dateTime.Year;
                                    Month = dateTime.Month;
                                    Day = dateTime.Day;
                                }

                                public int Year { get; }
                                public int Month { get; }
                                public int Day { get; }

                                public DateTime ToDateTime() {
                                    return new DateTime(Year, Month, Day);
                                }

                                public override bool Equals(object? obj) {
                                    if (obj is Date other) {
                                        return Year == other.Year && Month == other.Month && Day == other.Day;
                                    }
                                    return false;
                                }
                                public override int GetHashCode() {
                                    return HashCode.Combine(Year, Month, Day);
                                }
                                public override string ToString() {
                                    return $"{Year:0000}/{Month:00}/{Day:00}";
                                }
                                public string ToString(string format) {
                                    return ToDateTime().ToString(format);
                                }

                                [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(date))]
                                public static explicit operator DateTime?(Date? date) => date == null ? null : new DateTime(date.Year, date.Month, date.Day);
                                [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(datetime))]
                                public static explicit operator Date?(DateTime? datetime) => datetime == null ? null : new Date(datetime.Value);

                                public static bool operator ==(Date? left, Date? right) {
                                    return Equals(left, right);
                                }
                                public static bool operator !=(Date? left, Date? right) {
                                    return !Equals(left, right);
                                }
                                public static bool operator <(Date? left, Date? right) {
                                    if (left == null || right == null)
                                        return false;
                                    if (left.Year != right.Year)
                                        return left.Year < right.Year;
                                    if (left.Month != right.Month)
                                        return left.Month < right.Month;
                                    return left.Day < right.Day;
                                }
                                public static bool operator >(Date? left, Date? right) {
                                    if (left == null || right == null)
                                        return false;
                                    if (left.Year != right.Year)
                                        return left.Year > right.Year;
                                    if (left.Month != right.Month)
                                        return left.Month > right.Month;
                                    return left.Day > right.Day;
                                }
                                public static bool operator <=(Date? left, Date? right) {
                                    return left < right || left == right;
                                }
                                public static bool operator >=(Date? left, Date? right) {
                                    return left > right || left == right;
                                }

                                #region IComparableインターフェースの実装
                                public int CompareTo(object? obj) {
                                    if (obj == null) return 1; // nullは常に小さい
                                    if (obj is not Date other) throw new InvalidOperationException("Date型とそれ以外の大小を比較できません。");
                                    return CompareTo(other);
                                }
                                public int CompareTo(Date? other) {
                                    if (other == null) return 1; // nullは常に小さい
                                    return ToDateTime().CompareTo(other.ToDateTime()); // DateTimeに変換して比較
                                }
                                #endregion IComparableインターフェースの実装

                                /// <summary>
                                /// Entity Framework Core 用のDBとC#の型変換定義
                                /// </summary>
                                public class EFCoreDateConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Date, DateTime> {
                                    public EFCoreDateConverter() : base(
                                        date => date.ToDateTime(),
                                        datetime => new Date(datetime)) { }
                                }
                            }
                            """,
                    });
                    utilDir.Generate(new SourceFile {
                        FileName = "ICommandResult.cs",
                        Contents = $$"""
                            namespace {{ctx.Config.RootNamespace}};

                            /// <summary>
                            /// 未使用
                            /// </summary>
                            public interface ICommandResult {
                            }
                            """,
                    });
                    utilDir.Generate(DisplayData.RenderBaseClass());
                    utilDir.Generate(ISaveCommandConvertible.Render());
                });
            });
            ctx.ReactProject(dir => {
                dir.Directory("util", utilDir => {
                });
            });

            ctx.Use<AuthorizedAction>();
            ctx.Use<DisplayDataTypeList>();
            ctx.Use<UiConstraintTypes>();

            applicationService.Add(RenderLegacyAuthorizedLevelMethod());
            applicationService.Add(RenderLegacyExcelAppServiceMethods(ctx));
        }

        private static string RenderLegacyAuthorizedLevelMethod() {
            return $$"""
                /// <summary>
                /// 権限レベル取得メソッド
                /// ※詳細な処理はoverrideして実装する
                /// </summary>
                public virtual E_AuthLevel GetAuthorizedLevel({{AuthorizedAction.ENUM_Name}} auth) {
                    return E_AuthLevel.Write;
                }
                """;
        }

        private static string RenderLegacyExcelAppServiceMethods(CodeRenderingContext ctx) {
            var roots = ctx.Schema.GetRootAggregates()
                .Where(root => root.Model is ReadModel2)
                .ToArray();

            return $$"""
                #region 一覧検索結果Excel出力
                {{roots.SelectTextTemplate(RenderAggregate)}}
                #endregion 一覧検索結果Excel出力
                """;

            static string RenderAggregate(RootAggregate rootAggregate) {
                var searchCondition = new SearchCondition.Entry(rootAggregate);
                var displayData = new DisplayData(rootAggregate);
                var loadMethodName = $"Load{rootAggregate.PhysicalName}";
                var columns = EnumerateColumns(rootAggregate, "x", useValuesContainer: true).ToArray();

                return $$"""
                    /// <summary>
                    /// {{rootAggregate.DisplayName}}の一覧検索を行ない、その結果をレンダリングしたExcelブックオブジェクトを返します。
                    /// </summary>
                    /// <param name="searchCondition">一覧検索条件</param>
                    public virtual ExcelBook CreateSearchResultExcelBook({{searchCondition.CsClassName}} searchCondition, IPresentationContext context) {
                        // TODO KR-0029: ExcelBookクラスの作成によって以下の処理が変更必要な場合は適宜変えてください。
                        //               変える必要がなければこのコメントを削除してください。
                        var book = new ExcelBook();

                        // シートにどう出力するかを定義する
                        var sheet = book.AddSheet<{{displayData.CsClassName}}>("Sheet1");
                    {{columns.SelectTextTemplate(column => $$"""
                        sheet.AddColumn(x => {{column.Accessor}}, ["{{column.OwnerDisplayName.Replace("\"", "\\\"")}}", "{{column.MemberDisplayName.Replace("\"", "\\\"")}}"]);
                    """)}}

                        // 通常の一覧検索と同じ処理を流用して検索
                        var searchResult = {{loadMethodName}}(searchCondition, context);

                        // 検索結果をExcelシートにレンダリング
                        sheet.RenderRows(searchResult);

                        return book;
                    }
                    """;
            }

            static IEnumerable<(string Accessor, string OwnerDisplayName, string MemberDisplayName)> EnumerateColumns(AggregateBase aggregate, string accessor, bool useValuesContainer) {
                foreach (var member in aggregate.GetMembers()) {
                    switch (member) {
                        case ValueMember valueMember when !valueMember.OnlySearchCondition:
                            yield return ($"{accessor}{(useValuesContainer ? ".Values?" : string.Empty)}.{valueMember.PhysicalName}", aggregate.DisplayName, valueMember.DisplayName);
                            break;
                        case RefToMember refToMember:
                            foreach (var column in EnumerateColumns(refToMember.RefTo, $"{accessor}{(useValuesContainer ? ".Values?" : string.Empty)}.{refToMember.PhysicalName}?", useValuesContainer: false)) {
                                yield return column;
                            }
                            break;
                        case ChildAggregate childAggregate:
                            foreach (var column in EnumerateColumns(childAggregate, $"{accessor}.{childAggregate.PhysicalName}?", useValuesContainer: true)) {
                                yield return column;
                            }
                            break;
                    }
                }
            }
        }
    }
}
