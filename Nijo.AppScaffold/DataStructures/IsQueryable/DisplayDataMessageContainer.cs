using Nijo.CodeGenerating;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nijo.DataStructures.Aggregates;

namespace Nijo.DataStructures.IsQueryable;

/// <summary>
/// <see cref="DisplayData"/> の形と一致するメッセージの入れ物
/// </summary>
internal class DisplayDataMessageContainer : MessageContainer.Setter {
    public DisplayDataMessageContainer(AggregateBase aggregate) : base(aggregate) {
    }

    internal override string CsClassName => $"{_aggregate.PhysicalName}DisplayDataMessages";
    internal override string TsTypeName => $"{_aggregate.PhysicalName}DisplayDataMessages";

    protected override IEnumerable<MessageContainer.IMember> GetMembers() {
        var displayData = new DisplayData(_aggregate);
        foreach (var member in displayData.GetValueMembers()) {
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

    private class ContainerMemberImpl : MessageContainer.IMember {
        public required string PhysicalName { get; init; }
        public required string DisplayName { get; init; }
        public required DisplayDataMessageContainer? NestedObject { get; init; }
        public required string? CsType { get; init; }
        public required bool IsArray { get; init; }

        MessageContainer.Setter? MessageContainer.IMember.NestedObject => NestedObject;

        public IEnumerable<string> GetPathSinceParent() {
            yield return PhysicalName;
        }
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
