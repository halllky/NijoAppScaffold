using Nijo.CodeGenerating;
using Nijo.Parts.CSharp;
using Nijo.Parts.Common;

namespace Nijo.DataStructures.IsStored;

/// <summary>
/// 自動生成されるコマンドと、アプリケーション側で実装される保存処理との間の取り決め。
/// 保存処理の流れはシステムごとに要件が大きく異なるため自動生成せず、スキーマに依存する部品だけをここで定義する。
/// スキーマ定義にデータ構造が1個も無くても保存処理のコンパイルが通るように、常に生成される。
/// </summary>
internal static class SaveCommandContract {

    internal const string INTERFACE = "ISaveCommand";
    internal const string INTERFACE_DELETE = "IDeleteCommand";
    internal const string AUDIT_INFO_INTERFACE = "IAuditInfo";
    internal const string PREDICATE_CLASS = "SaveCommandQueryPredicate";

    internal const string ERR_ID_UNKNOWN = "UnknownUpdateError";
    internal const string ERR_DATA_NOT_FOUND = "DataNotFoundError";
    internal const string ERR_CONCURRENCY = "ConcurrencyError";
    internal const string ERR_UNIQUE_CONSTRAINT = "UniqueConstraintError";

    internal static void RegisterMessages(CodeRenderingContext ctx) {
        ctx.Use<MsgFactory>()
            .AddMessage(ERR_ID_UNKNOWN,
                "登録/更新/削除のタイミングでRDBMS上で何らかのエラーが生じた場合のメッセージ",
                "登録処理でエラーが発生しました: {0}")
            .AddMessage(ERR_DATA_NOT_FOUND,
                "更新対象・削除対象のデータがデータベース上で見つからなかったときのメッセージ",
                "更新対象のデータが見つかりません。")
            .AddMessage(ERR_CONCURRENCY,
                "楽観排他制御に引っかかったときのメッセージ",
                "ほかのユーザーが更新しました。")
            .AddMessage(ERR_UNIQUE_CONSTRAINT,
                "一意制約に違反したときのメッセージ",
                "同じ値のデータが既に存在します。");
    }

    internal static SourceFile Render(CodeRenderingContext ctx) {
        return new SourceFile {
            FileName = "SaveCommandContract.cs",
            Contents = $$"""
                using System.Linq.Expressions;
                using System.Text.Json.Nodes;

                namespace {{ctx.Config.RootNamespace}};

                /// <summary>
                /// データの更新を記録するときに使う情報。
                /// </summary>
                public interface {{AUDIT_INFO_INTERFACE}} {
                    /// <summary>
                    /// <para>
                    /// 現在時刻。データ更新時の更新時刻の記録などに使用。
                    /// <see cref="DateTime.Now"/> を使ってしまうと現在時刻に依存する処理のテストが困難になるので、基本的にはこのプロパティを使うこと。
                    /// </para>
                    /// <para>
                    /// 同一のリクエストの中では、たとえ実時刻に多少のずれがあったとしてもすべて同じ時刻（リクエスト開始時点の時刻）になる。
                    /// 理由は、例えば深夜0時前後の処理で処置の途中で日付が変わることでロジックに影響が出る、などといった事象を防ぐため。
                    /// </para>
                    /// <para>
                    /// ちなみにログ出力の時刻にはこのプロパティが用いられず、正確な現在時刻が出力される。
                    /// </para>
                    /// </summary>
                    DateTime CurrentTime { get; }
                    /// <summary>
                    /// 現在操作中のユーザーの名前。データ更新時の更新者の記録などに使用。
                    /// </summary>
                    string CurrentUser { get; }
                }

                /// <summary>
                /// 保存処理が必要とする、コマンドの機能。
                /// 通常の業務ロジックからは使わない。
                /// </summary>
                public interface {{INTERFACE}} {
                    /// <summary>このコマンドの現在の状態に対するバリデーションエラーを返します。コマンドの状態は変更しません。</summary>
                    {{MessageContainer.SETTER_CLASS}} GetValidationErrors();
                    /// <summary>
                    /// 監査項目と楽観排他のバージョンを設定し、エンティティの変更追跡を開始します。
                    /// 1つのコマンドに対して1回しか呼べません。2回目以降は例外を送出します。
                    /// </summary>
                    /// <returns>変更追跡を開始したエンティティ。保存後にこれらの変更追跡を終了するのは呼び出し側の責務です。</returns>
                    IReadOnlyList<object> StartTracking();
                    /// <summary>同一の保存処理の中でのキー重複の検出に使うキーを返します。保存時に採番されるのでキーが未確定の場合は null を返します。</summary>
                    object? GetKeyForDuplicateCheck();
                    /// <summary>ログに出力する情報（集約名、操作の種類、キー）を返します。</summary>
                    JsonNode ToLogJson();
                }

                /// <inheritdoc cref="{{INTERFACE}}"/>
                /// <typeparam name="TUpdateCommand">保存後のデータを表す更新コマンドの型</typeparam>
                public interface {{INTERFACE}}<TUpdateCommand> : {{INTERFACE}} {
                    /// <summary>保存後のデータを表す、このインスタンスとは別の更新コマンドを返します。削除の場合は null を返します。</summary>
                    TUpdateCommand? ToSavedCommand();
                }

                /// <summary>
                /// 削除コマンド。
                /// 保存処理の戻り値に保存後のデータが含まれないことを型で表すためにオーバーロードを分けるのに使う。
                /// </summary>
                /// <typeparam name="TUpdateCommand">削除コマンドと同じ集約の更新コマンドの型</typeparam>
                public interface {{INTERFACE_DELETE}}<TUpdateCommand> : {{INTERFACE}}<TUpdateCommand> {
                }

                /// <summary>
                /// 複数のキーでデータを読み込むときの検索条件を組み立てる。
                /// </summary>
                internal static class {{PREDICATE_CLASS}} {
                    /// <summary>引数の条件のいずれかに一致することを表す条件を返します。</summary>
                    internal static Expression<Func<T, bool>> AnyOf<T>(IEnumerable<Expression<Func<T, bool>>> predicates) {
                        var parameter = Expression.Parameter(typeof(T), "e");
                        var body = predicates
                            .Select(predicate => new ParameterReplacer(predicate.Parameters[0], parameter).Visit(predicate.Body))
                            .Aggregate(Expression.OrElse);
                        return Expression.Lambda<Func<T, bool>>(body, parameter);
                    }

                    private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor {
                        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : node;
                    }
                }
                """,
        };
    }
}
