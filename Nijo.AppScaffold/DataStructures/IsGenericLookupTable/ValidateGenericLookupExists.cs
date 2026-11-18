using Nijo.CodeGenerating;
using Nijo.Parts.Common;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Nijo.DataStructures.IsStored;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures;

namespace Nijo.DataStructures.IsGenericLookupTable {
    /// <summary>
    /// 汎用参照テーブルへの参照で、指定された区分値が存在するかを検証する。
    /// </summary>
    internal class ValidateGenericLookupExists : ValidatorBase {

        private const string MSG_ID_LOOKUP_NOT_FOUND = "GenericLookupValueNotFound";

        public override string CommentName => "汎用参照テーブル存在チェック";
        public override string MethodName => "ValidateGenericLookupExists";
        public override string MsgId => MSG_ID_LOOKUP_NOT_FOUND;
        public override string MsgTemplate => "{0} に指定された区分値が存在しません。";

        /// <summary>エラーの文言を返す、DbContext のフックの名前</summary>
        private const string MESSAGE_HOOK = "GetGenericLookupValueNotFoundMessage";

        /// <summary>
        /// エラーの文言を手書きの DbContext に任せるフック。
        /// 手書きのコードがオーバーライドを常に置いておけるよう、汎用参照テーブルが無くても宣言する。
        /// </summary>
        internal override IEnumerable<string> RenderHookDeclarations(CodeRenderingContext ctx) {
            yield return $$"""
                /// <summary>
                /// 汎用参照テーブルを参照する項目に、テーブルに存在しない区分値が指定されたときに、利用者に見せるメッセージを返します。
                /// </summary>
                /// <param name="displayName">項目の表示名</param>
                public abstract string {{MESSAGE_HOOK}}(string displayName);
                """;
        }

        protected override ValidateStatement? GetIfStatement(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) => null;

        protected override ValidateStatement? GetIfStatement(RefToMember refTo, IEnumerable<IInstanceProperty> keyProps, CodeRenderingContext ctx) {
            if (!GenericLookupRefToInfo.TryCreate(refTo, out var info)) return null;

            var keyPropByMember = keyProps.ToDictionary(
                prop => ((SaveCommand.SaveCommandValueMember)prop.Metadata).Member,
                prop => prop);

            var keyConditions = info.NonHardCodedKeyMembers
                .Select(member => {
                    if (!keyPropByMember.TryGetValue(member, out var prop)) {
                        throw new InvalidOperationException($"キー項目が見つかりません: {member.PhysicalName}");
                    }
                    return $"candidate.{member.PhysicalName} == {RenderPrimitiveValue(prop, "?.")}";
                })
                .ToArray();

            var hasAnyValueExpression = info.NonHardCodedKeyMembers
                .Select(member => {
                    if (!keyPropByMember.TryGetValue(member, out var prop)) {
                        throw new InvalidOperationException($"キー項目が見つかりません: {member.PhysicalName}");
                    }
                    return RenderHasValue(prop);
                })
                .Join("\r\n|| ");

            var utilExpression = $"{SaveCommand.FIELD_DBCONTEXT}.{info.RootAggregate.PhysicalName}Util.{info.Category.Name}";
            var existsExpression = keyConditions.Length == 0
                ? $"{utilExpression}.Any()"
                : $"{utilExpression}.Any(candidate => {keyConditions.Join("\r\n&& ")})";

            return new ValidateStatement {
                If = $"({hasAnyValueExpression})\r\n&& !({existsExpression})",
                RenderErrorMessage = $$"""
                    {{SaveCommand.FIELD_DBCONTEXT}}.{{MESSAGE_HOOK}}("{{refTo.DisplayName.Replace("\"", "\\\"")}}")
                    """,
            };
        }

        private static string RenderHasValue(IInstanceProperty prop) {
            var path = RenderPrimitiveValue(prop, "?.");
            return prop.Metadata switch {
                IInstanceValuePropertyMetadata vm when vm.Type.CsPrimitiveTypeName == "string"
                    => $"!string.IsNullOrWhiteSpace({path})",
                _ => $"{path} != null",
            };
        }
    }
}
