using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Nijo.Models.DataModelModules;

/// <summary>
/// IsGenericLookupTable が指定されたルート集約に対するコード生成機能。
/// <list type="bullet">
/// <item>カテゴリごとのビューEFCoreエンティティとDbSet</item>
/// <item>汎用参照テーブルユーティリティクラス（○○Util）</item>
/// <item>DbContextへの GetGeneralLookupViewsInfo() 静的メソッド追加</item>
/// </list>
/// </summary>
internal static class GenericLookupTableFeature {

    /// <summary>
    /// IsGenericLookupTable が設定されたルート集約のコードを生成します。
    /// DataModel.GenerateCode から呼ばれます。
    /// </summary>
    internal static void GenerateCode(
        CodeRenderingContext ctx,
        RootAggregate rootAggregate,
        SourceFileByAggregate aggregateFile) {

        // 汎用参照テーブルでなければスキップ
        if (rootAggregate.XElement.Attribute(BasicNodeOptions.IsGenericLookupTable.AttributeName) == null) return;

        var parser = new GenericLookupTableParser(ctx.SchemaParser);
        var categories = parser.GetCategoriesOf(rootAggregate).ToArray();

        if (categories.Length == 0) return;

        // カテゴリごとのビューエンティティを生成
        foreach (var category in categories) {
            var viewEntity = new GenericLookupViewEntity(rootAggregate, category);

            // C#クラス定義 + DbContext partial クラス内のOnModelCreatingメソッド
            aggregateFile.AddCSharpClass(
                GenericLookupViewEntity.RenderClassDeclaring(viewEntity, ctx),
                $"Class_EFCoreEntity_{category.Name}");

            // DbContextへの DbSet 登録
            ctx.Use<DbContextClass>().AddEntities([viewEntity]);
        }

        // ○○Util クラスをアプリケーションサービスに追加
        ctx.Use<ApplicationService>().Add(RenderUtilProperty(rootAggregate, ctx));
        ctx.CoreLibrary(dir => {
            dir.Directory("Util", utilDir => {
                utilDir.Generate(new() {
                    FileName = $"{GetUtilityClassName(rootAggregate, ctx)}.cs",
                    Contents = RenderUtilityClass(categories, rootAggregate, ctx),
                });
            });
        });

        // GetGeneralLookupViewsInfo() 静的メソッド用にデータを蓄積
        ctx.Use<GenericLookupViewsInfoProvider>().AddTable(rootAggregate, categories);
    }

    // -----------------------------------------------------------------
    //  ○○Util クラス レンダリング
    // -----------------------------------------------------------------

    private static string GetUtilityClassName(RootAggregate rootAggregate, CodeRenderingContext ctx)
        => IsLegacyDynamicEnumUtility(rootAggregate, ctx) ? "区分マスタUtil" : $"{rootAggregate.PhysicalName}Util";
    private static string GetUtilityCategoryClassName(RootAggregate rootAggregate, CodeRenderingContext ctx)
        => IsLegacyDynamicEnumUtility(rootAggregate, ctx) ? "DynamicEnumCache" : $"{rootAggregate.PhysicalName}UtilCategory";

    private static string RenderUtilProperty(RootAggregate rootAggregate, CodeRenderingContext ctx) {
        var utilClassName = GetUtilityClassName(rootAggregate, ctx);
        var fieldName = IsLegacyDynamicEnumUtility(rootAggregate, ctx) ? "_区分マスタUtil" : $"_{rootAggregate.PhysicalName}Util";

        if (IsLegacyDynamicEnumUtility(rootAggregate, ctx)) {
            return $$"""
                /*__LEGACY_CTOR__*/
                区分マスタUtil = new(this);
                /*__END_LEGACY_CTOR__*/

                /// <summary>
                /// 区分マスタを参照する処理に関するユーティリティ。
                /// 主な役割は何度もDBへのアクセスが発生するのを防ぐためにメモリ上にキャッシュを持つこと。
                /// </summary>
                public {{utilClassName}} 区分マスタUtil { get; }
                """;
        }

        return $$"""
            #region 汎用参照テーブル '{{rootAggregate.DisplayName}}' ユーティリティ
            /// <summary>
            /// '{{rootAggregate.DisplayName}}' の汎用参照テーブルユーティリティ。
            /// カテゴリごとのハードコード値取得とデータソース取得を提供します。
            /// </summary>
            public {{utilClassName}} {{rootAggregate.PhysicalName}}Util => {{fieldName}} ??= new {{utilClassName}}(this.DbContext);
            private {{utilClassName}}? {{fieldName}};
            #endregion 汎用参照テーブル '{{rootAggregate.DisplayName}}' ユーティリティ
            """;
    }

    private static string RenderUtilityClass(
        IEnumerable<GenericLookupTableParser.GenericLookupTableCategory> categories,
        RootAggregate rootAggregate,
        CodeRenderingContext ctx) {

        var utilClassName = GetUtilityClassName(rootAggregate, ctx);
        var categoryClassName = GetUtilityCategoryClassName(rootAggregate, ctx);

        if (IsLegacyDynamicEnumUtility(rootAggregate, ctx)) {
            return RenderLegacyDynamicEnumUtilityClass(categories, rootAggregate, ctx);
        }

        // ハードコードキーのValueMember解決ヘルパー
        var allMembers = rootAggregate.GetMembers().OfType<ValueMember>().ToArray();
        ValueMember? FindMemberByUniqueId(string uid) =>
            allMembers.FirstOrDefault(m =>
                m.XElement.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)?.Value == uid);

        // 全カテゴリにわたるユニークなハードコードメンバーを収集
        var uniqueHardcodedMembers = categories
            .SelectMany(cat => cat.HardCodedKeys)
            .Select(k => FindMemberByUniqueId(k.UniqueId))
            .OfType<ValueMember>()
            .DistinctBy(m => m.PhysicalName)
            .ToArray();

        return $$"""
            using System.Collections;

            namespace {{ctx.Config.RootNamespace}};

            public sealed class {{utilClassName}} {
                internal {{utilClassName}}({{ctx.Config.DbContextName}} dbContext) {
                    _dbContext = dbContext;
                }
                private readonly {{ctx.Config.DbContextName}} _dbContext;

                /// <summary>{{utilClassName}} に定義されているカテゴリの一覧を返します。</summary>
                public IEnumerable<{{categoryClassName}}> GetAllCategories() {
            {{categories.SelectTextTemplate(cat => $$"""
                    yield return {{cat.DisplayName.ToCSharpSafe()}};
            """)}}
                }
            {{categories.SelectTextTemplate(cat => $$"""

                {{WithIndent(RenderPropertyForCategory(cat))}}
            """)}}
            }

            public class {{categoryClassName}} {
                public required string PhysicalName { get; init; }
                public required string DisplayName { get; init; }
            {{uniqueHardcodedMembers.SelectTextTemplate(m => $$"""
                /// <summary>{{m.DisplayName}} のハードコード値</summary>
                public required {{m.Type.CsPrimitiveTypeName}}? {{m.PhysicalName}} { get; init; }
            """)}}
            }

            public sealed class {{categoryClassName}}<T> : {{categoryClassName}}, IEnumerable<T> {
                internal {{categoryClassName}}(IQueryable<T> getAllQuery) {
                    _getAllQuery = getAllQuery;
                }
                private readonly IQueryable<T> _getAllQuery;
                private List<T>? _cachedList;

                public IEnumerator<T> GetEnumerator() => (_cachedList ??= _getAllQuery.ToList()).GetEnumerator();
                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }
            """;

        string RenderPropertyForCategory(GenericLookupTableParser.GenericLookupTableCategory cat) {
            var categoryIdentifier = cat.DisplayName.ToCSharpSafe();
            var keyMap = cat.HardCodedKeys.ToDictionary(k => k.UniqueId, k => k.Value);
            var propInits = uniqueHardcodedMembers.SelectTextTemplate(m => {
                var uid = m.XElement.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)?.Value;
                var propValue = uid != null && keyMap.TryGetValue(uid, out var v)
                    ? RenderLiteral(v, m.Type.CsPrimitiveTypeName)
                    : "null";
                return $"{m.PhysicalName} = {propValue},";
            });
            return $$"""
                private {{categoryClassName}}<{{rootAggregate.PhysicalName}}_{{categoryIdentifier}}DbEntity>? _{{categoryIdentifier}};
                /// <summary>カテゴリ '{{cat.DisplayName}}' のユーティリティ</summary>
                public {{categoryClassName}}<{{rootAggregate.PhysicalName}}_{{categoryIdentifier}}DbEntity> {{categoryIdentifier}} => _{{categoryIdentifier}} ??= new(_dbContext.{{rootAggregate.PhysicalName}}_{{categoryIdentifier}}DbSet) {
                    PhysicalName = "{{cat.Name}}",
                    DisplayName = "{{cat.DisplayName.Replace("\"", "\\\"")}}",
                    {{propInits}}
                };
                """;
        }
    }

    /// <summary>C#リテラル表現を返します。</summary>
    private static string RenderLiteral(string value, string csTypeName) {
        // int, long等の数値型の場合はそのまま、それ以外は文字列リテラル
        return csTypeName switch {
            "int" or "long" or "short" or "decimal" or "double" or "float" => value,
            _ => $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"",
        };
    }

    private static bool IsLegacyDynamicEnumUtility(RootAggregate rootAggregate, CodeRenderingContext ctx) {
        return ctx.IsLegacyCompatibilityMode()
            && rootAggregate.XElement.Attribute(BasicNodeOptions.IsGenericLookupTable.AttributeName) != null;
    }

    private static string RenderLegacyDynamicEnumUtilityClass(
        IEnumerable<GenericLookupTableParser.GenericLookupTableCategory> categories,
        RootAggregate rootAggregate,
        CodeRenderingContext ctx) {

        var efCoreEntity = new Nijo.Models.WriteModel2Modules.EFCoreEntity(rootAggregate);
        var categoriesArray = categories.ToArray();
        var keyMembers = rootAggregate.GetMembers().OfType<ValueMember>()
            .Where(member => member.IsKey || member.PhysicalName == "値CD")
            .ToArray();

        return $$"""
            using System.Collections;

            namespace {{ctx.Config.RootNamespace}};

            public class 区分マスタUtil {
                public 区分マスタUtil(AutoGeneratedApplicationService app) {
                {{categoriesArray.SelectTextTemplate(category => $$"""
                    {{category.DisplayName.ToCSharpSafe()}} = new("{{category.Name}}", "{{category.DisplayName.Replace("\"", "\\\"")}}", app);
                """)}}
                }

            {{categoriesArray.SelectTextTemplate(category => $$"""
                public DynamicEnumCache {{category.DisplayName.ToCSharpSafe()}} { get; }
            """)}}

                /// <summary>
                /// 区分の種類を列挙します。
                /// </summary>
                public IEnumerable<KeyValuePair<string, string>> EnumerateTypes() {
                {{categoriesArray.SelectTextTemplate(category => $$"""
                    yield return KeyValuePair.Create("{{category.Name}}", "{{category.DisplayName.Replace("\"", "\\\"")}}");
                """)}}
                }
            }

            /// <summary>
            /// 区分マスタの内容をメモリ上にキャッシュしておく仕組み。
            /// </summary>
            public partial class DynamicEnumCache : IEnumerable<{{efCoreEntity.ClassName}}> {

                public DynamicEnumCache(string typeKey, string displayName, AutoGeneratedApplicationService app) {
                    種別 = typeKey;
                    表示名称 = displayName;
                    _app = app;
                }
                /// <summary>
                /// この区分の種別コード
                /// </summary>
                public string 種別 { get; }
                /// <summary>
                /// この区分の種類の名前
                /// </summary>
                public string 表示名称 { get; }

                private readonly AutoGeneratedApplicationService _app;

                /// <summary>
                /// この種類の区分を全てDBから読み込んだキャッシュ。辞書のキーは値CD。
                /// </summary>
                public IReadOnlyDictionary<string, {{efCoreEntity.ClassName}}> Cache {
                    get {
                        return _cache ??= _app.DbContext.{{efCoreEntity.DbSetName}}
                            .Where(x => x.種別 == this.種別)
                            .ToDictionary(x => x.値CD!);
                    }
                }
                private IReadOnlyDictionary<string, {{efCoreEntity.ClassName}}>? _cache;

            {{keyMembers.SelectTextTemplate(member => $$"""
                /// <summary>
                /// この区分に指定の{{member.DisplayName}}が含まれているかどうかを返します。
                /// </summary>
                /// <param name="{{GetLegacyContainsParamName(member)}}">{{member.DisplayName}}</param>
                public bool Contains{{member.PhysicalName}}({{member.Type.CsPrimitiveTypeName}} {{GetLegacyContainsParamName(member)}}) {
                    return Cache.Values.Any(x => x.{{member.PhysicalName}} == {{GetLegacyContainsParamName(member)}});
                }
            """)}}

                public IEnumerator<{{efCoreEntity.ClassName}}> GetEnumerator() {
                    return Cache.Values.GetEnumerator();
                }
                IEnumerator IEnumerable.GetEnumerator() {
                    return GetEnumerator();
                }
            }
            """;
    }

    private static string GetLegacyContainsParamName(ValueMember member) {
        return member.PhysicalName switch {
            "値CD" => "ataiCd",
            _ => "pk",
        };
    }
}
