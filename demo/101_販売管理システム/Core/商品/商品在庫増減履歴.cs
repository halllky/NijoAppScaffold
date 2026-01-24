namespace MyApp;

partial class OverridedApplicationService {
    protected override IQueryable<商品在庫増減履歴SearchResult> CreateQuerySource(商品在庫増減履歴SearchCondition searchCondition, IPresentationContext<商品在庫増減履歴SearchConditionMessages> context) {
        return DbContext.入荷明細DbSet.Select(e => new 商品在庫増減履歴SearchResult {
            商品SEQ = e.商品_商品SEQ,
            外部システム側ID = e.商品!.外部システム側ID,
            商品名 = e.商品!.商品名,
            日時 = e.入荷!.入荷日時,
            増減数 = e.入荷数量,
            事由 = e.備考,
            入荷ID = e.入荷_入荷ID,
            増減履歴引当元売上一覧 = e.RefFrom引当明細_入荷.Select(x => new 増減履歴引当元売上一覧SearchResult {
                売上SEQ = x.Parent_Parent_売上SEQ,
            }).ToList(),
            Version = e.Version!.Value,
        });
    }
}
