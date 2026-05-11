

namespace MyApp;

partial class OverridedApplicationService {

    public override Task Executeログイン状態確認Async(IPresentationContextWithReturnValue<ログインユーザー情報DisplayData, MessageSetter> context) {

        if (LoginUser != null) {
            context.ReturnValue = new() {
                従業員番号 = LoginUser.従業員番号,
                氏名 = LoginUser.氏名,
                システム管理者 = LoginUser.Isシステム管理者,
                入荷機能を利用可能 = LoginUser.CanUse入荷登録,
                販売機能を利用可能 = LoginUser.CanUse売上登録,
            };
        } else {
            context.Messages.AddError("ログインしていません。");
        }

        return Task.CompletedTask;
    }
}
