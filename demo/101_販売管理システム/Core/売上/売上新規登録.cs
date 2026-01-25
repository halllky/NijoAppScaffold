using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute売上新規登録Async(売上詳細DisplayData param, IPresentationContext<売上詳細Messages> context) {
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
            var productId = detail.Values.商品.商品SEQ;
            var quantity = detail.Values.売上数量;

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
        if (context.ValidationOnly) {
            context.AddConfirm("登録を確定します。よろしいですか？");
            return;
        }

        // 保存処理
        await using var tran = await BeginTransactionAsync();

        // 念のため自動計算分の売上総額を再計算しておく
        Simulate(param);

        // 売上ヘッダ作成
        var result = await Create売上Async(new() {
            売上日時 = param.Values.売上日時 ?? CurrentTime,
            担当者 = new() { 従業員番号 = LoginUser.従業員番号 },
            備考 = param.Values.備考,
            売上の売上明細 = allocationPlans.Select(x => new 売上の売上明細CreateCommand {
                明細ID = Guid.NewGuid().ToString(),
                商品 = new() { 商品SEQ = x.Detail.Values.商品.商品SEQ },
                区分 = 売上明細区分.売上,
                売上数量 = x.Detail.Values.売上数量,

                // 手修正分があればそちらを優先、なければ自動計算分を使用
                売上総額_税込 = x.Detail.Values.売上総額_税込_手修正 ?? x.Detail.Values.売上総額_税込_自動計算,

                引当明細 = x.Plan.Select(p => new 引当明細CreateCommand {
                    入荷 = new() { 入荷明細ID = p.StockId },
                    引当数量 = p.Deduct,
                }).ToList(),
            }).ToList(),
        }, context);

        if (result.Result != DataModelSaveResultType.Completed) return;

        // 入荷明細の残数量更新
        // 同じ入荷明細に対して複数回の更新を行うと楽観排他制御エラーになるため、
        // 入荷明細IDごとに減算数量を集計し、更新処理を1回にまとめる。
        var stockUpdates = allocationPlans
            .SelectMany(x => x.Plan)
            .GroupBy(x => x.StockId)
            .Select(g => new { StockId = g.Key, Deduct = g.Sum(x => x.Deduct) });

        foreach (var update in stockUpdates) {
            var stockId = update.StockId;
            var deduct = update.Deduct;

            // 楽観排他制御のため、stockCacheにあるVersionを使用する。
            // 読込時点から他者による更新がないことを保証するため、Versionは必須。
            var stockEntity = stockCache.Values.SelectMany(x => x).FirstOrDefault(x => x.入荷明細ID == stockId);
            var version = stockEntity?.Version;

            var updateResult = await Update入荷明細Async(stockId, version, x => {
                x.残数量 -= deduct;
            }, context);

            if (updateResult.Result != DataModelSaveResultType.Completed) {
                // エラーメッセージは Update入荷明細Async 内で設定されるはず
                return; // トランザクションはコミットされずに終了＝ロールバック
            }
        }

        await tran.CommitAsync();
    }
}
