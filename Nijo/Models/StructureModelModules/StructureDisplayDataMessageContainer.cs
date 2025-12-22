using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.StructureModelModules {
    /// <summary>
    /// <see cref="StructureModel.StructureDisplayData"/> の形と一致するメッセージの入れ物。
    /// この構造体がいずれかのコマンドモデルの引数に指定されている場合のみレンダリングされる。
    /// </summary>
    internal class StructureDisplayDataMessageContainer : MessageContainer.Setter {
        public StructureDisplayDataMessageContainer(AggregateBase aggregate) : base(aggregate) {
        }

        internal override string CsClassName => $"{_aggregate.PhysicalName}Messages";
        internal override string TsTypeName => $"{_aggregate.PhysicalName}Messages";

        protected override IEnumerable<MessageContainer.IMember> GetMembers() {
            var plainStructure = _aggregate is RootAggregate root
                ? new PlainStructure(root)
                : new StructureDescendantMember(_aggregate);

            foreach (var member in ((IInstancePropertyOwnerMetadata)plainStructure).GetMembers()) {
                switch (member) {
                    case StructureValueMember valueMember:
                        yield return new ContainerMemberImpl {
                            PhysicalName = member.GetPropertyName(E_CsTs.CSharp),
                            DisplayName = member.DisplayName,
                            NestedObject = null,
                            CsType = null,
                            IsArray = false,
                            IsInValuesObject = true,
                        };
                        break;
                    case StructureRefToMember refToMember:
                        var targetStructure = refToMember.GetTargetStructure();
                        MessageContainer.Setter? targetMessage = targetStructure switch {
                            QueryModelModules.DisplayData disp => new QueryModelModules.DisplayDataMessageContainer(disp.Aggregate),
                            QueryModelModules.SearchCondition.Entry sc => new QueryModelModules.SearchConditionMessageContainer(sc.EntryAggregate),
                            QueryModelModules.DisplayDataRef.Entry => null, // 子孫要素が無いので
                            PlainStructure str => new StructureDisplayDataMessageContainer(str.Aggregate),
                            _ => throw new NotImplementedException(),
                        };
                        yield return new ContainerMemberImpl {
                            PhysicalName = member.GetPropertyName(E_CsTs.CSharp),
                            DisplayName = member.DisplayName,
                            NestedObject = targetMessage,
                            CsType = targetMessage?.CsClassName,
                            IsArray = false,
                            IsInValuesObject = true,
                        };
                        break;
                    case StructureDescendantMember descendantMember:
                        var nestedObject = new StructureDisplayDataMessageContainer(descendantMember.Aggregate);
                        yield return new ContainerMemberImpl {
                            PhysicalName = member.GetPropertyName(E_CsTs.CSharp),
                            DisplayName = member.DisplayName,
                            NestedObject = nestedObject,
                            CsType = null,
                            IsArray = descendantMember.IsArray,
                            IsInValuesObject = false,
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
                .Select(agg => new StructureDisplayDataMessageContainer(agg))
                .ToArray();

            return $$"""
                #region 構造体のコマンド引数用メッセージの入れ物クラス
                {{tree.SelectTextTemplate(container => $$"""
                {{container.RenderCSharp()}}
                """)}}
                #endregion 構造体のコマンド引数用メッセージの入れ物クラス
                """;
        }

        private class ContainerMemberImpl : MessageContainer.IMember {
            public required string PhysicalName { get; init; }
            public required string DisplayName { get; init; }
            public required MessageContainer.Setter? NestedObject { get; init; }
            public required string? CsType { get; init; }
            public required bool IsArray { get; init; }
            /// <summary>このメンバーが Values オブジェクト内のメンバーであるか否か</summary>
            public required bool IsInValuesObject { get; init; }

            public IEnumerable<string> GetPathSinceParent() {
                if (IsInValuesObject) {
                    yield return Parts.Common.EditablePresentationObject.VALUES_TS;
                }
                yield return PhysicalName;
            }
        }
    }
}
