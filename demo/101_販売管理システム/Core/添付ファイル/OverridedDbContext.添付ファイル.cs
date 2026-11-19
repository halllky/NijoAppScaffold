using Microsoft.EntityFrameworkCore;

namespace MyApp;

partial class OverridedDbContext {

    /// <summary>
    /// 変更追跡中の追加・変更されるデータが参照している添付ファイルを確かめ、一時保存や参照なしのファイルを確定の状態にする。
    /// 管理テーブルの行の変更は変更追跡に加えるだけなので、呼び出し元の SaveChanges で業務のデータと一緒に保存される。
    /// 添付できるのは、自分が一時保存したファイルと、確定しているファイルと、参照なしのファイル。
    /// 確定しているファイルを添付できるのは、データを複製するときなどに同じファイルを使い回せるようにするため。
    /// </summary>
    /// <returns>変更追跡に加えた管理テーブルの行。添付できないファイルがあれば null</returns>
    private async Task<List<添付ファイルDbEntity>?> Track添付ファイルの確定Async(CancellationToken ct) {
        // 管理テーブルへの外部キーは、EF Core のモデルから探す。添付ファイルを使うデータモデルが増えても、ここを直す必要はない
        var 識別子一覧 = ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .SelectMany(entry => entry.Metadata.GetForeignKeys()
                .Where(fk => fk.PrincipalEntityType.ClrType == typeof(添付ファイルDbEntity))
                .Select(fk => entry.Property(fk.Properties.Single().Name).CurrentValue as string))
            .OfType<string>()
            .Distinct()
            .ToArray();
        if (識別子一覧.Length == 0) return [];

        var files = await 添付ファイルDbSet
            .AsNoTracking()
            .Where(f => 識別子一覧.Contains(f.識別子))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        // 存在しないファイル
        if (files.Count != 識別子一覧.Length) return null;
        // 他のユーザーが一時保存したファイル。作成者には、一時保存したログインユーザー（いなければ処理の名前）が記録されている
        if (files.Any(f => f.状態 == 添付ファイルの状態.一時保存 && f.CreateUser != CurrentUser.Name)) return null;

        var confirmed = files.Where(f => f.状態 != 添付ファイルの状態.確定).ToList();
        foreach (var file in confirmed) {
            var version = file.Version;
            file.状態 = 添付ファイルの状態.確定;
            file.参照なし日時 = null;
            file.UpdatedAt = CurrentTime.Value;
            file.UpdateUser = CurrentUser.Name;
            file.Version = version + 1;

            var entry = Entry(file);
            entry.State = EntityState.Modified;
            entry.Property(f => f.Version).OriginalValue = version;
        }
        return confirmed;
    }
}
