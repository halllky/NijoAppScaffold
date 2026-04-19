using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute売上修正Async(売上詳細DisplayData param, IPresentationContext<売上詳細Messages> context) {
        // 権限チェック
        if (LoginUser == null || !LoginUser.CanUse売上登録) {
            context.Messages.AddError("販売担当のみ実行可能です。");
            return;
        }

        // 入力チェック
        if (param.売上SEQ == null) {
            context.Messages.AddError("売上SEQが指定されていません。");
            return;
        }

        // 既存データの取得（存在確認とバージョンチェックのため）
        // Update売上Async 内でも取得されるが、ここでは事前の在庫計算のために必要
        // また、既存明細のUUIDを知る必要がある（UpdateCommand構築のため）
        // しかし、Update売上Async の updater に渡される command は FromDbEntity で作られるので、
        // 既存明細の情報はそこに含まれている。
        // ここでは在庫計算だけできればよい。

        var allocationPlans = new List<(売上詳細の売上明細DisplayData Detail, List<(string StockId, int Deduct)> Plan)>();

        // 入荷明細のキャッシュ
        var stockCache = new Dictionary<int, List<入荷明細DbEntity>>();
        var currentStockMap = new Dictionary<string, int>();

        // 新規追加明細の特定と在庫計算
        for (var i = 0; i < param.売上詳細の売上明細.Count; i++) {
            var detail = param.売上詳細の売上明細[i];
            var message = context.Messages.売上詳細の売上明細[i];

            // 既存明細はスキップ
            if (detail.ExistsInDatabase) {
                // 編集されていたらエラー
                if (detail.WillBeChanged || detail.WillBeDeleted) {
                    message.AddError("既存の売上明細の編集・削除はできません。新規追加のみ可能です。");
                }

                continue;
            }

            var productId = detail.商品.商品SEQ;
            var quantity = detail.売上数量;

            if (productId == null) {
                message.商品.AddError("商品を選択してください。");
                continue;
            }
            if (quantity == null || quantity == 0) {
                message.売上数量.AddError("売上数量は0以外の整数を入力してください。");
                continue;
            }

            // 在庫取得
            if (!stockCache.TryGetValue(productId.Value, out var stocksForProduct)) {
                stocksForProduct = new List<入荷明細DbEntity>();
                stockCache[productId.Value] = stocksForProduct;
            }

            var plan = new List<(string StockId, int Deduct)>();

            if (quantity > 0) {
                // 売上：在庫引当 (FIFO)
                var needed = quantity.Value;

                while (needed > 0) {
                    // メモリ上の在庫から引当可能なものを探す
                    var availableStocks = stocksForProduct
                        .Where(x => (currentStockMap.ContainsKey(x.入荷明細ID!) ? currentStockMap[x.入荷明細ID!] : 0) > 0)
                        .OrderBy(x => x.CreatedAt)
                        .ToList();

                    foreach (var stock in availableStocks) {
                        var currentAmount = currentStockMap.TryGetValue(stock.入荷明細ID!, out var value) ? value : 0;
                        if (currentAmount <= 0) continue;

                        var deduct = Math.Min(currentAmount, needed);
                        plan.Add((stock.入荷明細ID!, deduct));
                        currentStockMap[stock.入荷明細ID!] = currentAmount - deduct;
                        needed -= deduct;
                        if (needed == 0) break;
                    }

                    if (needed == 0) break;

                    // 不足分をDBから追加取得
                    var loadedIds = stocksForProduct.Select(x => x.入荷明細ID).ToList();
                    var newStocks = await DbContext.入荷明細DbSet
                        .Include(x => x.入荷)
                        .Where(x => x.商品_商品SEQ == productId.Value)
                        .Where(x => !loadedIds.Contains(x.入荷明細ID))
                        .OrderBy(x => x.CreatedAt) // FIFO
                        .Take(10) // 10件ずつ取得
                        .ToListAsync();

                    if (newStocks.Count == 0) {
                        // これ以上在庫がない
                        var totalAvailable = stocksForProduct.Sum(x => currentStockMap.TryGetValue(x.入荷明細ID!, out var value) ? value : 0);
                        message.売上数量.AddError($"在庫不足です。（現在在庫: {totalAvailable}）");
                        break;
                    }

                    // キャッシュに追加
                    foreach (var s in newStocks) {
                        stocksForProduct.Add(s);
                        if (!currentStockMap.ContainsKey(s.入荷明細ID!)) {
                            currentStockMap[s.入荷明細ID!] = s.残数量 ?? 0;
                        }
                    }
                }
            } else {
                // 取消：在庫戻し (LIFO)
                var returnAmount = -quantity.Value; // 正の値
                var needed = returnAmount;

                while (needed > 0) {
                    // メモリ上の在庫から戻し可能なものを探す
                    var returnableStocks = stocksForProduct
                        .Where(x => (x.入荷数量 ?? 0) - (currentStockMap.TryGetValue(x.入荷明細ID!, out var value) ? value : 0) > 0)
                        .OrderByDescending(x => x.CreatedAt) // LIFO
                        .ToList();

                    foreach (var stock in returnableStocks) {
                        var currentAmount = currentStockMap.TryGetValue(stock.入荷明細ID!, out var value) ? value : 0;
                        var maxReturn = (stock.入荷数量 ?? 0) - currentAmount;
                        if (maxReturn <= 0) continue;

                        var deduct = -Math.Min(maxReturn, needed); // 負の値（在庫を増やす）
                        plan.Add((stock.入荷明細ID!, deduct));

                        currentStockMap[stock.入荷明細ID!] = currentAmount - deduct; // マイナスを引く＝足す
                        needed += deduct; // neededは正、deductは負
                        if (needed == 0) break;
                    }

                    if (needed == 0) break;

                    // 不足分をDBから追加取得
                    var loadedIds = stocksForProduct.Select(x => x.入荷明細ID).ToList();
                    var newStocks = await DbContext.入荷明細DbSet
                        .Include(x => x.入荷)
                        .Where(x => x.商品_商品SEQ == productId.Value)
                        .Where(x => !loadedIds.Contains(x.入荷明細ID))
                        .OrderByDescending(x => x.CreatedAt) // LIFO
                        .Take(10) // 10件ずつ取得
                        .ToListAsync();

                    if (newStocks.Count == 0) {
                        // これ以上戻せる在庫がない
                        var totalReturnable = stocksForProduct.Sum(x => (x.入荷数量 ?? 0) - (currentStockMap.TryGetValue(x.入荷明細ID!, out var value) ? value : 0));
                        message.売上数量.AddError($"取消可能な在庫引当履歴が不足しています。（取消可能数: {totalReturnable}）");
                        break;
                    }

                    // キャッシュに追加
                    foreach (var s in newStocks) {
                        stocksForProduct.Add(s);
                        if (!currentStockMap.ContainsKey(s.入荷明細ID!)) {
                            currentStockMap[s.入荷明細ID!] = s.残数量 ?? 0;
                        }
                    }
                }
            }

            if (message.HasError()) continue;

            allocationPlans.Add((detail, plan));
        }

        if (context.Messages.HasError()) {
            return;
        }
        if (context.ValidationOnly) {
            context.AddConfirm("修正を確定します。よろしいですか？");
            return;
        }

        // 保存処理
        await using var tran = await BeginTransactionAsync();

        var updateResult = await Update売上Async(param.売上SEQ, null, command => {
            // 備考の更新
            command.備考 = param.備考;

            // 新規明細の追加
            foreach (var (detail, plan) in allocationPlans) {
                command.売上の売上明細.Add(new() {
                    明細ID = Guid.NewGuid().ToString(),
                    商品 = new() { 商品SEQ = detail.商品.商品SEQ },
                    区分 = detail.区分,
                    売上数量 = detail.売上数量,

                    // 手修正があればそちらを優先
                    売上総額_税込 = detail.売上総額_税込_手修正 ?? detail.売上総額_税込_自動計算,

                    引当明細 = plan.Select(p => new 引当明細UpdateCommand {
                        入荷 = new() { 入荷明細ID = p.StockId },
                        引当数量 = p.Deduct,
                    }).ToList(),
                });
            }
        }, context);

        if (!updateResult.IsSaveCompleted()) return;

        // 入荷明細の残数量更新
        foreach (var (detail, plan) in allocationPlans) {
            foreach (var (stockId, deduct) in plan) {
                var stockEntity = stockCache.Values.SelectMany(x => x).FirstOrDefault(x => x.入荷明細ID == stockId);
                var version = stockEntity?.Version;

                var updateStockResult = await Update入荷明細Async(stockId, version, x => {
                    x.残数量 -= deduct; // deductが負の場合は残数量が増える
                }, context);

                if (!updateStockResult.IsSaveCompleted()) {
                    return;
                }
            }
        }

        await tran.CommitAsync();
        context.Messages.AddInfo("売上修正が完了しました。");
    }
}
