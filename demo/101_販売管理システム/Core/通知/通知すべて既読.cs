namespace MyApp;

partial class OverridedApplicationService {
    protected override async Task OnExecute通知すべて既読Async(MessageSetter messages, OperationContext context) {
        // 作成者・更新者などを記録するため、DB を直接更新せず、1件ずつ保存の処理に渡す。
        // 書き込むのは宛先の本人だけなので、ここで読み込んだ時点のバージョンで楽観排他をかける
        var unread = await DbContext.LoadForEdit通知Async(query => Filter自分宛て(query).Where(n => n.既読日時 == null));

        var commands = new List<ValidatedCommand<通知UpdateCommand>>();
        foreach (var notification in unread) {
            notification.既読日時 = CurrentTime;
            if (!notification.TryValidate(out var validated, out var errors)) {
                errors.CopyTo(messages);
                return;
            }
            commands.Add(validated);
        }
        if (commands.Count == 0 || context.ValidationOnly) return;

        await using var tran = await BeginTransactionAsync();
        var result = await DbContext.TrySaveAsync(commands);
        if (!result.Success) {
            result.Errors.CopyTo(messages);
            return;
        }
        await tran.CommitAsync();
    }
}
