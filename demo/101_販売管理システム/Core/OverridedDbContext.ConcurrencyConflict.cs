using System.Reflection;

namespace MyApp;

partial class OverridedDbContext {

    /// <summary>
    /// この DbContext で、楽観排他のエラーが起きたかどうか。
    /// 入力チェックで画面が読み込んだ時点のバージョンとの食い違いが見つかった場合と、保存の時点で追い越された場合の両方を含む。
    /// 画面に返すときに、楽観排他のエラーをほかのエラーと区別するために使う。
    /// </summary>
    public bool HasConcurrencyConflict => ((ConcurrencyErrorRecorder)(object)MSG).ConcurrencyErrorCreated;

    /// <summary>
    /// 楽観排他のエラーのメッセージを作ったかどうかを覚えておく、メッセージの部品の包み。
    /// 楽観排他のエラーは、自動生成された入力チェックと保存の処理のどちらでも、必ずこのメッセージを作って報告されるので、
    /// メッセージを作ったことをもって、楽観排他のエラーが起きたとみなす。文言そのものは、包んだ部品が作る。
    /// メッセージの部品のメンバーは nijo.xml の内容で変わるので、メンバーを個別に書かずに、すべての呼び出しを中継する。
    /// </summary>
    internal class ConcurrencyErrorRecorder : DispatchProxy {
        private IDisplayMessageFactory _inner = null!;

        public bool ConcurrencyErrorCreated { get; private set; }

        public static IDisplayMessageFactory Wrap(IDisplayMessageFactory inner) {
            var proxy = Create<IDisplayMessageFactory, ConcurrencyErrorRecorder>();
            ((ConcurrencyErrorRecorder)(object)proxy)._inner = inner;
            return proxy;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) {
            ArgumentNullException.ThrowIfNull(targetMethod);
            if (targetMethod.Name == nameof(IDisplayMessageFactory.ConcurrencyError)) {
                ConcurrencyErrorCreated = true;
            }
            return targetMethod.Invoke(_inner, args);
        }
    }
}
