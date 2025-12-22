namespace MyApp;

partial class OverridedDummyDataGenerator {

    protected override IEnumerable<従業員CreateCommand> CreatePatternsOf従業員(DummyDataGenerateContext context) {
        // 固定ユーザー: demo101-admin / demo101-admin
        var salt = OverridedApplicationService.GenerateSalt();
        var hash = OverridedApplicationService.ComputeHash("demo101-admin", salt);

        yield return new 従業員CreateCommand {
            従業員番号 = "demo101-admin",
            氏名 = "デモ用ユーザー",
            パスワード = hash,
            SALT = salt,
            入荷担当 = true,
            販売担当 = true,
            システム管理者 = true,
        };

        // デフォルトのランダム生成ユーザーも維持する場合
        foreach (var item in base.CreatePatternsOf従業員(context)) {
            yield return item;
        }
    }

}
