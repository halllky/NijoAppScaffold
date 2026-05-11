using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.Common;
using System;

namespace Nijo.Models.DataModelModules {
    /// <summary>
    /// 数値項目の桁数チェック
    /// </summary>
    internal class ValidateDigitsAndScales : ValidatorBase {

        internal const string METHOD_NAME = "ValidateDigitsAndScales";
        private const string MSG_ID_DIGITS = "DigitsError";

        public override string CommentName => "数値項目の桁数チェック";
        public override string MethodName => METHOD_NAME;
        public override string MsgId => MSG_ID_DIGITS;
        public override string MsgTemplate => "{0} は整数部 {1} 桁、小数部 {2} 桁以内で入力してください。";

        protected override ValidateStatement? GetIfStatement(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) {
            if (vm.TotalDigit == null) return null;

            var path = prop.GetJoinedPathFromInstance(E_CsTs.CSharp);

            if (vm.Type.CsPrimitiveTypeName == "int" || vm.Type.CsPrimitiveTypeName == "long") {
                var maxInt = 1L;
                for (var i = 0; i < vm.TotalDigit.Value; i++) maxInt *= 10L;

                return new ValidateStatement {
                    If = $$"""
                        {{path}} != null && Math.Abs((long){{path}}.Value) >= {{maxInt}}
                        """,
                    RenderErrorMessage = $$"""
                        {{MsgFactory.MSG}}.{{MSG_ID_DIGITS}}("{{vm.DisplayName.Replace("\"", "\\\"")}}", "{{vm.TotalDigit}}", "0")
                        """,
                };

            } else if (vm.Type.CsPrimitiveTypeName == "decimal") {
                var integerDigits = vm.TotalDigit.Value - (vm.DecimalPlace ?? 0);
                var scaleDigits = vm.DecimalPlace ?? 0;

                // 整数部チェック
                // 10^n を計算しておく
                var maxInt = 1m;
                for (var i = 0; i < integerDigits; i++) maxInt *= 10m;

                // 小数部チェック
                // 10^n を計算しておく
                var scaleMulti = 1m;
                for (var i = 0; i < scaleDigits; i++) scaleMulti *= 10m;

                return new ValidateStatement {
                    If = $$"""
                        {{path}} != null && (
                            Math.Abs(Math.Truncate({{path}}.Value)) >= {{maxInt}}m ||
                            {{path}}.Value * {{scaleMulti}}m % 1 != 0)
                        """,
                    RenderErrorMessage = $$"""
                        {{MsgFactory.MSG}}.{{MSG_ID_DIGITS}}("{{vm.DisplayName.Replace("\"", "\\\"")}}", "{{integerDigits}}", "{{scaleDigits}}")
                        """,
                };
            } else {
                throw new InvalidOperationException($"ValidateDigitsAndScales cannot be applied to type {vm.Type.CsPrimitiveTypeName}.");
            }
        }
    }
}
