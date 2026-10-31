namespace MyApp;

/// <summary>
/// sequence 型の項目の採番設定。
/// この override 先のメソッドは、スキーマ定義に sequence 型の項目が1つ以上あるときだけ自動生成される。
/// </summary>
partial class OverridedDbContext {

    protected override void ConfigureSequenceMember(
        Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder,
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder entity,
        Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<int?> property,
        string sequenceName) {

        // SQLite の場合（SQLiteにはシーケンスがないため、AUTO_INCREMENTを使用）
        property.HasAnnotation("Sqlite:Autoincrement", true);
    }
}
