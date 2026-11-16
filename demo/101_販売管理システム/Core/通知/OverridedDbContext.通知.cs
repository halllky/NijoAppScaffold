using Microsoft.EntityFrameworkCore;

namespace MyApp;

partial class OverridedDbContext {

    protected override void OnModelCreating通知(ModelBuilder modelBuilder) {
        base.OnModelCreating通知(modelBuilder);

        // 未読の件数は画面を移動するたびに数えるので、宛先と既読日時で引けるようにする。
        // nijo.xml では一意でないインデックスを定義できないので、ここで定義する
        modelBuilder.Entity<通知DbEntity>()
            .HasIndex(e => new { e.宛先, e.既読日時 })
            .HasDatabaseName("IX_通知_宛先_既読日時");
    }
}
