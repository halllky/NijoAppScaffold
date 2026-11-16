using Nijo.CodeGenerating;
using System;
using System.Collections.Generic;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures;

namespace Nijo.DataStructures.IsStored {
    /// <summary>
    /// 数値項目の桁数チェック
    /// </summary>
    internal class ValidateDigitsAndScales : DbContextHookValidator {

        internal const string METHOD_NAME = "ValidateDigitsAndScales";
        private const string MSG_ID_DIGITS = "DigitsError";

        public override string CommentName => "数値項目の桁数チェック";
        public override string MethodName => METHOD_NAME;
        public override string MsgId => MSG_ID_DIGITS;
        public override string MsgTemplate => "{0} は整数部 {1} 桁、小数部 {2} 桁以内で入力してください。";

        internal override IEnumerable<string> RenderHookDeclarations(CodeRenderingContext ctx) {
            yield return $$"""
                /// <summary>
                /// 数値項目の桁数チェック。属性 TotalDigit が付いた項目の登録・更新時に呼ばれる。
                /// null を返すとチェックOK、文字列を返すとその内容でエラー登録されます。
                /// </summary>
                /// <param name="value">チェック対象の値。整数の項目も decimal に変換して渡される</param>
                /// <param name="totalDigit">属性 TotalDigit の値（整数部と小数部を合わせた桁数）</param>
                /// <param name="decimalPlace">属性 DecimalPlace の値（小数部の桁数）。整数の項目と、指定がない項目は 0</param>
                /// <param name="displayName">項目の表示名</param>
                /// <returns>エラーの場合はメッセージ文字列、正常なら null</returns>
                public abstract string? {{METHOD_NAME}}(decimal? value, int totalDigit, int decimalPlace, string displayName);
                """;
        }

        protected override ValidateStatement? GetIfStatement(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) {
            if (vm.TotalDigit == null) return null;

            var decimalPlace = vm.Type.CsPrimitiveTypeName switch {
                "int" or "long" => 0,
                "decimal" => vm.DecimalPlace ?? 0,
                _ => throw new InvalidOperationException($"ValidateDigitsAndScales cannot be applied to type {vm.Type.CsPrimitiveTypeName}."),
            };

            return CallHooks([
                RenderHookCall(METHOD_NAME, RenderPrimitiveValue(prop), vm.TotalDigit.Value.ToString(), decimalPlace.ToString(), ToLiteral(vm.DisplayName)),
            ]);
        }
    }
}
