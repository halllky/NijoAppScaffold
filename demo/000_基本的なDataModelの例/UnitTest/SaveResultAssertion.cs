namespace MyApp.UnitTest;

/// <summary>
/// コマンドの検証と保存処理の結果の検証
/// </summary>
internal static class SaveResultAssertion {
    /// <summary>検証エラーが無いことを検証し、検証済みコマンドを返します。</summary>
    public static TValidated AssertValid<TValidated, TMessages>(this ISaveCommand<TValidated, TMessages> command)
        where TValidated : ValidatedCommand
        where TMessages : MessageSetter {

        if (command.TryValidate(out var validated, out var errors)) return validated;

        throw new AssertionException(string.Join(Environment.NewLine, errors.GetAllMessages()));
    }
    /// <summary>保存が成功したことを検証します。失敗した場合はエラーメッセージをテスト結果に出力します。</summary>
    public static void AssertSucceeded(this SaveResult result) {
        Assert.That(result.Success, Is.True, string.Join(Environment.NewLine, result.Errors.GetAllMessages()));
    }
    /// <summary>保存が成功したことを検証し、保存後のデータを返します。</summary>
    public static T AssertSucceeded<T>(this SaveResult<T> result) {
        ((SaveResult)result).AssertSucceeded();
        return result.Updated;
    }
}
