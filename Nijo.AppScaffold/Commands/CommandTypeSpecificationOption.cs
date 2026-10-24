using Nijo.CodeGenerating;
using Nijo.SchemaParsing;
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
        // 値未指定は不正
        var splitted = ctx.Value.Split(':', StringSplitOptions.RemoveEmptyEntries);
        var targetPhysicalName = splitted.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(targetPhysicalName)) {
            ctx.AddError("参照先が未指定です。");
            return;
        }

        // コロンが含まれるならその後ろは参照可能なオブジェクトの種類のみ
        var targetType = splitted.Skip(1).FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(targetType) &&
            !AvailableFromCommandToQuery.ContainsKey(targetType)) {
            ctx.AddError($"参照先の種類は {string.Join(" または ", AvailableFromCommandToQuery.Keys)} のみ指定できます。");
            return;
        }

        // 参照先の存在確認
        var targetElement = ctx.SchemaParseContext.Document
            .Root
            ?.Element(SchemaParseContext.SECTION_DATA_STRUCTURES)
            ?.Element(targetPhysicalName);
        if (targetElement == null) {
            ctx.AddError($"参照先の集約が見つかりません。物理名: {targetPhysicalName}");
        }
    }
}
