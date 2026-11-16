

namespace MyApp;

partial class OverridedApplicationService {

    protected override Task<ログインユーザー情報DisplayData?> OnExecuteログイン状態確認Async(MessageSetter messages, OperationContext context) {

        if (LoginUser == null) {
            messages.AddError("ログインしていません。");
            return Task.FromResult<ログインユーザー情報DisplayData?>(null);
        }

        return Task.FromResult<ログインユーザー情報DisplayData?>(LoginUser.ToDisplayData());
    }
}
