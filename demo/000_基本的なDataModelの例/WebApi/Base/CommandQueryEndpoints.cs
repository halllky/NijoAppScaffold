using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MyApp.WebApi.Base;

/// <summary>
/// 自動生成されたクエリ・コマンド（<see cref="CommandQuery{TCommandQueryContext}.All"/>）を、1件ずつ HTTP エンドポイントとして公開する。
///
/// HTTPリクエスト・レスポンスと、処理の引数・戻り値・メッセージとの変換方法は NijoAppScaffold として決まりがあるわけではなく、
/// クライアント側の呼び出し処理と整合性がとれていればよい。
/// 以下のような詳細は、アプリケーションの要件に応じて自由に変更してよい。
///
/// <list type="bullet">
///   <item>引数や戻り値のJSONシリアライズ/デシリアライズの方法</item>
///   <item>成功時・失敗時のHTTPステータスコード</item>
///   <item>エラー時のレスポンス内容</item>
///   <item>Content-TypeやCORSなどのHTTPヘッダー</item>
/// </list>
/// </summary>
public static class CommandQueryEndpoints {

    /// <summary>
    /// クライアント側から送られてくるパラメータは
    /// Content-Type: multipart/form-data のリクエストボディのこの名前のセクションにJSONで入っている
    /// </summary>
    private const string PARAM_DATA = "complex-post-request-body";
    /// <summary>
    /// 確認メッセージが承諾された後の本処理を行うHTTPリクエストに付されるクエリパラメータ
    /// </summary>
    private const string PARAM_IGNORE_CONFIRM = "ignore-confirm";

    /// <summary>
    /// すべてのクエリ・コマンドを HTTP POST のエンドポイントとして登録します。
    /// </summary>
    /// <param name="configure">
    /// クエリ・コマンドごとのエンドポイントの設定。
    /// 認可の要否など、処理ごとに異なる設定がある場合に使う。
    /// </param>
    /// <returns>全エンドポイント共通の設定（エンドポイントフィルターなど）を追加するためのグループ</returns>
    public static RouteGroupBuilder MapCommandQueryEndpoints(this IEndpointRouteBuilder endpoints, Action<CommandQuery<CommandQueryContext>, RouteHandlerBuilder>? configure = null) {
        var group = endpoints.MapGroup("");
        var mapping = new EndpointMapping(group);
        foreach (var commandQuery in CommandQuery<CommandQueryContext>.All) {
            var endpoint = commandQuery.Accept(mapping);
            configure?.Invoke(commandQuery, endpoint);
        }
        return group;
    }

    /// <summary>
    /// <see cref="IEndpointRouteBuilder"/> に <see cref="HandleAsync"/> を登録する。
    /// </summary>
    private sealed class EndpointMapping(IEndpointRouteBuilder endpoints) : ICommandQueryVisitor<CommandQueryContext, RouteHandlerBuilder> {
        public RouteHandlerBuilder Visit<TParameter, TReturnValue, TMessages>(CommandQuery<CommandQueryContext, TParameter, TReturnValue, TMessages> commandQuery)
            where TMessages : IMessageSetter {
            // RequestDelegate を受け取るオーバーロードが選ばれると戻り値の IResult が無視されるため、型を明示する
            Func<HttpContext, Task<IResult>> handler = httpContext => HandleAsync(commandQuery, httpContext);
            return endpoints
                .MapPost(commandQuery.Path, handler)
                .WithDisplayName(commandQuery.DisplayName);
        }
    }

    /// <summary>
    /// 実際のHTTPリクエスト実行時の処理。
    /// HTTPリクエストを処理の引数に変換して処理を実行し、処理結果をHTTPレスポンスに変換する。
    ///
    /// HTTPリクエスト・レスポンスのルールは、クライアント側の処理と整合しさえしていればよく、自由に変更可能。
    /// </summary>
    private static async Task<IResult> HandleAsync<TParameter, TReturnValue, TMessages>(CommandQuery<CommandQueryContext, TParameter, TReturnValue, TMessages> commandQuery, HttpContext httpContext)
        where TMessages : IMessageSetter {

        var app = httpContext.RequestServices.GetRequiredService<OverridedApplicationService>();
        var jsonOptions = new JsonSerializerOptions().EditDefaultJsonSerializerOptions();

        // ---------------------------------------------------------
        // HTTPリクエストボディを処理の引数に変換する
        // ---------------------------------------------------------
        if (!httpContext.Request.HasFormContentType) {
            app.Log.LogWarning("リクエストの Content-Type が multipart/form-data ではありません。");
            return Results.BadRequest($"リクエストのContent-Typeは multipart/form-data で、セクション '{PARAM_DATA}' を含んでいる必要があります。");
        }
        var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
        if (!form.TryGetValue(PARAM_DATA, out var dataSection)) {
            app.Log.LogWarning("リクエストに '{ParamData}' セクションが含まれていません。", PARAM_DATA);
            return Results.BadRequest($"リクエストのContent-Typeは multipart/form-data で、セクション '{PARAM_DATA}' を含んでいる必要があります。");
        }

        TParameter parameter;
        try {
            parameter = JsonSerializer.Deserialize<TParameter>(dataSection.ToString(), jsonOptions)
                ?? throw new JsonException("リクエストボディが null です。");

            app.Log.LogDebug("HTTP REQUEST BODY DATA: {DataSection}", app.SerializeForLog(parameter));

        } catch (JsonException jsonEx) {
            app.Log.LogWarning(jsonEx, "{ParameterType} 型へのデシリアライズに失敗しました。", typeof(TParameter).FullName);
            return Results.BadRequest($"リクエストの型が不正です。{typeof(TParameter).FullName} 型として認識できません。");
        }

        // ---------------------------------------------------------
        // 処理を実行する
        // ---------------------------------------------------------
        var ignoreConfirm = httpContext.Request.Query.TryGetValue(PARAM_IGNORE_CONFIRM, out var strIgnoreConfirm)
            && bool.TryParse(strIgnoreConfirm, out var blnIgnoreConfirm)
            && blnIgnoreConfirm;
        var context = new CommandQueryContext { ValidationOnly = !ignoreConfirm };
        var messages = MessageSetter.GetImpl<TMessages>([], new MessageContainer());

        var returnValue = await commandQuery.InvokeAsync(app, parameter, messages, context);

        // ---------------------------------------------------------
        // 処理結果をHTTPレスポンスに変換する
        // ---------------------------------------------------------
        var responseBody = new JsonObject();
        if (returnValue != null) {
            responseBody["returnValue"] = JsonSerializer.SerializeToNode(returnValue, jsonOptions);
        }
        responseBody["confirms"] = new JsonArray([.. context.Confirms.Select(text => JsonValue.Create(text))]);

        var messageContainer = messages.GetState();
        responseBody["detail"] = messageContainer == null ? new JsonObject() : ToJsonObject(messageContainer);

        // エラーがある場合、またはコマンドの1回目のリクエスト（入力チェックのみ）の場合は 202 Accepted を返し、
        // クライアント側に確認メッセージの表示を促す。
        // クエリはデータを変更しないので確認を必要としない。
        var statusCode = messages.HasError() || (!commandQuery.IsQuery && !ignoreConfirm)
            ? HttpStatusCode.Accepted
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
