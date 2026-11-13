using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Core.Authorization;

/// <summary>
/// 現在のログインユーザーを特定する。
/// データ更新時の作成者・更新者として記録するユーザーもこのクラスが決める。
/// DIコンテナのスコープ（Webアプリケーションであれば1回のリクエスト）につき1個のインスタンスが作られる。
/// </summary>
public class LoginUserProvider : ICurrentUser {

    // DbContext は作成者・更新者の記録のためにこのクラスに依存するので、
    // コンストラクタで DbContext を受け取ると循環依存になる
    public LoginUserProvider(IServiceProvider serviceProvider, ISessionKeyProvider sessionKeyProvider, ICurrentProcess currentProcess) {
        _serviceProvider = serviceProvider;
        _sessionKeyProvider = sessionKeyProvider;
        _currentProcess = currentProcess;
    }
    private readonly IServiceProvider _serviceProvider;
    private readonly ISessionKeyProvider _sessionKeyProvider;
    private readonly ICurrentProcess _currentProcess;

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

    /// <summary>
    /// 作成者・更新者として記録する名前。
    /// ログインユーザーがいない処理（ログイン処理そのものやバッチなど）では、その処理の名前を記録する。
    /// </summary>
    string ICurrentUser.Name => GetLoginUser()?.従業員番号 ?? _currentProcess.Name;

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

        // DBのセッションテーブルにはセッションキーのハッシュ値が保存されているので、クライアントから送られた値のハッシュ値と突合
        var storedKey = SessionKey.ToStoredValue(sessionKey);
        var dbContext = _serviceProvider.GetRequiredService<OverridedDbContext>();
        var dbEntity = dbContext.セッションDbSet
            .Include(e => e.ユーザ!)
            .ThenInclude(e => e.従業員の権限)
            .SingleOrDefault(e => e.セッションキー == storedKey);

        // ログアウトした場合や、DBの古いデータが掃除された場合など
        if (dbEntity == null) {
            _sessionKeyProvider.ClearSessionKey();
            return null;
        }

        // 最後に操作してから一定時間が経っていれば、ログインしていないものとみなす。
        // セッションの行はここでは消さず、クリーンナップに任せる
        var now = _serviceProvider.GetRequiredService<ICurrentTime>().Value;
        if (dbEntity.最終操作日時 == null || dbEntity.最終操作日時 < now - SessionKey.IdleTimeout) {
            _sessionKeyProvider.ClearSessionKey();
            return null;
        }

        // 最終操作日時を更新する。負荷を抑えるため、前回の更新から一定時間が経ったときだけ更新する。
        // 楽観排他や更新者の記録は不要なので、変更追跡を通さずに直接更新する
        if (dbEntity.最終操作日時 < now - SessionKey.LastAccessUpdateInterval) {
            dbContext.セッションDbSet
                .Where(e => e.セッションキー == storedKey)
                .ExecuteUpdate(setters => setters.SetProperty(e => e.最終操作日時, now));
        }

        return LoginUser.From(dbEntity.ユーザ ?? throw new InvalidOperationException());
    }
}
