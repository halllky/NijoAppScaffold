using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute在庫調整登録(在庫調整ParameterDisplayData param, IPresentationContext<在庫調整ParameterMessages> context) {
        // 権限チェック
        if (LoginUser == null || !LoginUser.CanUse入荷登録) {
            context.Messages.AddError("入荷担当のみ実行可能です。");
            return;
        }

        // 入力チェック
        if (param.Values.商品.商品SEQ == null) {
            context.Messages.商品.AddError("商品を選択してください。");
            return;
        }
        if (param.Values.増減数 == null && param.Values.絶対数 == null) {
            context.Messages.AddError("増減数または絶対数のいずれかを指定してください。");
            return;
        }
        if (param.Values.増減数 != null && param.Values.絶対数 != null) {
            context.Messages.AddError("増減数と絶対数の両方を指定することはできません。");
            return;
        }

        // 増減数の計算
        int delta;
        if (param.Values.増減数 != null) {
            delta = param.Values.増減数.Value;
        } else {
            // 現在の在庫数を計算
            var currentInventory = await DbContext.入荷明細DbSet
                .Where(x => x.商品_商品SEQ == param.Values.商品.商品SEQ)
                .SumAsync(x => x.残数量);
            delta = param.Values.絶対数!.Value - currentInventory!.Value;
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
                .Where(x => x.商品_商品SEQ == param.Values.商品.商品SEQ && x.残数量 > 0)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            var totalAvailable = sortedStocks.Sum(x => x.残数量);
            if (totalAvailable < needed) {
                context.Messages.AddError($"在庫不足のため減算できません。（現在在庫: {totalAvailable}, 減算指定: {needed}）");
                return;
            }

            targetStocks = sortedStocks;
        }

        // 保存処理
        // IgnoreConfirm=true のときのみ実行
        if (context.Options.IgnoreConfirm) {
            using var tran = await DbContext.Database.BeginTransactionAsync();

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

            // 在庫調整の登録
            var adjustmentResult = await Create在庫調整Async(new() {
                在庫調整ID = adjustmentId,
                在庫調整日時 = DateOnly.FromDateTime(CurrentTime),
                担当者 = new() { 従業員番号 = LoginUser.従業員番号 },
                商品 = new() { 商品SEQ = param.Values.商品.商品SEQ },
                増減数 = param.Values.増減数,
                絶対数 = param.Values.絶対数,
                在庫調整理由 = param.Values.在庫調整理由,
                在庫調整引当明細 = adjustmentDetails,
            }, context);

            // 在庫調整の登録に失敗した場合は処理中断。
            // コミットせずにトランザクションのスコープを抜けることでロールバックする。
            if (!adjustmentResult.Success) {
                return;
            }

            var hasError = false;
            if (delta > 0) {
                // 増加：新しい入荷明細を作成
                var newStockResult = await Create入荷明細Async(new() {
                    入荷明細ID = Guid.NewGuid().ToString(),
                    入荷 = null,
                    在庫調整 = adjustmentId,
                    商品 = new() { 商品SEQ = param.Values.商品.商品SEQ },
                    仕入単価_税抜 = 0, // 在庫調整のため0とする
                    消費税区分 = 消費税区分.非課税,
                    入荷数量 = delta,
                    残数量 = delta,
                    備考 = $"在庫調整（増加）: {param.Values.在庫調整理由}",
                }, context);

                if (!newStockResult.Success) {
                    hasError = true;
                }

            } else {
                // 減少：既存の入荷明細の更新
                foreach (var (stock, deduct) in allocationPlan) {
                    var updateStockResult = await Update入荷明細Async(stock.入荷明細ID, stock.Version, x => {
                        x.残数量 -= deduct;
                    }, context);

                    if (!updateStockResult.Success) {
                        hasError = true;
                        break;
                    }
                }
            }

            if (!hasError) {
                await tran.CommitAsync();
            }
        }
    }
}
