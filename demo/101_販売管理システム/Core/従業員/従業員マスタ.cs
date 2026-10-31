
namespace MyApp;

partial class OverridedApplicationService {
    protected override IQueryable<従業員マスタSearchResult> CreateQuerySource(従業員マスタSearchCondition searchCondition, IPresentationContext<従業員マスタSearchConditionMessages> context) {
        return DbContext.従業員DbSet.Select(e => new 従業員マスタSearchResult {
            従業員番号 = e.従業員番号,
            氏名 = e.氏名,
            権限 = e.従業員の権限.Select(x => new 従業員マスタの権限SearchResult { 権限区分 = x.権限区分 }).ToList(),
            Version = e.Version!.Value,
        });
    }
}
