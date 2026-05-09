using Nijo.Core;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.WriteModel2Features {

    /// <summary>
    /// シーケンスはデータの新規登録や更新のタイミングで
    /// 自動生成される処理の内部で自動的に採番しに行く
    /// </summary>
    internal class GenerateAndSetSequenceMethod {

        internal const string METHOD_NAME = "GenerateAndSetSequenceValue";

        internal GenerateAndSetSequenceMethod(GraphNode<Aggregate> rootAggregate) {
            _rootAggregate = rootAggregate;
        }
        private readonly GraphNode<Aggregate> _rootAggregate;

        internal bool HasSequenceMember() {
            return _rootAggregate
                .EnumerateThisAndDescendants()
                .SelectMany(agg => agg.GetMembers())
                .OfType<AggregateMember.ValueMember>()
                .Any(vm => vm.Options.MemberType is Core.AggregateMemberTypes.SequenceMember
                        && vm.DeclaringAggregate == vm.Owner);
        }

        internal string RenderAppSrvMethod(CodeRenderingContext ctx) {
            // シーケンスが無い集約の場合はソース生成割愛
            if (!HasSequenceMember()) {
                return SKIP_MARKER;
            }

            var efCoreEntity = new EFCoreEntity(_rootAggregate);

            return $$"""
                /// <summary>
                /// {{_rootAggregate.Item.DisplayName}} に含まれるシーケンス項目のうち、
                /// 値がnullのものについて、DBにアクセスして採番を行なう。
                /// </summary>
                protected virtual void {{METHOD_NAME}}({{efCoreEntity.ClassName}} entity) {
                    var conn = DbContext.Database.GetDbConnection();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.Text;

                    {{WithIndent(RenderAggregate(efCoreEntity, "entity", _rootAggregate), "    ")}}
                }
                """;

            static string RenderAggregate(EFCoreEntity entity, string instance, GraphNode<Aggregate> instanceAggregate) {
                var sequences = entity
                    .GetTableColumnMembers()
                    .Where(vm => vm.Options.MemberType is Core.AggregateMemberTypes.SequenceMember
                            && vm.DeclaringAggregate == vm.Owner)
                    .ToArray();
                var children = entity.Aggregate
                    .GetMembers()
                    .OfType<AggregateMember.RelationMember>()
                    .Where(rm => rm is AggregateMember.Child
                              || rm is AggregateMember.Children
                              || rm is AggregateMember.VariationItem);

                if (entity.Aggregate.IsChildrenMember()) {
                    var depth = entity.Aggregate.EnumerateAncestors().Count();
                    var x = depth == 1 ? "x" : $"x{depth - 1}";

                    return $$"""
                        foreach (var {{x}} in {{instance}}.{{entity.Aggregate.GetFullPathAsDbEntity(since: instanceAggregate).Join("?.")}} ?? []) {
                        {{sequences.SelectTextTemplate(vm => $$"""
                            // {{vm.DisplayName}}
                            if ({{x}}.{{vm.GetFullPathAsDbEntity(since: entity.Aggregate).Join("?.")}} == null) {
                                cmd.CommandText = $"SELECT \"{{vm.Options.SeqName}}\".nextval FROM DUAL";
                                {{x}}.{{vm.GetFullPathAsDbEntity(since: entity.Aggregate).Join(".")}} = Convert.ToInt32(cmd.ExecuteScalar())!;
                            }
                        """)}}

                        {{children.SelectTextTemplate(c => $$"""
                            {{WithIndent(RenderAggregate(new EFCoreEntity(c.MemberAggregate), x, entity.Aggregate), "    ")}}
                        """)}}
                        }
                        """;

                } else {
                    return $$"""
                        {{sequences.SelectTextTemplate(vm => $$"""
                        // {{vm.DisplayName}}
                        if ({{instance}}.{{vm.GetFullPathAsDbEntity(since: instanceAggregate).Join("?.")}} == null) {
                            cmd.CommandText = $"SELECT \"{{vm.Options.SeqName}}\".nextval FROM DUAL";
                            {{instance}}.{{vm.GetFullPathAsDbEntity(since: instanceAggregate).Join(".")}} = Convert.ToInt32(cmd.ExecuteScalar())!;
                        }
                        """)}}
                        {{children.SelectTextTemplate(children => $$"""
                        {{WithIndent(RenderAggregate(new EFCoreEntity(children.MemberAggregate), instance, instanceAggregate), "")}}
                        """)}}
                        """;
                }
            }
        }

    }
}
