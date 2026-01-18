using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyApp;

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
