using Microsoft.EntityFrameworkCore;

namespace MyApp.Core.Authorization;

/// <summary>
/// 現在のログインユーザーを特定する。
/// DIコンテナのスコープ（Webアプリケーションであれば1回のリクエスト）につき1個のインスタンスが作られる。
/// </summary>
public class LoginUserProvider {
    public LoginUserProvider(OverridedDbContext dbContext, ISessionKeyProvider sessionKeyProvider) {
        _dbContext = dbContext;
        _sessionKeyProvider = sessionKeyProvider;
    }
    private readonly OverridedDbContext _dbContext;
    private readonly ISessionKeyProvider _sessionKeyProvider;

    /// <summary>
    /// ログインユーザーを返す。ログインしていない場合は null を返す。
    /// セッションキーに対応するセッションがDBに無い場合は、ログインしていないものとしてセッションキーをクリアする。
    /// 権限の判定はこのクラスでは行わない。
    /// </summary>
    public LoginUser? GetLoginUser() {
        // 一度のHTTPリクエスト内で何度もDBアクセスしないようにキャッシュする
        if (!_isLoginUserLoaded) {
            _loginUserCache = LoadLoginUser();
            _isLoginUserLoaded = true;
        }
        return _loginUserCache;
    }
    private LoginUser? _loginUserCache = null;
    private bool _isLoginUserLoaded = false;

#if DEBUG
    /// <summary>
    /// セッションを介さずにログインユーザーを確定させる。
    /// セッションの有無に関係なく特定のユーザーとして処理を実行する必要があるダミーデータ生成処理で使う。
    /// </summary>
    internal void SetLoginUserForDummyData(LoginUser loginUser) {
        _loginUserCache = loginUser;
        _isLoginUserLoaded = true;
    }
#endif

    private LoginUser? LoadLoginUser() {
        // ログイン済みならセッションキーは必ず存在する
        var sessionKey = _sessionKeyProvider.GetSessionKey();
        if (sessionKey == null) {
            return null;
        }

        // セッションキーはDBのセッションテーブルにも保存されるので、クライアントから送られた値と突合
        var dbEntity = _dbContext.セッションDbSet
            .Include(e => e.ユーザ!)
            .ThenInclude(e => e.従業員の権限)
            .SingleOrDefault(e => e.セッションキー == sessionKey);

        // 有効期限切れや、DBの古いデータが掃除された場合など
        if (dbEntity == null) {
            _sessionKeyProvider.ClearSessionKey();
            return null;
        }

        return LoginUser.From(dbEntity.ユーザ ?? throw new InvalidOperationException());
    }
}
