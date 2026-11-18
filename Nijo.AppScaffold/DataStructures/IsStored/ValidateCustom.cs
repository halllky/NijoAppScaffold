using Nijo.CodeGenerating;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures;

namespace Nijo.DataStructures.IsStored {
    /// <summary>
    /// カスタム属性を用いた入力検証。
    /// IsValidation=true のカスタム属性が付与された項目に対して検証フックを生成する。
    /// </summary>
    internal class ValidateCustom : DbContextHookValidator {

        internal const string MSG_ID = "CustomAttributeValidationError";

        private readonly NijoXmlCustomAttribute _attribute;

        internal ValidateCustom(NijoXmlCustomAttribute attribute) {
            _attribute = attribute;
        }

        public override string CommentName => "カスタム属性バリデーション";
        public override string MethodName => $"Validate{_attribute.PhysicalName}";
        public override string MsgId => MSG_ID;
        public override string MsgTemplate => "{0}";

        /// <summary>
        /// このカスタム属性が指定された項目の型ごとに、フックを1つずつ宣言する。
        /// これを1回オーバーライドすれば、このカスタム属性が指定されたすべての集約のコマンドの検証で使われる。
        /// 例えば数値が0以上であることを保証するカスタム属性「NotNegative」が int と decimal の
        /// メンバーに指定されていた場合、引数の型が int の ValidateNotNegative(int? value, ...) と
        /// 引数の型が decimal の ValidateNotNegative(decimal? value, ...) の2つのメソッドが生成される。
        /// </summary>
        internal override IEnumerable<string> RenderHookDeclarations(CodeRenderingContext ctx) {
            var domainTypeNames = ctx.Schema
                .GetDataStructures()
                .Where(agg => agg.IsStored)
                .SelectMany(agg => agg.EnumerateThisAndDescendants())
                .SelectMany(agg => agg.GetMembers())
                .OfType<ValueMember>()
                .Where(vm => vm.XElement.Attribute(_attribute.UniqueId!) != null)
                .Select(vm => vm.Type.CsDomainTypeName)
                .Distinct();

            foreach (var domainTypeName in domainTypeNames) {
                var parameters = _attribute.Type == NijoXmlCustomAttribute.E_Type.Boolean
                        ? $"{domainTypeName}? value, System.Reflection.PropertyInfo propertyInfo"
                        : $"{domainTypeName}? value, string attributeValue, System.Reflection.PropertyInfo propertyInfo";

                yield return $$"""
                    /// <summary>
                    /// カスタム属性 '{{_attribute.PhysicalName}}' 用の入力検証フック。
                    /// null を返すとバリデーションOK、文字列を返すとその内容でエラー登録されます。
                    /// </summary>
                    /// <param name="value">検証対象の値</param>
                    {{If(_attribute.Type != NijoXmlCustomAttribute.E_Type.Boolean, () => $$"""
                    /// <param name="attributeValue">カスタム属性に設定された値。</param>
                    """)}}
                    /// <param name="propertyInfo">検証対象プロパティの <see cref="System.Reflection.PropertyInfo"/>。</param>
                    /// <returns>エラーの場合はメッセージ文字列、正常なら null</returns>
                    public abstract string? {{MethodName}}({{parameters}});
                    """;
            }
        }

        protected override ValidateStatement? GetIfStatement(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) => null;
        protected override ValidateStatement? GetIfStatement(RefToMember refTo, IEnumerable<IInstanceProperty> keyProps, CodeRenderingContext ctx) => null;

        protected override IEnumerable<ValidateStatement> GetIfStatements(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) {
            var attrNode = vm.XElement.Attribute(_attribute.UniqueId!);
            if (attrNode == null) yield break;

            var declaringTypeName = $"{vm.Owner.PhysicalName}DbEntity";
            var propertyInfoExpr = $"typeof({declaringTypeName}).GetProperty(\"{prop.Metadata.GetPropertyName(E_CsTs.CSharp)}\")!";
            var valueExpr = prop.GetJoinedPathFromInstance(E_CsTs.CSharp);

            if (_attribute.Type == NijoXmlCustomAttribute.E_Type.Boolean) {
                var raw = attrNode.Value;
                var enabled = !string.IsNullOrWhiteSpace(raw)
                    && (bool.TryParse(raw, out var parsed) && parsed || raw == "1");
                if (!enabled) yield break;

                yield return CallHooks([RenderHookCall(MethodName, valueExpr, propertyInfoExpr)]);
            } else {
                var attrValue = attrNode.Value;

                // 値が空の場合はスキーマ定義の検証でエラーになるので、ここには来ない
                if (string.IsNullOrWhiteSpace(attrValue)) yield break;

                yield return CallHooks([RenderHookCall(MethodName, valueExpr, ToLiteral(attrValue), propertyInfoExpr)]);
            }
        }
    }
}
