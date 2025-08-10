using System;
using System.Collections.Generic;
using System.Linq;
using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Util.DotnetEx;
using Nijo.ValueMemberTypes;

namespace Nijo.Parts.Common;

/// <summary>
/// EFCoreのEntityのメタデータ。
/// 自動生成後のプロジェクトで動的にDB構造の情報を取り扱いときに使用する。
/// </summary>
internal class MetadataOfEFCoreEntity : IMultiAggregateSourceFile {

    private readonly List<RootAggregate> _rootAggregates = new();
    internal MetadataOfEFCoreEntity Register(RootAggregate rootAggregate) {
        _rootAggregates.Add(rootAggregate);
        return this;
    }

    void IMultiAggregateSourceFile.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
        // 特になし
    }

    void IMultiAggregateSourceFile.Render(CodeRenderingContext ctx) {
        ctx.CoreLibrary(dir => {
            dir.Directory("Util", utilDir => {
                utilDir.Generate(RenderCSharp(ctx));
            });
        });

        ctx.ReactProject(dir => {
            dir.Directory("util", utilDir => {
                utilDir.Generate(RenderTypeScriptType());
            });
        });
    }

    private SourceFile RenderCSharp(CodeRenderingContext ctx) {
        var efCoreEntities = _rootAggregates
            .OrderByDataFlow()
            .Select(agg => new EFCoreEntity(agg));

        var staticContainers = _rootAggregates
            .OrderByDataFlow()
            .Select(agg => new Container(agg))
            .ToArray();

        return new SourceFile {
            FileName = "MetadataOfEFCoreEntity.cs",
            Contents = $$"""
                using System.Collections.Generic;
                using System.Text.Json.Serialization;

                namespace {{ctx.Config.RootNamespace}};

                /// <summary>
                /// DataModelのメタデータ
                /// </summary>
                public class MetadataOfEFCoreEntity {
                {{staticContainers.SelectTextTemplate(container => $$"""
                    public {{container.CsClassName}} {{container.PhysicalName}} => _cache_{{container.PhysicalName}} ??= new();
                """)}}

                {{staticContainers.SelectTextTemplate(container => $$"""
                    private {{container.CsClassName}}? _cache_{{container.PhysicalName}};
                """)}}
                {{staticContainers.SelectTextTemplate(container => $$"""


                    {{WithIndent(container.RenderCSharpRecursively(), "    ")}}
                """)}}

                    /// <summary>
                    /// データフローの上流から順番にデータモデルの集約を列挙する。
                    /// </summary>
                    public static IEnumerable<Aggregate> EnumerateDataModelsOrderByDataFlow() {
                {{efCoreEntities.SelectTextTemplate(agg => $$"""
                        yield return {{WithIndent(RenderAggregate(agg), "        ")}};
                """)}}
                    }

                    #region 型
                    /// <summary>
                    /// 集約。ルート集約、Child, Children のいずれか。
                    /// </summary>
                    public class Aggregate : IAggregateMember {
                        /// <summary>
                        /// "root", "child", "children" のいずれか。
                        /// </summary>
                        [JsonPropertyName("type")]
                        public required string Type { get; set; }
                        /// <summary>
                        /// この集約の物理名のパス。この集約がChild, Children の場合はルート集約からのスラッシュ区切り。
                        /// </summary>
                        [JsonPropertyName("path")]
                        public required string Path { get; set; }
                        /// <summary>
                        /// 直近の親集約のパス。直近の親がルート集約でない場合はスラッシュ区切り。
                        /// この集約がルート集約の場合はnull。
                        /// </summary>
                        [JsonPropertyName("parentAggregatePath")]
                        public required string? ParentAggregatePath { get; set; }
                        [JsonPropertyName("physicalName")]
                        public required string PhysicalName { get; set; }
                        [JsonPropertyName("displayName")]
                        public required string DisplayName { get; set; }
                        [JsonPropertyName("tableName")]
                        public required string TableName { get; set; }
                        [JsonPropertyName("description")]
                        public required string Description { get; set; }
                        [JsonPropertyName("members")]
                        public required List<IAggregateMember> Members { get; set; }
                    }
                    /// <summary>
                    /// 集約のメンバー
                    /// </summary>
                    public interface IAggregateMember {
                        string Type { get; }
                    }
                    /// <summary>
                    /// 値メンバー。DBのカラムに対応する。文字列、数値、日付など。
                    /// このテーブル自身に定義されたカラム、親テーブルの主キー、外部参照先テーブルの主キーのいずれか。
                    /// </summary>
                    public class ValueMember : IAggregateMember {
                        /// <summary>
                        /// "own-column", "parent-key", "ref-key", "ref-parent-key" のいずれか。
                        /// </summary>
                        [JsonPropertyName("type")]
                        public required string Type { get; set; }
                        [JsonPropertyName("physicalName")]
                        public required string PhysicalName { get; set; }
                        [JsonPropertyName("displayName")]
                        public required string DisplayName { get; set; }
                        [JsonPropertyName("columnName")]
                        public required string ColumnName { get; set; }
                        [JsonPropertyName("description")]
                        public required string Description { get; set; }
                        /// <summary>
                        /// 値メンバーの型名。XMLスキーマ定義上の型名。
                        /// </summary>
                        [JsonPropertyName("typeName")]
                        public required string TypeName { get; set; }
                        /// <summary>
                        /// 列挙体種類名。
                        /// このメンバーが列挙体でない場合はnull。
                        /// </summary>
                        [JsonPropertyName("enumType")]
                        public required string? EnumType { get; set; }

                        [JsonPropertyName("isPrimaryKey")]
                        public required bool IsPrimaryKey { get; set; }
                        [JsonPropertyName("isNullable")]
                        public required bool IsNullable { get; set; }

                        /// <summary>
                        /// 外部参照先とこの集約の関係性の名前。
                        /// テーブルAからBへ複数の参照経路がある場合にそれらの識別に用いる。
                        /// このメンバーがref-keyでない場合はnull。
                        /// </summary>
                        [JsonPropertyName("refToRelationName")]
                        public required string? RefToRelationName { get; set; }
                        /// <summary>
                        /// 外部参照先テーブルのルート集約からのパス（スラッシュ区切り）。
                        /// このメンバーがref-keyでない場合はnull。
                        /// </summary>
                        [JsonPropertyName("refToAggregatePath")]
                        public required string? RefToAggregatePath { get; set; }
                        /// <summary>
                        /// このメンバーと対応する、外部参照先テーブルのメンバーのDB上のカラム名。
                        /// このメンバーがref-keyでない場合はnull。
                        /// </summary>
                        [JsonPropertyName("refToColumnName")]
                        public required string? RefToColumnName { get; set; }
                    }
                    #endregion 型
                }
                """,
        };

        static string RenderAggregate(EFCoreEntity entity) {
            var type = entity.Aggregate switch {
                RootAggregate => "root",
                ChildAggregate => "child",
                ChildrenAggregate => "children",
                _ => throw new InvalidOperationException(),
            };
            var path = entity.Aggregate.EnumerateThisAndAncestors().Select(a => a.PhysicalName).Join("/");
            var parentPath = entity.Aggregate is RootAggregate
                ? "null"
                : entity.Aggregate.EnumerateAncestors().Select(a => a.PhysicalName).Join("/");

            return $$"""
                new Aggregate {
                    Type = "{{type}}",
                    Path = "{{path}}",
                    ParentAggregatePath = "{{parentPath}}",
                    PhysicalName = "{{entity.Aggregate.PhysicalName}}",
                    DisplayName = "{{entity.Aggregate.DisplayName.Replace("\"", "\\\"")}}",
                    TableName = "{{entity.Aggregate.DbName}}",
                    Description = "{{entity.Aggregate.GetComment(E_CsTs.CSharp).Replace("\"", "\\\"")}}",
                    Members = new List<IAggregateMember> {
                {{RenderMembers(entity).OrderBy(x => x.Order).SelectTextTemplate(x => $$"""
                        {{WithIndent(x.SourceCode, "        ")}},
                """)}}
                    },
                }
                """;
        }
        static IEnumerable<(string SourceCode, decimal Order)> RenderMembers(EFCoreEntity entity) {

            // カラムを列挙
            foreach (var column in entity.GetColumns()) {
                var type = column switch {
                    EFCoreEntity.OwnColumnMember => "own-column",
                    EFCoreEntity.ParentKeyMember => "parent-key",
                    EFCoreEntity.RefKeyMember refTo => refTo.IsParentKey
                        ? "ref-parent-key"
                        : "ref-key",
                    _ => throw new InvalidOperationException(),
                };
                // 列挙体種類名はJavaScriptで引き当てる際に使用するためTS型名を使用
                var enumType = column.Member.Type is StaticEnumMember staticEnumMember
                    ? $"\"{staticEnumMember.Definition.TsTypeName.Replace("\"", "\\\"")}\""
                    : "null";

                string? refToRelationName = null;
                string? refToAggregatePath = null;
                string? refToColumnName = null;
                if (column is EFCoreEntity.RefKeyMember refKeyMember) {
                    refToRelationName = $"\"{refKeyMember.RefEntry.DisplayName.Replace("\"", "\\\"")}\"";

                    refToAggregatePath = $"\"{refKeyMember.RefEntry.RefTo.EnumerateThisAndAncestors().Select(a => a.PhysicalName).Join("/")}\"";

                    var mappingKey = column.Member.ToMappingKey();
                    var refToColumns = new EFCoreEntity(refKeyMember.RefEntry.RefTo).GetColumns();
                    refToColumnName = $"\"{refToColumns.First(c => c.Member.ToMappingKey() == mappingKey).DbName}\"";
                } else {
                    refToRelationName = "null";
                    refToAggregatePath = "null";
                    refToColumnName = "null";
                }

                yield return ($$"""
                    new ValueMember {
                        Type = "{{type}}",
                        PhysicalName = "{{column.PhysicalName}}",
                        DisplayName = "{{column.DisplayName.Replace("\"", "\\\"")}}",
                        ColumnName = "{{column.DbName}}",
                        Description = "{{column.Member.GetComment(E_CsTs.CSharp).Replace("\"", "\\\"")}}",
                        TypeName = "{{column.Member.Type.SchemaTypeName}}",
                        EnumType = {{enumType}},
                        IsPrimaryKey = {{(column.IsKey ? "true" : "false")}},
                        IsNullable = {{(!column.IsKey && !column.Member.IsRequired ? "true" : "false")}},
                        RefToRelationName = {{refToRelationName}},
                        RefToAggregatePath = {{refToAggregatePath}},
                        RefToColumnName = {{refToColumnName}},
                    }
                    """, column.Member.Order);
            }

            // 子テーブルを列挙
            foreach (var member in entity.Aggregate.GetMembers()) {
                if (member is ChildAggregate child) {
                    yield return (RenderAggregate(new EFCoreEntity(child)), child.Order);

                } else if (member is ChildrenAggregate children) {
                    yield return (RenderAggregate(new EFCoreEntity(children)), children.Order);

                } else {
                    // 無視
                }
            }
        }
    }

    /// <summary>
    /// C#側でレンダリングされた型と対応するTypeScriptの型定義。
    /// アプリケーションテンプレートにレンダリングされたものを nijo ui で使用するというトリッキーな参照のされ方をする。
    /// </summary>
    private static SourceFile RenderTypeScriptType() {
        return new SourceFile {
            FileName = "metadata-of-efcore-entity.ts",
            Contents = $$"""
                /** DataModelのメタデータ */
                export namespace MetadataOfEFCoreEntity {

                  /** 集約 */
                  export type Aggregate = {
                    type: "root" | "child" | "children"
                    /**
                     * この集約の物理名のパス。この集約がChild, Children の場合はルート集約からのスラッシュ区切り。
                     */
                    path: string
                    /**
                     * 直近の親集約のパス。直近の親がルート集約でない場合はスラッシュ区切り。
                     * この集約がルート集約の場合はnull。
                     */
                    parentAggregatePath: string | null
                    physicalName: string
                    displayName: string
                    tableName: string
                    description: string
                    members: (AggregateMember | Aggregate)[]

                    // 以下はAggregateMemberにしか無いメンバー
                    columnName?: never
                    typeName?: never
                    enumType?: never
                    isPrimaryKey?: never
                    isNullable?: never
                    refToRelationName?: never
                    refToAggregatePath?: never
                    refToColumnName?: never
                  }

                  /** 集約のメンバー */
                  export type AggregateMember = {
                    type: "own-column" | "parent-key" | "ref-key" | "ref-parent-key"
                    physicalName: string
                    displayName: string
                    columnName: string
                    description: string
                    /**
                     * 値メンバーの型名。XMLスキーマ定義上の型名。
                     */
                    typeName: string
                    /**
                     * 列挙体種類名。
                     * このメンバーが列挙体でない場合はnull。
                     */
                    enumType: string | null
                    isPrimaryKey: boolean
                    isNullable: boolean
                    /**
                     * 外部参照先とこの集約の関係性の名前。
                     * テーブルAからBへ複数の参照経路がある場合にそれらの識別に用いる。
                     * このメンバーがref-keyでない場合はnull。
                     */
                    refToRelationName: string | null
                    /**
                     * 外部参照先テーブルのルート集約からのパス（スラッシュ区切り）。
                     * このメンバーがref-keyでない場合はnull。
                     */
                    refToAggregatePath: string | null
                    /**
                     * このメンバーと対応する、外部参照先テーブルのメンバーのDB上のカラム名。
                     * このメンバーがref-keyでない場合はnull。
                     */
                    refToColumnName: string | null

                    // 以下はAggregateにしか無いメンバー
                    path?: never
                    parentAggregatePath?: never
                    tableName?: never
                    members?: never
                  }
                }
                """,
        };
    }


    /// <summary>
    /// コンテナ。集約ごとの静的アクセスのレンダリング用（EFCore版）。
    /// </summary>
    private class Container {
        internal Container(AggregateBase aggregate) {
            _aggregate = aggregate;
        }
        private readonly AggregateBase _aggregate;

        internal string PhysicalName => _aggregate.PhysicalName;
        internal string CsClassName => $"{_aggregate.PhysicalName}Metadata";

        internal IEnumerable<IMetadataMember> GetMembers() {
            // 値メンバー
            foreach (var column in new EFCoreEntity(_aggregate).GetColumns()) {
                var type = column switch {
                    EFCoreEntity.OwnColumnMember => "own-column",
                    EFCoreEntity.ParentKeyMember => "parent-key",
                    EFCoreEntity.RefKeyMember refTo => refTo.IsParentKey ? "ref-parent-key" : "ref-key",
                    _ => throw new InvalidOperationException(),
                };
                var enumType = column.Member.Type is StaticEnumMember staticEnumMember
                    ? staticEnumMember.Definition.TsTypeName
                    : null;

                string? refToRelationName;
                string? refToAggregatePath;
                string? refToColumnName;
                if (column is EFCoreEntity.RefKeyMember refKeyMember) {
                    refToRelationName = refKeyMember.RefEntry.DisplayName;
                    refToAggregatePath = refKeyMember.RefEntry.RefTo.EnumerateThisAndAncestors().Select(a => a.PhysicalName).Join("/");
                    var mappingKey = column.Member.ToMappingKey();
                    var refToColumns = new EFCoreEntity(refKeyMember.RefEntry.RefTo).GetColumns();
                    refToColumnName = refToColumns.First(c => c.Member.ToMappingKey() == mappingKey).DbName;
                } else {
                    refToRelationName = null;
                    refToAggregatePath = null;
                    refToColumnName = null;
                }

                var data = new ValueMemberData(
                    type,
                    column.PhysicalName,
                    column.DisplayName,
                    column.DbName,
                    column.Member.GetComment(E_CsTs.CSharp),
                    column.Member.Type.SchemaTypeName,
                    enumType,
                    column.IsKey,
                    !column.IsKey && !column.Member.IsRequired,
                    refToRelationName,
                    refToAggregatePath,
                    refToColumnName
                );
                yield return new MetadataValueMember(data);
            }
            // 子集約
            foreach (var member in _aggregate.GetMembers()) {
                if (member is ChildAggregate child) {
                    yield return new MetadataDescendantMember(child);
                } else if (member is ChildrenAggregate children) {
                    yield return new MetadataDescendantMember(children);
                }
            }
        }

        internal string RenderCSharpRecursively() {
            var thisAndDescendants = _aggregate
                .EnumerateThisAndDescendants()
                .Select(agg => new Container(agg));

            return $$"""
                #region {{_aggregate.DisplayName}}
                {{thisAndDescendants.SelectTextTemplate(metadata => $$"""
                {{Render(metadata)}}
                """)}}
                #endregion {{_aggregate.DisplayName}}
                """;

            static string Render(Container metadata) {
                var members = metadata.GetMembers().ToArray();
                return $$"""
                    public class {{metadata.CsClassName}} {
                    {{members.SelectTextTemplate(m => $$"""
                        {{WithIndent(m.RenderCSharp(), "    ")}}
                    """)}}
                    }
                    """;
            }
        }
    }

    #region メンバー(Renderers)
    private interface IMetadataMember {
        string PhysicalName { get; }
        string RenderCSharp();
    }

    private record ValueMemberData(
        string Type,
        string PhysicalName,
        string DisplayName,
        string ColumnName,
        string Description,
        string TypeName,
        string? EnumType,
        bool IsPrimaryKey,
        bool IsNullable,
        string? RefToRelationName,
        string? RefToAggregatePath,
        string? RefToColumnName
    );

    private class MetadataValueMember : IMetadataMember {
        internal MetadataValueMember(ValueMemberData data) {
            _data = data;
        }
        private readonly ValueMemberData _data;

        public string PhysicalName => _data.PhysicalName;

        public string RenderCSharp() {
            var enumType = _data.EnumType != null ? $"\"{_data.EnumType.Replace("\"", "\\\"")}\"" : "null";
            var refToRelationName = _data.RefToRelationName != null ? $"\"{_data.RefToRelationName.Replace("\"", "\\\"")}\"" : "null";
            var refToAggregatePath = _data.RefToAggregatePath != null ? $"\"{_data.RefToAggregatePath.Replace("\"", "\\\"")}\"" : "null";
            var refToColumnName = _data.RefToColumnName != null ? $"\"{_data.RefToColumnName.Replace("\"", "\\\"")}\"" : "null";

            return $$"""
                public ValueMember {{PhysicalName}} { get; } = new() {
                    Type = "{{_data.Type}}",
                    PhysicalName = "{{_data.PhysicalName}}",
                    DisplayName = "{{_data.DisplayName.Replace("\"", "\\\"")}}",
                    ColumnName = "{{_data.ColumnName}}",
                    Description = "{{_data.Description.Replace("\"", "\\\"")}}",
                    TypeName = "{{_data.TypeName}}",
                    EnumType = {{enumType}},
                    IsPrimaryKey = {{(_data.IsPrimaryKey ? "true" : "false")}},
                    IsNullable = {{(_data.IsNullable ? "true" : "false")}},
                    RefToRelationName = {{refToRelationName}},
                    RefToAggregatePath = {{refToAggregatePath}},
                    RefToColumnName = {{refToColumnName}},
                };
                """;
        }
    }

    private class MetadataDescendantMember : IMetadataMember {
        internal MetadataDescendantMember(ChildAggregate child) { _aggregate = child; }
        internal MetadataDescendantMember(ChildrenAggregate children) { _aggregate = children; }
        private readonly AggregateBase _aggregate;
        public string PhysicalName => _aggregate.PhysicalName;

        public string RenderCSharp() {
            var desc = new Container(_aggregate);
            var privateField = $"_cache_{PhysicalName}";
            return $$"""
                public {{desc.CsClassName}} {{PhysicalName}} => {{privateField}} ??= new();
                private {{desc.CsClassName}}? {{privateField}};
                """;
        }
    }
    #endregion メンバー(Renderers)
}
