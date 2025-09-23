using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp;

partial class OverridedApplicationService {

    #region アカウント
    protected override IQueryable<アカウントViewSearchResult> CreateQuerySource(アカウントViewSearchCondition searchCondition, IPresentationContext<アカウントViewSearchConditionMessages> context) {
        throw new NotImplementedException();
    }
    #endregion アカウント

    #region チャンネル
    protected override IQueryable<チャンネル画面SearchResult> CreateQuerySource(チャンネル画面SearchCondition searchCondition, IPresentationContext<チャンネル画面SearchConditionMessages> context) {
        throw new NotImplementedException();
    }
    public override Task<メッセージ追加読み込みReturnValue> Execute(メッセージ追加読み込みParameter param, IPresentationContext<メッセージ追加読み込みParameterMessages> context) {
        throw new NotImplementedException();
    }
    #endregion チャンネル

    #region スレッド
    protected override IQueryable<スレッド詳細SearchResult> CreateQuerySource(スレッド詳細SearchCondition searchCondition, IPresentationContext<スレッド詳細SearchConditionMessages> context) {
        throw new NotImplementedException();
    }
    #endregion スレッド

    #region メッセージ
    protected override IQueryable<メッセージViewSearchResult> CreateQuerySource(メッセージViewSearchCondition searchCondition, IPresentationContext<メッセージViewSearchConditionMessages> context) {
        throw new NotImplementedException();
    }
    public override Task<新規投稿ReturnValue> Execute(新規投稿Parameter param, IPresentationContext<新規投稿ParameterMessages> context) {
        throw new NotImplementedException();
    }
    public override Task<既存メッセージ編集ReturnValue> Execute(既存メッセージ編集Parameter param, IPresentationContext<既存メッセージ編集ParameterMessages> context) {
        throw new NotImplementedException();
    }
    #endregion メッセージ

}
