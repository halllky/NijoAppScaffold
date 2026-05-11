using System;
using System.Collections.Generic;
using System.Linq;
using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;

namespace Nijo.Models.StructureModelModules;

/// <summary>
/// C#/TS で共通のデータ構造をもつ構造体。
/// スキーマ定義通りのシンプルな構造。
/// </summary>
internal class PlainStructure : IInstancePropertyOwnerMetadata, ICreatablePresentationLayerStructure {
    internal PlainStructure(RootAggregate aggregate) { Aggregate = aggregate; }
    protected PlainStructure(AggregateBase aggregate) { Aggregate = aggregate; }
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
            {{new PlainStructure(rootAggregate).RenderCSharpDeclaring(ctx)}}
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
            {{new PlainStructure(rootAggregate).RenderTypeScriptObjectCreationFunction(ctx)}}
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
              {{WithIndent(RenderMemberTsNewObjectCreation(member))}}
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
            {{NijoAttr.RenderAttributeValues(ctx, Aggregate)}}
            public partial class {{CsClassName}} {
            {{members.SelectTextTemplate(member => $$"""
                {{WithIndent(RenderMemberCSharp(member, ctx))}}
            """)}}
            }
            """;

        static string RenderMemberCSharp(IInstancePropertyMetadata member, CodeRenderingContext ctx) {
            if (member is IInstanceValuePropertyMetadata v) {
                return $$"""
                    {{NijoAttr.RenderAttributeValues(ctx, v.SchemaPathNode)}}
                    public {{v.Type.CsDomainTypeName}}? {{member.GetPropertyName(E_CsTs.CSharp)}} { get; set; }
                    """;
            } else if (member is StructureRefToMember refTo) {
                return $$"""
                    {{NijoAttr.RenderAttributeValues(ctx, refTo.RefToMember)}}
                    public {{refTo.GetTargetStructure().CsClassName}} {{member.GetPropertyName(E_CsTs.CSharp)}} { get; set; } = new();
                    """;
            } else if (member is StructureDescendantMember s) {
                var csType = s.Aggregate is ChildrenAggregate
                    ? $"List<{s.CsClassName}>"
                    : s.CsClassName;
                var initializer = s.Aggregate is ChildrenAggregate ? "new()" : "new()";
                return $$"""
                    {{NijoAttr.RenderAttributeValues(ctx, s.Aggregate)}}
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
            {{new PlainStructure(rootAggregate).RenderTypeScriptType(ctx)}}
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
              {{WithIndent(RenderMemberTs(member, ctx))}}
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
    internal RefToMember RefToMember => _refToMember;

    internal ICreatablePresentationLayerStructure GetTargetStructure() {
        return _refToMember.RefToObject switch {
            RefToMember.E_RefToObject.DisplayData => new QueryModelModules.DisplayData(_refToMember.RefTo),
            RefToMember.E_RefToObject.RefTarget => new QueryModelModules.DisplayDataRef.Entry(_refToMember.RefTo),
            _ => _refToMember.RefTo is RootAggregate root
                ? new PlainStructure(root)
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
internal class StructureDescendantMember : PlainStructure, IInstanceStructurePropertyMetadata {
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
