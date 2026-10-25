using Nijo.CodeGenerating;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System;

namespace Nijo.Commands;

/// <summary>
/// Commands セクション。
/// コマンドは引数を受け取り戻り値を返す処理で、Webサーバー・クライアント間で常に同期された型定義を提供する。
/// </summary>
internal sealed class CommandsSection : Section {
    public override E_Section Kind => E_Section.Commands;

    public override void Validate(IReadOnlyList<XElement> rootAggregateElements, SchemaParseContext context, SchemaValidationErrors errors) {
        // コマンドは子孫XML要素を定義できない。引数と戻り値の型は属性で指定する
        foreach (var rootAggregateElement in rootAggregateElements) {
            if (rootAggregateElement.Elements().Any()) {
                errors.Add(rootAggregateElement, $"コマンドのルート集約は子孫XML要素を定義できません。引数と戻り値の型は{ParameterOption.ATTRIBUTE_NAME}属性と{ReturnValueOption.ATTRIBUTE_NAME}属性で指定してください。");
            }
        }
    }

    public override void Render(CodeRenderingContext ctx) {
        RenderEach(ctx, ctx.Schema.GetCommands(), rootAggregate => RenderRootAggregate(ctx, rootAggregate));
    }

    private static void RenderRootAggregate(CodeRenderingContext ctx, CommandRoot rootAggregate) {
        var aggregateFile = new SourceFileByAggregate(rootAggregate);

        // 処理: TypeScript用マッピング、Webエンドポイント、本処理抽象メソッド
        var commandProcessing = new CommandProcessing(rootAggregate);
        aggregateFile.AddWebapiControllerAction(commandProcessing.RenderAspNetCoreControllerAction(ctx));
        aggregateFile.AddAppSrvMethod(commandProcessing.RenderAppSrvMethods(ctx), "コマンド処理");

        // カスタムロジック用モジュール
        ctx.Use<CommandQueryMappings>().AddCommandModel(rootAggregate);

        aggregateFile.ExecuteRendering(ctx);
    }
}
