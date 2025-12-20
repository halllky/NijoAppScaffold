namespace MyApp.Core.Authorization {
    public class LoginUser {
        public required string 従業員番号 { get; init; }
        public required bool Isシステム管理者 { get; init; }
        /// <summary>
        /// 入荷登録権限があるか、システム管理者ならtrue
        /// </summary>
        public required bool CanUse入荷登録 { get; init; }
        /// <summary>
        /// 売上登録権限があるか、システム管理者ならtrue
        /// </summary>
        public required bool CanUse売上登録 { get; init; }
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

                // セッションキーを取得。
                // ログイン済みなら必ず存在する。
                var sessionKeyProvider = ServiceProvider.GetRequiredService<ISessionKeyProvider>();
                var sessionKey = sessionKeyProvider.GetSessionKey();
                if (sessionKey == null) {
                    _isLoginUserLoaded = true;
                    return null;
                }

                // セッションキーはDBのセッションテーブルにも保存されるので、Cookieの値と突合
                var dbEntity = DbContext.セッションDbSet
                    .Include(e => e.ユーザ)
                    .SingleOrDefault(e => e.セッションキー == sessionKey);
                if (dbEntity == null) {
                    _isLoginUserLoaded = true;
                    return null;
                }

                _loginUserCache = new LoginUser {
                    従業員番号 = dbEntity.ユーザ_従業員番号 ?? throw new InvalidOperationException(),
                    Isシステム管理者 = dbEntity.ユーザ?.システム管理者 ?? false,
                    CanUse入荷登録 = (dbEntity.ユーザ?.システム管理者 ?? false) || (dbEntity.ユーザ?.入荷担当 ?? false),
                    CanUse売上登録 = (dbEntity.ユーザ?.システム管理者 ?? false) || (dbEntity.ユーザ?.販売担当 ?? false),
                };
                _isLoginUserLoaded = true;

                return _loginUserCache;
            }
        }

        private LoginUser? _loginUserCache = null;
        private bool _isLoginUserLoaded = false;
    }
}
