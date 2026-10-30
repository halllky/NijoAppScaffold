using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nijo.DataStructures.Aggregates;
using Nijo.CodeGenerating;

namespace Nijo.DataStructures.IsStored {
    internal class SaveCommandMessageContainer : MessageContainer.Setter {
        public SaveCommandMessageContainer(AggregateBase aggregate) : base(aggregate) {
        }

        internal override string CsClassName => $"{_aggregate.PhysicalName}SaveCommandMessages";

        protected override IEnumerable<MessageContainer.IMember> GetMembers() {
            // SaveCommandと同じデータ型になるのでSaveCommandの処理を流用する。
            // Updateを使っているのは、Create, Update のうちUpdateの方が多くの項目を持っているため
            return new SaveCommand(_aggregate, SaveCommand.E_Type.Update)
                .GetMembers()
                .Select(m => new MessageContainerMemberImpl {
                    PhysicalName = m.PhysicalName,
                    DisplayName = m.DisplayName,
                    IsArray = m.Member is ChildrenAggregate,
                    NestedObject = m.Member switch {
                        ChildAggregate child => new SaveCommandMessageContainer(child),
                        ChildrenAggregate children => new SaveCommandMessageContainer(children),
                        _ => null,
                    },
                });
        }


        #region レンダリング
        internal static string RenderTree(RootAggregate rootAggregate) {
            var tree = rootAggregate
                .EnumerateThisAndDescendants()
                .Select(agg => new SaveCommandMessageContainer(agg))
                .ToArray();

            return $$"""
                #region 登録更新の過程で発生したメッセージの入れ物クラス
                {{tree.SelectTextTemplate(container => $$"""
                {{container.RenderCSharp()}}
                """)}}
                #endregion 登録更新の過程で発生したメッセージの入れ物クラス
                """;
        }
        #endregion レンダリング

        private class MessageContainerMemberImpl : MessageContainer.IMember {
            public required string PhysicalName { get; set; }
            public required string DisplayName { get; set; }
            public required bool IsArray { get; set; }
            public required SaveCommandMessageContainer? NestedObject { get; set; }
            MessageContainer.Setter? MessageContainer.IMember.NestedObject => NestedObject;
            public string? CsType => null;

            public IEnumerable<string> GetPathSinceParent() {
                yield return PhysicalName;
            }
        }
    }
}

namespace Nijo.DataStructures {
    using Nijo.DataStructures.IsStored;

    partial class SchemaPathNodeExtensions {

        /// <summary>
        /// <see cref="GetPathFromEntry(ISchemaPathNode)"/> の結果を <see cref="SaveCommandMessageContainer"/> のルールに沿ったパスとして返す
        /// </summary>
        public static IEnumerable<string> AsSaveCommandMessage(this IEnumerable<ISchemaPathNode> path) {
            var entry = path.FirstOrDefault()?.GetEntry();
            var isOutOfEntryTree = false;

            foreach (var node in path) {
                if (node == entry) continue; // パスの一番最初（エントリー）はスキップ
                if (node.PreviousNode is RefToMember) continue; // refの1つ次の要素の名前はrefで列挙済みのためスキップ

                // 外部参照のナビゲーションプロパティを辿るパス
                if (node is RefToMember refTo) {
                    var previous = (AggregateBase?)node.PreviousNode ?? throw new InvalidOperationException("reftoの前は必ず参照元集約か参照先集約になるのでありえない");

                    // 参照元から参照先へ辿るパス
                    if (previous == refTo.Owner) {
                        if (!isOutOfEntryTree) {
                            // エントリーの集約内部から外に出る瞬間の場合
                            var member = new SaveCommand.SaveCommandRefMember(refTo);
                            yield return member.Member.PhysicalName;

                            isOutOfEntryTree = true;

                            // ref-toに対するメッセージはref-toのIDや名称ではなくref-to自身に付すため、ここで終わり
                            break;

                        } else {
                            // 参照先のキーの中でさらに他の集約への参照が発生した場合
                            var key = new KeyClass.KeyClassRefMember(refTo);
                            yield return key.PhysicalName;
                            continue;
                        }
                    }
                    // 参照先から参照元へ辿るパス
                    if (previous == refTo.RefTo) {
                        throw new InvalidOperationException("更新処理引数クラスでは参照先から参照元へ辿ることはできない");
                    }
                    throw new InvalidOperationException("reftoの前は必ず参照元集約か参照先集約になるのでありえない");
                }

                // 親子間のナビゲーションプロパティを辿るパス
                if (node is AggregateBase curr && node.PreviousNode is AggregateBase prev) {

                    // 子から親へ辿るパス
                    if (curr.IsParentOf(prev)) {

                        // エントリーの集約内部では子から親へ辿るパターンは無い
                        if (!isOutOfEntryTree) throw new InvalidOperationException("エントリーの集約内部では子から親へ辿るパターンは無い");

                        var parentMember = new KeyClass.KeyClassEntry(curr);
                        yield return parentMember.PhysicalName;
                        continue;
                    }
                    // 親から子へ辿るパス
                    if (curr.IsChildOf(prev)) {

                        // 参照先のキーの中では親から子へ辿るパターンは無い
                        if (isOutOfEntryTree) throw new InvalidOperationException("参照先のキーの中では親から子へ辿るパターンは無い");

                        var childMemberPhysicalName = curr switch {
                            ChildAggregate child => new SaveCommand.SaveCommandChildMember(child, SaveCommand.E_Type.Create).PhysicalName, // CreateとUpdateで名前同じなのでCにしている
                            ChildrenAggregate children => new SaveCommand.SaveCommandChildrenMember(children, SaveCommand.E_Type.Create).PhysicalName, // CreateとUpdateで名前同じなのでCにしている
                            _ => throw new InvalidOperationException("ありえない"),
                        };
                        yield return childMemberPhysicalName;
                        continue;
                    }
                    throw new InvalidOperationException("必ず 親→子, 子→親 のどちらかになるのでありえない");
                }

                // 末端のメンバー
                if (node is not ValueMember vm) throw new InvalidOperationException("この分岐まで来るケースは値の場合しか無いのでありえない");

                var valueMember = new SaveCommand.SaveCommandValueMember(vm);
                yield return valueMember.PhysicalName;
            }
        }

    }
}
