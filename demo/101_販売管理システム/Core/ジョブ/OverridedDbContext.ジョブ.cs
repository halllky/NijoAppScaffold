using Microsoft.EntityFrameworkCore;

namespace MyApp;

partial class OverridedDbContext {

    protected override void OnModelCreatingジョブ(ModelBuilder modelBuilder) {
        base.OnModelCreatingジョブ(modelBuilder);

        // 待機中のジョブを登録順に取り出す検索のためのインデックス。
        // nijo.xml では一意でないインデックスを定義できないので、ここで定義する
        modelBuilder.Entity<ジョブDbEntity>()
            .HasIndex(e => new { e.状態, e.CreatedAt })
            .HasDatabaseName("IX_ジョブ_状態_登録日時");
    }
}
