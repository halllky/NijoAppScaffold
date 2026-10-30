using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute売上新規登録Async(売上詳細DisplayData param, IPresentationContextWithReturnValue<売上新規登録ReturnValueDisplayData, 売上詳細Messages> context) {
        // 権限チェック
        if (LoginUser == null || !LoginUser.CanUse売上登録) {
            context.Messages.AddError("販売担当のみ実行可能です。");
            return;
        }

        // 入力チェック
        if (param.売上詳細の売上明細.Count == 0) {
            context.Messages.売上詳細の売上明細.AddError("売上明細を1件以上登録してください。");
            return;
        }

        // 在庫引当計算
        var allocationPlans = new List<(売上詳細の売上明細DisplayData Detail, List<(string StockId, int Deduct)> Plan)>();

        // 入荷明細のキャッシュ（商品SEQ -> List<入荷明細DbEntity>）
        var stockCache = new Dictionary<int, List<入荷明細DbEntity>>();
        // 計算用の残数量管理（入荷明細ID -> 残数量）
        var currentStockMap = new Dictionary<string, int>();

        for (var i = 0; i < param.売上詳細の売上明細.Count; i++) {
            var detail = param.売上詳細の売上明細[i];
            var message = context.Messages.売上詳細の売上明細[i];
            var productId = detail.商品.商品SEQ;
            var quantity = detail.売上数量;

            if (productId == null) {
                message.商品.AddError("商品を選択してください。");
                continue;
            }
            if (quantity == null || quantity <= 0) {
                message.売上数量.AddError("売上数量は1以上の整数を入力してください。");
                continue;
            }

            // 在庫取得（キャッシュになければDBから取得）
            if (!stockCache.TryGetValue(productId.Value, out var availableStocks)) {
                var stocks = await DbContext.入荷明細DbSet
                    .Include(x => x.入荷)
                    .Where(x => x.商品_商品SEQ == productId.Value && x.残数量 > 0)
                    .OrderBy(x => x.CreatedAt) // FIFO
                    .ToListAsync();
                availableStocks = stocks;
                stockCache[productId.Value] = availableStocks;

                foreach (var s in stocks) {
                    if (!currentStockMap.ContainsKey(s.入荷明細ID!)) {
                        currentStockMap[s.入荷明細ID!] = s.残数量 ?? 0;
                    }
                }
            }
            // currentStockMap を使って有効在庫を計算
            var totalAvailable = availableStocks.Sum(x => currentStockMap.TryGetValue(x.入荷明細ID!, out var value) ? value : 0);

            if (totalAvailable < quantity) {
                message.売上数量.AddError($"在庫不足です。（現在在庫: {totalAvailable}）");
                continue;
            }

            // 引当計画作成
            var plan = new List<(string StockId, int Deduct)>();
            var needed = quantity.Value;

            foreach (var stock in availableStocks) {
                var currentAmount = currentStockMap.TryGetValue(stock.入荷明細ID!, out var value) ? value : 0;
                if (currentAmount <= 0) continue;

                var deduct = Math.Min(currentAmount, needed);
                plan.Add((stock.入荷明細ID!, deduct));
                // メモリ上の残数量を減らす（次の明細のために）
                currentStockMap[stock.入荷明細ID!] = currentAmount - deduct;
                needed -= deduct;

                if (needed == 0) break;
            }

            allocationPlans.Add((detail, plan));
        }

        if (context.Messages.HasError()) {
            return;
        }

        // 念のため自動計算分の売上総額を再計算しておく
        Simulate(param);

        // 売上ヘッダ
        var command = DbContext.StartCreating売上();
        command.売上日時 = param.売上日時 ?? CurrentTime;
        command.担当者 = new() { 従業員番号 = LoginUser.従業員番号 };
        command.備考 = param.備考;
        command.売上の売上明細 = allocationPlans.Select(x => new 売上の売上明細CreateCommand {
            明細ID = Guid.NewGuid().ToString(),
            商品 = new() { 商品SEQ = x.Detail.商品.商品SEQ },
            区分 = 売上明細区分.売上,
            売上数量 = x.Detail.売上数量,

            // 手修正分があればそちらを優先、なければ自動計算分を使用
            売上総額_税込 = x.Detail.売上総額_税込_手修正 ?? x.Detail.売上総額_税込_自動計算,

            引当明細 = x.Plan.Select(p => new 引当明細CreateCommand {
                入荷 = new() { 入荷明細ID = p.StockId },
                引当数量 = p.Deduct,
            }).ToList(),
        }).ToList();

        var errors = command.GetValidationErrors();
        Validate売上新規(command, errors);
        errors.CopyTo(context.Messages);
        errors.売上の売上明細.CopyTo(context.Messages.売上詳細の売上明細);

        // 入荷明細の残数量更新
        // 読込時点から他者による更新がないことを保証するため、stockCacheにあるVersionを使用する。
        var stockCommands = await LoadForEdit入荷明細の残数量Async(
            stockCache.Values.SelectMany(x => x),
            allocationPlans.SelectMany(x => x.Plan),
            context.Messages);

        if (context.Messages.HasError()) {
            return;
        }
        if (context.ValidationOnly) {
            context.AddConfirm("登録を確定します。よろしいですか？");
            return;
        }

        // 保存処理
        await using var tran = await BeginTransactionAsync();

        var result = await DbContext.TrySaveAsync(command);
        if (!result.Success) {
            result.Errors.CopyTo(context.Messages);
            return; // トランザクションはコミットされずに終了＝ロールバック
        }

        var stockResult = await DbContext.TrySaveAsync(stockCommands);
        if (!stockResult.Success) {
            stockResult.Errors.CopyTo(context.Messages);
            return;
        }
        await Notify在庫数量Async(stockCommands.Select(x => x.商品?.商品SEQ));

        await tran.CommitAsync();
        context.ReturnValue.売上SEQ = result.Updated.売上SEQ;
        context.Messages.AddInfo("売上登録が完了しました。");
    }
}
