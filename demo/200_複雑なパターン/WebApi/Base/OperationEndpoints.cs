using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MyApp.WebApi.Base;

/// <summary>
/// 自動生成されたクエリ・コマンド（<see cref="Operation{TOperationContext}.All"/>）を、1件ずつ HTTP エンドポイントとして公開する。
///
/// HTTPリクエスト・レスポンスと、処理の引数・戻り値・メッセージとの変換方法は NijoAppScaffold として決まりがあるわけではなく、
/// クライアント側の呼び出し処理と整合性がとれていればよい。
/// ここでは以下の最も単純な取り決めにしている。アプリケーションの要件に応じて自由に変更してよい。
///
/// <list type="bullet">
///   <item>リクエスト: 処理の引数を JSON にしたものをリクエストボディで送る（Content-Type: application/json）</item>
///   <item>レスポンス: 戻り値とメッセージを JSON で返す。ステータスコードは、エラーが無ければ 200、エラーがあれば 422</item>
///   <item>確認メッセージ（<see cref="OperationContext.Confirms"/>）は扱わない。処理は常に入力チェックと更新を1回で行う</item>
/// </list>
/// </summary>
public static class OperationEndpoints {

    /// <summary>
    /// すべてのクエリ・コマンドを HTTP POST のエンドポイントとして登録します。
    /// </summary>
    /// <param name="configure">
    /// クエリ・コマンドごとのエンドポイントの設定。
    /// 認可の要否など、処理ごとに異なる設定がある場合に使う。
    /// </param>
    /// <returns>全エンドポイント共通の設定（エンドポイントフィルターなど）を追加するためのグループ</returns>
    public static RouteGroupBuilder MapOperationEndpoints(this IEndpointRouteBuilder endpoints, Action<Operation<OperationContext>, RouteHandlerBuilder>? configure = null) {
        var group = endpoints.MapGroup("");
        var mapping = new EndpointMapping(group);
        foreach (var operation in Operation<OperationContext>.All) {
            var endpoint = operation.Accept(mapping);
            configure?.Invoke(operation, endpoint);
        }
        return group;
    }

    /// <summary>
    /// <see cref="IEndpointRouteBuilder"/> に <see cref="HandleAsync"/> を登録する。
    /// </summary>
    private sealed class EndpointMapping(IEndpointRouteBuilder endpoints) : IOperationVisitor<OperationContext, RouteHandlerBuilder> {
        public RouteHandlerBuilder Visit<TParameter, TReturnValue, TMessages>(Operation<OperationContext, TParameter, TReturnValue, TMessages> operation)
            where TMessages : IMessageSetter {
            // RequestDelegate を受け取るオーバーロードが選ばれると戻り値の IResult が無視されるため、型を明示する
            Func<HttpContext, Task<IResult>> handler = httpContext => HandleAsync(operation, httpContext);
            return endpoints
                .MapPost(operation.Path, handler)
                .WithDisplayName(operation.DisplayName);
        }
    }

    /// <summary>
    /// 実際のHTTPリクエスト実行時の処理。
    /// HTTPリクエストを処理の引数に変換して処理を実行し、処理結果をHTTPレスポンスに変換する。
    /// </summary>
    private static async Task<IResult> HandleAsync<TParameter, TReturnValue, TMessages>(Operation<OperationContext, TParameter, TReturnValue, TMessages> operation, HttpContext httpContext)
        where TMessages : IMessageSetter {

        var app = httpContext.RequestServices.GetRequiredService<OverridedApplicationService>();
        var jsonOptions = new JsonSerializerOptions().EditDefaultJsonSerializerOptions();

        // HTTPリクエストボディを処理の引数に変換する
        if (!httpContext.Request.HasJsonContentType()) {
            app.Log.LogWarning("リクエストの Content-Type が application/json ではありません。");
            return Results.BadRequest("リクエストの Content-Type は application/json である必要があります。");
        }
        TParameter parameter;
        try {
            parameter = await httpContext.Request.ReadFromJsonAsync<TParameter>(jsonOptions, httpContext.RequestAborted)
                ?? throw new JsonException("リクエストボディが null です。");

            app.Log.LogDebug("HTTP REQUEST BODY DATA: {Parameter}", app.SerializeForLog(parameter));

        } catch (JsonException jsonEx) {
            app.Log.LogWarning(jsonEx, "{ParameterType} 型へのデシリアライズに失敗しました。", typeof(TParameter).FullName);
            return Results.BadRequest($"リクエストの型が不正です。{typeof(TParameter).FullName} 型として認識できません。");
        }

        // 処理を実行する
        var context = new OperationContext { ValidationOnly = false };
        var messages = MessageSetter.GetImpl<TMessages>([], new MessageContainer());

        var returnValue = await operation.InvokeAsync(app, parameter, messages, context);

        // 処理結果をHTTPレスポンスに変換する
        var messageContainer = messages.GetState();
        var responseBody = new JsonObject {
            ["returnValue"] = JsonSerializer.SerializeToNode(returnValue, jsonOptions),
            ["messages"] = messageContainer == null ? new JsonObject() : ToJsonObject(messageContainer),
        };
        var statusCode = messages.HasError()
            ? HttpStatusCode.UnprocessableEntity
            : HttpStatusCode.OK;

        return Results.Content(
            JsonSerializer.Serialize(responseBody, jsonOptions),
            "application/json",
            Encoding.UTF8,
            (int)statusCode);
    }

    /// <summary>
    /// メッセージをJsonNode型に変換します。
    /// オブジェクトの構造はクライアント側でのメッセージ表示処理に合わせた形になっています。
    ///
    /// <list type="bullet">
    /// <item>このメソッドは、このオブジェクトおよび子孫オブジェクトが持っているメッセージを再帰的に集め、以下のようなJSONオブジェクトに変換します。</item>
    /// <item>メッセージコンテナは、エラー、警告、インフォメーションの3種類のメッセージを、それぞれ配列として持ちます。</item>
    /// <item>エラーだけ持っているなど、一部の種類のメッセージのみ持っている場合、他の種類の配列は配列自体が存在しなくなります。</item>
    /// <item>子要素は children という名前のオブジェクトにまとめて格納されます。</item>
    /// <item>
    /// 3種類のメッセージのいずれも持っていない項目のプロパティは存在しません。
    /// 例えば以下のオブジェクトで「項目A」「項目B」以外に「項目X」が存在するが、Xにはメッセージが発生していない場合、Xのプロパティは存在しません。
    /// </item>
    /// <item>ネストされたオブジェクトのメッセージも生成されます。（下記「子オブジェクトのメッセージ」）</item>
    /// <item>
    /// 配列は、配列インデックスをキーとしたオブジェクトになります。（下記「子配列のメッセージ」）
    /// 配列インデックスか否かは、 children 直下のオブジェクトのキーが半角整数のみから成るか否かで判定できます。
    /// </item>
    /// </list>
    /// <code>
    /// {
    ///   "error": ["xxxがエラーです"],
    ///   "children": {
    ///     "項目A": { "error": ["xxxがエラーです"], "warn": ["xxxという警告があります"], "info": ["xxxという情報があります"] },
    ///     "項目B": { "error": ["xxxがエラーです", "yyyがエラーです"], "warn": ["xxxという警告があります"], "info": ["xxxという情報があります"] },
    ///     "子オブジェクトのメッセージ": {
    ///       "children": {
    ///         "項目C": { "error": ["xxxがエラーです"] },
    ///         "項目D": { "error": ["xxxがエラーです"] },
    ///       },
    ///     },
    ///     "子配列のメッセージ": {
    ///       "error": ["xxxがエラーです"],
    ///       "children": {
    ///         "1": {
    ///           "children": {
    ///             "項目E": { "error": ["xxxがエラーです"] },
    ///           },
    ///         },
    ///         "5": {
    ///           "error": ["xxxがエラーです"],
    ///           "children": {
    ///             "項目E": { "error": ["xxxがエラーです"] },
    ///           },
    ///         },
    ///       },
    ///     }
    ///   }
    /// }
    /// </code>
    /// </summary>
    internal static JsonObject ToJsonObject(IReadOnlyMessageContainer messageContainer) {
        var result = new JsonObject();

        if (messageContainer.Errors.Any()) {
            var strArray = new JsonArray();
            foreach (var str in messageContainer.Errors) strArray.Add(str);
            result["error"] = strArray;
        }
        if (messageContainer.Warns.Any()) {
            var strArray = new JsonArray();
            foreach (var str in messageContainer.Warns) strArray.Add(str);
            result["warn"] = strArray;
        }
        if (messageContainer.Infos.Any()) {
            var strArray = new JsonArray();
            foreach (var str in messageContainer.Infos) strArray.Add(str);
            result["info"] = strArray;
        }

        var children = new JsonObject();
        foreach (var child in messageContainer.Children) {
            var childJson = ToJsonObject(child.Value);
            if (childJson != null) children[child.Key] = childJson;
        }
        if (children.Count > 0) result["children"] = children;

        return result;
    }
}
