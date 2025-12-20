


namespace MyApp;

partial class OverridedApplicationService {
    protected override IQueryable<商品一覧SearchResult> CreateQuerySource(商品一覧SearchCondition searchCondition, IPresentationContext<商品一覧SearchConditionMessages> context) {
        return DbContext.商品DbSet.Select(e => new 商品一覧SearchResult {
            商品SEQ = e.商品SEQ,
            外部システム側ID = e.外部システム側ID,
            商品名 = e.商品名,
            売値単価_税抜 = e.売値単価_税抜,
            消費税区分 = e.消費税区分,
            在庫数 = e.RefFrom入荷明細_商品.Sum(x => x.残数量),
            Version = e.Version!.Value,
        });
    }
}
