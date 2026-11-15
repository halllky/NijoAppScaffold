using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Core.Idempotency;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 同じ冪等キーの操作は二重に処理されない() {
        var scope = CreateScopeWith冪等キー(nameof(同じ冪等キーの操作は二重に処理されない), "KEY-1");

        await using (var app = NewRequest(scope)) {
            await using var tran = await app.Value.BeginTransactionAsync();
            await tran.CommitAsync();
        }

        await using (var app = NewRequest(scope)) {
            Assert.ThrowsAsync<AlreadyExecutedException>(async () => {
                await using var tran = await app.Value.BeginTransactionAsync();
            });
        }
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 取り消された操作は同じ冪等キーで送り直せば処理される() {
        var scope = CreateScopeWith冪等キー(nameof(取り消された操作は同じ冪等キーで送り直せば処理される), "KEY-1");

        // コミットせずに終わると、冪等キーの記録も取り消される
        await using (var app = NewRequest(scope)) {
            await using var tran = await app.Value.BeginTransactionAsync();
        }

        await using (var app = NewRequest(scope)) {
            await using var tran = await app.Value.BeginTransactionAsync();
            await tran.CommitAsync();
        }

        Assert.That(await scope.App.DbContext.冪等キーDbSet.CountAsync(k => k.キー == "KEY-1"), Is.EqualTo(1));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 同じ呼び出しの中でトランザクションを複数回開始しても冪等キーで中断されない() {
        var scope = CreateScopeWith冪等キー(nameof(同じ呼び出しの中でトランザクションを複数回開始しても冪等キーで中断されない), "KEY-1");

        await using var app = NewRequest(scope);
        for (var i = 0; i < 2; i++) {
            await using var tran = await app.Value.BeginTransactionAsync();
            await tran.CommitAsync();
        }

        Assert.That(await scope.App.DbContext.冪等キーDbSet.CountAsync(k => k.キー == "KEY-1"), Is.EqualTo(1));
    }

    /// <summary>
    /// 呼び出し元から常に同じ冪等キーが渡される状態のテスト用スコープを作る。
    /// </summary>
    private static TestScopeImpl CreateScopeWith冪等キー(string testCaseName, string key) {
        return TestUtilImpl.Instance.CreateScope(testCaseName, services => {
            services.AddSingleton<IIdempotencyKeyProvider>(new 固定の冪等キー(key));
        });
    }

    /// <summary>
    /// 同じ DB に対する新しいリクエストを表す、新しいアプリケーションサービスを作る。
    /// </summary>
    private static ScopedApp NewRequest(TestScopeImpl scope) {
        var requestScope = scope.ServiceProvider.CreateAsyncScope();
        return new(requestScope, requestScope.ServiceProvider.GetRequiredService<OverridedApplicationService>());
    }

    private sealed record ScopedApp(AsyncServiceScope Scope, OverridedApplicationService Value) : IAsyncDisposable {
        public ValueTask DisposeAsync() => Scope.DisposeAsync();
    }

    private sealed class 固定の冪等キー(string key) : IIdempotencyKeyProvider {
        public string? Key => key;
    }
}
