namespace MyApp;

partial class OverridedApplicationService {
    protected override IQueryable<売上一覧SearchResult> CreateQuerySource(売上一覧SearchCondition searchCondition, IPresentationContext<売上一覧SearchConditionMessages> context) {
        return DbContext.売上DbSet.Select(x => new 売上一覧SearchResult {
            売上SEQ = x.売上SEQ,
            売上日時 = x.売上日時,
            担当者_従業員番号 = x.担当者!.従業員番号,
            担当者_氏名 = x.担当者!.氏名,
            備考 = x.備考,
            合計金額 = x.売上の売上明細!.Sum(d => d.売上総額_税込),
            売上数量合計 = x.売上の売上明細!.Sum(d => d.売上数量),
        });
    }
}
