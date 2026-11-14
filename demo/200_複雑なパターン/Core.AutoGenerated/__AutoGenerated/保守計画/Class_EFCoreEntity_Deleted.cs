
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : DataStructuresSection.cs
// ※ このファイルを生成したメソッド: Render

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MyApp;


#region Entity Framework Core エンティティ定義
/// <summary>
/// 論理削除された保守計画の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.LatinName("Maintenance Plan"), NijoAttr.IsStored, NijoAttr.UseSoftDelete]
public partial class 保守計画DeletedDbEntity {
    /// <summary>論理削除UUID</summary>
    public Guid? DeletedUuid { get; set; }
    /// <summary>保守計画ID</summary>
    [NijoAttr.DbName("PLAN_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 保守計画ID { get; set; }
    /// <summary>計画名</summary>
    [NijoAttr.DbName("PLAN_NAME"), NijoAttr.MaxLength(100)]
    public string? 計画名 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>論理削除日時</summary>
    public DateTime? DeletedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }
    /// <summary>論理削除を行ったユーザー</summary>
    public string? DeletedUser { get; set; }
    /// <summary>楽観排他制御用のバージョニング用カラム</summary>
    public int? Version { get; set; }

    [NijoAttr.DbName("PROCESS")]
    public virtual ICollection<作業工程DeletedDbEntity> 作業工程 { get; set; } = [];

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(保守計画DeletedDbEntity entity) {
        if (entity.DeletedUuid != this.DeletedUuid) return false;
        return true;
    }
}
/// <summary>
/// 論理削除された作業工程の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("PROCESS")]
public partial class 作業工程DeletedDbEntity {
    /// <summary>論理削除UUID</summary>
    public Guid? DeletedUuid { get; set; }
    /// <summary>保守計画ID</summary>
    [NijoAttr.DbName("PLAN_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_保守計画ID { get; set; }
    /// <summary>工程番号</summary>
    [NijoAttr.DbName("PROCESS_NO"), NijoAttr.IsKey]
    public int? 工程番号 { get; set; }
    /// <summary>工程名</summary>
    [NijoAttr.DbName("PROCESS_NAME"), NijoAttr.MaxLength(100)]
    public string? 工程名 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.LatinName("Maintenance Plan"), NijoAttr.IsStored, NijoAttr.UseSoftDelete]
    public virtual 保守計画DeletedDbEntity? Parent { get; set; }
    [NijoAttr.DbName("PROCEDURE")]
    public virtual ICollection<作業手順DeletedDbEntity> 作業手順 { get; set; } = [];
    [NijoAttr.DbName("WORK_TEAM")]
    public virtual 作業体制DeletedDbEntity? 作業体制 { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(作業工程DeletedDbEntity entity) {
        if (entity.DeletedUuid != this.DeletedUuid) return false;
        if (entity.工程番号 != this.工程番号) return false;
        return true;
    }
}
/// <summary>
/// 論理削除された作業手順の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("PROCEDURE")]
public partial class 作業手順DeletedDbEntity {
    /// <summary>論理削除UUID</summary>
    public Guid? DeletedUuid { get; set; }
    /// <summary>保守計画ID</summary>
    [NijoAttr.DbName("PLAN_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_Parent_保守計画ID { get; set; }
    /// <summary>工程番号</summary>
    [NijoAttr.DbName("PROCESS_NO"), NijoAttr.IsKey]
    public int? Parent_工程番号 { get; set; }
    /// <summary>手順番号</summary>
    [NijoAttr.DbName("PROCEDURE_NO"), NijoAttr.IsKey]
    public int? 手順番号 { get; set; }
    /// <summary>手順内容</summary>
    [NijoAttr.DbName("PROCEDURE_TEXT"), NijoAttr.MaxLength(1000)]
    public string? 手順内容 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.DbName("PROCESS")]
    public virtual 作業工程DeletedDbEntity? Parent { get; set; }
    [NijoAttr.DbName("CHECK_ITEM")]
    public virtual ICollection<確認項目DeletedDbEntity> 確認項目 { get; set; } = [];

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(作業手順DeletedDbEntity entity) {
        if (entity.DeletedUuid != this.DeletedUuid) return false;
        if (entity.Parent_工程番号 != this.Parent_工程番号) return false;
        if (entity.手順番号 != this.手順番号) return false;
        return true;
    }
}
/// <summary>
/// 論理削除された確認項目の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("CHECK_ITEM")]
public partial class 確認項目DeletedDbEntity {
    /// <summary>論理削除UUID</summary>
    public Guid? DeletedUuid { get; set; }
    /// <summary>保守計画ID</summary>
    [NijoAttr.DbName("PLAN_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_Parent_Parent_保守計画ID { get; set; }
    /// <summary>工程番号</summary>
    [NijoAttr.DbName("PROCESS_NO"), NijoAttr.IsKey]
    public int? Parent_Parent_工程番号 { get; set; }
    /// <summary>手順番号</summary>
    [NijoAttr.DbName("PROCEDURE_NO"), NijoAttr.IsKey]
    public int? Parent_手順番号 { get; set; }
    /// <summary>確認項目番号</summary>
    [NijoAttr.DbName("CHECK_ITEM_NO"), NijoAttr.IsKey]
    public int? 確認項目番号 { get; set; }
    /// <summary>確認内容</summary>
    [NijoAttr.DbName("CHECK_TEXT"), NijoAttr.MaxLength(100)]
    public string? 確認内容 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.DbName("PROCEDURE")]
    public virtual 作業手順DeletedDbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(確認項目DeletedDbEntity entity) {
        if (entity.DeletedUuid != this.DeletedUuid) return false;
        if (entity.Parent_Parent_工程番号 != this.Parent_Parent_工程番号) return false;
        if (entity.Parent_手順番号 != this.Parent_手順番号) return false;
        if (entity.確認項目番号 != this.確認項目番号) return false;
        return true;
    }
}
/// <summary>
/// 論理削除された作業体制の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("WORK_TEAM")]
public partial class 作業体制DeletedDbEntity {
    /// <summary>論理削除UUID</summary>
    public Guid? DeletedUuid { get; set; }
    /// <summary>保守計画ID</summary>
    [NijoAttr.DbName("PLAN_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_Parent_保守計画ID { get; set; }
    /// <summary>工程番号</summary>
    [NijoAttr.DbName("PROCESS_NO"), NijoAttr.IsKey]
    public int? Parent_工程番号 { get; set; }
    /// <summary>責任者名</summary>
    [NijoAttr.DbName("LEADER_NAME"), NijoAttr.MaxLength(50)]
    public string? 責任者名 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.DbName("PROCESS")]
    public virtual 作業工程DeletedDbEntity? Parent { get; set; }
    [NijoAttr.DbName("WORKER")]
    public virtual ICollection<作業者DeletedDbEntity> 作業者 { get; set; } = [];

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(作業体制DeletedDbEntity entity) {
        if (entity.DeletedUuid != this.DeletedUuid) return false;
        if (entity.Parent_工程番号 != this.Parent_工程番号) return false;
        return true;
    }
}
/// <summary>
/// 論理削除された作業者の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("WORKER")]
public partial class 作業者DeletedDbEntity {
    /// <summary>論理削除UUID</summary>
    public Guid? DeletedUuid { get; set; }
    /// <summary>保守計画ID</summary>
    [NijoAttr.DbName("PLAN_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_Parent_Parent_保守計画ID { get; set; }
    /// <summary>工程番号</summary>
    [NijoAttr.DbName("PROCESS_NO"), NijoAttr.IsKey]
    public int? Parent_Parent_工程番号 { get; set; }
    /// <summary>作業者番号</summary>
    [NijoAttr.DbName("WORKER_NO"), NijoAttr.IsKey]
    public int? 作業者番号 { get; set; }
    /// <summary>作業者名</summary>
    [NijoAttr.DbName("WORKER_NAME"), NijoAttr.MaxLength(50)]
    public string? 作業者名 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.DbName("WORK_TEAM")]
    public virtual 作業体制DeletedDbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(作業者DeletedDbEntity entity) {
        if (entity.DeletedUuid != this.DeletedUuid) return false;
        if (entity.Parent_Parent_工程番号 != this.Parent_Parent_工程番号) return false;
        if (entity.作業者番号 != this.作業者番号) return false;
        return true;
    }
}

partial class AutoGeneratedDbContext {
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating保守計画Deleted(ModelBuilder modelBuilder) {
        modelBuilder.Entity<保守計画DeletedDbEntity>(entity => {
            entity.ToTable("保守計画_DELETED");
    
            entity.HasKey(e => new {
                e.DeletedUuid,
            })
            .HasName("PK_保守計画_DELETED");
    
            entity.Property(e => e.DeletedUuid)
                .HasColumnName("DeletedUuid")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.保守計画ID)
                .HasColumnName("PLAN_ID")
                .HasMaxLength(20)
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.計画名)
                .HasColumnName("PLAN_NAME")
                .HasMaxLength(100)
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.CreateUser)
                .HasColumnName("CreateUser")
                .IsRequired(false)
                .HasColumnOrder(4);
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false)
                .HasColumnOrder(5);
            entity.Property(e => e.UpdateUser)
                .HasColumnName("UpdateUser")
                .IsRequired(false)
                .HasColumnOrder(6);
            entity.Property(e => e.Version)
                .HasColumnName("Version")
                .IsRequired(false)
                .IsConcurrencyToken(false)
                .HasColumnOrder(7);
    
            // 論理削除テーブルは削除日時でソートされることが多いのでインデックスを張る
            entity.HasIndex(e => e.DeletedAt)
                .HasDatabaseName("IX_保守計画_DELETED_DELETED_AT");
    
            entity.HasMany(e => e.作業工程)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.DeletedUuid,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DELETED_保守計画_PROCESS");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating作業工程Deleted(ModelBuilder modelBuilder) {
        modelBuilder.Entity<作業工程DeletedDbEntity>(entity => {
            entity.ToTable("PROCESS_DELETED");
    
            entity.HasKey(e => new {
                e.DeletedUuid,
                e.工程番号,
            })
            .HasName("PK_PROCESS_DELETED");
    
            entity.Property(e => e.DeletedUuid)
                .HasColumnName("DeletedUuid")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.Parent_保守計画ID)
                .HasColumnName("Parent_PLAN_ID")
                .HasMaxLength(20)
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.工程番号)
                .HasColumnName("PROCESS_NO")
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.工程名)
                .HasColumnName("PROCESS_NAME")
                .HasMaxLength(100)
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired(false)
                .HasColumnOrder(4);
            entity.Property(e => e.CreateUser)
                .HasColumnName("CreateUser")
                .IsRequired(false)
                .HasColumnOrder(5);
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false)
                .HasColumnOrder(6);
            entity.Property(e => e.UpdateUser)
                .HasColumnName("UpdateUser")
                .IsRequired(false)
                .HasColumnOrder(7);
    
            entity.HasMany(e => e.作業手順)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.DeletedUuid,
                    e.Parent_工程番号,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DELETED_PROCESS_PROCEDURE");
    
            entity.HasOne(e => e.作業体制)
                .WithOne(e => e.Parent)
                .HasForeignKey<作業体制DeletedDbEntity>(e => new {
                    e.DeletedUuid,
                    e.Parent_工程番号,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DELETED_PROCESS_WORK_TEAM");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating作業手順Deleted(ModelBuilder modelBuilder) {
        modelBuilder.Entity<作業手順DeletedDbEntity>(entity => {
            entity.ToTable("PROCEDURE_DELETED");
    
            entity.HasKey(e => new {
                e.DeletedUuid,
                e.Parent_工程番号,
                e.手順番号,
            })
            .HasName("PK_PROCEDURE_DELETED");
    
            entity.Property(e => e.DeletedUuid)
                .HasColumnName("DeletedUuid")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.Parent_Parent_保守計画ID)
                .HasColumnName("Parent_Parent_PLAN_ID")
                .HasMaxLength(20)
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.Parent_工程番号)
                .HasColumnName("Parent_PROCESS_NO")
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.手順番号)
                .HasColumnName("PROCEDURE_NO")
                .IsRequired(true)
                .HasColumnOrder(3);
            entity.Property(e => e.手順内容)
                .HasColumnName("PROCEDURE_TEXT")
                .HasMaxLength(1000)
                .IsRequired(false)
                .HasColumnOrder(4);
            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired(false)
                .HasColumnOrder(5);
            entity.Property(e => e.CreateUser)
                .HasColumnName("CreateUser")
                .IsRequired(false)
                .HasColumnOrder(6);
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false)
                .HasColumnOrder(7);
            entity.Property(e => e.UpdateUser)
                .HasColumnName("UpdateUser")
                .IsRequired(false)
                .HasColumnOrder(8);
    
            entity.HasMany(e => e.確認項目)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.DeletedUuid,
                    e.Parent_Parent_工程番号,
                    e.Parent_手順番号,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DELETED_PROCEDURE_CHECK_ITEM");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating確認項目Deleted(ModelBuilder modelBuilder) {
        modelBuilder.Entity<確認項目DeletedDbEntity>(entity => {
            entity.ToTable("CHECK_ITEM_DELETED");
    
            entity.HasKey(e => new {
                e.DeletedUuid,
                e.Parent_Parent_工程番号,
                e.Parent_手順番号,
                e.確認項目番号,
            })
            .HasName("PK_CHECK_ITEM_DELETED");
    
            entity.Property(e => e.DeletedUuid)
                .HasColumnName("DeletedUuid")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.Parent_Parent_Parent_保守計画ID)
                .HasColumnName("Parent_Parent_Parent_PLAN_ID")
                .HasMaxLength(20)
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.Parent_Parent_工程番号)
                .HasColumnName("Parent_Parent_PROCESS_NO")
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.Parent_手順番号)
                .HasColumnName("Parent_PROCEDURE_NO")
                .IsRequired(true)
                .HasColumnOrder(3);
            entity.Property(e => e.確認項目番号)
                .HasColumnName("CHECK_ITEM_NO")
                .IsRequired(true)
                .HasColumnOrder(4);
            entity.Property(e => e.確認内容)
                .HasColumnName("CHECK_TEXT")
                .HasMaxLength(100)
                .IsRequired(false)
                .HasColumnOrder(5);
            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired(false)
                .HasColumnOrder(6);
            entity.Property(e => e.CreateUser)
                .HasColumnName("CreateUser")
                .IsRequired(false)
                .HasColumnOrder(7);
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false)
                .HasColumnOrder(8);
            entity.Property(e => e.UpdateUser)
                .HasColumnName("UpdateUser")
                .IsRequired(false)
                .HasColumnOrder(9);
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating作業体制Deleted(ModelBuilder modelBuilder) {
        modelBuilder.Entity<作業体制DeletedDbEntity>(entity => {
            entity.ToTable("WORK_TEAM_DELETED");
    
            entity.HasKey(e => new {
                e.DeletedUuid,
                e.Parent_工程番号,
            })
            .HasName("PK_WORK_TEAM_DELETED");
    
            entity.Property(e => e.DeletedUuid)
                .HasColumnName("DeletedUuid")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.Parent_Parent_保守計画ID)
                .HasColumnName("Parent_Parent_PLAN_ID")
                .HasMaxLength(20)
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.Parent_工程番号)
                .HasColumnName("Parent_PROCESS_NO")
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.責任者名)
                .HasColumnName("LEADER_NAME")
                .HasMaxLength(50)
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired(false)
                .HasColumnOrder(4);
            entity.Property(e => e.CreateUser)
                .HasColumnName("CreateUser")
                .IsRequired(false)
                .HasColumnOrder(5);
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false)
                .HasColumnOrder(6);
            entity.Property(e => e.UpdateUser)
                .HasColumnName("UpdateUser")
                .IsRequired(false)
                .HasColumnOrder(7);
    
            entity.HasMany(e => e.作業者)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.DeletedUuid,
                    e.Parent_Parent_工程番号,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DELETED_WORK_TEAM_WORKER");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating作業者Deleted(ModelBuilder modelBuilder) {
        modelBuilder.Entity<作業者DeletedDbEntity>(entity => {
            entity.ToTable("WORKER_DELETED");
    
            entity.HasKey(e => new {
                e.DeletedUuid,
                e.Parent_Parent_工程番号,
                e.作業者番号,
            })
            .HasName("PK_WORKER_DELETED");
    
            entity.Property(e => e.DeletedUuid)
                .HasColumnName("DeletedUuid")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.Parent_Parent_Parent_保守計画ID)
                .HasColumnName("Parent_Parent_Parent_PLAN_ID")
                .HasMaxLength(20)
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.Parent_Parent_工程番号)
                .HasColumnName("Parent_Parent_PROCESS_NO")
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.作業者番号)
                .HasColumnName("WORKER_NO")
                .IsRequired(true)
                .HasColumnOrder(3);
            entity.Property(e => e.作業者名)
                .HasColumnName("WORKER_NAME")
                .HasMaxLength(50)
                .IsRequired(false)
                .HasColumnOrder(4);
            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired(false)
                .HasColumnOrder(5);
            entity.Property(e => e.CreateUser)
                .HasColumnName("CreateUser")
                .IsRequired(false)
                .HasColumnOrder(6);
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false)
                .HasColumnOrder(7);
            entity.Property(e => e.UpdateUser)
                .HasColumnName("UpdateUser")
                .IsRequired(false)
                .HasColumnOrder(8);
        });
    }
}
#endregion Entity Framework Core エンティティ定義

