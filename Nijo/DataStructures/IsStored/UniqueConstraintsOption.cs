using Nijo.DataStructures.Members;
using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.DataStructures.IsStored;

/// <summary>
/// 永続化されるデータ構造のテーブルにつくユニーク制約を指定する属性
/// </summary>
internal sealed class UniqueConstraintsOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "UniqueConstraints";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "ユニーク制約";
    public override E_NodeOptionType Type => E_NodeOptionType.String;
    public override string HelpText => $$"""
        ユニーク制約を指定します。対象の属性は {{SchemaParseContext.ATTR_UNIQUE_ID}} 属性で指定します。
        複数のカラムから成るユニーク制約を指定する場合は、対象の属性をカンマ(,)で区切ってください。
        複数指定する場合はセミコロン(;)で区切ってください。
        例: "xxxx;yyyy;zzzz;" と指定した場合、Xの列、Yの列、Zの列それぞれにユニーク制約がつきます。
        例: "xxxx,yyyy;zzzz;" と指定した場合、Xの列とYの列の組み合わせと、Zの列にユニーク制約がつきます。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.IsStored
            && (target.NodeType == E_NodeType.RootAggregate
             || target.NodeType == E_NodeType.ChildAggregate
             || target.NodeType == E_NodeType.ChildrenAggregate);
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        var raw = ctx.Value?.Trim();
        if (string.IsNullOrEmpty(raw)) {
            ctx.AddError("ユニーク制約の定義が空です。少なくとも1つ以上の対象項目を指定してください。");
            return;
        }

        // 「;」区切りでユニーク制約1件分、「,」区切りでカラム1件分
        var constraintTexts = raw
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
        if (constraintTexts.Length == 0) {
            ctx.AddError("ユニーク制約の定義が空です。書式を確認してください。");
            return;
        }

        // 同一テーブル（同一集約）直下の UniqueId 付き要素のみ収集
        var directMembers = ctx.XElement.Elements().ToArray();
        var elementsByUniqueId = directMembers
            .Where(e => e.Attribute(SchemaParseContext.ATTR_UNIQUE_ID) != null)
            .GroupBy(e => e.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 主キー(IsKey)で構成される UniqueId セット（完全一致の重複定義を検出するため）
        var keyUniqueIds = directMembers
            .Where(e => e.Attribute(IsKeyOption.ATTRIBUTE_NAME) != null)
            .Select(e => e.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)?.Value)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
        var primaryKeyCanonical = keyUniqueIds.Length > 0
            ? string.Join(",", keyUniqueIds)
            : null;

        // 同じ制約の二重定義検出用
        var seenConstraintKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var constraintText in constraintTexts) {
            var ids = constraintText
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToArray();

            if (ids.Length == 0) {
                ctx.AddError("ユニーク制約の定義に空のグループがあります。',' や ';' の位置を確認してください。");
                continue;
            }

            // ソートして順序に依存しないキーを作成
            var canonical = string.Join(",", ids.OrderBy(id => id, StringComparer.Ordinal));
            if (!seenConstraintKeys.Add(canonical)) {
                ctx.AddError($"同じユニーク制約 '{constraintText}' が複数回指定されています。");
            }

            // 主キーと完全に同じ組み合わせであれば冗長
            if (primaryKeyCanonical != null && canonical == primaryKeyCanonical) {
                ctx.AddError("主キーで構成されるユニーク制約は暗黙に存在するため、UniqueConstraints に同じ組み合わせを指定する必要はありません。");
            }

            foreach (var id in ids) {
                if (!elementsByUniqueId.TryGetValue(id, out var targetElements) || targetElements.Count == 0) {
                    // 指定されたIDの要素がなければエラー
                    ctx.AddError($"{SchemaParseContext.ATTR_UNIQUE_ID}='{id}' を持つ要素が見つかりません。");
                    continue;
                }

                if (targetElements.Count > 1) {
                    // ID重複エラー（同一集約内で UniqueId が一意でない）
                    ctx.AddError($"{SchemaParseContext.ATTR_UNIQUE_ID}='{id}' を持つ要素が同じ集約内に複数存在します。{SchemaParseContext.ATTR_UNIQUE_ID} 属性は集約内で一意である必要があります。");
                }

                // Child や Children など、値メンバー／外部参照以外は指定不可
                foreach (var el in targetElements) {
                    var nodeType = ctx.SchemaParseContext.GetNodeType(el);
                    if (nodeType != E_NodeType.ValueMember && nodeType != E_NodeType.Ref) {
                        ctx.AddError($"UniqueConstraints で指定できる対象は値メンバーまたは外部参照のみです。{SchemaParseContext.ATTR_UNIQUE_ID}='{id}' は {nodeType} を指しています。");
                        break;
                    }
                }
            }
        }
    }
}
