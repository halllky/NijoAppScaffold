using MyApp;
using System.Text.Json.Nodes;

namespace MyApp.WebApi.Base;

/// <summary>
/// <see cref="PresentationContextInWebApi{TMessageRoot}"/> の基底クラス
/// </summary>
public abstract class PresentationContextInWebApi : IConfirmablePresentationContext {
    #region 確認メッセージ
    /// <summary>
    /// HTTPクライアントから送られてくる、確認メッセージを無視するかどうかのフラグ。
    /// 何らかのデータを更新する画面などで、「保存」などのボタンを押した時、
    /// 1回目のHTTPリクエストでは入力チェックのみを行い、確認メッセージをレスポンスとして返し、
    /// 2回目のHTTPリクエストで再度入力チェックしたうえで実際の更新処理を行う、という2段階の処理を行うことが多い。
    /// このプロパティを介してその挙動を実現する。
    ///
    /// このプロパティは、自動生成されるソースコードの中では直接は参照されていない。
    /// アプリケーションごとの仕様により自由に実装を変更してよい。
    /// </summary>
    public required bool IgnoreConfirm { get; init; }
    /// <summary>
    /// この処理中で発生した確認メッセージの一覧
    /// </summary>
    public required List<string> Confirms { get; init; }
    #endregion 確認メッセージ
}

/// <summary>
/// webapiにおける <see cref="IPresentationContext"/> のデフォルトの実装
/// </summary>
public class PresentationContextInWebApi<TMessageRoot> : PresentationContextInWebApi, IPresentationContext<TMessageRoot>
    where TMessageRoot : IMessageSetter {

    public required TMessageRoot Messages { get; init; }
    bool IPresentationContext.ValidationOnly => !IgnoreConfirm;

    // この型は戻り値を持たないので常にnullになる。戻り値ありの方（このクラスを継承した方）で使用される。
    public object? ReturnValue { get; set; } = null;

    public bool HasError() {
        return Messages.UnderlyingContext.Root.HasError();
    }

    public IPresentationContext<T> As<T>() where T : IMessageSetter {
        return new PresentationContextInWebApi<T> {
            Messages = Messages.As<T>(),
            IgnoreConfirm = IgnoreConfirm,
            Confirms = Confirms,
        };
    }
}

/// <summary>
/// <see cref="IPresentationContextWithReturnValue{TReturnValue, TMessageRoot}"/> のデフォルトの実装
/// </summary>
public class PresentationContextInWebApi<TReturnValue, TMessageRoot> : PresentationContextInWebApi<TMessageRoot>, IPresentationContextWithReturnValue<TReturnValue, TMessageRoot>
    where TMessageRoot : IMessageSetter
    where TReturnValue : new() {

    public PresentationContextInWebApi() {
        base.ReturnValue = new TReturnValue();
    }

    TReturnValue IPresentationContextWithReturnValue<TReturnValue, TMessageRoot>.ReturnValue {
        get => (TReturnValue)base.ReturnValue!;
        set => base.ReturnValue = value;
    }
}
