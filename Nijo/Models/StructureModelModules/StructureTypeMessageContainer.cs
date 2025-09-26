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
                switch (member) {
                    case StructureModel.StructureValueMember valueMember:
                        yield return new ContainerMemberImpl {
                            PhysicalName = member.GetPropertyName(E_CsTs.CSharp),
                            DisplayName = member.DisplayName,
                            NestedObject = null,
                            CsType = null,
                            IsArray = false,
                        };
                        break;
                    case StructureModel.StructureRefToMember refToMember:
                        var targetStructure = refToMember.GetTargetStructure();
                        MessageContainer targetMessage = targetStructure switch {
                            QueryModelModules.DisplayData disp => new QueryModelModules.DisplayDataMessageContainer(disp.Aggregate),
                            QueryModelModules.SearchCondition.Entry sc => new QueryModelModules.SearchConditionMessageContainer(sc.EntryAggregate),
                            StructureModel.StructureType str => new StructureTypeMessageContainer(str.Aggregate),
                            _ => throw new NotImplementedException(),
                        };
                        yield return new ContainerMemberImpl {
                            PhysicalName = member.GetPropertyName(E_CsTs.CSharp),
                            DisplayName = member.DisplayName,
                            NestedObject = targetMessage,
                            CsType = targetMessage.CsClassName,
                            IsArray = false,
                        };
                        break;
                    case StructureModel.StructureDescendantMember descendantMember:
                        var nestedObject = new StructureTypeMessageContainer(descendantMember.Aggregate);
                        yield return new ContainerMemberImpl {
                            PhysicalName = member.GetPropertyName(E_CsTs.CSharp),
                            DisplayName = member.DisplayName,
                            NestedObject = nestedObject,
                            CsType = null,
                            IsArray = descendantMember.IsArray,
                        };
                        break;
                    default:
                        throw new NotImplementedException();
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
            public required MessageContainer? NestedObject { get; init; }
            public required string? CsType { get; init; }
            public required bool IsArray { get; init; }
        }
    }
}
