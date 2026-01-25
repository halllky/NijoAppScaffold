namespace MyApp;

partial class OverridedApplicationService {
    protected override IQueryable<商品在庫増減履歴SearchResult> CreateQuerySource(商品在庫増減履歴SearchCondition searchCondition, IPresentationContext<商品在庫増減履歴SearchConditionMessages> context) {

        // 入荷による在庫増
        var increaseByReceives = DbContext.入荷明細DbSet
            .Where(e => e.在庫調整 == null || e.在庫調整 == "")
            .Select(e => new {
                商品SEQ = e.商品_商品SEQ,
                外部システム側ID = e.商品!.外部システム側ID,
                商品名 = e.商品!.商品名,
                日時 = e.入荷!.入荷日時!,
                増減数 = e.入荷数量 ?? 0,
                事由 = "入荷",
                入荷ID = e.入荷_入荷ID,
                売上SEQ = (int?)null,
                Version = e.Version ?? 0,
            });

        // 在庫調整による在庫増
        var increaseByAdjustments = DbContext.入荷明細DbSet
            .Where(e => e.在庫調整 != null && e.在庫調整 != "")
            .Join(DbContext.在庫調整DbSet,
                e => e.在庫調整,
                adj => adj.在庫調整ID,
                (e, adj) => new {
                    商品SEQ = e.商品_商品SEQ,
                    外部システム側ID = e.商品!.外部システム側ID,
                    商品名 = e.商品!.商品名,
                    日時 = adj.在庫調整日時 ?? e.入荷!.入荷日時!,
                    増減数 = e.入荷数量 ?? 0,
                    事由 = "在庫調整（増）",
                    入荷ID = e.入荷_入荷ID,
                    売上SEQ = (int?)null,
                    Version = e.Version ?? 0,
                });

        // 売上による在庫減
        var decreasesSales = DbContext.引当明細DbSet
            .Select(e => new {
                商品SEQ = e.入荷!.商品_商品SEQ,
                外部システム側ID = e.入荷!.商品!.外部システム側ID,
                商品名 = e.入荷!.商品!.商品名,
                日時 = e.Parent!.Parent!.売上日時,
                増減数 = (e.引当数量 ?? 0) * -1,
                事由 = "売上",
                入荷ID = e.入荷!.入荷_入荷ID,
                売上SEQ = e.Parent!.Parent!.売上SEQ,
                Version = 0,
            });

        // 在庫調整による在庫減
        var decreasesAdjs = DbContext.在庫調整引当明細DbSet
            .Select(e => new {
                商品SEQ = e.入荷明細!.商品_商品SEQ,
                外部システム側ID = e.入荷明細!.商品!.外部システム側ID,
                商品名 = e.入荷明細!.商品!.商品名,
                日時 = e.Parent!.在庫調整日時,
                増減数 = (e.引当数 ?? 0) * -1,
                事由 = "在庫調整（減）",
                入荷ID = e.入荷明細!.入荷_入荷ID,
                売上SEQ = (int?)null,
                Version = e.Parent!.Version ?? 0,
            });

        return increaseByReceives
            .Concat(increaseByAdjustments)
            .Concat(decreasesSales)
            .Concat(decreasesAdjs)
            .Select(x => new 商品在庫増減履歴SearchResult {
                商品SEQ = x.商品SEQ,
                外部システム側ID = x.外部システム側ID,
                商品名 = x.商品名,
                日時 = x.日時,
                増減数 = x.増減数,
                事由 = x.事由,
                入荷ID = x.入荷ID,
                増減履歴引当元売上一覧 = x.売上SEQ.HasValue
                    ? new List<増減履歴引当元売上一覧SearchResult> { new() { 売上SEQ = x.売上SEQ.Value } }
                    : new List<増減履歴引当元売上一覧SearchResult>(),
                Version = x.Version,
            });
    }
}
