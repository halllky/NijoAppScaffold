using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.Common;
using System;

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
    }
}
