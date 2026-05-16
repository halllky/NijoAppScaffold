using Nijo.Core;
using Nijo.Parts.WebServer;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.WriteModel2Features {
    /// <summary>
    /// 文字列系項目の文字種チェック
    /// </summary>
    internal class CharacterTypeCheck {

        internal const string METHOD_NAME = "CheckCharacterType";

        private static string GetCsMethodName(string? characterTypeName) {
            return $"CheckIfStringIs{characterTypeName ?? throw new InvalidOperationException()}";
        }

        /// <summary>
        /// 文字列系項目の文字種チェック。
        /// 新規作成処理と更新処理で計2回出てくる
        /// </summary>
        /// <param name="rootAggregate">ルート集約</param>
        internal static string Render(GraphNode<Aggregate> rootAggregate, CodeRenderingContext ctx) {
            var rootDbEntity = new EFCoreEntity(rootAggregate);
            var dataClass = new DataClassForSave(rootAggregate, DataClassForSave.E_Type.UpdateOrDelete);

            return $$"""
                /// <summary>
                /// 文字列系項目の文字種チェック。違反する項目があった場合はその旨が第2引数のオブジェクト内に追記されます。
                /// </summary>
                public virtual void {{METHOD_NAME}}({{rootDbEntity.ClassName}} dbEntity, {{SaveContext.BEFORE_SAVE}}<{{dataClass.MessageDataCsInterfaceName}}> e) {
                {{If(RenderAggregate(rootAggregate, "dbEntity", rootAggregate).Any(), () => $$"""
                    string? temp;
                    var config = ServiceProvider.GetRequiredService<{{Parts.Configure.ABSTRACT_CLASS_NAME}}>();

                """).Else(() => $$"""
                    // 該当項目なし
                """)}}
                    {{WithIndent(RenderAggregate(rootAggregate, "dbEntity", rootAggregate), "    ")}}
                }
                """;
        }

        private static IEnumerable<string> RenderAggregate(GraphNode<Aggregate> renderingAggregate, string instance, GraphNode<Aggregate> instanceAggregate) {
            foreach (var member in renderingAggregate.GetMembers()) {
                var memberDisplayName = member.DisplayName.Replace("\"", "\\\"");

                if (member is AggregateMember.Schalar schalar) {
                    if (schalar.DeclaringAggregate != renderingAggregate) continue; // 親や参照先の項目はParentやRefの分岐でチェックする
                    if (schalar.Options.CharacterType == null) continue;

                    var path = schalar.Declared.GetFullPathAsDbEntity(since: instanceAggregate);
                    var value = $"{instance}.{path.Join("?.")}";

                    var casted = schalar.Options.MemberType switch {
                        Core.AggregateMemberTypes.ValueObjectMember => $"((string?){value})",
                        _ => value,
                    };

                    yield return $$"""
                        temp = {{casted}};
                        if (!string.IsNullOrEmpty(temp) && !config.{{GetCsMethodName(schalar.Options.CharacterType)}}(temp, {{schalar.Options.MaxLength?.ToString() ?? "null"}})) {
                            e.{{GetErrorMemberPath(member).Join(".")}}.AddError(MSG.ERRC0004("{{schalar.Options.CharacterType}}"));
                        }
                        """;

                } else if (member is AggregateMember.Parent) {
                    continue;

                } else if (member is AggregateMember.Ref) {
                    continue;

                } else if (member is AggregateMember.Child child) {
                    yield return $$"""

                        {{WithIndent(RenderAggregate(child.ChildAggregate, instance, instanceAggregate), "")}}
                        """;

                } else if (member is AggregateMember.VariationItem variationItem) {
                    yield return $$"""

                        {{WithIndent(RenderAggregate(variationItem.VariationAggregate, instance, instanceAggregate), "")}}
                        """;

                } else if (member is AggregateMember.Variation) {
                    continue;

                } else if (member is AggregateMember.Children children) {
                    var childrenPath = children.GetFullPathAsDbEntity(since: instanceAggregate);

                    var depth = renderingAggregate.EnumerateAncestors().Count();
                    var i = depth == 0 ? "i" : $"i{depth}";
                    var item = depth == 0 ? "item" : $"item{depth}";
                    yield return $$"""

                        for (var {{i}} = 0; {{i}} < {{instance}}.{{childrenPath.Join("?.")}}.Count; {{i}}++) {
                            var {{item}} = {{instance}}.{{childrenPath.Join("!.")}}.ElementAt({{i}});

                            {{WithIndent(RenderAggregate(children.ChildrenAggregate, item, children.ChildrenAggregate), "    ")}}
                        }
                        """;
                }
            }
        }

        /// <summary>
        /// エラーメッセージの該当プロパティのパスを返す。
        /// 配列インデックスの名前は i, i1, i2, ... で決め打ち。
        /// </summary>
        private static IEnumerable<string> GetErrorMemberPath(AggregateMember.AggregateMemberBase member) {
            /// 決め打ち。<see cref="SaveContext"/> のファイルを参照。
            yield return "Messages";

            foreach (var e in member.Owner.PathFromEntry()) {
                var edge = e.As<Aggregate>();

                if (!edge.IsParentChild()) throw new InvalidOperationException("この分岐にくることは無いはず");

                var child = edge.Terminal.AsChildRelationMember();
                if (child is AggregateMember.Children children) {
                    var depth = children.ChildrenAggregate.EnumerateAncestors().Count() - 1; // 深さはChildren自身ではなく親基準なのでマイナス1
                    var i = depth == 0 ? "i" : $"i{depth}";
                    yield return $"{child.MemberName}[{i}]";

                } else {
                    yield return child.MemberName;
                }
            }

            yield return member.MemberName;
        }



        #region ロジック
        /// <summary>
        /// スキーマ定義で指定されている文字種を列挙する
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        internal static IEnumerable<string> EnumerateCharacterTypeNames(CodeRenderingContext ctx) {
            var characterTypes = ctx.Schema
                .RootAggregates()
                .SelectMany(agg => agg.EnumerateThisAndDescendants())
                .SelectMany(agg => agg.GetMembers())
                .OfType<AggregateMember.Schalar>()
                .Select(vm => vm.Options.CharacterType)
                .Where(name => name != null)
                .Distinct();
            foreach (var characterType in characterTypes.OrderBy(name => name)) {
                yield return characterType!;
            }
        }

        internal static void RenderLogic(CodeRenderingContext ctx) {
            var configure = ctx.UseSummarizedFile<Parts.Configure>();

            foreach (var characterType in EnumerateCharacterTypeNames(ctx)) {

                // C#側のロジック
                configure.AddMethod($$"""
                    /// <summary>
                    /// データ登録更新前の、文字列が{{characterType}}か否かを判定するロジック
                    /// </summary>
                    public abstract bool {{GetCsMethodName(characterType)}}(string value, int? maxLength);
                    """);

                // TypeScript側のロジックは自動生成されない
            }
        }
        #endregion ロジック
    }
}
