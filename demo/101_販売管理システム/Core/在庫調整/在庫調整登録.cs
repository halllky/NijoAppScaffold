using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute在庫調整登録Async(在庫調整ParameterDisplayData param, IPresentationContext<在庫調整ParameterMessages> context) {
        // 権限チェック
        if (LoginUser == null || !LoginUser.CanUse入荷登録) {
            context.Messages.AddError("入荷担当のみ実行可能です。");
            return;
        }

        // 入力チェック: 商品
        // ※ 自動生成されたコードでもチェックされるが、商品のキーがないとこのあとの処理が成り立たないので先にチェックする。
        if (param.商品.商品SEQ == null) {
            context.Messages.商品.AddError("商品を選択してください。");
            return;
        }

        // 入力チェック: 増減数・絶対数
        // ※ 「どちらか片方のみ」というのは自動生成されたコードでは表現できないので、ここでチェックする。
        if (param.増減数 == null && param.絶対数 == null) {
            context.Messages.AddError("増減数または絶対数のいずれかを指定してください。");
            return;
        }
        if (param.増減数 != null && param.絶対数 != null) {
            context.Messages.AddError("増減数と絶対数の両方を指定することはできません。");
            return;
        }

        // 増減数の計算
        int delta;
        if (param.増減数 != null) {
            delta = param.増減数.Value;
        } else {
            // 現在の在庫数を計算
            var currentInventory = await DbContext.入荷明細DbSet
                .Where(x => x.商品_商品SEQ == param.商品.商品SEQ)
                .SumAsync(x => x.残数量);
            delta = param.絶対数!.Value - currentInventory!.Value;
        }

        if (delta == 0) {
            context.Messages.AddError("在庫数に変更がありません。");
            return;
        }

        // 減少の場合の在庫不足チェック
        List<入荷明細DbEntity> targetStocks = new();
        if (delta < 0) {
            var needed = -delta;

            // 引当対象の入荷明細を取得（FIFO）
            // 入荷日時は登録漏れや過去日の登録により順序が狂うことがあるため、システム登録日時 CreatedAt を使用する。
            var sortedStocks = await DbContext.入荷明細DbSet
                .Include(x => x.入荷)
                .Where(x => x.商品_商品SEQ == param.商品.商品SEQ && x.残数量 > 0)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            var totalAvailable = sortedStocks.Sum(x => x.残数量);
            if (totalAvailable < needed) {
                context.Messages.AddError($"在庫不足のため減算できません。（現在在庫: {totalAvailable}, 減算指定: {needed}）");
                return;
            }

            targetStocks = sortedStocks;
        }

        // 在庫調整
        var adjustmentId = Guid.NewGuid().ToString();
        var allocationPlan = new List<(入荷明細DbEntity Stock, int Deduct)>();
        var adjustmentDetails = new List<在庫調整引当明細CreateCommand>();

        if (delta < 0) {
            // 減少：引当計画の作成
            var needed = -delta;
            foreach (var stock in targetStocks) {
                var deduct = Math.Min(stock.残数量!.Value, needed);

                allocationPlan.Add((stock, deduct));
                adjustmentDetails.Add(new() {
                    入荷明細 = new() { 入荷明細ID = stock.入荷明細ID },
                    引当数 = deduct,
                });

                needed -= deduct;
                if (needed == 0) break;
            }
        }

        var adjustment = DbContext.StartCreating在庫調整();
        adjustment.在庫調整ID = adjustmentId;
        adjustment.在庫調整日時 = CurrentTime;
        adjustment.担当者 = new() { 従業員番号 = LoginUser.従業員番号 };
        adjustment.商品 = new() { 商品SEQ = param.商品.商品SEQ };
        adjustment.増減数 = param.増減数;
        adjustment.絶対数 = param.絶対数;
        adjustment.在庫調整理由 = param.在庫調整理由;
        adjustment.在庫調整引当明細 = adjustmentDetails;
        adjustment.GetValidationErrors().CopyTo(context.Messages);

        // 増加：新しい入荷明細を作成
        入荷明細CreateCommand? newStock = null;
        if (delta > 0) {
            newStock = DbContext.StartCreating入荷明細();
            newStock.入荷明細ID = Guid.NewGuid().ToString();
            newStock.入荷 = null;
            newStock.在庫調整 = adjustmentId;
            newStock.商品 = new() { 商品SEQ = param.商品.商品SEQ };
            newStock.仕入単価_税抜 = 0; // 在庫調整のため0とする
            newStock.消費税区分 = 消費税区分.非課税;
            newStock.入荷数量 = delta;
            newStock.残数量 = delta;
            newStock.備考 = $"在庫調整（増加）: {param.在庫調整理由}";
            newStock.GetValidationErrors().CopyTo(context.Messages);
        }

        // 減少：既存の入荷明細の更新
        var stockCommands = await LoadForEdit入荷明細の残数量Async(
            allocationPlan.Select(x => x.Stock),
            allocationPlan.Select(x => (x.Stock.入荷明細ID!, x.Deduct)),
            context.Messages);

        if (context.Messages.HasError() || context.ValidationOnly) return;

        // 保存処理。
        // 失敗した場合はコミットせずにトランザクションのスコープを抜けることでロールバックする。
        await using var tran = await BeginTransactionAsync();

        var adjustmentResult = await DbContext.TrySaveAsync(adjustment);
        if (!adjustmentResult.Success) {
            adjustmentResult.Errors.CopyTo(context.Messages);
            return;
        }

        if (newStock != null) {
            var newStockResult = await DbContext.TrySaveAsync(newStock);
            if (!newStockResult.Success) {
                newStockResult.Errors.CopyTo(context.Messages);
                return;
            }
        }

        var stockResult = await DbContext.TrySaveAsync(stockCommands);
        if (!stockResult.Success) {
            stockResult.Errors.CopyTo(context.Messages);
            return;
        }
        await Notify在庫数量Async([param.商品.商品SEQ]);

        await tran.CommitAsync();
    }
}
