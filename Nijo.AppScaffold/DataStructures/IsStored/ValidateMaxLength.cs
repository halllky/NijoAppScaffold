using Nijo.CodeGenerating;
using System.Collections.Generic;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures;

namespace Nijo.DataStructures.IsStored {
    /// <summary>
    /// 文字列最大長チェック
    /// </summary>
    internal class ValidateMaxLength : DbContextHookValidator {

        internal const string METHOD_NAME = "ValidateMaxLength";

        public override string CommentName => "文字列最大長チェック";
        public override string MethodName => METHOD_NAME;

        internal override IEnumerable<string> RenderHookDeclarations(CodeRenderingContext ctx) {
            yield return $$"""
                /// <summary>
                /// 文字列最大長チェック。属性 MaxLength が付いた項目の登録・更新時に呼ばれる。
                /// null を返すとチェックOK、文字列を返すとその内容でエラー登録されます。
                /// </summary>
                /// <param name="value">チェック対象の値。値オブジェクトは文字列に変換して渡される</param>
                /// <param name="maxLength">属性 MaxLength の値</param>
                /// <param name="displayName">項目の表示名</param>
                /// <returns>エラーの場合はメッセージ文字列、正常なら null</returns>
                public abstract string? {{METHOD_NAME}}(string? value, int maxLength, string displayName);
                """;
        }

        protected override ValidateStatement? GetIfStatement(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) {
            if (vm.MaxLength == null) return null;

            return CallHooks([
                RenderHookCall(METHOD_NAME, RenderPrimitiveValue(prop), vm.MaxLength.Value.ToString(), ToLiteral(vm.DisplayName)),
            ]);
        }
    }
}
