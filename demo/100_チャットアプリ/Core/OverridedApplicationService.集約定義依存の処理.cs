using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace MyApp;

partial class OverridedApplicationService {

    #region アカウント
    /// <summary>
    /// ログイン
    /// </summary>
    public override async Task Executeログイン(ログインParameterDisplayData param, IPresentationContextWithReturnValue<アカウントViewDisplayData, ログインParameterMessages> context) {
        // パラメータ検証
        if (string.IsNullOrWhiteSpace(param.Values.ユーザーID)) {
            context.Messages.ユーザーID.AddError("ユーザーIDを入力してください。");
        }
        if (string.IsNullOrWhiteSpace(param.Values.パスワード)) {
            context.Messages.パスワード.AddError("パスワードを入力してください。");
        }
        if (context.HasError()) {
            return;
        }

        // ユーザー認証
        var user = await DbContext.アカウントDbSet
            .FirstOrDefaultAsync(u => u.アカウントID == param.Values.ユーザーID);

        if (user == null || !user.VerifyPassword(param.Values.パスワード!)) {
            context.Messages.AddError("ユーザーIDまたはパスワードが正しくありません。");
            return;
        }

        // Cookie認証のClaimsを作成
        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.アカウントID!),
            new Claim(ClaimTypes.Name, user.アカウント名!),
        };

        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // HttpContextからサインイン
        var httpContext = ServiceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
        if (httpContext != null) {
            await httpContext.SignInAsync("Cookies", claimsPrincipal);
        }

        // レスポンス用のデータを作成
        context.ReturnValue = new アカウントViewDisplayData {
            Values = new() {
                アカウントID = user.アカウントID,
                アカウント名 = user.アカウント名,
            },
            ExistsInDatabase = true,
            WillBeChanged = false,
            WillBeDeleted = false,
            Version = (int)user.Version!,
        };
    }

    /// <summary>
    /// ログアウト
    /// </summary>
    public override async Task Executeログアウト(IPresentationContext<MessageSetter> context) {
        // HttpContextからサインアウト
        var httpContext = ServiceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
        if (httpContext != null) {
            await httpContext.SignOutAsync("Cookies");
        }
    }
    /// <summary>
    /// アカウントの画面表示用データをデータベースのどの項目から取得するかの定義
    /// </summary>
    protected override IQueryable<アカウントViewSearchResult> CreateQuerySource(アカウントViewSearchCondition searchCondition, IPresentationContext<アカウントViewSearchConditionMessages> context) {
        return DbContext.アカウントDbSet.Select(e => new アカウントViewSearchResult {
            アカウントID = e.アカウントID,
            アカウント名 = e.アカウント名,
            Version = (int)e.Version!,
        });
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
            メッセージ一覧 = e.RefFromメッセージ_チャンネル
                .Where(x => x.チャンネル直下か == true) // チャンネル直下のメッセージのみ表示
                .OrderBy(x => x.CreatedAt) // 作成日時の昇順で並ぶ
                .Select(x => new メッセージ一覧SearchResult {
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

    public override async Task Executeメッセージ追加読み込み(メッセージ追加読み込みParameterDisplayData param, IPresentationContextWithReturnValue<メッセージ追加読み込みReturnValueDisplayData, メッセージ追加読み込みParameterMessages> context) {
        // このシーケンスより古いメッセージN件を読み込む
        var messages = await DbContext.メッセージDbSet
            .Where(m => m.メッセージSEQ < param.Values.前メッセージSEQ && m.チャンネル直下か == true)
            .OrderByDescending(m => m.メッセージSEQ) // 新しい順で取得してから
            .Take(param.Values.読み込み件数 ?? 20) // 件数制限
            .Select(m => new メッセージViewSearchResult {
                メッセージSEQ = m.メッセージSEQ,
                本文 = m.本文,
                記載者_アカウントID = m.記載者_アカウントID,
                記載者_アカウント名 = m.記載者!.アカウント名,
                チャンネル直下か = m.チャンネル直下か,
                編集済みか = m.編集済みか,
                Version = (int)m.Version!,
            })
            .ToListAsync();

        context.ReturnValue = new() {
            読み込み結果 = messages.Select(m => new 読み込み結果DisplayData {
                Values = new() {
                    メッセージ = new() {
                        Values = new() {
                            メッセージSEQ = m.メッセージSEQ,
                            本文 = m.本文,
                            記載者 = new() {
                                アカウントID = m.記載者_アカウントID,
                                アカウント名 = m.記載者_アカウント名,
                            },
                            チャンネル直下か = m.チャンネル直下か,
                            編集済みか = m.編集済みか,
                        },
                        ExistsInDatabase = true,
                        WillBeChanged = false,
                        WillBeDeleted = false,
                        Version = m.Version,
                    },
                },
                ExistsInDatabase = true,
            }).ToList(),
        };
    }
    #endregion チャンネル

    #region スレッド
    /// <summary>
    /// スレッドの画面表示用データをデータベースのどの項目から取得するかの定義
    /// </summary>
    protected override IQueryable<スレッド詳細SearchResult> CreateQuerySource(スレッド詳細SearchCondition searchCondition, IPresentationContext<スレッド詳細SearchConditionMessages> context) {
        return DbContext.メッセージDbSet
            .Where(m => m.チャンネル直下か == true) // チャンネル直下のメッセージを基準とする
            .Select(e => new スレッド詳細SearchResult {
                // チャンネル直下のメッセージ
                チャンネル直下のメッセージ_メッセージSEQ = e.メッセージSEQ,
                チャンネル直下のメッセージ_本文 = e.本文,
                チャンネル直下のメッセージ_記載者_アカウントID = e.記載者_アカウントID,
                チャンネル直下のメッセージ_記載者_アカウント名 = e.記載者!.アカウント名,
                チャンネル直下のメッセージ_チャンネル直下か = e.チャンネル直下か,
                チャンネル直下のメッセージ_編集済みか = e.編集済みか,
                // 返信一覧：同じ返信先のメッセージをすべて表示する
                返信一覧 = DbContext.メッセージDbSet
                    .Where(r => r.返信先メッセージSEQ == e.メッセージSEQ)
                    .Select(r => new 返信一覧SearchResult {
                        メッセージ_メッセージSEQ = r.メッセージSEQ,
                        メッセージ_本文 = r.本文,
                        メッセージ_記載者_アカウントID = r.記載者_アカウントID,
                        メッセージ_記載者_アカウント名 = r.記載者!.アカウント名,
                        メッセージ_チャンネル直下か = r.チャンネル直下か,
                        メッセージ_編集済みか = r.編集済みか,
                    }).ToList(),
                Version = (int)e.Version!,
            });
    }
    #endregion スレッド

    #region メッセージ
    /// <summary>
    /// メッセージの画面表示用データをデータベースのどの項目から取得するかの定義
    /// </summary>
    protected override IQueryable<メッセージViewSearchResult> CreateQuerySource(メッセージViewSearchCondition searchCondition, IPresentationContext<メッセージViewSearchConditionMessages> context) {
        return DbContext.メッセージDbSet.Select(e => new メッセージViewSearchResult {
            メッセージSEQ = e.メッセージSEQ,
            本文 = e.本文,
            記載者_アカウントID = e.記載者_アカウントID,
            記載者_アカウント名 = e.記載者!.アカウント名,
            チャンネル直下か = e.チャンネル直下か,
            編集済みか = e.編集済みか,
            Version = (int)e.Version!,
        });
    }

    public override async Task Execute新規メッセージ投稿(新規投稿ParameterDisplayData param, IPresentationContext<新規投稿ParameterMessages> context) {

        using var transaction = await DbContext.Database.BeginTransactionAsync();

        // 自動生成されたメソッドを使用してメッセージを作成
        var createCommand = new メッセージCreateCommand {
            本文 = param.Values.本文,
            記載者 = new アカウントKey { アカウントID = "dummy_user" }, // 実際のアプリケーションでは認証情報から取得
            チャンネル = new チャンネルKey { チャンネルID = param.Values.チャンネルID },
            チャンネル直下か = param.Values.返信先メッセージSEQ == null, // NULLならチャンネル直下
            返信先メッセージSEQ = param.Values.返信先メッセージSEQ,
            編集済みか = false,
        };

        await CreateメッセージAsync(createCommand, context);

        if (context.HasError()) {
            return;
        }

        // エラーがなければコミット
        await transaction.CommitAsync();
    }

    /// <summary>
    /// 既存メッセージ編集
    /// </summary>
    public override async Task Execute既存メッセージ編集(メッセージViewDisplayData param, IPresentationContext<メッセージViewDisplayDataMessages> context) {

        using var transaction = await DbContext.Database.BeginTransactionAsync();

        // 自動生成されたメソッドを使用してメッセージを更新
        await UpdateメッセージAsync(param.Values.メッセージSEQ, param.Version, data => {
            data.本文 = param.Values.本文;
            data.編集済みか = true;
        }, context);

        // エラーメッセージはメッセージのコンテナに含めて画面側に返す
        if (context.HasError()) {
            return;
        }

        // エラーがなければコミット
        await transaction.CommitAsync();
    }
    #endregion メッセージ

}
