namespace MyApp;

partial class OverridedApplicationService {
    protected override IQueryable<ジョブ一覧SearchResult> CreateQuerySource(ジョブ一覧SearchCondition searchCondition, OperationContext context) {
        return Filter閲覧できるジョブ(DbContext.ジョブDbSet).Select(j => new ジョブ一覧SearchResult {
            ジョブID = j.ジョブID,
            種類 = j.種類,
            状態 = j.状態,
            起動したユーザー = j.起動したユーザー,
            登録日時 = j.CreatedAt,
            開始日時 = j.開始日時,
            終了日時 = j.終了日時,
            結果メッセージ = j.結果メッセージ,
        });
    }

    /// <summary>
    /// スプールのファイルは DB に保存していないので、ファイル名では絞り込めない。
    /// </summary>
    public override void ValidateSearchCondition(ジョブ一覧SearchCondition searchCondition, ジョブ一覧SearchConditionMessages messages, OperationContext context) {
        base.ValidateSearchCondition(searchCondition, messages, context);
        if (!string.IsNullOrWhiteSpace(searchCondition.Filter?.スプールのファイル?.ファイル名)) {
            messages.AddError(MSG.SpoolFileNameFilterError());
        }
    }

    /// <summary>
    /// 並び順の指定がなければ、新しく登録したジョブから並べる。
    /// 同じ日時に登録されたジョブの並びがページの切り替えで入れ替わらないよう、最後にジョブIDで並べる。
    /// </summary>
    protected override IQueryable<ジョブ一覧SearchResult> AppendOrderByClause(IQueryable<ジョブ一覧SearchResult> query, ジョブ一覧SearchCondition searchCondition) {
        if (searchCondition.Sort.Count > 0) {
            return base.AppendOrderByClause(query, searchCondition);
        }
        return query
            .OrderByDescending(j => j.登録日時)
            .ThenBy(j => j.ジョブID);
    }

    /// <summary>
    /// スプールのファイルは DB に保存していないので、表示中のページのジョブについてだけ、サーバーのフォルダからファイル名を取得する。
    /// </summary>
    protected override IEnumerable<ジョブ一覧DisplayData> OnAfterLoaded(IEnumerable<ジョブ一覧DisplayData> currentPageItems, ジョブ一覧SearchCondition searchCondition, OperationContext context) {
        foreach (var item in currentPageItems) {
            item.スプールのファイル = Getスプールのファイル名(item.ジョブID!)
                .Select(name => new スプールのファイルDisplayData { ファイル名 = name, ExistsInDatabase = true })
                .ToList();
            yield return item;
        }
    }
}
