namespace MyApp.UnitTest;

/// <summary>
/// 保存処理の結果の検証
/// </summary>
internal static class SaveResultAssertion {
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
