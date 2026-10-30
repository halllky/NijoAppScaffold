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

        // 更新・削除の対象を読み込む。
        // WillBeChanged や WillBeDeleted はJavaScript側でtrueを設定する処理を記述する必要がある。
        var keys = param.更新対象従業員一覧
            .Where(x => x.ExistsInDatabase && (x.WillBeChanged || x.WillBeDeleted))
            .Select(x => new 従業員KeyWithVersion(x.従業員.Version) {
                従業員番号 = x.従業員.従業員番号,
            });
        var loaded = await DbContext.LoadForEdit従業員Async(keys);

        // ループ後にまとめて保存するのでメモリ上に蓄積する
        var saveCommands = new List<従業員SaveCommand>();

        for (var i = 0; i < param.更新対象従業員一覧.Count; i++) {
            var item = param.更新対象従業員一覧[i];
            var message = context.Messages.更新対象従業員一覧[i];

            // 新規追加
            if (!item.ExistsInDatabase) {

                // ユニーク制約によるチェックはかかるものの、
                // 行単位でエラーを見せるためにここでもチェックする
                if (param.更新対象従業員一覧.Any(e => e != item && e.従業員.従業員番号 == item.従業員.従業員番号)
                    || await DbContext.従業員DbSet.AnyAsync(e => e.従業員番号 == item.従業員.従業員番号)) {

                    message.従業員.従業員番号.AddError($"従業員番号 '{item.従業員.従業員番号}' は既に存在します。");
                    continue;
                }

                // 初期パスワード
                var salt = GenerateSalt();

                var createCommand = DbContext.StartCreating従業員();
                createCommand.従業員番号 = item.従業員.従業員番号;
                createCommand.氏名 = item.従業員.氏名;
                createCommand.入荷担当 = item.従業員.入荷担当;
                createCommand.販売担当 = item.従業員.販売担当;
                createCommand.システム管理者 = item.従業員.システム管理者;
                createCommand.パスワード = ComputeHash(INIT_PASSWORD, salt);
                createCommand.SALT = salt;

                createCommand.GetValidationErrors().CopyTo(message.従業員);
                saveCommands.Add(createCommand);
                continue;
            }

            // 変更なし
            if (!item.WillBeChanged && !item.WillBeDeleted) continue;

            // 既存データの引当
            if (!loaded.TryGetValue(new() { 従業員番号 = item.従業員.従業員番号 }, out var updateCommand)) {
                message.AddError(MSG.DataNotFoundError());
                continue;
            }

            // 削除
            if (item.WillBeDeleted) {
                var deleteCommand = updateCommand.ToDeleteCommand();
                deleteCommand.GetValidationErrors().CopyTo(message.従業員);
                saveCommands.Add(deleteCommand);
                continue;
            }

            // 更新
            updateCommand.氏名 = item.従業員.氏名;
            updateCommand.入荷担当 = item.従業員.入荷担当;
            updateCommand.販売担当 = item.従業員.販売担当;
            updateCommand.システム管理者 = item.従業員.システム管理者;

            updateCommand.GetValidationErrors().CopyTo(message.従業員);
            saveCommands.Add(updateCommand);
        }

        // いずれか1件でもエラーなら中断
        if (context.Messages.HasError()) return;

        // 1巡目はここまで
        if (context.ValidationOnly) {
            context.AddConfirm("更新しますか？");
            return;
        }

        if (saveCommands.Count == 0) {
            context.Messages.AddInfo("更新された内容はありません。");
            return;
        }

        // 保存実行。全件まとめて成功か失敗かとする
        await using var tran = await BeginTransactionAsync();

        var result = await DbContext.TrySaveAsync(saveCommands);
        if (!result.Success) {
            result.Errors.CopyTo(context.Messages);
            return;
        }

        await tran.CommitAsync();
        context.Messages.AddInfo("保存しました。");
    }
}
