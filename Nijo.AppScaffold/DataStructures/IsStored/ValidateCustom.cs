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
    internal class ValidateCustom : ValidatorBase {

        internal const string MSG_ID = "CustomAttributeValidationError";

        private readonly NijoXmlCustomAttribute _attribute;

        internal ValidateCustom(NijoXmlCustomAttribute attribute) {
            _attribute = attribute;
        }

        public override string CommentName => "カスタム属性バリデーション";
        public override string MethodName => GetMethodName(_attribute);
        public override string MsgId => MSG_ID;
        public override string MsgTemplate => "{0}";

        protected override IEnumerable<string> RenderMethodHead() {
            yield return $$"""
                string? errorMessage;
                """;
        }

        protected override ValidateStatement? GetIfStatement(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) => null;
        protected override ValidateStatement? GetIfStatement(RefToMember refTo, IEnumerable<IInstanceProperty> keyProps, CodeRenderingContext ctx) => null;

        protected override IEnumerable<ValidateStatement> GetIfStatements(ValueMember vm, IInstanceProperty prop, CodeRenderingContext ctx) {
            if (string.IsNullOrWhiteSpace(_attribute.UniqueId) || string.IsNullOrWhiteSpace(_attribute.PhysicalName)) yield break;

            var attrNode = vm.XElement.Attribute(_attribute.UniqueId);
            if (attrNode == null) yield break;

            var declaringTypeName = $"{vm.Owner.PhysicalName}DbEntity";
            var propertyInfoExpr = $"typeof({declaringTypeName}).GetProperty(\"{prop.Metadata.GetPropertyName(E_CsTs.CSharp)}\")!";
            var valueExpr = prop.GetJoinedPathFromInstance(E_CsTs.CSharp);
            var methodName = $"{SaveCommand.FIELD_DBCONTEXT}.{GetMethodName(_attribute)}";

            if (_attribute.Type == NijoXmlCustomAttribute.E_Type.Boolean) {
                var raw = attrNode.Value;
                var enabled = !string.IsNullOrWhiteSpace(raw)
                    && (bool.TryParse(raw, out var parsed) && parsed || raw == "1");
                if (!enabled) yield break;

                yield return new ValidateStatement {
                    IfFactory = _ => $"(errorMessage = {methodName}({valueExpr}, {propertyInfoExpr})) is not null",
                    RenderErrorMessage = "errorMessage",
                };
            } else {
                var attrValue = attrNode.Value;
                var attrDisplayName = _attribute.DisplayName ?? _attribute.PhysicalName ?? _attribute.UniqueId ?? string.Empty;

                if (string.IsNullOrWhiteSpace(attrValue)) {
                    yield return new ValidateStatement {
                        If = "true",
                        RenderErrorMessage = $$"""
                            {{MsgFactory.MSG}}.{{MSG_ID}}("{{vm.DisplayName.Replace("\"", "\\\"")}} のカスタム属性 '{{attrDisplayName.Replace("\"", "\\\"")}}' が設定されていません。")
                            """,
                    };
                    yield break;
                }

                var attrValueLiteral = ToLiteral(attrValue);
                yield return new ValidateStatement {
                    IfFactory = _ => $"(errorMessage = {methodName}({valueExpr}, {attrValueLiteral}, {propertyInfoExpr})) is not null",
                    RenderErrorMessage = "errorMessage",
                };
            }
        }

        /// <summary>
        /// DbContext に abstract メソッドを登録する。
        /// これを1回オーバーライドすれば、このカスタム属性が指定されたすべての集約のコマンドの検証で使われる。
        /// メソッドはこのカスタムバリデータが指定されている ValueMember の種類ごとに作成される。
        /// 例えば数値が0以上であることを保証するカスタム属性「NotNegative」が int と decimal の
        /// メンバーに指定されていた場合、引数の型が int の ValidateNotNegative(int? value, ...) と
        /// 引数の型が decimal の ValidateNotNegative(decimal? value, ...) の2つのメソッドが生成される。
        /// </summary>
        internal static void RegisterDbContextHooks(CodeRenderingContext ctx) {

            // nijo.xml で定義されているカスタム属性の一覧
            var customAttributes = ctx.Schema.CustomAttributes
                .Where(attr => attr.IsValidation
                            && !string.IsNullOrWhiteSpace(attr.UniqueId)
                            && !string.IsNullOrWhiteSpace(attr.PhysicalName))
                .ToDictionary(attr => attr.UniqueId!);

            // 上記カスタム属性を利用している ValueMember を型ごとにグルーピング
            var grouped = ctx.Schema
                .GetDataStructures()
                .Where(agg => agg.IsStored)
                .SelectMany(agg => agg.EnumerateThisAndDescendants())
                .SelectMany(agg => agg.GetMembers())
                .OfType<ValueMember>()
                .SelectMany(vm => vm.XElement.Attributes()
                    .Where(attr => customAttributes.ContainsKey(attr.Name.LocalName))
                    .Select(attr => new { ValueMember = vm, Attribute = customAttributes[attr.Name.LocalName] }))
                .GroupBy(x => new {
                    x.Attribute.UniqueId,
                    x.ValueMember.Type.CsDomainTypeName,
                })
                .Select(group => new {
                    group.First().Attribute,
                    group.Key.CsDomainTypeName,
                });

            var dbContext = ctx.Use<DbContextClass>();
            foreach (var item in grouped) {
                var methodName = GetMethodName(item.Attribute);
                var parameters = item.Attribute.Type == NijoXmlCustomAttribute.E_Type.Boolean
                        ? $"{item.CsDomainTypeName}? value, System.Reflection.PropertyInfo propertyInfo"
                        : $"{item.CsDomainTypeName}? value, string attributeValue, System.Reflection.PropertyInfo propertyInfo";

                dbContext.AddAdditionalMethod($$"""
                    /// <summary>
                    /// カスタム属性 '{{item.Attribute.PhysicalName}}' 用の入力検証フック。
                    /// null を返すとバリデーションOK、文字列を返すとその内容でエラー登録されます。
                    /// </summary>
                    /// <param name="value">検証対象の値</param>
                    {{If(item.Attribute.Type != NijoXmlCustomAttribute.E_Type.Boolean, () => $$"""
                    /// <param name="attributeValue">カスタム属性に設定された値。</param>
                    """)}}
                    /// <param name="propertyInfo">検証対象プロパティの <see cref="System.Reflection.PropertyInfo"/>。</param>
                    /// <returns>エラーの場合はメッセージ文字列、正常なら null</returns>
                    public abstract string? {{methodName}}({{parameters}});
                    """);
            }
        }

        private static string GetMethodName(NijoXmlCustomAttribute attr) => $"Validate{attr.PhysicalName}";

        private static string ToLiteral(string? value) {
            if (value == null) return "null";
            return $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
        }
    }
}
