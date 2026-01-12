using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.Common;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nijo.Models.DataModelModules {
    /// <summary>
    /// 必須入力チェック
    /// </summary>
    internal class ValidateRequired : ValidatorBase {

        internal const string METHOD_NAME = "ValidateRequired";
        private const string MSG_ID_REQUIRED = "RequiredError";
        private const string IS_CREATE = "isCreate";

        public override string CommentName => "必須入力チェック";
        public override string MethodName => METHOD_NAME;
        public override string MsgId => MSG_ID_REQUIRED;
        public override string MsgTemplate => "{0} を入力してください。";

        /// <summary>
        /// 必須のシーケンス属性が存在するか
        /// </summary>
        private static bool HasRequiredSequence(RootAggregate rootAggregate) {
            return rootAggregate
                .EnumerateThisAndDescendants()
                .SelectMany(agg => agg.GetMembers())
                .Any(m => m is ValueMember vm
                       && vm.Type is ValueMemberTypes.SequenceMember
                       && (vm.IsKey || vm.IsRequired));
        }

        protected override IEnumerable<AdditionalArgs> GetAdditionalMethodArgs(RootAggregate rootAggregate) {
            // シーケンスがある場合、新規登録時にシーケンスのnullチェックを割愛する必要がある
            if (HasRequiredSequence(rootAggregate)) {
                yield return new() {
                    Type = "bool",
                    Name = IS_CREATE,
                    Comment = "新規登録時かどうかを示すフラグ",
                };
            }
        }

        protected override ValidateStatement? GetIfStatement(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) {
            if (!vm.IsKey && !vm.IsRequired) return null;

            var sb = new StringBuilder();

            // シーケンスの場合、新規登録時はnullチェックをスキップする
            if (vm.Type is ValueMemberTypes.SequenceMember) {
                sb.Append($"!{IS_CREATE} && ");
            }

            // stringなら IsNullOrWhiteSpace で判定。それ以外は単に != null で判定
            if (vm.Type.CsPrimitiveTypeName == "string") {
                sb.Append($"string.IsNullOrWhiteSpace({prop.GetJoinedPathFromInstance(E_CsTs.CSharp)})");
            } else {
                sb.Append($"{prop.GetJoinedPathFromInstance(E_CsTs.CSharp)} == null");
            }

            return new ValidateStatement {
                If = sb.ToString(),
                RenderErrorMessage = $$"""
                    {{MsgFactory.MSG}}.{{MSG_ID_REQUIRED}}("{{vm.DisplayName.Replace("\"", "\\\"")}}")
                    """,
            };
        }

        protected override ValidateStatement? GetIfStatement(RefToMember refTo, IEnumerable<IInstanceProperty> fkProps, CodeRenderingContext ctx) {
            if (!refTo.IsKey && !refTo.IsRequired) return null;

            var conditions = fkProps.Select(prop => {
                if (prop.Metadata is IInstanceValuePropertyMetadata vm && vm.Type.CsPrimitiveTypeName == "string") {
                    return $"string.IsNullOrWhiteSpace({prop.GetJoinedPathFromInstance(E_CsTs.CSharp)})";
                } else {
                    return $"{prop.GetJoinedPathFromInstance(E_CsTs.CSharp)} == null";
                }
            });

            return new ValidateStatement {
                If = conditions.Join(" || "),
                RenderErrorMessage = $$"""
                    {{MsgFactory.MSG}}.{{MSG_ID_REQUIRED}}("{{refTo.DisplayName.Replace("\"", "\\\"")}}")
                    """,
            };
        }

        internal override string RenderCaller(object caller, RootAggregate rootAggregate, string dbEntityInstanceName, string messageInstanceName) {
            var hasSequence = HasRequiredSequence(rootAggregate);
            if (hasSequence && caller is CreateMethod) {
                return $$"""
                    {{MethodName}}({{dbEntityInstanceName}}, {{messageInstanceName}}, {{IS_CREATE}}: true)
                    """;
            } else if (hasSequence) {
                return $$"""
                    {{MethodName}}({{dbEntityInstanceName}}, {{messageInstanceName}}, {{IS_CREATE}}: false)
                    """;
            } else {
                return $$"""
                    {{MethodName}}({{dbEntityInstanceName}}, {{messageInstanceName}})
                    """;
            }
        }
    }
}
