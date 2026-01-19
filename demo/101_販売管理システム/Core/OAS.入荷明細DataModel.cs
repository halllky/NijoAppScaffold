
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MyApp;

partial class OverridedApplicationService {

    // 入荷明細の新規登録後処理（SQL発行後、コミット前）
    public override async Task OnAfterCreate入荷明細Async(入荷明細DbEntity newValue, I入荷明細SaveCommandMessages messages, IPresentationContext context) {
        // 最新の在庫数量を商品管理システムに通知する
        var totalStock = await DbContext.入荷明細DbSet
            .Where(x => x.商品_商品SEQ == newValue.商品_商品SEQ)
            .SumAsync(x => x.残数量)
            ?? 0;

        // 外部システム側IDを取得するために商品エンティティをロード
        var shohin = await DbContext.Entry(newValue)
            .Reference(x => x.商品)
            .Query()
            .AsNoTracking()
            .SingleAsync();

        await 商品管理システム.Update在庫数量Async(
            externalProductId: shohin.外部システム側ID!,
            newStockQuantity: totalStock
        );
    }

    // 入荷明細の更新後処理（SQL発行後、コミット前）
    public override async Task OnAfterUpdate入荷明細Async(入荷明細DbEntity newValue, 入荷明細DbEntity oldValue, I入荷明細SaveCommandMessages messages, IPresentationContext context) {
        // 在庫数量が変化した場合、最新の在庫数量を商品管理システムに通知する
        if (newValue.残数量 == oldValue.残数量) {
            Log.LogInformation("在庫数量に変更がないため、商品管理システムへの通知をスキップ（入荷明細: {入荷明細ID}, 商品: {商品SEQ}, {在庫数量}個）",
                newValue.入荷明細ID,
                newValue.商品_商品SEQ,
                newValue.残数量);
            return;
        }

        var totalStock = await DbContext.入荷明細DbSet
            .Where(x => x.商品_商品SEQ == newValue.商品_商品SEQ)
            .SumAsync(x => x.残数量)
            ?? 0;

        // 外部システム側IDを取得するために商品エンティティをロード
        var shohin = await DbContext.Entry(newValue)
            .Reference(x => x.商品)
            .Query()
            .AsNoTracking()
            .SingleAsync();

        await 商品管理システム.Update在庫数量Async(
            externalProductId: shohin.外部システム側ID!,
            newStockQuantity: totalStock
        );
    }
}
