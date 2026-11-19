namespace MyApp;

partial class OverridedApplicationService {
    protected override IQueryable<添付ファイルRefSearchResult> CreateQuerySource(添付ファイルRefSearchCondition searchCondition, OperationContext context) {
        return DbContext.添付ファイルDbSet.Select(e => new 添付ファイルRefSearchResult {
            識別子 = e.識別子,
            名前 = e.名前,
            サイズ = e.サイズ,
            種類 = e.種類,
        });
    }
}
