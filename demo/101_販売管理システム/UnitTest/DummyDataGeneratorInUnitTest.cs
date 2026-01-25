namespace MyApp.UnitTest;

/// <summary>
/// 単体テスト用のダミーデータ生成クラス。
/// Webと違い通常のログインのプロセスが無いので直接ログイン済み状態を作り出している。
/// </summary>
public class DummyDataGeneratorInUnitTest : OverridedDummyDataGenerator {
    public DummyDataGeneratorInUnitTest(Func<IMessageSetter, IPresentationContext<MessageSetter>> createPresentationContext) : base(createPresentationContext) {
    }

    protected override IEnumerable<セッションCreateCommand> CreatePatternsOfセッション(DummyDataGenerateContext context) {
        yield return new() {
            セッションキー = TestUtilImpl.SessionKeyProviderInUnitTest.SESSION_KEY,
            ユーザ = new() {
                従業員番号 = ADMIN_USER_ID,
            },
            最終ログイン日時 = new DateTime(2026, 1, 1),
        };
    }
}
