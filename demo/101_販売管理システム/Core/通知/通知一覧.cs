namespace MyApp;

partial class OverridedApplicationService {
    protected override IQueryable<通知一覧SearchResult> CreateQuerySource(通知一覧SearchCondition searchCondition, OperationContext context) {
        return Filter自分宛て(DbContext.通知DbSet).Select(n => new 通知一覧SearchResult {
            通知ID = n.通知ID,
            本文 = n.本文,
            リンク先 = n.リンク先,
            既読日時 = n.既読日時,
            作成日時 = n.CreatedAt,
        });
    }

    /// <summary>
    /// 並び順の指定がなければ、新しい通知から並べる。
    /// 同じ日時に作られた通知の並びがページの切り替えで入れ替わらないよう、最後に通知IDで並べる。
    /// </summary>
    protected override IQueryable<通知一覧SearchResult> AppendOrderByClause(IQueryable<通知一覧SearchResult> query, 通知一覧SearchCondition searchCondition) {
        if (searchCondition.Sort.Count > 0) {
            return base.AppendOrderByClause(query, searchCondition);
        }
        return query
            .OrderByDescending(n => n.作成日時)
            .ThenBy(n => n.通知ID);
    }
}
