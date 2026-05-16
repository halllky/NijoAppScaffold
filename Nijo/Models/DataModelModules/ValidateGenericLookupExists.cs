using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.DataModelModules {
    /// <summary>
    /// 汎用参照テーブルへの参照で、指定された区分値が存在するかを検証する。
    /// </summary>
    internal class ValidateGenericLookupExists : ValidatorBase {

        private const string MSG_ID_LOOKUP_NOT_FOUND = "GenericLookupValueNotFound";

        public override string CommentName => "汎用参照テーブル存在チェック";
        public override string MethodName => "ValidateGenericLookupExists";
        public override string MsgId => MSG_ID_LOOKUP_NOT_FOUND;
        public override string MsgTemplate => "{0} に指定された区分値が存在しません。";

        protected override ValidateStatement? GetIfStatement(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) => null;

        protected override ValidateStatement? GetIfStatement(RefToMember refTo, IEnumerable<IInstanceProperty> fkProps, CodeRenderingContext ctx) {
            if (!GenericLookupRefToInfo.TryCreate(refTo, out var info)) return null;

            var fkPropByMember = fkProps
                .Where(prop => prop.Metadata is EFCoreEntity.RefKeyMember)
                .ToDictionary(
                    prop => ((EFCoreEntity.RefKeyMember)prop.Metadata).Member,
                    prop => prop);

            var keyConditions = info.NonHardCodedKeyMembers
                .Select(member => {
                    if (!fkPropByMember.TryGetValue(member, out var prop)) {
                        throw new InvalidOperationException($"外部キー項目が見つかりません: {member.PhysicalName}");
                    }
                    return $"candidate.{member.PhysicalName} == {prop.GetJoinedPathFromInstance(E_CsTs.CSharp)}";
                })
                .ToArray();

            var hasAnyValueExpression = info.NonHardCodedKeyMembers
                .Select(member => {
                    if (!fkPropByMember.TryGetValue(member, out var prop)) {
                        throw new InvalidOperationException($"外部キー項目が見つかりません: {member.PhysicalName}");
                    }
                    return RenderHasValue(prop);
                })
                .Join("\r\n|| ");

            var utilExpression = $"{info.RootAggregate.PhysicalName}Util.{info.Category.DisplayName.ToCSharpSafe()}";
            var existsExpression = keyConditions.Length == 0
                ? $"{utilExpression}.Any()"
                : $"{utilExpression}.Any(candidate => {keyConditions.Join("\r\n&& ")})";

            return new ValidateStatement {
                If = $"({hasAnyValueExpression})\r\n&& !({existsExpression})",
                RenderErrorMessage = $$"""
                    {{MsgFactory.MSG}}.{{MSG_ID_LOOKUP_NOT_FOUND}}("{{refTo.DisplayName.Replace("\"", "\\\"")}}")
                    """,
            };
        }

        private static string RenderHasValue(IInstanceProperty prop) {
            var path = prop.GetJoinedPathFromInstance(E_CsTs.CSharp);
            return prop.Metadata switch {
                IInstanceValuePropertyMetadata vm when vm.Type.CsPrimitiveTypeName == "string"
                    => $"!string.IsNullOrWhiteSpace({path})",
                _ => $"{path} != null",
            };
        }
    }
}
