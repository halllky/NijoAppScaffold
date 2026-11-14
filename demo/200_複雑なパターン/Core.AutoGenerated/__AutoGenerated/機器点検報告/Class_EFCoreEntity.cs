
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
/// 機器点検報告の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.LatinName("Equipment Inspection Report"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 機器点検報告DbEntity {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 対象機器_Parent_機器ID { get; set; }
    /// <summary>保管庫ID</summary>
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 対象機器_保管庫_保管庫ID { get; set; }
    /// <summary>点検日</summary>
    [NijoAttr.DbName("SURVEY_DATE"), NijoAttr.IsNotNull]
    public DateOnly? 点検日 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 点検担当者_医療従事者ID { get; set; }
    /// <summary>実地確認数</summary>
    [NijoAttr.DbName("ACTUAL_COUNT"), NijoAttr.IsNotNull]
    public int? 実地確認数 { get; set; }
    /// <summary>在庫差異</summary>
    [NijoAttr.DbName("INVENTORY_DIFF")]
    public int? 在庫差異 { get; set; }
    /// <summary>点検メモ</summary>
    [NijoAttr.DbName("SURVEY_NOTE"), NijoAttr.MaxLength(500)]
    public string? 点検メモ { get; set; }
    /// <summary>写真URL</summary>
    [NijoAttr.DbName("PHOTO_URL"), NijoAttr.MaxLength(200)]
    public string? 写真URL { get; set; }
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

    [NijoAttr.DbName("INVENTORY_REF"), NijoAttr.IsKey]
    public virtual 在庫情報DbEntity? 対象機器 { get; set; }
    [NijoAttr.DbName("SURVEYOR_ID"), NijoAttr.IsNotNull]
    public virtual 医療従事者マスタDbEntity? 点検担当者 { get; set; }
    [NijoAttr.DbName("ACTIONS")]
    public virtual ICollection<対応措置DbEntity> 対応措置 { get; set; } = [];

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(機器点検報告DbEntity entity) {
        if (entity.対象機器_Parent_機器ID != this.対象機器_Parent_機器ID) return false;
        if (entity.対象機器_保管庫_保管庫ID != this.対象機器_保管庫_保管庫ID) return false;
        return true;
    }
}
/// <summary>
/// 対応措置の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("ACTIONS")]
public partial class 対応措置DbEntity {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_対象機器_Parent_機器ID { get; set; }
    /// <summary>保管庫ID</summary>
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? Parent_対象機器_保管庫_保管庫ID { get; set; }
    /// <summary>措置ID</summary>
    [NijoAttr.DbName("ACTION_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? 措置ID { get; set; }
    /// <summary>措置種別</summary>
    [NijoAttr.DbName("ACTION_TYPE")]
    public action_type? 措置種別 { get; set; }
    /// <summary>実施状況</summary>
    [NijoAttr.DbName("STATUS")]
    public status_type? 実施状況 { get; set; }
    /// <summary>実施日</summary>
    [NijoAttr.DbName("ACTION_DATE")]
    public DateOnly? 実施日 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 実施担当者_医療従事者ID { get; set; }
    /// <summary>措置詳細</summary>
    [NijoAttr.DbName("ACTION_DETAIL"), NijoAttr.MaxLength(500)]
    public string? 措置詳細 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.LatinName("Equipment Inspection Report"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public virtual 機器点検報告DbEntity? Parent { get; set; }
    [NijoAttr.DbName("EXECUTOR_ID")]
    public virtual 医療従事者マスタDbEntity? 実施担当者 { get; set; }
    [NijoAttr.DbName("ACTION_REF"), NijoAttr.IsKey]
    public virtual 措置結果DbEntity? RefFrom措置結果_対象措置 { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(対応措置DbEntity entity) {
        if (entity.Parent_対象機器_Parent_機器ID != this.Parent_対象機器_Parent_機器ID) return false;
        if (entity.Parent_対象機器_保管庫_保管庫ID != this.Parent_対象機器_保管庫_保管庫ID) return false;
        if (entity.措置ID != this.措置ID) return false;
        return true;
    }
}

partial class AutoGeneratedDbContext {
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating機器点検報告(ModelBuilder modelBuilder) {
        modelBuilder.Entity<機器点検報告DbEntity>(entity => {
            entity.ToTable("機器点検報告");
    
            entity.HasKey(e => new {
                e.対象機器_Parent_機器ID,
                e.対象機器_保管庫_保管庫ID,
            })
            .HasName("PK_機器点検報告");
    
            entity.Property(e => e.対象機器_Parent_機器ID)
                .HasColumnName("対象機器_Parent_PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.対象機器_保管庫_保管庫ID)
                .HasColumnName("対象機器_保管庫_WAREHOUSE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.点検日)
                .HasColumnName("SURVEY_DATE")
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.点検担当者_医療従事者ID)
                .HasColumnName("点検担当者_ID")
                .IsRequired(true)
                .HasColumnOrder(3);
            entity.Property(e => e.実地確認数)
                .HasColumnName("ACTUAL_COUNT")
                .IsRequired(true)
                .HasColumnOrder(4);
            entity.Property(e => e.在庫差異)
                .HasColumnName("INVENTORY_DIFF")
                .IsRequired(false)
                .HasColumnOrder(5);
            entity.Property(e => e.点検メモ)
                .HasColumnName("SURVEY_NOTE")
                .HasMaxLength(500)
                .IsRequired(false)
                .HasColumnOrder(6);
            entity.Property(e => e.写真URL)
                .HasColumnName("PHOTO_URL")
                .HasMaxLength(200)
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
    
            entity.HasMany(e => e.対応措置)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.Parent_対象機器_Parent_機器ID,
                    e.Parent_対象機器_保管庫_保管庫ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_機器点検報告_ACTIONS");
    
            entity.HasOne(e => e.対象機器)
                .WithOne(e => e.RefFrom機器点検報告_対象機器)
                .HasForeignKey<機器点検報告DbEntity>(e => new {
                    e.対象機器_Parent_機器ID,
                    e.対象機器_保管庫_保管庫ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_INVENTORY_機器点検報告_X62E8D1C");
    
            entity.HasOne(e => e.点検担当者)
                .WithMany(e => e.RefFrom機器点検報告_点検担当者)
                .HasForeignKey(e => new {
                    e.点検担当者_医療従事者ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_EMPLOYEE_機器点検報告_XC084C68");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating対応措置(ModelBuilder modelBuilder) {
        modelBuilder.Entity<対応措置DbEntity>(entity => {
            entity.ToTable("ACTIONS");
    
            entity.HasKey(e => new {
                e.Parent_対象機器_Parent_機器ID,
                e.Parent_対象機器_保管庫_保管庫ID,
                e.措置ID,
            })
            .HasName("PK_ACTIONS");
    
            entity.Property(e => e.Parent_対象機器_Parent_機器ID)
                .HasColumnName("Parent_対象機器_Parent_PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.Parent_対象機器_保管庫_保管庫ID)
                .HasColumnName("Parent_対象機器_保管庫_WAREHOUSE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.措置ID)
                .HasColumnName("ACTION_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.措置種別)
                .HasColumnName("ACTION_TYPE")
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.実施状況)
                .HasColumnName("STATUS")
                .IsRequired(false)
                .HasColumnOrder(4);
            entity.Property(e => e.実施日)
                .HasColumnName("ACTION_DATE")
                .IsRequired(false)
                .HasColumnOrder(5);
            entity.Property(e => e.実施担当者_医療従事者ID)
                .HasColumnName("実施担当者_ID")
                .IsRequired(false)
                .HasColumnOrder(6);
            entity.Property(e => e.措置詳細)
                .HasColumnName("ACTION_DETAIL")
                .HasMaxLength(500)
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
    
            entity.HasOne(e => e.実施担当者)
                .WithMany(e => e.RefFrom対応措置_実施担当者)
                .HasForeignKey(e => new {
                    e.実施担当者_医療従事者ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_EMPLOYEE_ACTIONS_X4FD1CEF");
        });
    }
}
#endregion Entity Framework Core エンティティ定義

