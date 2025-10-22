using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.QueryModelModules;

/// <summary>
/// <see cref="DisplayData"/> の形と一致するメッセージの入れ物
/// </summary>
internal class DisplayDataMessageContainer : MessageContainer.Setter {
    public DisplayDataMessageContainer(AggregateBase aggregate) : base(aggregate) {
    }

    internal override string CsClassName => $"{_aggregate.PhysicalName}DisplayDataMessages";
    internal override string TsTypeName => $"{_aggregate.PhysicalName}DisplayDataMessages";

    protected override IEnumerable<string> GetCsClassImplements() {
        // この集約がデータモデルの場合、登録更新削除処理で使われるメッセージの入れ物のインタフェースを実装する
        // ただしビューの場合はSaveCommandが生成されないため実装しない
        if (_aggregate.GetRoot() is RootAggregate root
            && root.Model is DataModel
            && !root.IsView) {
            var saveCommandMessage = new SaveCommandMessageContainer(_aggregate);
            yield return saveCommandMessage.InterfaceName;
        }
    }

    protected override IEnumerable<MessageContainer.IMember> GetMembers() {
        var displayData = new DisplayData(_aggregate);
        foreach (var member in displayData.Values.GetMembers()) {
            yield return new ContainerMemberImpl {
                PhysicalName = member.GetPropertyName(E_CsTs.CSharp),
                DisplayName = member.DisplayName,
                NestedObject = null,
                CsType = null,
                IsArray = false,
            };
        }
        foreach (var member in displayData.GetChildMembers()) {
            yield return new ContainerMemberImpl {
                PhysicalName = member.PhysicalName,
                DisplayName = member.DisplayName,
                NestedObject = new DisplayDataMessageContainer(member.Aggregate),
                CsType = null,
                IsArray = member.Aggregate is ChildrenAggregate,
            };
        }
    }

    /// <summary>
    /// この集約がデータモデルの場合、Child, Children のプロパティは明示的にSaveCommandのメンバーにキャストする必要がある
    /// </summary>
    protected override string RenderCSharpAdditionalSource() {
        // この集約がデータモデルでないのであれば関係なし
        // ビューの場合もSaveCommandが生成されないため関係なし
        if (_aggregate.GetRoot() is not RootAggregate root) return SKIP_MARKER;
        if (root.Model is not DataModel) return SKIP_MARKER;
        if (root.IsView) return SKIP_MARKER;

        var saveCommandMessage = new SaveCommandMessageContainer(_aggregate);
        var childMembers = GetMembers()
            .Where(member => member.NestedObject != null)
            .Select(member => {
                var memberAggregate = ((ContainerMemberImpl)member).NestedObject!._aggregate;
                var childMsgContainer = new SaveCommandMessageContainer(memberAggregate);

                return new {
                    member.PhysicalName,
                    InterfaceName = memberAggregate is ChildrenAggregate
                        ? $"{MessageContainer.SETTER_INTERFACE_LIST}<{childMsgContainer.InterfaceName}>"
                        : childMsgContainer.InterfaceName,
                };
            });

        return $$"""

            {{childMembers.SelectTextTemplate(member => $$"""
            {{member.InterfaceName}} {{saveCommandMessage.InterfaceName}}.{{member.PhysicalName}} => this.{{member.PhysicalName}};
            """)}}
            """;
    }

    private class ContainerMemberImpl : MessageContainer.IMember {
        public required string PhysicalName { get; init; }
        public required string DisplayName { get; init; }
        public required DisplayDataMessageContainer? NestedObject { get; init; }
        public required string? CsType { get; init; }
        public required bool IsArray { get; init; }

        MessageContainer.Setter? MessageContainer.IMember.NestedObject => NestedObject;
    }

    internal static string RenderCSharpRecursively(RootAggregate rootAggregate) {
        var tree = rootAggregate
            .EnumerateThisAndDescendants()
            .Select(agg => new DisplayDataMessageContainer(agg))
            .ToArray();

        return $$"""
                #region 画面表示用クラスのデータ構造と対応するメッセージの入れ物クラス
                {{tree.SelectTextTemplate(container => $$"""
                {{container.RenderCSharp()}}
                """)}}
                #endregion 画面表示用クラスのデータ構造と対応するメッセージの入れ物クラス
                """;
    }
}
