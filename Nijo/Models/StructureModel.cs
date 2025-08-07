using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.Models {
    /// <summary>
    /// 単純な構造体モデル。
    /// サーバー(C#)とクライアント(TypeScript)の双方で共有するための入れ子可能なオブジェクトの定義を生成する。
    /// </summary>
    internal class StructureModel : IModel {
        internal const string SCHEMA_NAME = "structure-model";
        public string SchemaName => SCHEMA_NAME;

        public string RenderModelValidateSpecificationMarkdown() {
            return $$"""
                - 外部参照（`{{SchemaParseContext.NODE_TYPE_REFTO}}`）は使用できません。
                - 主キー属性（`{{BasicNodeOptions.IsKey.AttributeName}}`）には特別な意味はありません。
                """;
        }

        public string RenderTypeAttributeSpecificationMarkdown() {
            return $$"""
                - 入れ子になった子集約 `{{SchemaParseContext.NODE_TYPE_CHILD}}` を定義できます。
                - 子配列 `{{SchemaParseContext.NODE_TYPE_CHILDREN}}` を定義できます。
                - その他メンバーに定義できる属性については [属性種類定義](./{{ValueObjectTypesMd.FILE_NAME}}) を参照してください。
                """;
        }

        public void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            // StructureModel では外部参照は禁止
            var refElements = rootAggregateElement
                .DescendantsAndSelf()
                .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value?.StartsWith(SchemaParseContext.NODE_TYPE_REFTO + ":") == true)
                .ToList();
            foreach (var el in refElements) {
                addError(el, $"StructureModelでは{SchemaParseContext.NODE_TYPE_REFTO}を使用できません。");
            }
        }

        public void GenerateCode(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            var aggregateFile = new SourceFileByAggregate(rootAggregate);

            // C# クラス定義（ルートと子孫）
            aggregateFile.AddCSharpClass(StructureType.RenderCSharpRecursively(rootAggregate, ctx), "Class_Structure");

            // TypeScript 型定義（ルートと子孫）
            aggregateFile.AddTypeScriptTypeDef(StructureType.RenderTypeScriptRecursively(rootAggregate, ctx));

            aggregateFile.ExecuteRendering(ctx);
        }

        public void GenerateCode(CodeRenderingContext ctx) {
            // 特になし
        }

        /// <summary>
        /// C#/TS の構造体定義をレンダリングするためのヘルパ。
        /// </summary>
        private class StructureType : IInstancePropertyOwnerMetadata {
            internal StructureType(AggregateBase aggregate) {
                Aggregate = aggregate;
            }
            internal AggregateBase Aggregate { get; }

            internal string CsClassName => Aggregate is RootAggregate root
                ? $"{root.PhysicalName}Structure"
                : $"{Aggregate.GetRoot().PhysicalName}Structure_{Aggregate.PhysicalName}";
            internal string TsTypeName => Aggregate is RootAggregate root
                ? $"{root.PhysicalName}Structure"
                : $"{Aggregate.GetRoot().PhysicalName}Structure_{Aggregate.PhysicalName}";

            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() {
                foreach (var m in Aggregate.GetMembers()) {
                    if (m is ValueMember vm) {
                        yield return new StructureValueMember(vm);
                    } else if (m is ChildAggregate child) {
                        yield return new StructureDescendantMember(child);
                    } else if (m is ChildrenAggregate children) {
                        yield return new StructureDescendantMember(children);
                    }
                }
            }

            internal IEnumerable<StructureDescendantMember> GetDescendants() {
                foreach (var d in Aggregate.EnumerateDescendants()) {
                    yield return new StructureDescendantMember(d);
                }
            }

            internal static string RenderCSharpRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
                var tree = rootAggregate
                    .EnumerateThisAndDescendants()
                    .Select(agg => new StructureType(agg));

                return $$"""
                    #region 構造体定義
                    {{tree.SelectTextTemplate(node => $$"""
                    {{node.RenderCSharpDeclaring(ctx)}}
                    """)}}
                    #endregion 構造体定義
                    """;
            }

            internal string RenderCSharpDeclaring(CodeRenderingContext ctx) {
                var members = ((IInstancePropertyOwnerMetadata)this).GetMembers().ToArray();

                return $$"""
                    /// <summary>
                    /// {{Aggregate.DisplayName}}の構造体。
                    /// </summary>
                    public partial class {{CsClassName}} {
                    {{members.SelectTextTemplate(member => $$"""
                        {{WithIndent(RenderMemberCSharp(member, ctx), "    ")}}
                    """)}}
                    }
                    """;

                static string RenderMemberCSharp(IInstancePropertyMetadata member, CodeRenderingContext ctx) {
                    if (member is IInstanceValuePropertyMetadata v) {
                        return $$"""
                            public {{v.Type.CsDomainTypeName}}? {{member.GetPropertyName(E_CsTs.CSharp)}} { get; set; }
                            """;
                    }
                    var s = (StructureDescendantMember)member;
                    var csType = s.Aggregate is ChildrenAggregate
                        ? $"List<{s.CsType}>"
                        : s.CsType;
                    var initializer = s.Aggregate is ChildrenAggregate ? "new()" : "new()";
                    return $$"""
                        public {{csType}} {{member.GetPropertyName(E_CsTs.CSharp)}} { get; set; } = {{initializer}};
                        """;
                }
            }

            internal static string RenderTypeScriptRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
                var tree = rootAggregate
                    .EnumerateThisAndDescendants()
                    .Select(agg => new StructureType(agg));

                return $$"""
                    //#region 構造体定義
                    {{tree.SelectTextTemplate(node => $$"""
                    {{node.RenderTypeScriptType(ctx)}}
                    """)}}
                    //#endregion 構造体定義
                    """;
            }

            private string RenderTypeScriptType(CodeRenderingContext ctx) {
                var members = ((IInstancePropertyOwnerMetadata)this).GetMembers().ToArray();

                return $$"""
                    /** {{Aggregate.DisplayName}}の構造体 */
                    export type {{TsTypeName}} = {
                    {{members.SelectTextTemplate(member => $$"""
                      {{WithIndent(RenderMemberTs(member, ctx), "  ")}}
                    """)}}
                    }
                    """;

                static string RenderMemberTs(IInstancePropertyMetadata member, CodeRenderingContext ctx) {
                    if (member is IInstanceValuePropertyMetadata v) {
                        return $$"""
                            {{member.GetPropertyName(E_CsTs.TypeScript)}}: {{v.Type.TsTypeName}} | undefined
                            """;
                    }
                    var s = (StructureDescendantMember)member;
                    var arraySuffix = s.Aggregate is ChildrenAggregate ? "[]" : string.Empty;
                    return $$"""
                        {{member.GetPropertyName(E_CsTs.TypeScript)}}: {{s.TsTypeName}}{{arraySuffix}}
                        """;
                }
            }
        }

        private class StructureValueMember : IInstanceValuePropertyMetadata {
            internal StructureValueMember(ValueMember vm) { _vm = vm; }
            private readonly ValueMember _vm;

            public IValueMemberType Type => _vm.Type;
            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => _vm;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => _vm.PhysicalName;
        }

        private class StructureDescendantMember : IInstanceStructurePropertyMetadata {
            internal StructureDescendantMember(AggregateBase aggregate) { Aggregate = aggregate; }
            internal AggregateBase Aggregate { get; }

            internal string CsType => $"{Aggregate.GetRoot().PhysicalName}Structure_{Aggregate.PhysicalName}";
            internal string TsTypeName => $"{Aggregate.GetRoot().PhysicalName}Structure_{Aggregate.PhysicalName}";

            public bool IsArray => Aggregate is ChildrenAggregate;
            public string GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? CsType : TsTypeName;

            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => Aggregate;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => Aggregate.PhysicalName;
            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() {
                foreach (var m in Aggregate.GetMembers()) {
                    if (m is ValueMember vm) {
                        yield return new StructureValueMember(vm);
                    } else if (m is ChildAggregate child) {
                        yield return new StructureDescendantMember(child);
                    } else if (m is ChildrenAggregate children) {
                        yield return new StructureDescendantMember(children);
                    }
                }
            }
        }
    }
}
