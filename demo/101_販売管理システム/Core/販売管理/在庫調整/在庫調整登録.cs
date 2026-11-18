using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    protected override async Task OnExecute在庫調整登録Async(在庫調整ParameterDisplayData param, 在庫調整ParameterMessages messages, OperationContext context) {
        // 入力チェック: 商品
        // ※ 自動生成されたコードでもチェックされるが、商品のキーがないとこのあとの処理が成り立たないので先にチェックする。
        if (param.商品.商品SEQ == null) {
            messages.商品.AddError("商品を選択してください。");
            return;
        }

        // 入力チェック: 増減数・絶対数
        // ※ 「どちらか片方のみ」というのは自動生成されたコードでは表現できないので、ここでチェックする。
        if (param.増減数 == null && param.絶対数 == null) {
            messages.AddError("増減数または絶対数のいずれかを指定してください。");
            return;
        }
        if (param.増減数 != null && param.絶対数 != null) {
            messages.AddError("増減数と絶対数の両方を指定することはできません。");
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
            messages.AddError("在庫数に変更がありません。");
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
                messages.AddError($"在庫不足のため減算できません。（現在在庫: {totalAvailable}, 減算指定: {needed}）");
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
        adjustment.担当者 = new() { 従業員番号 = LoginUser?.従業員番号 };
        adjustment.商品 = new() { 商品SEQ = param.商品.商品SEQ };
        adjustment.増減数 = param.増減数;
        adjustment.絶対数 = param.絶対数;
        adjustment.在庫調整理由 = param.在庫調整理由;
        adjustment.在庫調整引当明細 = adjustmentDetails;
        adjustment.TryValidate(out var validatedAdjustment, out var adjustmentErrors);
        adjustmentErrors.CopyTo(messages);

        // 増加：新しい入荷明細を作成
        ValidatedCommand<入荷明細UpdateCommand>? validatedNewStock = null;
        if (delta > 0) {
            var newStock = DbContext.StartCreating入荷明細();
            newStock.入荷明細ID = Guid.NewGuid().ToString();
            newStock.入荷 = null;
            newStock.在庫調整 = adjustmentId;
            newStock.商品 = new() { 商品SEQ = param.商品.商品SEQ };
            newStock.仕入単価_税抜 = 0; // 在庫調整のため0とする
            newStock.消費税区分 = 消費税区分.非課税;
            newStock.入荷数量 = delta;
            newStock.残数量 = delta;
            newStock.備考 = $"在庫調整（増加）: {param.在庫調整理由}";
            newStock.TryValidate(out validatedNewStock, out var newStockErrors);
            newStockErrors.CopyTo(messages);
        }

        // 減少：既存の入荷明細の更新
        var stockCommands = await LoadForEdit入荷明細の残数量Async(
            allocationPlan.Select(x => x.Stock),
            allocationPlan.Select(x => (x.Stock.入荷明細ID!, x.Deduct)),
            messages);

        if (validatedAdjustment == null || messages.HasError() || context.ValidationOnly) return;

        // 保存処理。
        // 失敗した場合はコミットせずにトランザクションのスコープを抜けることでロールバックする。
        await using var tran = await BeginTransactionAsync();

        var adjustmentResult = await DbContext.TrySaveAsync(validatedAdjustment);
        if (!adjustmentResult.Success) {
            adjustmentResult.Errors.CopyTo(messages);
            return;
        }

        if (validatedNewStock != null) {
            var newStockResult = await DbContext.TrySaveAsync(validatedNewStock);
            if (!newStockResult.Success) {
                newStockResult.Errors.CopyTo(messages);
                return;
            }
        }

        var stockResult = await DbContext.TrySaveAsync(stockCommands);
        if (!stockResult.Success) {
            stockResult.Errors.CopyTo(messages);
            return;
        }
        await Notify在庫数量Async([param.商品.商品SEQ]);
        await Create入荷担当への在庫調整の通知Async(param.商品.商品SEQ.Value, delta);

        await tran.CommitAsync();
    }

    /// <summary>
    /// 在庫を調整したら、調整した本人以外の、入荷担当の権限をもつ全員に知らせる。
    /// システム管理者はすべての権限をもつとみなされるが、知らせるのは入荷担当の権限を明示的にもつ人だけにする。
    /// </summary>
    private async Task Create入荷担当への在庫調整の通知Async(int 商品SEQ, int 増減数) {
        if (LoginUser == null) return;
        var 調整した人 = LoginUser.従業員番号;

        var 宛先一覧 = await DbContext.従業員DbSet
            .Where(e => e.従業員番号 != 調整した人
                     && e.従業員の権限.Any(p => p.権限区分 == 権限区分.入荷担当))
            .Select(e => e.従業員番号!)
            .ToListAsync();
        var 商品名 = await DbContext.商品DbSet
            .Where(x => x.商品SEQ == 商品SEQ)
            .Select(x => x.商品名)
            .SingleAsync();

        await Create通知Async(
            宛先一覧,
            $"商品「{商品名}」の在庫が {LoginUser.氏名} さんにより調整されました（{増減数:+#;-#}個）。",
            PAGES_URL_DEMO101.P301(商品SEQ));
    }
}
