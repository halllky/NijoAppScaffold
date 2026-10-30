using Nijo.CodeGenerating;
using Nijo.Parts.Common;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures;

namespace Nijo.DataStructures.IsStored {
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
            if (!vm.IsKey && !vm.IsNotNull) return null;

            return new ValidateStatement {
                If = RenderIsEmpty(prop, "."),
                RenderErrorMessage = $$"""
                    {{MsgFactory.MSG}}.{{MSG_ID_REQUIRED}}("{{vm.DisplayName.Replace("\"", "\\\"")}}")
                    """,
            };
        }

        protected override ValidateStatement? GetIfStatement(RefToMember refTo, IEnumerable<IInstanceProperty> keyProps, CodeRenderingContext ctx) {
            if (!refTo.IsKey && !refTo.IsNotNull) return null;

            return new ValidateStatement {
                If = keyProps.Select(prop => RenderIsEmpty(prop, "?.")).Join(" || "),
                RenderErrorMessage = $$"""
                    {{MsgFactory.MSG}}.{{MSG_ID_REQUIRED}}("{{refTo.DisplayName.Replace("\"", "\\\"")}}")
                    """,
            };
        }

        /// <summary>
        /// stringなら IsNullOrWhiteSpace で判定。それ以外は単に == null で判定
        /// </summary>
        private static string RenderIsEmpty(IInstanceProperty prop, string nullableSeparator) {
            return ((IInstanceValuePropertyMetadata)prop.Metadata).Type.CsPrimitiveTypeName == "string"
                ? $"string.IsNullOrWhiteSpace({RenderPrimitiveValue(prop, nullableSeparator)})"
                : $"{prop.GetJoinedPathFromInstance(E_CsTs.CSharp, nullableSeparator)} == null";
        }
    }
}
