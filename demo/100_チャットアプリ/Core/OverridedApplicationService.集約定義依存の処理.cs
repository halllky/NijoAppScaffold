using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp;

partial class OverridedApplicationService {

    #region アカウント
    /// <summary>
    /// アカウントの画面表示用データをデータベースのどの項目から取得するかの定義
    /// </summary>
    protected override IQueryable<アカウントViewSearchResult> CreateQuerySource(アカウントViewSearchCondition searchCondition, IPresentationContext<アカウントViewSearchConditionMessages> context) {
        throw new NotImplementedException();
    }
    #endregion アカウント

    #region チャンネル
    /// <summary>
    /// チャンネルの画面表示用データをデータベースのどの項目から取得するかの定義
    /// </summary>
    protected override IQueryable<チャンネル画面SearchResult> CreateQuerySource(チャンネル画面SearchCondition searchCondition, IPresentationContext<チャンネル画面SearchConditionMessages> context) {
        return DbContext.チャンネルDbSet.Select(e => new チャンネル画面SearchResult {
            チャンネルID = e.チャンネルID,
            チャンネル名 = e.チャンネル名,
            メッセージ一覧 = e.RefFromメッセージ_チャンネル.Select(x => new メッセージ一覧SearchResult {
                メッセージ_メッセージSEQ = x.メッセージSEQ,
                メッセージ_本文 = x.本文,
                メッセージ_チャンネル直下か = x.チャンネル直下か,
                メッセージ_編集済みか = x.編集済みか,
                メッセージ_記載者_アカウントID = x.記載者_アカウントID,
                メッセージ_記載者_アカウント名 = x.記載者!.アカウント名,
            }).ToList(),
            Version = (int)e.Version!,
        });
    }
    public override Task<メッセージ追加読み込みReturnValue> Execute(メッセージ追加読み込みParameter param, IPresentationContext<メッセージ追加読み込みParameterMessages> context) {
        throw new NotImplementedException();
    }
    #endregion チャンネル

    #region スレッド
    /// <summary>
    /// スレッドの画面表示用データをデータベースのどの項目から取得するかの定義
    /// </summary>
    protected override IQueryable<スレッド詳細SearchResult> CreateQuerySource(スレッド詳細SearchCondition searchCondition, IPresentationContext<スレッド詳細SearchConditionMessages> context) {
        throw new NotImplementedException();
    }
    #endregion スレッド

    #region メッセージ
    /// <summary>
    /// メッセージの画面表示用データをデータベースのどの項目から取得するかの定義
    /// </summary>
    protected override IQueryable<メッセージViewSearchResult> CreateQuerySource(メッセージViewSearchCondition searchCondition, IPresentationContext<メッセージViewSearchConditionMessages> context) {
        throw new NotImplementedException();
    }
    public override Task<object> Execute(新規投稿Parameter param, IPresentationContext<新規投稿ParameterMessages> context) {
        throw new NotImplementedException();
    }
    public override Task<object> Execute(メッセージViewDisplayData param, IPresentationContext<メッセージViewDisplayDataMessages> context) {
        throw new NotImplementedException();
    }
    #endregion メッセージ

}
