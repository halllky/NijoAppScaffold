using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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

    /// <summary>
    /// いずれかのデータから参照されている添付ファイルの識別子。
    /// 管理テーブルへの外部キーをもつ全テーブルと、それらの論理削除の削除済みテーブルを対象にする。
    /// テーブルは EF Core のモデルから探すので、添付ファイルを使うデータモデルが増えても、ここを直す必要はない。
    /// 手書きのテーブルなど、外部キーを張らずに識別子を持つ場所を参照として数えたい場合は、ここに足す。
    /// </summary>
    internal IQueryable<string?> 参照されている添付ファイルの識別子() {
        IQueryable<string?> result = 添付ファイルDbSet.Where(_ => false).Select(f => f.識別子);

        foreach (var entityType in Model.GetEntityTypes()) {
            foreach (var fk in entityType.GetForeignKeys().Where(fk => fk.PrincipalEntityType.ClrType == typeof(添付ファイルDbEntity))) {
                var propertyName = fk.Properties.Single().Name;
                result = result.Concat(SelectColumn(entityType, propertyName));

                // 論理削除の削除済みテーブルには外部キー制約が無いので、元のテーブルと同じ名前の列を見る
                var deletedType = Model.GetEntityTypes().SingleOrDefault(t => t.ClrType.Name == entityType.ClrType.Name.Replace("DbEntity", "DeletedDbEntity"));
                if (deletedType?.FindProperty(propertyName) != null) {
                    result = result.Concat(SelectColumn(deletedType, propertyName));
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 型の決まっていないエンティティの、文字列の列1つを取り出すクエリを作る。
    /// </summary>
    private IQueryable<string?> SelectColumn(IEntityType entityType, string propertyName) {
        var set = (IQueryable)typeof(DbContext).GetMethod(nameof(Set), Type.EmptyTypes)!
            .MakeGenericMethod(entityType.ClrType)
            .Invoke(this, null)!;
        var e = Expression.Parameter(entityType.ClrType, "e");
        var column = Expression.Call(typeof(EF), nameof(EF.Property), [typeof(string)], e, Expression.Constant(propertyName));
        var select = Expression.Call(typeof(Queryable), nameof(Queryable.Select), [entityType.ClrType, typeof(string)],
            set.Expression,
            Expression.Quote(Expression.Lambda(column, e)));
        return set.Provider.CreateQuery<string?>(select);
    }
}
