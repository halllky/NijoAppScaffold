using Nijo.Parts.CSharp;
using System.Collections.Generic;
using System.Threading;
using Nijo.DataStructures.IsStored;
using Nijo.CodeGenerating;
using Nijo.DataStructures.Members;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using System.Linq;
using System.Xml.Linq;
using System;
using Nijo.DataStructures.Aggregates;

namespace Nijo.DataStructures.IsGenericLookupTable;

/// <summary>
/// 永続化されるデータ構造が汎用参照テーブル（システムの運用中に値が追加削除されうる区分値を格納するテーブル）であることを表す属性
/// </summary>
internal sealed class IsGenericLookupTableOption : FeatureOption {
    internal const string ATTRIBUTE_NAME = "IsGenericLookupTable";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "汎用テーブル";
    public override E_NodeOptionType Type => E_NodeOptionType.Boolean;
    public override string HelpText => $$"""
        俗に汎用マスタ、区分マスタ、コードマスタなどと呼ばれるテーブル。
        国コード、通貨単位、分析集計表の分類など、システムの運用中に値の追加削除が発生しうる区分が格納されます。
        構成と指定は、複数の主キーから構成され、その主キーのうち一部がソースコード上にハードコードされ、
        残りの主キーが動的に登録されていきます。
        通常の列挙体（区分値がシステム運用中に変わらないもの）はこれではなくEnumで定義してください。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.IsStored && target.NodeType == E_NodeType.RootAggregate;
    }

    public override void ValidateRootAggregate(XElement rootAggregateElement, SchemaParseContext context, SchemaValidationErrors errors) {
        // * 主キーのうち1個以上はハードコードされる主キーでなければならない
        // * 主キーのうち1個以上はハードコードされない主キーでなければならない
        var keyMembers = rootAggregateElement.Elements()
            .Where(member => member.Attribute(IsKeyOption.ATTRIBUTE_NAME) != null)
            .ToList();
        var hasHardCodedKey = keyMembers.Any(m => m.Attribute(IsHardCodedPrimaryKeyOption.ATTRIBUTE_NAME) != null);
        var hasNonHardCodedKey = keyMembers.Any(m => m.Attribute(IsHardCodedPrimaryKeyOption.ATTRIBUTE_NAME) == null);

        if (!hasHardCodedKey) {
            errors.Add(rootAggregateElement, "汎用参照テーブルの主キーのうち、少なくとも1個はハードコードされる主キーでなければなりません。");
        }
        if (!hasNonHardCodedKey) {
            errors.Add(rootAggregateElement, "汎用参照テーブルの主キーのうち、少なくとも1個はハードコードされない主キーでなければなりません。");
        }
    }

    /// <summary>
    /// 汎用参照テーブルのルート集約1個に対し、以下を生成します。
    /// <list type="bullet">
    /// <item>カテゴリごとのビューEFCoreエンティティとDbSet</item>
    /// <item>汎用参照テーブルユーティリティクラス（○○Util）</item>
    /// <item>DbContextへの GetGeneralLookupViewsInfo() 静的メソッド追加</item>
    /// </list>
    /// </summary>
    public override void Render(CodeRenderingContext ctx, RootAggregate rootAggregate, SourceFileBySectionRoot aggregateFile) {
        var parser = new GenericLookupTableParser(ctx.Schema);
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

        // ○○Util クラスをDbContextに追加
        ctx.Use<DbContextClass>().AddAdditionalMethod(RenderUtilProperty(rootAggregate));
        ctx.CoreLibrary(dir => {
            dir.Directory("Util", utilDir => {
                utilDir.Generate(new() {
                    FileName = $"{GetUtilityClassName(rootAggregate)}.cs",
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

    private static string GetUtilityClassName(RootAggregate rootAggregate) => $"{rootAggregate.PhysicalName}Util";
    private static string GetUtilityCategoryClassName(RootAggregate rootAggregate) => $"{rootAggregate.PhysicalName}UtilCategory";

    private static string RenderUtilProperty(RootAggregate rootAggregate) {
        var utilClassName = GetUtilityClassName(rootAggregate);
        var fieldName = $"_{rootAggregate.PhysicalName}Util";

        return $$"""
            #region 汎用参照テーブル '{{rootAggregate.DisplayName}}' ユーティリティ
            /// <summary>
            /// '{{rootAggregate.DisplayName}}' の汎用参照テーブルユーティリティ。
            /// カテゴリごとのハードコード値取得とデータソース取得を提供します。
            /// </summary>
            public {{utilClassName}} {{rootAggregate.PhysicalName}}Util => {{fieldName}} ??= new {{utilClassName}}(this);
            private {{utilClassName}}? {{fieldName}};
            #endregion 汎用参照テーブル '{{rootAggregate.DisplayName}}' ユーティリティ
            """;
    }

    private static string RenderUtilityClass(
        IEnumerable<GenericLookupTableParser.GenericLookupTableCategory> categories,
        RootAggregate rootAggregate,
        CodeRenderingContext ctx) {

        var utilClassName = GetUtilityClassName(rootAggregate);
        var categoryClassName = GetUtilityCategoryClassName(rootAggregate);

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
                    yield return {{cat.Name}};
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
            var keyMap = cat.HardCodedKeys.ToDictionary(k => k.UniqueId, k => k.Value);
            var propInits = uniqueHardcodedMembers.SelectTextTemplate(m => {
                var uid = m.XElement.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)?.Value;
                var propValue = uid != null && keyMap.TryGetValue(uid, out var v)
                    ? RenderLiteral(v, m.Type.CsPrimitiveTypeName)
                    : "null";
                return $"{m.PhysicalName} = {propValue},";
            });
            return $$"""
                private {{categoryClassName}}<{{rootAggregate.PhysicalName}}_{{cat.Name}}DbEntity>? _{{cat.Name}};
                /// <summary>カテゴリ '{{cat.DisplayName}}' のユーティリティ</summary>
                public {{categoryClassName}}<{{rootAggregate.PhysicalName}}_{{cat.Name}}DbEntity> {{cat.Name}} => _{{cat.Name}} ??= new(_dbContext.{{rootAggregate.PhysicalName}}_{{cat.Name}}DbSet) {
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
}
