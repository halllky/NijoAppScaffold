
using Microsoft.EntityFrameworkCore;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// 在庫引当の結果を入荷明細の残数量に反映するためのコマンドを読み込んで検証します。
    /// 同じ入荷明細に対して複数回の更新を行うと楽観排他エラーになるため、入荷明細ごとに集計して1コマンドにまとめます。
    /// </summary>
    /// <param name="stocks">在庫引当の計算に使用した入荷明細。読み込み時点からの他者の更新の有無の判定に使うのでバージョンを読み込み済みであること。</param>
    /// <param name="plans">引当内容。負の引当数量は在庫を増やす。</param>
    /// <param name="messages">読み込めなかった場合と検証エラーの付与先</param>
    private async Task<List<ValidatedCommand<入荷明細UpdateCommand>>> LoadForEdit入荷明細の残数量Async(
        IEnumerable<入荷明細DbEntity> stocks,
        IEnumerable<(string StockId, int Deduct)> plans,
        IMessageSetter messages) {

        var deducts = plans
            .GroupBy(x => x.StockId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Deduct));
        var keys = stocks
            .DistinctBy(x => x.入荷明細ID)
            .Where(x => deducts.GetValueOrDefault(x.入荷明細ID!) != 0)
            .Select(x => new 入荷明細KeyWithVersion(x.Version) {
                入荷明細ID = x.入荷明細ID,
            })
            .ToArray();

        var loaded = await DbContext.LoadForEdit入荷明細Async(keys);

        var commands = new List<ValidatedCommand<入荷明細UpdateCommand>>();
        foreach (var key in keys) {
            if (!loaded.TryGetValue(key, out var command)) {
                messages.AddError(MSG.DataNotFoundError());
                continue;
            }
            command.残数量 -= deducts[key.入荷明細ID!];
            if (command.TryValidate(out var validated, out var errors)) {
                commands.Add(validated);
            }
            errors.CopyTo(messages);
        }
        return commands;
    }

    /// <summary>
    /// 最新の在庫数量を商品管理システムに通知します。
    /// 入荷明細の残数量が変わる保存処理を行なったら、そのトランザクションの中で必ず呼んでください。
    /// </summary>
    private async Task Notify在庫数量Async(IEnumerable<int?> 商品SEQ一覧) {
        foreach (var 商品SEQ in 商品SEQ一覧.Distinct()) {
            var totalStock = await DbContext.入荷明細DbSet
                .Where(x => x.商品_商品SEQ == 商品SEQ)
                .SumAsync(x => x.残数量)
                ?? 0;

            // 外部システム側IDを取得するために商品エンティティをロード
            var shohin = await DbContext.商品DbSet
                .AsNoTracking()
                .SingleAsync(x => x.商品SEQ == 商品SEQ);

            商品管理システム.Update在庫数量(
                externalProductId: shohin.外部システム側ID!,
                newStockQuantity: totalStock
            );
        }
    }
}
