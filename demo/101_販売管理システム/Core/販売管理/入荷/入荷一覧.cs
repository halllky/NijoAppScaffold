
namespace MyApp;

partial class OverridedApplicationService {
    protected override IQueryable<入荷一覧SearchResult> CreateQuerySource(入荷一覧SearchCondition searchCondition, IPresentationContext<入荷一覧SearchConditionMessages> context) {
        return DbContext.入荷DbSet.Select(e => new 入荷一覧SearchResult {
            入荷ID = e.入荷ID,
            入荷日時 = e.入荷日時,
            担当者_従業員番号 = e.担当者!.従業員番号,
            担当者_氏名 = e.担当者!.氏名,
            備考 = e.備考,
            入荷数量合計 = e.RefFrom入荷明細_入荷!.Sum(d => d.入荷数量),
            明細件数 = e.RefFrom入荷明細_入荷!.Count(),
        });
    }
}
