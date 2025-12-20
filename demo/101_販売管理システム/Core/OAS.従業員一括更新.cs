using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute従業員一括更新(従業員一括更新ParameterDisplayData param, IPresentationContext<従業員一括更新ParameterMessages> context) {
        // 権限チェック
        if (LoginUser == null || !LoginUser.Isシステム管理者) {
            context.Messages.AddError("システム管理者のみ実行可能です。");
            return;
        }

        // 一括更新処理。
        // ここで実装すべきなのは、トランザクションの範囲の定義と、
        // 画面項目からデータベース項目へのマッピング。
        for (var i = 0; i < param.更新対象従業員一覧.Count; i++) {

            // トランザクションの範囲は従業員1名ずつ。
            // 「保存しますか？」の確認前はエラーチェックのみなのでトランザクションは開始しない。
            using var tran = context.Options.IgnoreConfirm
                ? await DbContext.Database.BeginTransactionAsync()
                : null;

            var item = param.更新対象従業員一覧[i];
            var message = context.Messages.更新対象従業員一覧[i];

            // フラグ項目をもとに、追加・更新・削除のいずれかを実行する。
            // WillBeChanged や WillBeDeleted はJavaScript側でtrueを設定する処理を記述する必要がある。
            if (!item.ExistsInDatabase) {

                // 初期パスワード
                var salt = GenerateSalt();
                var hash = ComputeHash(INIT_PASSWORD, salt);

                // 新規追加
                await Create従業員Async(new() {
                    従業員番号 = item.Values.従業員.Values.従業員番号,
                    氏名 = item.Values.従業員.Values.氏名,
                    入荷担当 = item.Values.従業員.Values.入荷担当,
                    販売担当 = item.Values.従業員.Values.販売担当,
                    システム管理者 = item.Values.従業員.Values.システム管理者,
                    パスワード = hash,
                    SALT = salt,
                }, context, message);

            } else if (item.WillBeDeleted) {

                // 削除
                await Delete従業員Async(new() {
                    従業員番号 = item.Values.従業員.Values.従業員番号,
                    Version = item.Values.従業員.Version,
                }, context, message);

            } else if (item.WillBeChanged) {

                // 更新
                await Update従業員Async(item.Values.従業員.Values.従業員番号, item.Values.従業員.Version, employee => {
                    employee.氏名 = item.Values.従業員.Values.氏名;
                    employee.入荷担当 = item.Values.従業員.Values.入荷担当;
                    employee.販売担当 = item.Values.従業員.Values.販売担当;
                    employee.システム管理者 = item.Values.従業員.Values.システム管理者;
                }, context, message);
            }

            if (tran != null && !message.HasError()) {
                await tran.CommitAsync();
            }
        }
    }
}
