namespace MyApp.Core.Authorization {
    /// <summary>
    /// ログイン中のユーザー。機能を使ってよいかの判定に必要な情報をもつ。
    /// </summary>
    public class LoginUser {
        public required string 従業員番号 { get; init; }
        public required string 氏名 { get; init; }
        public required IReadOnlyCollection<権限区分> 権限 { get; init; }

        /// <summary>
        /// システム管理者ならtrue
        /// </summary>
        public bool Isシステム管理者 => 権限.Contains(権限区分.システム管理者);

        /// <summary>
        /// 指定された権限があるか、システム管理者ならtrue
        /// </summary>
        public bool Has権限(権限区分 対象) => Isシステム管理者 || 権限.Contains(対象);

        /// <summary>
        /// 従業員のデータからログインユーザー情報を作る。従業員の権限が読み込まれている必要がある。
        /// </summary>
        public static LoginUser From(従業員DbEntity 従業員) => new() {
            従業員番号 = 従業員.従業員番号 ?? throw new InvalidOperationException(),
            氏名 = 従業員.氏名 ?? throw new InvalidOperationException(),
            権限 = 従業員.従業員の権限.Select(x => x.権限区分 ?? throw new InvalidOperationException()).ToArray(),
        };

        /// <summary>
        /// JavaScriptで使うログインユーザー情報に変換する。
        /// </summary>
        public ログインユーザー情報DisplayData ToDisplayData() => new() {
            従業員番号 = 従業員番号,
            氏名 = 氏名,
            ログインユーザー情報の権限 = 権限.Select(x => new ログインユーザー情報の権限DisplayData { 権限区分 = x }).ToList(),
        };
    }
}

namespace MyApp {
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using MyApp.Core.Authorization;

    partial class OverridedApplicationService {

        /// <summary>
        /// ログインユーザー情報
        /// </summary>
        public LoginUser? LoginUser {
            get {
                // 一度のHTTPリクエスト内で何度もDBアクセスしないようにキャッシュする
                if (_isLoginUserLoaded) {
                    return _loginUserCache;
                }

                // Cookieからセッションキーを取得。
                // ログイン済みなら必ず存在する。
                var sessionKeyProvider = ServiceProvider.GetRequiredService<ISessionKeyProvider>();
                var sessionKey = sessionKeyProvider.GetSessionKey();
                if (sessionKey == null) {
                    _isLoginUserLoaded = true;
                    return null;
                }

                // セッションキーはDBのセッションテーブルにも保存されるので、Cookieの値と突合
                var dbEntity = DbContext.セッションDbSet
                    .Include(e => e.ユーザ!)
                    .ThenInclude(e => e.従業員の権限)
                    .SingleOrDefault(e => e.セッションキー == sessionKey);

                // 有効期限切れや、DBの古いデータが掃除された場合など
                if (dbEntity == null) {
                    sessionKeyProvider.ClearSessionKey();
                    _isLoginUserLoaded = true;
                    return null;
                }

                _loginUserCache = LoginUser.From(dbEntity.ユーザ ?? throw new InvalidOperationException());
                _isLoginUserLoaded = true;

                return _loginUserCache;
            }
        }

        private LoginUser? _loginUserCache = null;
        private bool _isLoginUserLoaded = false;
    }
}
