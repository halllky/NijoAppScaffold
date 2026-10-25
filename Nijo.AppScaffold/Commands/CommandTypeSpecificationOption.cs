using Nijo.CodeGenerating;
using Nijo.SchemaParsing;
using Nijo.DataStructures.IsStored;
using System;
using System.Collections.Generic;
using System.Linq;
using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures.Aggregates;

namespace Nijo.Commands;

/// <summary>
/// コマンドの引数または戻り値の型を指定する属性。
/// 値は、データ構造のルート集約名か、検索可能なデータ構造のルート集約名とそこから生成されるオブジェクトの種類をコロンで区切ったもの。
/// </summary>
internal abstract class CommandTypeSpecificationOption : NodeOption {

    /// <summary>
    /// コマンドから参照可能な、検索可能なデータ構造のオブジェクトの種類と、そのインスタンス作成処理
    /// </summary>
    internal static Dictionary<string, Func<AggregateBase, ICreatablePresentationLayerStructure>> AvailableFromCommandToQuery => new() {
        { DataStructures.Members.RefToObjectOption.REF_TO_OBJECT_DISPLAY_DATA, aggregate => new DisplayData(aggregate) },
        { DataStructures.Members.RefToObjectOption.REF_TO_OBJECT_SEARCH_CONDITION, aggregate => new SearchCondition.Entry(aggregate.GetRoot()) },
        { DataStructures.Members.RefToObjectOption.REF_TO_OBJECT_REF_TARGET, aggregate => new DisplayDataRef.Entry(aggregate) },
    };

    protected static void ValidateTypeSpecification(NodeOptionValidateContext ctx) {
        if (!CommandTypeSpecification.TryParse(ctx.Value, out var specification)) {
            ctx.AddError("値が不正です。「データ構造名」または「データ構造名:種類」の形式で指定してください。");
            return;
        }

        if (specification.ObjectKind != null && !AvailableFromCommandToQuery.ContainsKey(specification.ObjectKind)) {
            ctx.AddError($"参照先の種類は {string.Join(" または ", AvailableFromCommandToQuery.Keys)} のみ指定できます。");
            return;
        }

        var target = ctx.SchemaParseContext.Document.Root
            ?.Element(SchemaParseContext.SECTION_DATA_STRUCTURES)
            ?.Element(specification.DataStructureName);
        if (target == null) {
            ctx.AddError($"参照先の集約が見つかりません。物理名: {specification.DataStructureName}");
            return;
        }

        // 種類を省略した場合は構造体、種類を指定した場合は検索可能なデータ構造でなければならない
        if (specification.ObjectKind == null && (target.IsStored() || target.IsQueryable())) {
            ctx.AddError($"データ構造「{specification.DataStructureName}」は{IsStoredOption.ATTRIBUTE_NAME}属性も{IsQueryableOption.ATTRIBUTE_NAME}属性も指定されていないデータ構造ではありません。");
        } else if (specification.ObjectKind != null && !target.IsQueryable()) {
            ctx.AddError($"データ構造「{specification.DataStructureName}」は{IsQueryableOption.ATTRIBUTE_NAME}属性が指定されたデータ構造ではありません。");
        }
    }
}
