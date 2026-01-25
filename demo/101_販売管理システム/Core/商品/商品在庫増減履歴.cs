namespace MyApp;

partial class OverridedApplicationService {
    protected override IQueryable<商品在庫増減履歴SearchResult> CreateQuerySource(商品在庫増減履歴SearchCondition searchCondition, IPresentationContext<商品在庫増減履歴SearchConditionMessages> context) {
        var increases = DbContext.入荷明細DbSet
            .GroupJoin(DbContext.在庫調整DbSet,
                e => e.在庫調整,
                adj => adj.在庫調整ID,
                (e, adjs) => new { e, adjs })
            .SelectMany(
                x => x.adjs.DefaultIfEmpty(),
                (x, adj) => new {
                    商品SEQ = x.e.商品_商品SEQ,
                    外部システム側ID = x.e.商品!.外部システム側ID,
                    商品名 = x.e.商品!.商品名,
                    日時 = adj!.在庫調整日時 ?? x.e.入荷!.入荷日時!,
                    増減数 = x.e.入荷数量 ?? 0,
                    事由 = x.e.入荷 != null ? "入荷" : (adj != null ? "在庫調整" : x.e.備考) ?? "",
                    入荷ID = x.e.入荷_入荷ID,
                    売上SEQ = (int?)null,
                    Version = x.e.Version ?? 0,
                });

        var decreasesSales = DbContext.引当明細DbSet
            .Select(e => new {
                商品SEQ = e.入荷!.商品_商品SEQ,
                外部システム側ID = e.入荷!.商品!.外部システム側ID,
                商品名 = e.入荷!.商品!.商品名,
                日時 = e.Parent!.Parent!.売上日時,
                増減数 = -1 * (e.引当数量 ?? 0),
                事由 = "売上",
                入荷ID = e.入荷!.入荷_入荷ID,
                売上SEQ = e.Parent!.Parent!.売上SEQ,
                Version = 0,
            });

        var decreasesAdjs = DbContext.在庫調整引当明細DbSet
            .Select(e => new {
                商品SEQ = e.入荷明細!.商品_商品SEQ,
                外部システム側ID = e.入荷明細!.商品!.外部システム側ID,
                商品名 = e.入荷明細!.商品!.商品名,
                日時 = e.Parent!.在庫調整日時,
                増減数 = -1 * (e.引当数 ?? 0),
                事由 = "在庫調整",
                入荷ID = e.入荷明細!.入荷_入荷ID,
                売上SEQ = (int?)null,
                Version = e.Parent!.Version ?? 0,
            });

        return increases
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
