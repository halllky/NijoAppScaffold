using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.StructureModelModules {
    /// <summary>
    /// <see cref="StructureModel.StructureType"/> の形と一致するメッセージの入れ物。
    /// この構造体がいずれかのコマンドモデルの引数に指定されている場合のみレンダリングされる。
    /// </summary>
    internal class StructureTypeMessageContainer : MessageContainer {
        public StructureTypeMessageContainer(AggregateBase aggregate) : base(aggregate) {
        }

        internal override string CsClassName => $"{_aggregate.PhysicalName}Messages";
        internal override string TsTypeName => $"{_aggregate.PhysicalName}Messages";

        protected override IEnumerable<IMessageContainerMember> GetMembers() {
            var structureType = _aggregate is RootAggregate root
                ? new StructureModel.StructureType(root)
                : new StructureModel.StructureDescendantMember(_aggregate);
            foreach (var member in ((IInstancePropertyOwnerMetadata)structureType).GetMembers()) {
                if (member is IInstanceValuePropertyMetadata) {
                    yield return new ContainerMemberImpl {
                        PhysicalName = member.GetPropertyName(E_CsTs.CSharp),
                        DisplayName = member.DisplayName,
                        NestedObject = null,
                        CsType = null,
                    };
                } else if (member is IInstanceStructurePropertyMetadata structMember) {
                    // RefToMemberやDescendantMemberの場合
                    var nestedObject = structMember is StructureModel.StructureDescendantMember descendant
                        ? new StructureTypeMessageContainer(descendant.Aggregate)
                        : null;

                    yield return new ContainerMemberImpl {
                        PhysicalName = member.GetPropertyName(E_CsTs.CSharp),
                        DisplayName = member.DisplayName,
                        NestedObject = nestedObject,
                        CsType = null,
                    };
                }
            }
        }

        internal static string RenderCSharpRecursively(RootAggregate rootAggregate) {
            var tree = rootAggregate
                .EnumerateThisAndDescendants()
                .Select(agg => new StructureTypeMessageContainer(agg))
                .ToArray();

            return $$"""
                #region 構造体のコマンド引数用メッセージの入れ物クラス
                {{tree.SelectTextTemplate(container => $$"""
                {{container.RenderCSharp()}}
                """)}}
                #endregion 構造体のコマンド引数用メッセージの入れ物クラス
                """;
        }

        private class ContainerMemberImpl : IMessageContainerMember {
            public required string PhysicalName { get; init; }
            public required string DisplayName { get; init; }
            public required StructureTypeMessageContainer? NestedObject { get; init; }
            public required string? CsType { get; init; }

            MessageContainer? IMessageContainerMember.NestedObject => NestedObject;
        }
    }
}
