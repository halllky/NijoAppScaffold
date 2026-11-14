using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute従業員一括更新Async(従業員一括更新ParameterDisplayData param, 従業員一括更新ParameterMessages messages, OperationContext context) {
        // 権限チェック
        if (LoginUser == null || !LoginUser.Isシステム管理者) {
            messages.AddError("システム管理者のみ実行可能です。");
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
        var saveCommands = new List<ValidatedCommand<従業員UpdateCommand>>();

        for (var i = 0; i < param.更新対象従業員一覧.Count; i++) {
            var item = param.更新対象従業員一覧[i];
            var message = messages.更新対象従業員一覧[i];

            // 新規追加
            if (!item.ExistsInDatabase) {

                // ユニーク制約によるチェックはかかるものの、
                // 行単位でエラーを見せるためにここでもチェックする
                if (param.更新対象従業員一覧.Any(e => e != item && e.従業員.従業員番号 == item.従業員.従業員番号)
                    || await DbContext.従業員DbSet.AnyAsync(e => e.従業員番号 == item.従業員.従業員番号)) {

                    message.従業員.従業員番号.AddError($"従業員番号 '{item.従業員.従業員番号}' は既に存在します。");
                    continue;
                }

                var createCommand = DbContext.StartCreating従業員();
                createCommand.従業員番号 = item.従業員.従業員番号;
                createCommand.氏名 = item.従業員.氏名;
                createCommand.従業員の権限 = item.従業員.従業員マスタの権限
                    .Select(x => new 従業員の権限CreateCommand { 権限区分 = x.権限区分 })
                    .ToList();
                var initPassword = PasswordHash.Create(INIT_PASSWORD);
                createCommand.パスワード = initPassword.Hash;
                createCommand.SALT = initPassword.Salt;

                if (createCommand.TryValidate(out var validatedCreate, out var createErrors)) {
                    saveCommands.Add(validatedCreate);
                }
                createErrors.CopyTo(message.従業員);
                continue;
            }

            // 変更なし
            if (!item.WillBeChanged && !item.WillBeDeleted) continue;

            // 既存データの引当
            if (!loaded.TryGetValue(new() { 従業員番号 = item.従業員.従業員番号 }, out var updateCommand)) {
                message.AddError(MSG.DataNotFoundError());
                continue;
            }

            // 自分自身の削除と、自分自身のシステム管理者の権限を外すことはできない。
            // この処理はシステム管理者だけが実行できるので、これによりシステム管理者が1人もいなくなることも防げる
            if (item.従業員.従業員番号 == LoginUser.従業員番号) {
                if (item.WillBeDeleted) {
                    message.AddError("自分自身は削除できません。");
                    continue;
                }
                if (!item.従業員.従業員マスタの権限.Any(x => x.権限区分 == 権限区分.システム管理者)) {
                    message.AddError("自分自身のシステム管理者の権限は外せません。");
                    continue;
                }
            }

            // 削除
            if (item.WillBeDeleted) {
                if (updateCommand.ToDeleteCommand().TryValidate(out var validatedDelete, out var deleteErrors)) {
                    saveCommands.Add(validatedDelete);
                }
                deleteErrors.CopyTo(message.従業員);
                continue;
            }

            // 更新
            updateCommand.氏名 = item.従業員.氏名;
            updateCommand.従業員の権限 = item.従業員.従業員マスタの権限
                .Select(x => new 従業員の権限UpdateCommand { 権限区分 = x.権限区分 })
                .ToList();

            if (updateCommand.TryValidate(out var validatedUpdate, out var updateErrors)) {
                saveCommands.Add(validatedUpdate);
            }
            updateErrors.CopyTo(message.従業員);
        }

        // いずれか1件でもエラーなら中断
        if (messages.HasError()) return;

        // 1巡目はここまで
        if (context.ValidationOnly) {
            context.Confirms.Add("更新しますか？");
            return;
        }

        if (saveCommands.Count == 0) {
            messages.AddInfo("更新された内容はありません。");
            return;
        }

        // 保存実行。全件まとめて成功か失敗かとする
        await using var tran = await BeginTransactionAsync();

        var result = await DbContext.TrySaveAsync(saveCommands);
        if (!result.Success) {
            result.Errors.CopyTo(messages);
            return;
        }

        await tran.CommitAsync();
        messages.AddInfo("保存しました。");
    }
}
