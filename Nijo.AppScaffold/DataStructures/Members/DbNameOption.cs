using Nijo.DataStructures.IsQueryable;
using Nijo.SchemaParsing;
using System;
using System.Xml.Linq;
using System.Linq;

namespace Nijo.DataStructures.Members;

/// <summary>
/// データベース上のテーブル名またはカラム名を指定する属性
/// </summary>
internal sealed class DbNameOption : NodeOption {
    internal const string ATTRIBUTE_NAME = "DbName";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "データベース上名称";
    public override E_NodeOptionType Type => E_NodeOptionType.String;
    public override string HelpText => $$"""
        データベースのテーブル名またはカラム名を明示的に指定したい場合に設定してください。
        集約に定義した場合はテーブル名、値に定義した場合はカラム名になります。
        未指定の場合、物理名がそのままテーブル名やカラム名になります。
        ここで指定する値に改行を含めることはできません。
        複数の集約で同じテーブル名を指定することはできません。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.IsStored || target.IsQueryable;
    }

    public override void Validate(NodeOptionValidateContext ctx) {
        // データベース上の実体をもたないデータ構造には指定不可
        if (!ctx.Target.IsStored && ctx.XElement.GetRootAggregateElement().Attribute(MapToViewOption.ATTRIBUTE_NAME) == null) {
            ctx.AddError("ビューにマッピングされない場合は指定できません。");
        }

        // 改行不可
        if (ctx.Value.Contains('\n')) ctx.AddError("改行を含めることはできません。");
    }

    /// <summary>
    /// データベースのテーブル名またはカラム名。この属性が指定されていない場合は物理名をそのまま用いる。
    /// </summary>
    internal static string GetDbName(XElement xElement) {
        return xElement.Attribute(ATTRIBUTE_NAME)?.Value ?? xElement.Name.LocalName;
    }
}
