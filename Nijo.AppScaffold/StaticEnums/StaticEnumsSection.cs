using Nijo.CodeGenerating;
using Nijo.SchemaParsing;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System;

namespace Nijo.StaticEnums;

/// <summary>
/// StaticEnums セクション。
/// 静的区分は値がソースコード上にハードコードされる区分値で、C#はenumとして、TypeScriptはリテラル型として表れる。
/// </summary>
internal sealed class StaticEnumsSection : Section {
    public override E_Section Kind => E_Section.StaticEnums;

    public override void Validate(IReadOnlyList<XElement> rootAggregateElements, SchemaParseContext context, SchemaValidationErrors errors) {
        foreach (var rootAggregateElement in rootAggregateElements) {
            // key属性は必須。整数かどうかは key 属性の側で検証する
            var keyValues = new HashSet<int>();
            foreach (var valueElement in rootAggregateElement.Elements()) {
                var keyAttr = valueElement.Attribute(StaticEnumKeyOption.ATTRIBUTE_NAME);
                if (keyAttr == null) {
                    errors.Add(valueElement, $"key属性が定義されていません。整数値を指定してください。");
                    continue;
                }
                if (int.TryParse(keyAttr.Value, out var keyValue) && !keyValues.Add(keyValue)) {
                    errors.Add(valueElement, $"key値「{keyValue}」は既に他の値で使用されています。重複しない値を指定してください。");
                }
            }
        }
    }

    public override void Render(CodeRenderingContext ctx) {
        RenderEach(ctx, ctx.Schema.GetStaticEnums(), rootAggregate => RenderRootAggregate(ctx, rootAggregate));

        // 静的区分が1個も無くても、区分値の定義ファイルは生成する
        ctx.Use<EnumFile>();
    }

    private static void RenderRootAggregate(CodeRenderingContext ctx, StaticEnumRoot rootAggregate) {
        var enumFile = ctx.Use<EnumFile>();
        var parser = new EnumDefParser(rootAggregate.XElement, ctx.Schema);

        // データ型: enum, リテラル型定義
        var staticEnum = new StaticEnumDef(parser, rootAggregate);
        enumFile.Register(staticEnum);
        enumFile.AddCSharpSource(staticEnum.RenderCSharp());
        enumFile.AddTypeScriptSource(staticEnum.RenderTypeScript());

        // データ型: 検索条件オブジェクト
        var searchCondition = new StaticEnumSearchCondition(staticEnum, parser);
        enumFile.AddCSharpSource(searchCondition.RenderCSharp());
        enumFile.AddTypeScriptSource(searchCondition.RenderTypeScript());
    }
}
