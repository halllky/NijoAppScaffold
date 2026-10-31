using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute商品データ取込Async(IPresentationContext<MessageSetter> context) {

        foreach (var item in 商品管理システム.Enumerate商品データ()) {
            Log.LogInformation("処理開始: {itemName} ({itemExternalId})", item.Name, item.ExternalId);

            // 消費税区分の変換 (文字列 -> Enum)
            if (!Enum.TryParse<消費税区分>(item.TaxType, out var taxType)) {
                Log.LogWarning("不正な消費税区分です: {itemTaxType} (商品: {itemName})", item.TaxType, item.Name);
                continue;
            }

            // 外部システムIDで検索。
            // 読み込み時点のバージョンで楽観排他をかける。
            var loaded = await DbContext.LoadForEdit商品Async(query => query.Where(x => x.外部システム側ID == item.ExternalId));
            var existing = loaded.SingleOrDefault();

            ValidatedCommand<商品UpdateCommand>? validated;
            商品SaveCommandMessages errors;
            if (existing == null) {
                // 新規登録
                var createCommand = DbContext.StartCreating商品();
                createCommand.外部システム側ID = item.ExternalId;
                createCommand.商品名 = item.Name;
                createCommand.売値単価_税抜 = item.Price;
                createCommand.消費税区分 = taxType;
                createCommand.TryValidate(out validated, out errors);

            } else {
                // 更新
                existing.商品名 = item.Name;
                existing.売値単価_税抜 = item.Price;
                existing.消費税区分 = taxType;
                existing.TryValidate(out validated, out errors);
            }

            errors.CopyTo(context.Messages);
            if (validated == null || context.ValidationOnly) continue;

            // トランザクションの範囲は商品1件
            await using var tran = await BeginTransactionAsync();

            var result = await DbContext.TrySaveAsync(validated);
            if (!result.Success) {
                result.Errors.CopyTo(context.Messages);
                continue;
            }

            await tran.CommitAsync();
        }

        Log.LogInformation("商品データ取込が完了しました。");
    }
}
