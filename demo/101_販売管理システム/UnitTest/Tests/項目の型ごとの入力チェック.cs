namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    [TestCase("👨‍👩‍👧", 100, true)]
    [TestCase("\u304B\u3099", 100, true)] // 「か」と結合文字の濁点
    [TestCase("あ", 101, false)]
    public void 文字列の長さは見た目の1文字を1と数える(string character, int count, bool isValid) {
        var scope = TestUtilImpl.Instance.CreateScope($"{nameof(文字列の長さは見た目の1文字を1と数える)}_{TestContext.CurrentContext.Test.ID}");
        var command = Create従業員(scope.App.DbContext);
        command.氏名 = string.Concat(Enumerable.Repeat(character, count)); // 氏名の MaxLength は 100

        Assert.That(command.TryValidate(out _, out var errors), Is.EqualTo(isValid), string.Join(Environment.NewLine, errors.GetAllMessages()));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public void 空白だけの文字列は未入力とみなす() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(空白だけの文字列は未入力とみなす));
        var command = Create従業員(scope.App.DbContext);
        command.氏名 = " 　";

        Assert.That(command.TryValidate(out _, out var errors), Is.False);
        Assert.That(errors.GetAllMessages(), Has.Some.Contains("氏名 を入力してください。"));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    [TestCase("99999.99", 7, 2, true)]
    [TestCase("-99999.99", 7, 2, true)]
    [TestCase("1.50", 7, 2, true)]
    [TestCase("100000", 7, 2, false)]
    [TestCase("1.005", 7, 2, false)]
    [TestCase("99999", 5, 0, true)]
    [TestCase("-100000", 5, 0, false)]
    public void 桁数を超える数値は丸めずにエラーにする(string value, int totalDigit, int decimalPlace, bool isValid) {
        var scope = TestUtilImpl.Instance.CreateScope($"{nameof(桁数を超える数値は丸めずにエラーにする)}_{TestContext.CurrentContext.Test.ID}");

        var error = scope.App.DbContext.ValidateDigitsAndScales(decimal.Parse(value), totalDigit, decimalPlace, "数値");

        Assert.That(error, isValid ? Is.Null : Is.EqualTo($"数値 は整数部 {totalDigit - decimalPlace} 桁、小数部 {decimalPlace} 桁以内で入力してください。"));
    }

    private static 従業員CreateCommand Create従業員(OverridedDbContext db) {
        var command = db.StartCreating従業員();
        command.従業員番号 = "E1";
        command.氏名 = "氏名";
        command.パスワード = [1];
        command.SALT = [1];
        return command;
    }
}
