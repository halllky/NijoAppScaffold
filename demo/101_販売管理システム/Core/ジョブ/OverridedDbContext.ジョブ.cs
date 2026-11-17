using Microsoft.EntityFrameworkCore;

namespace MyApp;

partial class OverridedDbContext {

    protected override void OnModelCreatingジョブ(ModelBuilder modelBuilder) {
        base.OnModelCreatingジョブ(modelBuilder);

        // 待機中のジョブを登録の順に取り出すため、状態と登録日時で引けるようにする。
        // nijo.xml では一意でないインデックスを定義できないので、ここで定義する
        modelBuilder.Entity<ジョブDbEntity>()
            .HasIndex(e => new { e.状態, e.CreatedAt })
            .HasDatabaseName("IX_ジョブ_状態_登録日時");
    }
}
