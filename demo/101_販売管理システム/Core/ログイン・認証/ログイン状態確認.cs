

namespace MyApp;

partial class OverridedApplicationService {

    public override Task<ログインユーザー情報DisplayData?> Executeログイン状態確認Async(MessageSetter messages, IPresentationContext context) {

        if (LoginUser == null) {
            messages.AddError("ログインしていません。");
            return Task.FromResult<ログインユーザー情報DisplayData?>(null);
        }

        return Task.FromResult<ログインユーザー情報DisplayData?>(LoginUser.ToDisplayData());
    }
}
