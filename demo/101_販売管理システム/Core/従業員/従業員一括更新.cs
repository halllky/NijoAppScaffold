using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute従業員一括更新Async(従業員一括更新ParameterDisplayData param, IPresentationContext<従業員一括更新ParameterMessages> context) {
        // 権限チェック
        if (LoginUser == null || !LoginUser.Isシステム管理者) {
            context.Messages.AddError("システム管理者のみ実行可能です。");
            return;
        }

        // 一括更新処理。
        // ここで実装すべきなのは、トランザクションの範囲の定義と、
        // 画面項目からデータベース項目へのマッピング。
        var commitedIndex = new HashSet<int>();
        for (var i = 0; i < param.更新対象従業員一覧.Count; i++) {

            // サーバーの負荷を考慮し、トランザクションの範囲は従業員1名ずつ
            await using var tran = await BeginTransactionAsync();

            var item = param.更新対象従業員一覧[i];
            var message = context.Messages.更新対象従業員一覧[i];
            bool success;

            // フラグ項目をもとに、追加・更新・削除のいずれかを実行する。
            // WillBeChanged や WillBeDeleted はJavaScript側でtrueを設定する処理を記述する必要がある。
            if (!item.ExistsInDatabase) {

                // ユニーク制約によるチェックはかかるものの、
                // ここでチェックをかけた方がロールバックの挙動が分かりやすくなるので重ねてチェックする
                if (param.更新対象従業員一覧.Any(e => e != item && e.従業員.従業員番号 == item.従業員.従業員番号)
                    || await DbContext.従業員DbSet.AnyAsync(e => e.従業員番号 == item.従業員.従業員番号)) {

                    message.従業員.従業員番号.AddError($"従業員番号 '{item.従業員.従業員番号}' は既に存在します。");
                    continue;
                }

                // 初期パスワード
                var salt = GenerateSalt();
                var hash = ComputeHash(INIT_PASSWORD, salt);

                // 新規追加
                var result = await Create従業員Async(new() {
                    従業員番号 = item.従業員.従業員番号,
                    氏名 = item.従業員.氏名,
                    入荷担当 = item.従業員.入荷担当,
                    販売担当 = item.従業員.販売担当,
                    システム管理者 = item.従業員.システム管理者,
                    パスワード = hash,
                    SALT = salt,
                }, context, message);

                success = result.IsSaveCompleted();

            } else if (item.WillBeDeleted) {

                // 削除
                success = await Delete従業員Async(new() {
                    従業員番号 = item.従業員.従業員番号,
                    Version = item.従業員.Version,
                }, context, message);

            } else if (item.WillBeChanged) {

                // 更新
                var result = await Update従業員Async(item.従業員.従業員番号, item.従業員.Version, employee => {
                    employee.氏名 = item.従業員.氏名;
                    employee.入荷担当 = item.従業員.入荷担当;
                    employee.販売担当 = item.従業員.販売担当;
                    employee.システム管理者 = item.従業員.システム管理者;
                }, context, message);

                success = result.IsSaveCompleted();

            } else {
                // 変更なし
                continue;
            }

            if (!context.ValidationOnly && success) {
                await tran.CommitAsync();
                commitedIndex.Add(i);
            }
        }

        // 保存後メッセージ。
        // 一部の更新だけコミットされる場合があるので
        // 分かりやすさのためにメッセージを詳細に表示する
        if (!context.ValidationOnly) {

            if (context.Messages.HasError()) {
                if (commitedIndex.Count == 0) {
                    context.Messages.AddError("保存に失敗しました。");

                } else {
                    context.Messages.AddWarn("保存できなかったデータがあります。");
                    foreach (var i in commitedIndex) {
                        context.Messages.更新対象従業員一覧[i].AddInfo("保存しました。");
                    }
                }

            } else {
                if (commitedIndex.Count == 0) {
                    context.Messages.AddInfo("更新された内容はありません。");

                } else {
                    context.Messages.AddInfo("保存しました。");
                }
            }
        }
    }
}
