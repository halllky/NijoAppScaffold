using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute商品データ取込(IPresentationContext<MessageSetter> context) {

        // 外部システムとのインターフェースはDIコンテナで解決される
        var shohinKanriSystem = ServiceProvider.GetRequiredService<Core.外部システム.商品管理システム.I商品管理システム>();

        foreach (var item in shohinKanriSystem.Enumerate商品データ()) {
            Log.Info($"処理開始: {item.Name} ({item.ExternalId})");

            // トランザクションの範囲は商品1件
            using var tran = await DbContext.Database.BeginTransactionAsync();

            // 外部システムIDで検索
            var entity = await DbContext.商品DbSet
                .SingleOrDefaultAsync(x => x.外部システム側ID == item.ExternalId);

            // 消費税区分の変換 (文字列 -> Enum)
            if (!Enum.TryParse<消費税区分>(item.TaxType, out var taxType)) {
                Log.Warn($"不正な消費税区分です: {item.TaxType} (商品: {item.Name})");
                continue;
            }

            if (entity == null) {
                // 新規登録
                await Create商品Async(new 商品CreateCommand {
                    外部システム側ID = item.ExternalId,
                    商品名 = item.Name,
                    売値単価_税抜 = item.Price,
                    消費税区分 = taxType,
                }, context);

            } else {
                // 更新
                // バッチ処理なので楽観排他制御は行わず、現在のバージョンを取得して更新する
                await Update商品Async(entity.商品SEQ, entity.Version, command => {
                    command.商品名 = item.Name;
                    command.売値単価_税抜 = item.Price;
                    command.消費税区分 = taxType;
                }, context);
            }

            if (!context.HasError()) {
                await tran.CommitAsync();
            }
        }

        Log.Info("商品データ取込が完了しました。");
    }
}
