namespace MyApp;

// このファイルでは PresentationContext への追加機能を定義します。
// いずれも実装は必須ではありません。アプリケーションの要件に応じて調整してください。

/// <summary>
/// ユーザーに向けた確認メッセージを扱うことができる <see cref="IPresentationContext"/> の拡張インターフェイス。
/// 具体的にはWebアプリケーションならこれに該当。定周期バッチはこれに当てはまらない。
/// </summary>
public interface IConfirmablePresentationContext {
    /// <summary>
    /// この処理中で発生した確認メッセージの一覧
    /// </summary>
    List<string> Confirms { get; }
}

public static class PresentationContextExtensions {

    /// <summary>
    /// この処理の戻り値として何らかの確認メッセージ（「保存しますか？」など）を追加します。
    /// </summary>
    public static void AddConfirm<T>(this T context, string text) where T : IPresentationContext {
        if (context is IConfirmablePresentationContext cpc) {
            cpc.Confirms.Add(text);

        } else {
            // 定周期バッチ処理など、確認メッセージを扱えない処理で
            // このメソッドが呼ばれた場合の挙動を定義する。
            // 例えば厳密に実装ミスをなくしたい場合はここで実行時例外を投げることになる。
            // 一方、例えばGUIとバッチの両方から呼ばれうる処理があるなら、
            // バッチ側では確認メッセージを無視して処理を続行するようにすることも考えられる。
            // ここでは後者の挙動を採用する。
        }
    }
    /// <summary>
    /// この処理の戻り値として確認メッセージ（「保存しますか？」など）が含まれているかどうかを返します。
    /// </summary>
    public static bool HasConfirm<T>(this T context) where T : IPresentationContext {
        if (context is IConfirmablePresentationContext cpc) {
            return cpc.Confirms.Count > 0;
        } else {
            return false;
        }
    }

}
