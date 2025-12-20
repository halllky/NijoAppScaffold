
namespace MyApp;

partial class OverridedApplicationService {
    protected override IQueryable<従業員マスタSearchResult> CreateQuerySource(従業員マスタSearchCondition searchCondition, IPresentationContext<従業員マスタSearchConditionMessages> context) {
        return DbContext.従業員DbSet.Select(e => new 従業員マスタSearchResult {
            従業員番号 = e.従業員番号,
            氏名 = e.氏名,
            入荷担当 = e.入荷担当,
            販売担当 = e.販売担当,
            システム管理者 = e.システム管理者,
            Version = e.Version!.Value,
        });
    }
}
