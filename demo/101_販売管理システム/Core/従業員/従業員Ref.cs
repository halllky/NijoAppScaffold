

namespace MyApp;

partial class OverridedApplicationService {
    protected override IQueryable<従業員RefSearchResult> CreateQuerySource(従業員RefSearchCondition searchCondition, CommandQueryContext context) {
        return DbContext.従業員DbSet.Select(e => new 従業員RefSearchResult {
            従業員番号 = e.従業員番号,
            氏名 = e.氏名,
        });
    }
}
