using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.Common;
using System;

namespace Nijo.Models.DataModelModules {
    /// <summary>
    /// 文字列最大長チェック
    /// </summary>
    internal class ValidateMaxLength : ValidatorBase {

        internal const string METHOD_NAME = "ValidateMaxLength";
        private const string MSG_ID_MAX_LENGTH = "MaxLengthError";

        public override string CommentName => "文字列最大長チェック";
        public override string MethodName => METHOD_NAME;
        public override string MsgId => MSG_ID_MAX_LENGTH;
        public override string MsgTemplate => "{0} は {1} 文字以内で入力してください。";

        protected override ValidateStatement? GetIfStatement(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) {
            if (vm.MaxLength == null) return null;

            var path = prop.GetJoinedPathFromInstance(E_CsTs.CSharp);

            return new ValidateStatement {
                If = $$"""
                    !string.IsNullOrEmpty({{path}})
                    && new System.Globalization.StringInfo({{path}}).LengthInTextElements > {{vm.MaxLength}}
                    """,

                RenderErrorMessage = $$"""
                    {{MsgFactory.MSG}}.{{MSG_ID_MAX_LENGTH}}("{{vm.DisplayName.Replace("\"", "\\\"")}}", "{{vm.MaxLength}}")
                    """,
            };
        }
    }
}
