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
        ctx.Use<ApplicationService>().Add(RenderUtilProperty(rootAggregate));
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
            protected {{utilClassName}} {{rootAggregate.PhysicalName}}Util => {{fieldName}} ??= new {{utilClassName}}(this.DbContext);
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

                {{WithIndent(RenderPropertyForCategory(cat), "    ")}}
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

// -----------------------------------------------------------------
//  GetGeneralLookupViewsInfo() 静的メソッドを DbContext に追加する
//  IMultiAggregateSourceFile
// -----------------------------------------------------------------

/// <summary>
/// 全汎用参照テーブルのビュー情報を集約し、
/// DbContext に GetGeneralLookupViewsInfo() 静的メソッドを追加する。
/// </summary>
internal class GenericLookupViewsInfoProvider : IMultiAggregateSourceFile {

    private readonly Lock _lock = new();
    private readonly List<(RootAggregate Root, GenericLookupTableParser.GenericLookupTableCategory[] Categories)> _tables = [];
    private bool _dependenciesRegistered = false;

    internal void AddTable(
        RootAggregate rootAggregate,
        GenericLookupTableParser.GenericLookupTableCategory[] categories) {
        lock (_lock) {
            _tables.Add((rootAggregate, categories));
        }
    }

    void IMultiAggregateSourceFile.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
        lock (_lock) {
            if (_dependenciesRegistered) return;
            _dependenciesRegistered = true;
        }

        if (_tables.Count == 0) return;

        // DbContextクラスに静的メソッドとインターフェースを追加
        ctx.Use<DbContextClass>().AddAdditionalMethod(RenderViewsInfoMethod());
    }

    void IMultiAggregateSourceFile.Render(CodeRenderingContext ctx) {
        // 追加コードはすべて RegisterDependencies で AddAdditionalMethod 済み
    }

    private string RenderViewsInfoMethod() {
        return $$"""
            #region 汎用参照テーブルビュー情報

            /// <summary>
            /// 汎用参照テーブルのビューの情報
            /// </summary>
            public interface IGeneralLookupViewInfo {
                /// <summary>ビューの物理名</summary>
                string ViewName { get; }
                /// <summary>ビューの元になるテーブル名</summary>
                string SourceTableName { get; }
                /// <summary>WHERE 句でフィルタされるハードコード列の情報</summary>
                IGeneralLookupTableHardcodedColumn[] HardcodedColumns { get; }
                /// <summary>ビューが持つハードコードされない列の情報</summary>
                IGeneralLookupTableColumn[] NonHardcodedColumns { get; }
            }

            /// <summary>
            /// 汎用参照テーブルビューのハードコードされる列
            /// </summary>
            public interface IGeneralLookupTableHardcodedColumn {
                /// <summary>列の物理名</summary>
                string ColumnName { get; }
                /// <summary>WHERE 句でフィルタに使われるハードコード値（型はテーブル定義に従う）</summary>
                object Value { get; }
            }

            /// <summary>
            /// 汎用参照テーブルビューのハードコードされない列
            /// </summary>
            public interface IGeneralLookupTableColumn {
                /// <summary>列の物理名</summary>
                string ColumnName { get; }
            }

            /// <summary>
            /// このアプリケーションに定義されている汎用参照テーブルのビュー情報を列挙します。
            /// ビューのSQL生成などに使用できます。
            /// </summary>
            public static IEnumerable<IGeneralLookupViewInfo> GetGeneralLookupViewsInfo() {
            {{If(_tables.Count == 0, () => $$"""
                yield break;
            """)}}
            {{_tables.SelectTextTemplate(tableEntry => $$"""
                {{WithIndent(RenderGeneralLookupViewInfoImpl(tableEntry.Root, tableEntry.Categories), "    ")}}
            """)}}
            }

            private sealed class GeneralLookupViewInfoImpl : IGeneralLookupViewInfo {
                internal GeneralLookupViewInfoImpl(
                    string viewName,
                    string sourceTableName,
                    IGeneralLookupTableHardcodedColumn[] hardcodedColumns,
                    IGeneralLookupTableColumn[] nonHardcodedColumns) {
                    ViewName = viewName;
                    SourceTableName = sourceTableName;
                    HardcodedColumns = hardcodedColumns;
                    NonHardcodedColumns = nonHardcodedColumns;
                }
                public string ViewName { get; }
                public string SourceTableName { get; }
                public IGeneralLookupTableHardcodedColumn[] HardcodedColumns { get; }
                public IGeneralLookupTableColumn[] NonHardcodedColumns { get; }
            }

            private sealed class HardcodedColumnImpl : IGeneralLookupTableHardcodedColumn {
                internal HardcodedColumnImpl(string columnName, object value) {
                    ColumnName = columnName;
                    Value = value;
                }
                public string ColumnName { get; }
                public object Value { get; }
            }

            private sealed class ColumnImpl : IGeneralLookupTableColumn {
                internal ColumnImpl(string columnName) { ColumnName = columnName; }
                public string ColumnName { get; }
            }

            #endregion 汎用参照テーブルビュー情報
            """;
    }

    private static IEnumerable<string> RenderGeneralLookupViewInfoImpl(RootAggregate root, GenericLookupTableParser.GenericLookupTableCategory[] categories) {

        // ハードコードされない列: 全ValueMemberのうちハードコードでないもの
        var allMembers = root.GetMembers().OfType<ValueMember>().ToArray();

        return categories.Select(cat => {
            var hardcodedIds = cat.HardCodedKeys.Select(k => k.UniqueId).ToHashSet(StringComparer.Ordinal);
            var nonHardcodedMembers = allMembers.Where(m => {
                var uid = m.XElement.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)?.Value;
                return uid == null || !hardcodedIds.Contains(uid);
            }).ToArray();

            var hardcodedMembersWithValues = cat.HardCodedKeys
                .Select(k => new {
                    Member = allMembers.FirstOrDefault(m =>
                        m.XElement.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)?.Value == k.UniqueId),
                    k.Value,
                })
                .Where(x => x.Member != null)
                .ToArray();

            return $$"""
                yield return new GeneralLookupViewInfoImpl(
                    "{{root.DbName}}_{{cat.Name}}",
                    "{{root.DbName}}",
                    new IGeneralLookupTableHardcodedColumn[] {
                {{hardcodedMembersWithValues.SelectTextTemplate(x => $$"""
                        new HardcodedColumnImpl("{{x.Member!.DbName}}", {{RenderObjectLiteral(x.Value, x.Member.Type.CsPrimitiveTypeName)}}),
                """)}}
                    },
                    new IGeneralLookupTableColumn[] {
                {{nonHardcodedMembers.SelectTextTemplate(m => $$"""
                        new ColumnImpl("{{m.DbName}}"),
                """)}}
                    });
                """;
        });
    }

    /// <summary>C#オブジェクトリテラル表現を返します。</summary>
    private static string RenderObjectLiteral(string value, string csTypeName) {
        return csTypeName switch {
            "int" or "long" or "short" => value,
            "decimal" => $"{value}m",
            "double" => $"{value}d",
            "float" => $"{value}f",
            _ => $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"",
        };
    }
}
