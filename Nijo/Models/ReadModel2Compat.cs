using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.Models {
    /// <summary>
    /// 旧版 read-model-2 の互換モデル。
    /// 生成処理は現行 QueryModel を流用し、検証規則のみ旧版相当に寄せる。
    /// </summary>
    internal class ReadModel2Compat : QueryModel, IModel {
        internal new const string NODE_TYPE = "read-model-2";

        string IModel.SchemaName => NODE_TYPE;

        void IModel.Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            foreach (var aggregate in rootAggregateElement
                .DescendantsAndSelf()
                .Where(el => el.Parent?.Parent == el.Document?.Root
                          || el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILDREN)) {
                var hasKey = aggregate.Elements().Any(member => member.Attribute(BasicNodeOptions.IsKey.AttributeName) != null);
                if (!hasKey) {
                    addError(aggregate, $"{aggregate.GetDisplayName()}にキーが1つもありません。");
                }
            }
        }

        void IModel.GenerateCode(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            base.GenerateCode(ctx, rootAggregate);
        }

        void IModel.GenerateCode(CodeRenderingContext ctx) {
            // QueryModel と共通の静的ソースは QueryModel 本体が一度だけ生成する。
        }
    }
}
