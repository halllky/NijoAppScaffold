using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.Common;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.DataModelModules {
    /// <summary>
    /// 必須入力チェック
    /// </summary>
    internal class ValidateRequired : ValidatorBase {

        internal const string METHOD_NAME = "ValidateRequired";
        private const string MSG_ID_REQUIRED = "RequiredError";

        public override string CommentName => "必須入力チェック";
        public override string MethodName => METHOD_NAME;
        public override string MsgId => MSG_ID_REQUIRED;
        public override string MsgTemplate => "{0} を入力してください。";

        protected override ValidateStatement? GetIfStatement(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) {
            if (!vm.IsKey && !vm.IsRequired) return null;

            return new ValidateStatement {

                If = vm.Type.CsPrimitiveTypeName == "string"
                    ? $"string.IsNullOrWhiteSpace({prop.GetJoinedPathFromInstance(E_CsTs.CSharp)})"
                    : $"{prop.GetJoinedPathFromInstance(E_CsTs.CSharp)} == null",

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
    }
}
