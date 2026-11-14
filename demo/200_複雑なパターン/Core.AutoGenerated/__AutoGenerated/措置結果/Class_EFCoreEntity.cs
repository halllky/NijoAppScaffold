
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
/// 措置結果の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.LatinName("Action Result"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 措置結果DbEntity {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 対象措置_Parent_対象機器_Parent_機器ID { get; set; }
    /// <summary>保管庫ID</summary>
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 対象措置_Parent_対象機器_保管庫_保管庫ID { get; set; }
    /// <summary>措置ID</summary>
    [NijoAttr.DbName("ACTION_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? 対象措置_措置ID { get; set; }
    /// <summary>結果日時</summary>
    [NijoAttr.DbName("RESULT_DATETIME"), NijoAttr.IsNotNull]
    public DateTime? 結果日時 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 結果担当者_医療従事者ID { get; set; }
    /// <summary>改善効果</summary>
    [NijoAttr.DbName("ACHIEVEMENT")]
    public int? 改善効果 { get; set; }
    /// <summary>結果状態</summary>
    [NijoAttr.DbName("RESULT_STATUS"), NijoAttr.IsNotNull]
    public result_status? 結果状態 { get; set; }
    /// <summary>フィードバック</summary>
    [NijoAttr.DbName("FEEDBACK"), NijoAttr.MaxLength(1000)]
    public string? フィードバック { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }
    /// <summary>楽観排他制御用のバージョニング用カラム</summary>
    public int? Version { get; set; }

    [NijoAttr.DbName("ACTION_REF"), NijoAttr.IsKey]
    public virtual 対応措置DbEntity? 対象措置 { get; set; }
    [NijoAttr.DbName("RESULT_STAFF_ID"), NijoAttr.IsNotNull]
    public virtual 医療従事者マスタDbEntity? 結果担当者 { get; set; }
    [NijoAttr.DbName("ATTACHMENTS")]
    public virtual ICollection<添付資料DbEntity> 添付資料 { get; set; } = [];
    [NijoAttr.DbName("NEXT_ACTION")]
    public virtual 次回措置DbEntity? 次回措置 { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(措置結果DbEntity entity) {
        if (entity.対象措置_Parent_対象機器_Parent_機器ID != this.対象措置_Parent_対象機器_Parent_機器ID) return false;
        if (entity.対象措置_Parent_対象機器_保管庫_保管庫ID != this.対象措置_Parent_対象機器_保管庫_保管庫ID) return false;
        if (entity.対象措置_措置ID != this.対象措置_措置ID) return false;
        return true;
    }
}
/// <summary>
/// 添付資料の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("ATTACHMENTS")]
public partial class 添付資料DbEntity {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_対象措置_Parent_対象機器_Parent_機器ID { get; set; }
    /// <summary>保管庫ID</summary>
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? Parent_対象措置_Parent_対象機器_保管庫_保管庫ID { get; set; }
    /// <summary>措置ID</summary>
    [NijoAttr.DbName("ACTION_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? Parent_対象措置_措置ID { get; set; }
    /// <summary>資料ID</summary>
    [NijoAttr.DbName("DOCUMENT_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? 資料ID { get; set; }
    /// <summary>資料名</summary>
    [NijoAttr.DbName("DOCUMENT_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(100)]
    public string? 資料名 { get; set; }
    /// <summary>資料種別</summary>
    [NijoAttr.DbName("DOCUMENT_TYPE")]
    public document_type? 資料種別 { get; set; }
    /// <summary>ファイルパス</summary>
    [NijoAttr.DbName("FILE_PATH"), NijoAttr.MaxLength(200)]
    public string? ファイルパス { get; set; }
    /// <summary>登録日時</summary>
    [NijoAttr.DbName("REGISTER_DATETIME")]
    public DateTime? 登録日時 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.LatinName("Action Result"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public virtual 措置結果DbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(添付資料DbEntity entity) {
        if (entity.Parent_対象措置_Parent_対象機器_Parent_機器ID != this.Parent_対象措置_Parent_対象機器_Parent_機器ID) return false;
        if (entity.Parent_対象措置_Parent_対象機器_保管庫_保管庫ID != this.Parent_対象措置_Parent_対象機器_保管庫_保管庫ID) return false;
        if (entity.Parent_対象措置_措置ID != this.Parent_対象措置_措置ID) return false;
        if (entity.資料ID != this.資料ID) return false;
        return true;
    }
}
/// <summary>
/// 次回措置の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("NEXT_ACTION")]
public partial class 次回措置DbEntity {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_対象措置_Parent_対象機器_Parent_機器ID { get; set; }
    /// <summary>保管庫ID</summary>
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? Parent_対象措置_Parent_対象機器_保管庫_保管庫ID { get; set; }
    /// <summary>措置ID</summary>
    [NijoAttr.DbName("ACTION_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? Parent_対象措置_措置ID { get; set; }
    /// <summary>措置種別</summary>
    [NijoAttr.DbName("NEXT_ACTION_TYPE")]
    public action_type? 措置種別 { get; set; }
    /// <summary>予定日</summary>
    [NijoAttr.DbName("PLANNED_DATE")]
    public DateOnly? 予定日 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 担当者_医療従事者ID { get; set; }
    /// <summary>内容</summary>
    [NijoAttr.DbName("NEXT_CONTENT"), NijoAttr.MaxLength(500)]
    public string? 内容 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.LatinName("Action Result"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public virtual 措置結果DbEntity? Parent { get; set; }
    [NijoAttr.DbName("NEXT_STAFF_ID")]
    public virtual 医療従事者マスタDbEntity? 担当者 { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(次回措置DbEntity entity) {
        if (entity.Parent_対象措置_Parent_対象機器_Parent_機器ID != this.Parent_対象措置_Parent_対象機器_Parent_機器ID) return false;
        if (entity.Parent_対象措置_Parent_対象機器_保管庫_保管庫ID != this.Parent_対象措置_Parent_対象機器_保管庫_保管庫ID) return false;
        if (entity.Parent_対象措置_措置ID != this.Parent_対象措置_措置ID) return false;
        return true;
    }
}

partial class AutoGeneratedDbContext {
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating措置結果(ModelBuilder modelBuilder) {
        modelBuilder.Entity<措置結果DbEntity>(entity => {
            entity.ToTable("措置結果");
    
            entity.HasKey(e => new {
                e.対象措置_Parent_対象機器_Parent_機器ID,
                e.対象措置_Parent_対象機器_保管庫_保管庫ID,
                e.対象措置_措置ID,
            })
            .HasName("PK_措置結果");
    
            entity.Property(e => e.対象措置_Parent_対象機器_Parent_機器ID)
                .HasColumnName("対象措置_Parent_対象機器_Parent_PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.対象措置_Parent_対象機器_保管庫_保管庫ID)
                .HasColumnName("対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.対象措置_措置ID)
                .HasColumnName("対象措置_ACTION_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.結果日時)
                .HasColumnName("RESULT_DATETIME")
                .IsRequired(true)
                .HasColumnOrder(3);
            entity.Property(e => e.結果担当者_医療従事者ID)
                .HasColumnName("結果担当者_ID")
                .IsRequired(true)
                .HasColumnOrder(4);
            entity.Property(e => e.改善効果)
                .HasColumnName("ACHIEVEMENT")
                .IsRequired(false)
                .HasColumnOrder(5);
            entity.Property(e => e.結果状態)
                .HasColumnName("RESULT_STATUS")
                .IsRequired(true)
                .HasColumnOrder(6);
            entity.Property(e => e.フィードバック)
                .HasColumnName("FEEDBACK")
                .HasMaxLength(1000)
                .IsRequired(false)
                .HasColumnOrder(7);
            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired(false)
                .HasColumnOrder(8);
            entity.Property(e => e.CreateUser)
                .HasColumnName("CreateUser")
                .IsRequired(false)
                .HasColumnOrder(9);
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false)
                .HasColumnOrder(10);
            entity.Property(e => e.UpdateUser)
                .HasColumnName("UpdateUser")
                .IsRequired(false)
                .HasColumnOrder(11);
            entity.Property(e => e.Version)
                .HasColumnName("Version")
                .IsRequired(true)
                .IsConcurrencyToken(true)
                .HasColumnOrder(12);
    
            entity.HasMany(e => e.添付資料)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.Parent_対象措置_Parent_対象機器_Parent_機器ID,
                    e.Parent_対象措置_Parent_対象機器_保管庫_保管庫ID,
                    e.Parent_対象措置_措置ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_措置結果_ATTACHMENTS");
    
            entity.HasOne(e => e.次回措置)
                .WithOne(e => e.Parent)
                .HasForeignKey<次回措置DbEntity>(e => new {
                    e.Parent_対象措置_Parent_対象機器_Parent_機器ID,
                    e.Parent_対象措置_Parent_対象機器_保管庫_保管庫ID,
                    e.Parent_対象措置_措置ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_措置結果_NEXT_ACTION");
    
            entity.HasOne(e => e.対象措置)
                .WithOne(e => e.RefFrom措置結果_対象措置)
                .HasForeignKey<措置結果DbEntity>(e => new {
                    e.対象措置_Parent_対象機器_Parent_機器ID,
                    e.対象措置_Parent_対象機器_保管庫_保管庫ID,
                    e.対象措置_措置ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ACTIONS_措置結果_XFDF1611");
    
            entity.HasOne(e => e.結果担当者)
                .WithMany(e => e.RefFrom措置結果_結果担当者)
                .HasForeignKey(e => new {
                    e.結果担当者_医療従事者ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_EMPLOYEE_措置結果_XD70B07D");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating添付資料(ModelBuilder modelBuilder) {
        modelBuilder.Entity<添付資料DbEntity>(entity => {
            entity.ToTable("ATTACHMENTS");
    
            entity.HasKey(e => new {
                e.Parent_対象措置_Parent_対象機器_Parent_機器ID,
                e.Parent_対象措置_Parent_対象機器_保管庫_保管庫ID,
                e.Parent_対象措置_措置ID,
                e.資料ID,
            })
            .HasName("PK_ATTACHMENTS");
    
            entity.Property(e => e.Parent_対象措置_Parent_対象機器_Parent_機器ID)
                .HasColumnName("Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.Parent_対象措置_Parent_対象機器_保管庫_保管庫ID)
                .HasColumnName("Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.Parent_対象措置_措置ID)
                .HasColumnName("Parent_対象措置_ACTION_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.資料ID)
                .HasColumnName("DOCUMENT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(3);
            entity.Property(e => e.資料名)
                .HasColumnName("DOCUMENT_NAME")
                .HasMaxLength(100)
                .IsRequired(true)
                .HasColumnOrder(4);
            entity.Property(e => e.資料種別)
                .HasColumnName("DOCUMENT_TYPE")
                .IsRequired(false)
                .HasColumnOrder(5);
            entity.Property(e => e.ファイルパス)
                .HasColumnName("FILE_PATH")
                .HasMaxLength(200)
                .IsRequired(false)
                .HasColumnOrder(6);
            entity.Property(e => e.登録日時)
                .HasColumnName("REGISTER_DATETIME")
                .IsRequired(false)
                .HasColumnOrder(7);
            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired(false)
                .HasColumnOrder(8);
            entity.Property(e => e.CreateUser)
                .HasColumnName("CreateUser")
                .IsRequired(false)
                .HasColumnOrder(9);
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false)
                .HasColumnOrder(10);
            entity.Property(e => e.UpdateUser)
                .HasColumnName("UpdateUser")
                .IsRequired(false)
                .HasColumnOrder(11);
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating次回措置(ModelBuilder modelBuilder) {
        modelBuilder.Entity<次回措置DbEntity>(entity => {
            entity.ToTable("NEXT_ACTION");
    
            entity.HasKey(e => new {
                e.Parent_対象措置_Parent_対象機器_Parent_機器ID,
                e.Parent_対象措置_Parent_対象機器_保管庫_保管庫ID,
                e.Parent_対象措置_措置ID,
            })
            .HasName("PK_NEXT_ACTION");
    
            entity.Property(e => e.Parent_対象措置_Parent_対象機器_Parent_機器ID)
                .HasColumnName("Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.Parent_対象措置_Parent_対象機器_保管庫_保管庫ID)
                .HasColumnName("Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.Parent_対象措置_措置ID)
                .HasColumnName("Parent_対象措置_ACTION_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.措置種別)
                .HasColumnName("NEXT_ACTION_TYPE")
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.予定日)
                .HasColumnName("PLANNED_DATE")
                .IsRequired(false)
                .HasColumnOrder(4);
            entity.Property(e => e.担当者_医療従事者ID)
                .HasColumnName("担当者_ID")
                .IsRequired(false)
                .HasColumnOrder(5);
            entity.Property(e => e.内容)
                .HasColumnName("NEXT_CONTENT")
                .HasMaxLength(500)
                .IsRequired(false)
                .HasColumnOrder(6);
            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired(false)
                .HasColumnOrder(7);
            entity.Property(e => e.CreateUser)
                .HasColumnName("CreateUser")
                .IsRequired(false)
                .HasColumnOrder(8);
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false)
                .HasColumnOrder(9);
            entity.Property(e => e.UpdateUser)
                .HasColumnName("UpdateUser")
                .IsRequired(false)
                .HasColumnOrder(10);
    
            entity.HasOne(e => e.担当者)
                .WithMany(e => e.RefFrom次回措置_担当者)
                .HasForeignKey(e => new {
                    e.担当者_医療従事者ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_EMPLOYEE_NEXT_ACTION_XB332A41");
        });
    }
}
#endregion Entity Framework Core エンティティ定義

