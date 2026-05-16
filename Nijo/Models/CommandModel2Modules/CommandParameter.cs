using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.QueryModelModules;
using Nijo.Models.ReadModel2Modules;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;

using ReadModel2DisplayData = Nijo.Models.ReadModel2Modules.DisplayData;

namespace Nijo.Models.CommandModel2Modules {
    /// <summary>
    /// 旧版互換 CommandModel2 のパラメータ型定義。
    /// まずは parameter の C# / TypeScript 型と新規作成関数を復旧する。
    /// </summary>
    internal class CommandParameter {
        internal CommandParameter(AggregateBase aggregate) {
            Aggregate = aggregate;
        }

        internal AggregateBase Aggregate { get; }

        internal string CsClassName => $"{Aggregate.PhysicalName}Parameter{GetUniqueIdSuffix()}";
        internal string TsTypeName => $"{Aggregate.PhysicalName}Parameter{GetUniqueIdSuffix()}";
        internal string MessageDataCsClassName => $"{Aggregate.PhysicalName}ParameterMessages{GetUniqueIdSuffix()}";

        private string GetUniqueIdSuffix() {
            if (Aggregate is RootAggregate) return string.Empty;

            var uniqueId = Aggregate.GetRoot().XElement.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)?.Value;
            return string.IsNullOrWhiteSpace(uniqueId)
                ? $"_{Aggregate.GetRoot().PhysicalName}"
                : $"_{uniqueId.Substring(0, Math.Min(8, uniqueId.Length))}";
        }

        private IEnumerable<CommandParameter> EnumerateThisAndDescendants() {
            yield return this;

            foreach (var descendant in Aggregate.EnumerateDescendants()) {
                yield return new CommandParameter(descendant);
            }
        }

        private IEnumerable<Member> GetOwnMembers() {
            foreach (var member in Aggregate.GetMembers()) {
                yield return new Member(member);
            }
        }

        internal string RenderCSharpDeclaring(CodeRenderingContext ctx) {
            return EnumerateThisAndDescendants().SelectTextTemplate(param => $$"""
                /// <summary>
                /// {{param.Aggregate.GetRoot().DisplayName}}処理のパラメータ{{If(param.Aggregate is RootAggregate, () => string.Empty).Else(() => "の一部")}}
                /// </summary>
                public partial class {{param.CsClassName}} {
                {{If(param.Aggregate is ChildrenAggregate, () => $$"""
                    /// <summary>
                    /// 画面操作で使用される一意なID。このインスタンスが作成されたときに発番される。
                    /// このIDは永続化の対象とならない。
                    /// </summary>
                    [JsonPropertyName("{{ReadModel2DisplayData.UNIQUE_ID_TS}}")]
                    public required string {{ReadModel2DisplayData.UNIQUE_ID_CS}} { get; set; }
                """)}}
                {{param.GetOwnMembers().SelectTextTemplate(member => $$"""
                    public virtual {{member.GetCsTypeName()}}? {{member.PhysicalName}} { get; set; }
                """)}}
                }
                """);
        }

        internal string RenderTsDeclaring(CodeRenderingContext ctx) {
            return EnumerateThisAndDescendants().SelectTextTemplate(param => {
                var commentSuffix = param.Aggregate is RootAggregate ? string.Empty : "の一部";

                return $$"""
                /** {{param.Aggregate.GetRoot().DisplayName}}処理のパラメータ{{commentSuffix}} */
                export type {{param.TsTypeName}} = {
                {{If(param.Aggregate is ChildrenAggregate, () => $$"""
                  {{ReadModel2DisplayData.UNIQUE_ID_TS}}: string
                """)}}
                {{param.GetOwnMembers().SelectTextTemplate(member => $$"""
                  {{member.PhysicalName}}?: {{member.GetTsTypeName()}}
                """)}}
                }
                """;
            });
        }

        internal string RenderTsNewObjectFunction(CodeRenderingContext ctx) {
            var creatableAggregates = EnumerateThisAndDescendants()
                .Where(param => param.Aggregate is RootAggregate or ChildrenAggregate)
                .ToArray();

            return creatableAggregates.SelectTextTemplate(param => $$"""
                export const createNew{{param.TsTypeName}} = (): {{param.TsTypeName}} => ({
                {{If(param.Aggregate is ChildrenAggregate, () => $$"""
                  {{ReadModel2DisplayData.UNIQUE_ID_TS}}: UUID.generate(),
                """)}}
                {{param.GetOwnMembers().SelectTextTemplate(member => $$"""
                  {{member.PhysicalName}}: {{WithIndent(member.RenderTsInitializer(), "  ")}},
                """)}}
                })
                """);
        }

        internal string RenderCSharpMessageClassDeclaring(CodeRenderingContext ctx) {
            return EnumerateThisAndDescendants().SelectTextTemplate(param => param.RenderSingleCSharpMessageClass()).TrimEnd();
        }

        private string RenderSingleCSharpMessageClass() {
            var members = GetMessageMembers().ToArray();
            var isRoot = Aggregate is RootAggregate;
            var isInGrid = IsInGrid(Aggregate);
            var commentSuffix = isRoot ? string.Empty : "の一部";
            var messageBaseClass = isInGrid ? "DisplayMessageContainerInGrid" : "DisplayMessageContainerBase";
            var messagePathCtorArgs = isInGrid
                ? "IEnumerable<string> path, DisplayMessageContainerBase grid, int rowIndex"
                : isRoot
                ? string.Empty
                : "IEnumerable<string> path";
            var messagePathBaseCall = isInGrid
                ? "base(path, grid, rowIndex)"
                : isRoot
                ? "base([])"
                : "base(path)";

            return $$"""
                /// <summary>
                /// {{Aggregate.GetRoot().DisplayName}}処理のパラメータ{{commentSuffix}}のメッセージ格納用クラス
                /// </summary>
                public partial class {{MessageDataCsClassName}} : {{messageBaseClass}} {
                    public {{MessageDataCsClassName}}({{messagePathCtorArgs}}) : {{messagePathBaseCall}} {
                {{members.SelectTextTemplate(member => $$"""
                        {{WithIndent(member.RenderPathConstructor(isRoot, isInGrid), "        ")}}
                """)}}
                    }
                    /// <summary>すべてのメッセージを画面ルートに転送する場合に用いられるコンストラクタ</summary>
                    public {{MessageDataCsClassName}}(IDisplayMessageContainer origin) : base(origin) {
                {{members.SelectTextTemplate(member => $$"""
                        {{WithIndent(member.RenderOriginConstructor(), "        ")}}
                """)}}
                    }

                {{members.SelectTextTemplate(member => $$"""
                    public virtual {{member.TypeName}} {{member.PropertyName}} { get; }
                """)}}

                    public override IEnumerable<IDisplayMessageContainer> EnumerateChildren() {
                {{members.SelectTextTemplate(member => $$"""
                        yield return {{member.PropertyName}};
                """)}}
                    }
                }
                """.TrimEnd();
        }

        private static bool IsInGrid(AggregateBase aggregate) {
            return aggregate.EnumerateThisAndAncestors().Any(agg => agg is ChildrenAggregate);
        }

        private IEnumerable<LegacyMessageMember> GetMessageMembers() {
            foreach (var member in GetOwnMembers()) {
                switch (member.Metadata) {
                    case ValueMember:
                    case RefToMember:
                        yield return new LegacyMessageMember(member.PhysicalName, "IDisplayMessageContainer", IsValueMember: true);
                        break;
                    case ChildAggregate child:
                        yield return new LegacyMessageMember(member.PhysicalName, new CommandParameter(child).MessageDataCsClassName, IsValueMember: false);
                        break;
                    case ChildrenAggregate children:
                        yield return new LegacyMessageMember(member.PhysicalName, $"DisplayMessageContainerList<{new CommandParameter(children).MessageDataCsClassName}>", IsValueMember: false);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        internal class Member {
            internal Member(IAggregateMember member) {
                Metadata = member;
            }

            internal IAggregateMember Metadata { get; }
            internal string PhysicalName => Metadata.PhysicalName;

            internal string GetCsTypeName() {
                return Metadata switch {
                    ValueMember valueMember => valueMember.Type.CsDomainTypeName,
                    ChildAggregate child => new CommandParameter(child).CsClassName,
                    ChildrenAggregate children => $"List<{new CommandParameter(children).CsClassName}>",
                    RefToMember refTo => new DisplayDataRef.Entry(refTo.RefTo.AsEntry()).CsClassName,
                    _ => throw new NotImplementedException(),
                };
            }

            internal string GetTsTypeName() {
                return Metadata switch {
                    ValueMember valueMember => valueMember.Type.TsTypeName,
                    ChildAggregate child => new CommandParameter(child).TsTypeName,
                    ChildrenAggregate children => $"{new CommandParameter(children).TsTypeName}[]",
                    RefToMember refTo => new DisplayDataRef.Entry(refTo.RefTo.AsEntry()).TsTypeName,
                    _ => throw new NotImplementedException(),
                };
            }

            internal string RenderTsInitializer() {
                return Metadata switch {
                    ValueMember => "undefined",
                    ChildAggregate child => $$"""
                        {
                        {{WithIndent(new CommandParameter(child).GetOwnMembers().SelectTextTemplate(member => $$"""
                          {{member.PhysicalName}}: {{WithIndent(member.RenderTsInitializer(), "  ")}},
                        """), "  ")}}
                        }
                        """,
                    ChildrenAggregate => "[]",
                    RefToMember refTo => $"{new DisplayDataRef.Entry(refTo.RefTo.AsEntry()).TsNewObjectFunction}()",
                    _ => throw new NotImplementedException(),
                };
            }
        }

        private readonly record struct LegacyMessageMember(string PropertyName, string TypeName, bool IsValueMember) {
            internal string RenderPathConstructor(bool isRoot, bool isInGrid) {
                var path = isRoot
                    ? $"[\"{PropertyName}\"]"
                    : $"[.. path, \"{PropertyName}\"]";

                if (IsValueMember) {
                    return isInGrid
                        ? $"{PropertyName} = new DisplayMessageContainerInGrid({path}, grid, rowIndex);"
                        : $"{PropertyName} = new DisplayMessageContainer({path});";
                }

                if (TypeName.StartsWith("DisplayMessageContainerList<", StringComparison.Ordinal)) {
                    var itemType = TypeName["DisplayMessageContainerList<".Length..^1];
                    var childPath = isRoot
                        ? $"[\"{PropertyName}\", rowIndex.ToString()]"
                        : $"[.. path, \"{PropertyName}\", rowIndex.ToString()]";
                    return $$"""
                        {{PropertyName}} = new({{path}}, rowIndex => {
                            return new {{itemType}}({{childPath}}, {{PropertyName}}!, rowIndex);
                        });
                        """;
                }

                return $"{PropertyName} = new {TypeName}({path});";
            }

            internal string RenderOriginConstructor() {
                if (IsValueMember) {
                    return $"{PropertyName} = origin;";
                }

                if (TypeName.StartsWith("DisplayMessageContainerList<", StringComparison.Ordinal)) {
                    var itemType = TypeName["DisplayMessageContainerList<".Length..^1];
                    return $$"""
                        {{PropertyName}} = new(origin, rowIndex => {
                            return new {{itemType}}(origin);
                        });
                        """;
                }

                return $"{PropertyName} = new {TypeName}(origin);";
            }
        }
    }
}
