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
                #### 外部参照 `{{SchemaParseContext.NODE_TYPE_REFTO}}` について

                - StructureModel から参照できるのは、クエリモデル、`{{BasicNodeOptions.GenerateDefaultQueryModel.AttributeName}}`属性が付与されたデータモデル、または他のStructureModelの集約のみです。
                - 自身のツリーの集約を参照することはできません。
                - クエリモデルを参照する場合、`{{BasicNodeOptions.RefToObject.AttributeName}}`属性の指定が必須です。

                #### その他制約事項

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
            // 外部参照のチェック
            ValidateRefTo(rootAggregateElement, context, addError);
        }

        /// <summary>
        /// StructureModelの外部参照をチェックします
        /// </summary>
        private void ValidateRefTo(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            // 自身を起点とするすべての外部参照を取得
            var refElements = rootAggregateElement
                .Descendants()
                .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value?.StartsWith(SchemaParseContext.NODE_TYPE_REFTO + ":") == true)
                .ToList();

            if (!refElements.Any()) return;

            foreach (var refElement in refElements) {
                var refTo = context.FindRefTo(refElement);
                if (refTo == null) {
                    addError(refElement, $"参照先の要素が見つかりません: {refElement.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value}");
                    continue;
                }

                // 自身のツリーの集約を参照していないかチェック
                var rootElement = context.GetRootAggregateElement(refElement);
                var refToRoot = context.GetRootAggregateElement(refTo);

                if (rootElement == refToRoot) {
                    addError(refElement, "自身のツリーの集約を参照することはできません。");
                    continue;
                }

                // 参照先がクエリモデル、GDQMデータモデル、またはStructureModelか確認
                var refToType = refToRoot.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value;
                var isQueryModel = refToType == QueryModel.NODE_TYPE;
                var isGDQM = context.HasGenerateDefaultQueryModelAttribute(refToRoot);
                var isStructureModel = refToType == SCHEMA_NAME;

                if (!isQueryModel && !isGDQM && !isStructureModel) {
                    addError(refElement, $"StructureModelの集約からは、クエリモデル、{BasicNodeOptions.GenerateDefaultQueryModel.AttributeName}属性が付与されたデータモデル、またはStructureModelの集約しか参照できません。");
                    continue;
                }

                // クエリモデルまたはGDQMデータモデルを参照する場合はRefToObjectの指定が必須
                if ((isQueryModel || isGDQM) && refElement.Attribute(BasicNodeOptions.RefToObject.AttributeName) == null) {
                    addError(refElement, $"StructureModelからクエリモデルを外部参照する場合、{BasicNodeOptions.RefToObject.AttributeName}属性を指定する必要があります。");
                } else if ((isQueryModel || isGDQM) && refElement.Attribute(BasicNodeOptions.RefToObject.AttributeName) != null) {
                    var refToObject = refElement.Attribute(BasicNodeOptions.RefToObject.AttributeName)?.Value;
                    if (refToObject != BasicNodeOptions.REF_TO_OBJECT_DISPLAY_DATA && refToObject != BasicNodeOptions.REF_TO_OBJECT_SEARCH_CONDITION) {
                        addError(refElement, $"{BasicNodeOptions.RefToObject.AttributeName}属性の値は「{BasicNodeOptions.REF_TO_OBJECT_DISPLAY_DATA}」または「{BasicNodeOptions.REF_TO_OBJECT_SEARCH_CONDITION}」である必要があります。");
                    }
                }

                // StructureModelを参照する場合はRefToObjectの指定は不要（指定されていても無視）
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
