using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;

namespace Nijo.Models.DataModelModules {
    /// <summary>
    /// 文字種チェック
    /// </summary>
    internal class ValidateCharacterType : ValidatorBase {

        internal const string METHOD_NAME = "ValidateCharacterType";
        private const string MSG_ID = "CharacterTypeError";

        public override string CommentName => "文字種チェック";
        public override string MethodName => METHOD_NAME;
        public override string MsgId => MSG_ID;
        public override string MsgTemplate => "{0}で入力してください。";

        protected override ValidateStatement? GetIfStatement(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) {
            if (string.IsNullOrWhiteSpace(vm.CharacterType)) return null;

            // ヘルパーメソッドの登録（重複防止付き）
            ctx.Use<Helper>().Register(vm.CharacterType, ctx);

            var path = prop.GetJoinedPathFromInstance(E_CsTs.CSharp);
            var methodName = Helper.GetMethodName(vm.CharacterType);

            return new ValidateStatement {
                If = $$"""
                    !{{methodName}}({{path}})
                    """,

                RenderErrorMessage = $$"""
                    {{MsgFactory.MSG}}.{{MSG_ID}}("{{vm.CharacterType}}")
                    """,
            };
        }

        /// <summary>
        /// ApplicationConfigureにメソッドを追加するためのヘルパークラス
        /// </summary>
        internal class Helper : IMultiAggregateSourceFile {
            private readonly HashSet<string> _registeredTypes = new();

            internal static string GetMethodName(string characterType) {
                // メソッド名として使えない文字を除外
                return "ValidateIf" + characterType
                    .Replace(" ", "")
                    .Replace("　", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace("（", "")
                    .Replace("）", "")
                    .Replace("-", "")
                    .Replace("ー", "");
            }

            public void Register(string characterType, CodeRenderingContext ctx) {
                if (_registeredTypes.Contains(characterType)) return;
                _registeredTypes.Add(characterType);

                var methodName = GetMethodName(characterType);
                ctx.Use<ApplicationService>().Add($$"""
                    /// <summary>
                    /// 文字種チェック（{{characterType}}）。
                    /// エラー（不正な文字種）の場合は false を返します。
                    /// </summary>
                    public abstract bool {{methodName}}(string? value);
                    """);
            }

            public void RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
                // 何もしない
            }

            public void Render(CodeRenderingContext ctx) {
                // 何もしない
            }
        }
    }
}
