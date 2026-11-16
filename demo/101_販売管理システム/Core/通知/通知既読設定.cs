namespace MyApp;

partial class OverridedApplicationService {
    protected override async Task OnExecute通知既読設定Async(通知既読設定ParameterDisplayData param, 通知既読設定ParameterMessages messages, OperationContext context) {
        if (param.既読 == null) {
            messages.既読.AddError("既読にするか未読にするかを指定してください。");
            return;
        }

        // 他のユーザー宛ての通知は、存在しない通知と同じ扱いにし、存在することも知らせない。
        // 書き込むのは宛先の本人だけで、後から書いた方が勝っても困らないので、画面が読み込んだ時点ではなく、ここで読み込んだ時点のバージョンで楽観排他をかける
        var notificationId = param.通知ID;
        var notification = (await DbContext.LoadForEdit通知Async(query => Filter自分宛て(query).Where(n => n.通知ID == notificationId)))
            .SingleOrDefault();
        if (notification == null) {
            messages.AddError("通知が見つかりません。");
            return;
        }

        // すでに指定の状態なら、既読にした日時を変えないよう、何もしない
        if ((notification.既読日時 != null) == param.既読) return;
        notification.既読日時 = param.既読.Value ? CurrentTime : null;

        if (!notification.TryValidate(out var validated, out var errors)) {
            errors.CopyTo(messages);
            return;
        }
        if (context.ValidationOnly) return;

        await using var tran = await BeginTransactionAsync();
        var result = await DbContext.TrySaveAsync(validated);
        if (!result.Success) {
            result.Errors.CopyTo(messages);
            return;
        }
        await tran.CommitAsync();
    }
}
