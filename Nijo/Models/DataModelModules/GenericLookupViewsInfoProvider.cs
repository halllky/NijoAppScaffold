using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Nijo.Models.DataModelModules;

/// <summary>
/// 全汎用参照テーブルのビュー情報を集約し、
/// DbContext に GetGeneralLookupViewsInfo() 静的メソッドを追加する。
/// </summary>
internal class GenericLookupViewsInfoProvider : IMultiAggregateSourceFile {

    private readonly Lock _lock = new();
    private readonly List<(RootAggregate Root, GenericLookupTableParser.GenericLookupTableCategory[] Categories)> _tables = [];

    internal void AddTable(
        RootAggregate rootAggregate,
        GenericLookupTableParser.GenericLookupTableCategory[] categories) {
        lock (_lock) {
            _tables.Add((rootAggregate, categories));
        }
    }

    void IMultiAggregateSourceFile.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
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
                    [
                {{hardcodedMembersWithValues.SelectTextTemplate(x => $$"""
                        new HardcodedColumnImpl("{{x.Member!.DbName}}", {{RenderObjectLiteral(x.Value, x.Member.Type.CsPrimitiveTypeName)}}),
                """)}}
                    ],
                    [
                {{nonHardcodedMembers.SelectTextTemplate(m => $$"""
                        new ColumnImpl("{{m.DbName}}"),
                """)}}
                    ]);
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
