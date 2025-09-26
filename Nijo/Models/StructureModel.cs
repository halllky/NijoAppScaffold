using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using Nijo.Models.StructureModelModules;
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
                - SearchCondition を参照する場合、ルート集約の検索条件オブジェクトのみ参照可能です。
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
                var refToObject = refElement.Attribute(BasicNodeOptions.RefToObject.AttributeName);
                if ((isQueryModel || isGDQM) && refToObject == null) {
                    addError(refElement, $"StructureModelからクエリモデルを外部参照する場合、{BasicNodeOptions.RefToObject.AttributeName}属性を指定する必要があります。");
                }

                // RefToObjectの値が不正かチェック
                if ((isQueryModel || isGDQM)
                        && refToObject != null
                        && refToObject.Value != BasicNodeOptions.REF_TO_OBJECT_DISPLAY_DATA
                        && refToObject.Value != BasicNodeOptions.REF_TO_OBJECT_SEARCH_CONDITION) {
                    addError(refElement, $"{BasicNodeOptions.RefToObject.AttributeName}属性の値は「{BasicNodeOptions.REF_TO_OBJECT_DISPLAY_DATA}」または「{BasicNodeOptions.REF_TO_OBJECT_SEARCH_CONDITION}」である必要があります。");
                }

                // 検索条件を参照する場合はルート集約のみ参照可能
                if (refToObject?.Value == BasicNodeOptions.REF_TO_OBJECT_SEARCH_CONDITION
                        && refTo != refToRoot) {
                    addError(refElement, $"検索条件を参照する場合はルート集約のみ指定可能です。");
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

            // TypeScript 新規オブジェクト作成関数（ルートと子孫）
            aggregateFile.AddTypeScriptTypeDef(StructureType.RenderTsNewObjectFunctionRecursively(rootAggregate, ctx));

            // この構造体がいずれかのコマンドモデルの引数として参照されている場合、
            // メッセージストラクチャーの定義をレンダリングする
            if (rootAggregate.EnumerateCommandModelsRefferingAsParameter().Any()) {
                var messageContainer = new StructureTypeMessageContainer(rootAggregate);
                aggregateFile.AddCSharpClass(StructureTypeMessageContainer.RenderCSharpRecursively(rootAggregate), "Class_MessageContainer");
                aggregateFile.AddTypeScriptTypeDef(messageContainer.RenderTypeScript());
                ctx.Use<Parts.Common.MessageContainer.BaseClass>().Register(messageContainer.CsClassName, messageContainer.CsClassName);
            }

            // TypeScriptのマッピングファイルへの登録
            ctx.Use<Parts.Common.CommandQueryMappings>().AddStructureModel(rootAggregate);

            // 定数: メタデータ
            ctx.Use<Parts.Common.Metadata>().Add(rootAggregate);
            ctx.Use<Parts.Common.MetadataForPage>().Add(rootAggregate);

            aggregateFile.ExecuteRendering(ctx);
        }

        public void GenerateCode(CodeRenderingContext ctx) {
            // 特になし
        }

        /// <summary>
        /// C#/TS の構造体定義をレンダリングするためのヘルパ。
        /// </summary>
        internal class StructureType : IInstancePropertyOwnerMetadata, ICreatablePresentationLayerStructure {
            internal StructureType(RootAggregate aggregate) { Aggregate = aggregate; }
            protected StructureType(AggregateBase aggregate) { Aggregate = aggregate; }
            internal AggregateBase Aggregate { get; }

            public virtual string CsClassName => Aggregate.PhysicalName;
            public virtual string TsTypeName => Aggregate.PhysicalName;

            /// <summary>
            /// TypeScriptの新規オブジェクト作成関数の名前
            /// </summary>
            public string TsNewObjectFunction => $"createNew{TsTypeName}";

            IEnumerable<IInstancePropertyMetadata> IPresentationLayerStructure.GetMembers() {
                return ((IInstancePropertyOwnerMetadata)this).GetMembers();
            }
            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() {
                foreach (var m in Aggregate.GetMembers()) {
                    if (m is ValueMember vm) {
                        yield return new StructureValueMember(vm);
                    } else if (m is RefToMember refTo) {
                        yield return new StructureRefToMember(refTo);
                    } else if (m is ChildAggregate child) {
                        yield return new StructureDescendantMember(child);
                    } else if (m is ChildrenAggregate children) {
                        yield return new StructureDescendantMember(children);
                    } else {
                        throw new NotImplementedException();
                    }
                }
            }

            internal IEnumerable<StructureDescendantMember> GetDescendants() {
                foreach (var d in Aggregate.EnumerateDescendants()) {
                    yield return new StructureDescendantMember(d);
                }
            }

            internal static string RenderCSharpRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
                var descendants = rootAggregate
                    .EnumerateDescendants()
                    .Select(agg => new StructureDescendantMember(agg));

                return $$"""
                    #region 構造体定義
                    {{new StructureType(rootAggregate).RenderCSharpDeclaring(ctx)}}
                    {{descendants.SelectTextTemplate(node => $$"""
                    {{node.RenderCSharpDeclaring(ctx)}}
                    """)}}
                    #endregion 構造体定義
                    """;
            }

            /// <summary>
            /// TypeScript新規オブジェクト作成関数を再帰的にレンダリングします。
            /// </summary>
            internal static string RenderTsNewObjectFunctionRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
                var descendants = rootAggregate
                    .EnumerateDescendants()
                    .Select(agg => new StructureDescendantMember(agg));

                return $$"""
                    //#region 構造体新規作成用関数
                    {{new StructureType(rootAggregate).RenderTypeScriptObjectCreationFunction(ctx)}}
                    {{descendants.SelectTextTemplate(node => $$"""
                    {{node.RenderTypeScriptObjectCreationFunction(ctx)}}
                    """)}}
                    //#endregion 構造体新規作成用関数
                    """;
            }

            /// <summary>
            /// TypeScript新規オブジェクト作成関数をレンダリングします。
            /// </summary>
            private string RenderTypeScriptObjectCreationFunction(CodeRenderingContext ctx) {
                return $$"""
                    /** {{Aggregate.DisplayName}}の構造体の新しいインスタンスを作成します。 */
                    export const {{TsNewObjectFunction}} = (): {{TsTypeName}} => ({{RenderTsNewObjectFunctionBody()}})
                    """;
            }
            public string RenderTsNewObjectFunctionBody() {
                return $$"""
                    {
                    {{((IInstancePropertyOwnerMetadata)this).GetMembers().SelectTextTemplate(member => $$"""
                      {{WithIndent(RenderMemberTsNewObjectCreation(member), "  ")}}
                    """)}}
                    }
                    """;
                static string RenderMemberTsNewObjectCreation(IInstancePropertyMetadata member) {
                    if (member is IInstanceValuePropertyMetadata v) {
                        return $$"""
                            {{member.GetPropertyName(E_CsTs.TypeScript)}}: undefined,
                            """;
                    } else if (member is StructureRefToMember refTo) {
                        return $$"""
                            {{member.GetPropertyName(E_CsTs.TypeScript)}}: {{refTo.GetTargetStructure().TsNewObjectFunction}}(),
                            """;
                    } else if (member is StructureDescendantMember s) {
                        var initializer = s.IsArray ? "[]" : $"{s.RenderTsNewObjectFunctionBody()}";
                        return $$"""
                            {{member.GetPropertyName(E_CsTs.TypeScript)}}: {{initializer}},
                            """;
                    } else {
                        throw new NotImplementedException();
                    }
                }
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
                    } else if (member is StructureRefToMember refTo) {
                        return $$"""
                            public {{refTo.GetTargetStructure().CsClassName}} {{member.GetPropertyName(E_CsTs.CSharp)}} { get; set; } = new();
                            """;
                    } else if (member is StructureDescendantMember s) {
                        var csType = s.Aggregate is ChildrenAggregate
                            ? $"List<{s.CsClassName}>"
                            : s.CsClassName;
                        var initializer = s.Aggregate is ChildrenAggregate ? "new()" : "new()";
                        return $$"""
                            public {{csType}} {{member.GetPropertyName(E_CsTs.CSharp)}} { get; set; } = {{initializer}};
                            """;
                    } else {
                        throw new NotImplementedException();
                    }
                }
            }

            internal static string RenderTypeScriptRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
                var descendants = rootAggregate
                    .EnumerateDescendants()
                    .Select(agg => new StructureDescendantMember(agg));

                return $$"""
                    //#region 構造体定義
                    {{new StructureType(rootAggregate).RenderTypeScriptType(ctx)}}
                    {{descendants.SelectTextTemplate(node => $$"""
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
                    } else if (member is StructureRefToMember refTo) {
                        return $$"""
                            {{member.GetPropertyName(E_CsTs.TypeScript)}}: {{refTo.GetTargetStructure().TsTypeName}}
                            """;
                    } else if (member is StructureDescendantMember s) {
                        return $$"""
                            {{member.GetPropertyName(E_CsTs.TypeScript)}}: {{s.TsTypeName}}{{(s.IsArray ? "[]" : "")}}
                            """;
                    } else {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        /// <summary>
        /// 構造体モデルの値メンバー
        /// </summary>
        internal class StructureValueMember : IInstanceValuePropertyMetadata {
            internal StructureValueMember(ValueMember vm) { _vm = vm; }
            private readonly ValueMember _vm;

            public IValueMemberType Type => _vm.Type;
            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => _vm;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => _vm.PhysicalName;
        }

        /// <summary>
        /// 構造体モデルの ref-to メンバー
        /// </summary>
        internal class StructureRefToMember : IInstanceStructurePropertyMetadata {
            internal StructureRefToMember(RefToMember refToMember) {
                _refToMember = refToMember;
            }
            private readonly RefToMember _refToMember;

            internal ICreatablePresentationLayerStructure GetTargetStructure() {
                return _refToMember.RefToObject switch {
                    RefToMember.E_RefToObject.DisplayData => new QueryModelModules.DisplayData(_refToMember.RefTo),
                    RefToMember.E_RefToObject.SearchCondition => new QueryModelModules.SearchCondition.Entry((RootAggregate)_refToMember.RefTo),
                    _ => _refToMember.RefTo is RootAggregate root
                        ? new StructureType(root)
                        : new StructureDescendantMember(_refToMember.RefTo),
                };
            }

            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => _refToMember;
            bool IInstanceStructurePropertyMetadata.IsArray => false;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => _refToMember.PhysicalName;
            string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) {
                return csts == E_CsTs.CSharp
                    ? GetTargetStructure().CsClassName
                    : GetTargetStructure().TsTypeName;
            }
            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() {
                return GetTargetStructure().GetMembers();
            }
        }

        /// <summary>
        /// 構造体モデルの Child, Children メンバー
        /// </summary>
        internal class StructureDescendantMember : StructureType, IInstanceStructurePropertyMetadata {
            internal StructureDescendantMember(AggregateBase aggregate) : base(aggregate) {
                if (aggregate is not ChildAggregate && aggregate is not ChildrenAggregate) {
                    throw new ArgumentException("aggregate must be ChildAggregate or ChildrenAggregate");
                }
            }

            public override string CsClassName => $"{Aggregate.GetRoot().PhysicalName}_{Aggregate.PhysicalName}";
            public override string TsTypeName => $"{Aggregate.GetRoot().PhysicalName}_{Aggregate.PhysicalName}";

            public bool IsArray => Aggregate is ChildrenAggregate;
            string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? CsClassName : TsTypeName;
            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => Aggregate;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => Aggregate.PhysicalName;
        }
    }
}
