using Microsoft.EntityFrameworkCore;

namespace MyApp;

partial class OverridedApplicationService {
    protected override async Task OnExecute売上修正Async(売上詳細DisplayData param, 売上詳細Messages messages, OperationContext context) {
        // 入力チェック
        if (param.売上SEQ == null) {
            messages.AddError(MSG_DEMO101.NotSpecifiedError("売上SEQ"));
            return;
        }

        // 既存データの読み込み
        var command = await DbContext.LoadForEdit売上Async(new 売上KeyWithVersion(param.Version) {
            売上SEQ = param.売上SEQ,
        });
        if (command == null) {
            messages.AddError(MSG.DataNotFoundError());
            return;
        }

        // 整合性チェックに使用するので書き換える前の値を控えておく
        var before = command.売上の売上明細
            .Select(x => new 売上明細の更新前の値(x.明細ID, x.区分, x.商品?.商品SEQ, x.売上数量, x.売上総額_税込))
            .ToArray();

        var allocationPlans = new List<(売上詳細の売上明細DisplayData Detail, int RowIndex, List<(string StockId, int Deduct)> Plan)>();

        // 入荷明細のキャッシュ
        var stockCache = new Dictionary<int, List<入荷明細DbEntity>>();
        var currentStockMap = new Dictionary<string, int>();

        // 新規追加明細の特定と在庫計算
        for (var i = 0; i < param.売上詳細の売上明細.Count; i++) {
            var detail = param.売上詳細の売上明細[i];
            var message = messages.売上詳細の売上明細[i];

            // 既存明細はスキップ
            if (detail.ExistsInDatabase) {
                // 編集されていたらエラー
                if (detail.WillBeChanged || detail.WillBeDeleted) {
                    message.AddError(MSG_DEMO101.ExistingSalesDetailNotEditableError());
                }

                continue;
            }

            var productId = detail.商品.商品SEQ;
            var quantity = detail.売上数量;

            if (productId == null) {
                message.商品.AddError(MSG.SelectRequiredError("商品"));
                continue;
            }
            if (quantity == null || quantity == 0) {
                message.売上数量.AddError(MSG_DEMO101.SalesQuantityNonZeroError());
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
                        message.売上数量.AddError(MSG_DEMO101.InsufficientStockError(totalAvailable.ToString()));
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
                        message.売上数量.AddError(MSG_DEMO101.InsufficientReturnableStockError(totalReturnable.ToString()));
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

            allocationPlans.Add((detail, i, plan));
        }

        if (messages.HasError()) {
            return;
        }

        // 備考の更新
        command.備考 = param.備考;

        // 新規明細の追加
        var addedRows = new List<(int CommandIndex, int RowIndex)>();
        foreach (var (detail, rowIndex, plan) in allocationPlans) {
            addedRows.Add((command.売上の売上明細.Count, rowIndex));
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

        command.TryValidate(out var validated, out var errors);
        Validate売上修正(command, before, errors);
        errors.CopyTo(messages);
        foreach (var (commandIndex, rowIndex) in addedRows) {
            errors.売上の売上明細[commandIndex].CopyTo(messages.売上詳細の売上明細[rowIndex]);
        }

        // 入荷明細の残数量更新
        // 読込時点から他者による更新がないことを保証するため、stockCacheにあるVersionを使用する。
        var stockCommands = await LoadForEdit入荷明細の残数量Async(
            stockCache.Values.SelectMany(x => x),
            allocationPlans.SelectMany(x => x.Plan),
            messages);

        if (validated == null || messages.HasError()) {
            return;
        }
        if (context.ValidationOnly) {
            context.Confirms.Add(MSG_DEMO101.SalesUpdateConfirm());
            return;
        }

        // 保存処理
        await using var tran = await BeginTransactionAsync();

        var updateResult = await DbContext.TrySaveAsync(validated);
        if (!updateResult.Success) {
            updateResult.Errors.CopyTo(messages);
            return;
        }

        var stockResult = await DbContext.TrySaveAsync(stockCommands);
        if (!stockResult.Success) {
            stockResult.Errors.CopyTo(messages);
            return;
        }
        await Notify在庫数量Async(stockResult.Updated.Select(x => x.商品?.商品SEQ));
        await Create担当者への売上修正の通知Async(param.売上SEQ.Value, updateResult.Updated.担当者?.従業員番号);

        await tran.CommitAsync();
        messages.AddInfo(MSG_DEMO101.SalesUpdatedInfo());
    }

    /// <summary>
    /// 担当者以外の人が売上を修正したら、担当者に知らせる。
    /// </summary>
    private async Task Create担当者への売上修正の通知Async(int 売上SEQ, string? 担当者) {
        if (LoginUser == null || 担当者 == null || 担当者 == LoginUser.従業員番号) return;

        await Create通知Async(
            [担当者],
            MSG_DEMO101.SalesUpdatedNotification(売上SEQ.ToString(), LoginUser.氏名),
            PAGES_URL_DEMO101.P101(売上SEQ.ToString()));
    }
}
