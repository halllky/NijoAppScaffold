using Nijo.CodeGenerating;
using Nijo.Parts.Common;
using System.Collections.Generic;
using System.Linq;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures;

namespace Nijo.DataStructures.IsStored {
    /// <summary>
    /// 必須入力チェック
    /// </summary>
    internal class ValidateRequired : DbContextHookValidator {

        internal const string METHOD_NAME = "ValidateRequired";

        public override string CommentName => "必須入力チェック";
        public override string MethodName => METHOD_NAME;

        internal override IEnumerable<string> RenderHookDeclarations(CodeRenderingContext ctx) {
            yield return $$"""
                /// <summary>
                /// 必須入力チェック。属性 IsKey か IsNotNull が付いた項目の登録・更新時に呼ばれる。
                /// null を返すとチェックOK、文字列を返すとその内容でエラー登録されます。
                /// </summary>
                /// <param name="value">チェック対象の値。値オブジェクトは文字列に変換して渡される</param>
                /// <param name="displayName">項目の表示名</param>
                /// <returns>エラーの場合はメッセージ文字列、正常なら null</returns>
                public abstract string? {{METHOD_NAME}}(object? value, string displayName);
                """;
        }

        protected override ValidateStatement? GetIfStatement(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) {
            if (!vm.IsKey && !vm.IsNotNull) return null;

            return CallHooks([
                RenderHookCall(METHOD_NAME, RenderPrimitiveValue(prop), ToLiteral(vm.DisplayName)),
            ]);
        }

        protected override ValidateStatement? GetIfStatement(RefToMember refTo, IEnumerable<IInstanceProperty> keyProps, CodeRenderingContext ctx) {
            if (!refTo.IsKey && !refTo.IsNotNull) return null;

            return CallHooks(keyProps.Select(prop =>
                RenderHookCall(METHOD_NAME, RenderPrimitiveValue(prop, "?."), ToLiteral(refTo.DisplayName))));
        }
    }
}
