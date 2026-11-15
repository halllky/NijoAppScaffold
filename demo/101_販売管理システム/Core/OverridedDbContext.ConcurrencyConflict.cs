namespace MyApp;

partial class OverridedDbContext {

    /// <summary>
    /// この DbContext で、楽観排他のエラーが起きたかどうか。
    /// 入力チェックで画面が読み込んだ時点のバージョンとの食い違いが見つかった場合と、保存の時点で追い越された場合の両方を含む。
    /// 画面に返すときに、楽観排他のエラーをほかのエラーと区別するために使う。
    /// </summary>
    public bool HasConcurrencyConflict => ((ConcurrencyErrorRecorder)MSG).ConcurrencyErrorCreated;

    /// <summary>
    /// 楽観排他のエラーのメッセージを作ったかどうかを覚えておく、メッセージの部品。
    /// 楽観排他のエラーは、自動生成された入力チェックと保存の処理のどちらでも、必ずこのメッセージを作って報告されるので、
    /// メッセージを作ったことをもって、楽観排他のエラーが起きたとみなす。
    /// 文言そのものは、元の部品が作る。
    /// </summary>
    private sealed class ConcurrencyErrorRecorder(IDisplayMessageFactory inner) : IDisplayMessageFactory {
        public bool ConcurrencyErrorCreated { get; private set; }

        public string ConcurrencyError() {
            ConcurrencyErrorCreated = true;
            return inner.ConcurrencyError();
        }
        public string CustomAttributeValidationError(string arg0) => inner.CustomAttributeValidationError(arg0);
        public string DataNotFoundError() => inner.DataNotFoundError();
        public string DigitsError(string arg0, string arg1, string arg2) => inner.DigitsError(arg0, arg1, arg2);
        public string GenericLookupValueNotFound(string arg0) => inner.GenericLookupValueNotFound(arg0);
        public string MaxLengthError(string arg0, string arg1) => inner.MaxLengthError(arg0, arg1);
        public string RequiredError(string arg0) => inner.RequiredError(arg0);
        public string UniqueConstraintError() => inner.UniqueConstraintError();
        public string UnknownUpdateError() => inner.UnknownUpdateError();
    }
}
