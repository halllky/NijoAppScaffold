using System;
using System.Linq;
using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;

namespace Nijo.Models.StructureModelModules;

/// <summary>
/// 構造モデルがコマンドモデルの引数になる場合の、画面上での編集用オブジェクト。
/// 単なるデータだけでなく、保存時に削除するかどうかのフラグなど、編集のための仕組みも持つ。
/// </summary>
internal class StructureDisplayData : EditablePresentationObject {
    internal StructureDisplayData(AggregateBase aggregate) : base(aggregate) { }

    internal override string CsClassName => $"{Aggregate.PhysicalName}DisplayData";
    internal override string TsTypeName => $"{Aggregate.PhysicalName}DisplayData";

    internal override bool HasVersion => false;
    #region レンダリング
    internal static string RenderCSharpRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
        var tree = rootAggregate
            .EnumerateThisAndDescendants()
            .Select<AggregateBase, EditablePresentationObject>(agg => agg switch {
                RootAggregate root => new StructureDisplayData(root),
                ChildAggregate child => new EditablePresentationObjectChildDescendant(child),
                ChildrenAggregate children => new EditablePresentationObjectChildrenDescendant(children),
                _ => throw new InvalidOperationException(),
            });

        return $$"""
            #region 画面表示用データ
            {{tree.SelectTextTemplate(disp => $$"""
            {{disp.RenderCSharpDeclaring(ctx)}}
            """)}}
            #endregion 画面表示用データ
            """;
    }
    internal static string RenderTypeScriptRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
        var tree = rootAggregate
            .EnumerateThisAndDescendants()
            .Select<AggregateBase, EditablePresentationObject>(agg => agg switch {
                RootAggregate root => new StructureDisplayData(root),
                ChildAggregate child => new EditablePresentationObjectChildDescendant(child),
                ChildrenAggregate children => new EditablePresentationObjectChildrenDescendant(children),
                _ => throw new InvalidOperationException(),
            });

        return $$"""
            //#region 画面表示用データ
            {{tree.SelectTextTemplate(disp => $$"""
            {{disp.RenderTypeScriptType(ctx)}}
            """)}}
            //#endregion 画面表示用データ
            """;
    }
    #endregion レンダリング
}
