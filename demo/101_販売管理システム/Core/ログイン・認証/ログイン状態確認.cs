

namespace MyApp;

partial class OverridedApplicationService {

    public override Task Executeログイン状態確認Async(IPresentationContextWithReturnValue<ログインユーザー情報DisplayData, MessageSetter> context) {

        if (LoginUser != null) {
            context.ReturnValue = LoginUser.ToDisplayData();
        } else {
            context.Messages.AddError("ログインしていません。");
        }

        return Task.CompletedTask;
    }
}
