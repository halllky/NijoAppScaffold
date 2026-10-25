using Nijo.SchemaParsing;
using Nijo.DataStructures.IsStored;
using System;
using System.Collections.Generic;
using System.Linq;
using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures;
using Nijo.CodeGenerating;

namespace Nijo.Commands;

/// <summary>
/// コマンドの引数または戻り値の型を指定する属性。
/// 値は、データ構造のルート集約名か、検索可能なデータ構造のルート集約名とそこから生成されるオブジェクトの種類をコロンで区切ったもの。
/// </summary>
internal abstract class CommandTypeSpecificationOption : NodeOption {

    protected static void ValidateTypeSpecification(NodeOptionValidateContext ctx) {
        if (!CommandTypeSpecification.TryParse(ctx.Value, out var specification)) {
            ctx.AddError("値が不正です。「データ構造名」または「データ構造名:種類」の形式で指定してください。");
            return;
        }

        if (specification.ObjectKind != null && !QueryableObjectKind.All.Contains(specification.ObjectKind)) {
            ctx.AddError($"参照先の種類は {string.Join(" または ", QueryableObjectKind.All)} のみ指定できます。");
            return;
        }

        var target = specification.FindDataStructure(ctx.SchemaParseContext);
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
