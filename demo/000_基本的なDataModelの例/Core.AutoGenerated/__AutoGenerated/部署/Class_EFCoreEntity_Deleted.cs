
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
/// 論理削除された部署の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable, NijoAttr.UseSoftDelete]
public partial class 部署DeletedDbEntity {
    /// <summary>論理削除UUID</summary>
    public Guid? DeletedUuid { get; set; }
    /// <summary>部署ID</summary>
    [NijoAttr.IsKey]
    public int? 部署ID { get; set; }
    /// <summary>部署名</summary>
    [NijoAttr.IsNotNull]
    public string? 部署名 { get; set; }
    /// <summary>事業所ID</summary>
    [NijoAttr.IsKey]
    public string? 事業所_事業所ID { get; set; }
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

    [NijoAttr.UniqueConstraints("1f6febf0-adfe-4377-8749-9eb7223ef14c;")]
    public virtual ICollection<課DeletedDbEntity> 課 { get; set; } = [];

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(部署DeletedDbEntity entity) {
        if (entity.DeletedUuid != this.DeletedUuid) return false;
        return true;
    }
}
/// <summary>
/// 論理削除された課の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.UniqueConstraints("1f6febf0-adfe-4377-8749-9eb7223ef14c;")]
public partial class 課DeletedDbEntity {
    /// <summary>論理削除UUID</summary>
    public Guid? DeletedUuid { get; set; }
    /// <summary>部署ID</summary>
    [NijoAttr.IsKey]
    public int? Parent_部署ID { get; set; }
    /// <summary>コード</summary>
    [NijoAttr.IsKey]
    public string? コード { get; set; }
    /// <summary>旧システムコード</summary>
    [NijoAttr.IsKey]
    public string? 旧システムコード_旧システムコード { get; set; }
    /// <summary>課名称</summary>
    [NijoAttr.IsNotNull]
    public string? 課名称 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.IsStored, NijoAttr.IsQueryable, NijoAttr.UseSoftDelete]
    public virtual 部署DeletedDbEntity? Parent { get; set; }
    public virtual ICollection<係DeletedDbEntity> 係 { get; set; } = [];

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(課DeletedDbEntity entity) {
        if (entity.DeletedUuid != this.DeletedUuid) return false;
        if (entity.コード != this.コード) return false;
        return true;
    }
}
/// <summary>
/// 論理削除された係の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
public partial class 係DeletedDbEntity {
    /// <summary>論理削除UUID</summary>
    public Guid? DeletedUuid { get; set; }
    /// <summary>部署ID</summary>
    [NijoAttr.IsKey]
    public int? Parent_Parent_部署ID { get; set; }
    /// <summary>コード</summary>
    [NijoAttr.IsKey]
    public string? Parent_コード { get; set; }
    /// <summary>連番</summary>
    [NijoAttr.IsKey]
    public int? 連番 { get; set; }
    /// <summary>係名称</summary>
    [NijoAttr.IsNotNull]
    public string? 係名称 { get; set; }
    /// <summary>区分値</summary>
    [NijoAttr.IsKey]
    public string? 勤怠管理区分_区分値 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.UniqueConstraints("1f6febf0-adfe-4377-8749-9eb7223ef14c;")]
    public virtual 課DeletedDbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(係DeletedDbEntity entity) {
        if (entity.DeletedUuid != this.DeletedUuid) return false;
        if (entity.Parent_コード != this.Parent_コード) return false;
        if (entity.連番 != this.連番) return false;
        return true;
    }
}

partial class AutoGeneratedDbContext {
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating部署Deleted(ModelBuilder modelBuilder) {
        modelBuilder.Entity<部署DeletedDbEntity>(entity => {
            entity.ToTable("部署_DELETED");
    
            entity.HasKey(e => new {
                e.DeletedUuid,
            })
            .HasName("PK_部署_DELETED");
    
            entity.Property(e => e.DeletedUuid)
                .HasColumnName("DeletedUuid")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.部署ID)
                .HasColumnName("部署ID")
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.部署名)
                .HasColumnName("部署名")
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.事業所_事業所ID)
                .HasColumnName("事業所_事業所ID")
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
            entity.Property(e => e.Version)
                .HasColumnName("Version")
                .IsRequired(false)
                .IsConcurrencyToken(false)
                .HasColumnOrder(8);
    
            // 論理削除テーブルは削除日時でソートされることが多いのでインデックスを張る
            entity.HasIndex(e => e.DeletedAt)
                .HasDatabaseName("IX_部署_DELETED_DELETED_AT");
    
            entity.HasMany(e => e.課)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.DeletedUuid,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DELETED_部署_課");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating課Deleted(ModelBuilder modelBuilder) {
        modelBuilder.Entity<課DeletedDbEntity>(entity => {
            entity.ToTable("課_DELETED");
    
            entity.HasKey(e => new {
                e.DeletedUuid,
                e.コード,
            })
            .HasName("PK_課_DELETED");
    
            entity.Property(e => e.DeletedUuid)
                .HasColumnName("DeletedUuid")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.Parent_部署ID)
                .HasColumnName("Parent_部署ID")
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.コード)
                .HasColumnName("コード")
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.旧システムコード_旧システムコード)
                .HasColumnName("旧システムコード_旧システムコード")
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.課名称)
                .HasColumnName("課名称")
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
    
            entity.HasMany(e => e.係)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.DeletedUuid,
                    e.Parent_コード,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DELETED_課_係");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating係Deleted(ModelBuilder modelBuilder) {
        modelBuilder.Entity<係DeletedDbEntity>(entity => {
            entity.ToTable("係_DELETED");
    
            entity.HasKey(e => new {
                e.DeletedUuid,
                e.Parent_コード,
                e.連番,
            })
            .HasName("PK_係_DELETED");
    
            entity.Property(e => e.DeletedUuid)
                .HasColumnName("DeletedUuid")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.Parent_Parent_部署ID)
                .HasColumnName("Parent_Parent_部署ID")
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.Parent_コード)
                .HasColumnName("Parent_コード")
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.連番)
                .HasColumnName("連番")
                .IsRequired(true)
                .HasColumnOrder(3);
            entity.Property(e => e.係名称)
                .HasColumnName("係名称")
                .IsRequired(false)
                .HasColumnOrder(4);
            entity.Property(e => e.勤怠管理区分_区分値)
                .HasColumnName("勤怠管理区分_区分値")
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
}
#endregion Entity Framework Core エンティティ定義

