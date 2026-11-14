
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
/// 診療科マスタの Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 診療科マスタDbEntity {
    /// <summary>診療科ID</summary>
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 診療科ID { get; set; }
    /// <summary>診療科名</summary>
    [NijoAttr.DbName("STORE_NAME"), NijoAttr.MaxLength(50)]
    public string? 診療科名 { get; set; }
    /// <summary>電話番号</summary>
    [NijoAttr.DbName("PHONE"), NijoAttr.MaxLength(20)]
    public string? 電話番号 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 科長_医療従事者ID { get; set; }
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

    [NijoAttr.DbName("STORE_ADDRESS")]
    public virtual 診療科マスタの住所DbEntity? 診療科マスタの住所 { get; set; }
    [NijoAttr.DbName("BUSINESS_HOURS")]
    public virtual 診療時間DbEntity? 診療時間 { get; set; }
    [NijoAttr.DbName("MANAGER_ID")]
    public virtual 医療従事者マスタDbEntity? 科長 { get; set; }
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsNotNull]
    public virtual ICollection<診療履歴DbEntity> RefFrom診療履歴_診療科 { get; set; } = [];
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsKey]
    public virtual ICollection<勤務スケジュールDbEntity> RefFrom勤務スケジュール_診療科 { get; set; } = [];
    [NijoAttr.IsKey]
    public virtual ICollection<診療収益分析SearchResult> RefFrom診療収益分析_診療科 { get; set; } = [];

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(診療科マスタDbEntity entity) {
        if (entity.診療科ID != this.診療科ID) return false;
        return true;
    }
}
/// <summary>
/// 住所の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("STORE_ADDRESS")]
public partial class 診療科マスタの住所DbEntity {
    /// <summary>診療科ID</summary>
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? Parent_診療科ID { get; set; }
    /// <summary>郵便番号</summary>
    [NijoAttr.DbName("POSTAL_CODE"), NijoAttr.MaxLength(8), NijoAttr.AvailableCharacter(NijoAttr.AvailableCharacterAttribute.E_Value.半角数字および半角ハイフンのみ)]
    public string? 郵便番号 { get; set; }
    /// <summary>都道府県</summary>
    [NijoAttr.DbName("PREFECTURE"), NijoAttr.MaxLength(10)]
    public string? 都道府県 { get; set; }
    /// <summary>市区町村</summary>
    [NijoAttr.DbName("CITY"), NijoAttr.MaxLength(20)]
    public string? 市区町村 { get; set; }
    /// <summary>番地建物名</summary>
    [NijoAttr.DbName("ADDRESS_LINE"), NijoAttr.MaxLength(100)]
    public string? 番地建物名 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.IsStored, NijoAttr.IsQueryable]
    public virtual 診療科マスタDbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(診療科マスタの住所DbEntity entity) {
        if (entity.Parent_診療科ID != this.Parent_診療科ID) return false;
        return true;
    }
}
/// <summary>
/// 診療時間の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("BUSINESS_HOURS")]
public partial class 診療時間DbEntity {
    /// <summary>診療科ID</summary>
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? Parent_診療科ID { get; set; }
    /// <summary>開始時間</summary>
    [NijoAttr.DbName("OPENING_TIME"), NijoAttr.MaxLength(5)]
    public string? 開始時間 { get; set; }
    /// <summary>終了時間</summary>
    [NijoAttr.DbName("CLOSING_TIME"), NijoAttr.MaxLength(5)]
    public string? 終了時間 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.IsStored, NijoAttr.IsQueryable]
    public virtual 診療科マスタDbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(診療時間DbEntity entity) {
        if (entity.Parent_診療科ID != this.Parent_診療科ID) return false;
        return true;
    }
}

partial class AutoGeneratedDbContext {
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating診療科マスタ(ModelBuilder modelBuilder) {
        modelBuilder.Entity<診療科マスタDbEntity>(entity => {
            entity.ToTable("診療科マスタ");
    
            entity.HasKey(e => new {
                e.診療科ID,
            })
            .HasName("PK_診療科マスタ");
    
            entity.Property(e => e.診療科ID)
                .HasColumnName("STORE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.診療科名)
                .HasColumnName("STORE_NAME")
                .HasMaxLength(50)
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.電話番号)
                .HasColumnName("PHONE")
                .HasMaxLength(20)
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.科長_医療従事者ID)
                .HasColumnName("科長_ID")
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
                .IsRequired(true)
                .IsConcurrencyToken(true)
                .HasColumnOrder(8);
    
            entity.HasOne(e => e.診療科マスタの住所)
                .WithOne(e => e.Parent)
                .HasForeignKey<診療科マスタの住所DbEntity>(e => new {
                    e.Parent_診療科ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_診療科マスタ_STORE_ADDRESS");
    
            entity.HasOne(e => e.診療時間)
                .WithOne(e => e.Parent)
                .HasForeignKey<診療時間DbEntity>(e => new {
                    e.Parent_診療科ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_診療科マスタ_BUSINESS_HOURS");
    
            entity.HasOne(e => e.科長)
                .WithMany(e => e.RefFrom診療科マスタ_科長)
                .HasForeignKey(e => new {
                    e.科長_医療従事者ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_EMPLOYEE_診療科マスタ_X170D2D7");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating診療科マスタの住所(ModelBuilder modelBuilder) {
        modelBuilder.Entity<診療科マスタの住所DbEntity>(entity => {
            entity.ToTable("STORE_ADDRESS");
    
            entity.HasKey(e => new {
                e.Parent_診療科ID,
            })
            .HasName("PK_STORE_ADDRESS");
    
            entity.Property(e => e.Parent_診療科ID)
                .HasColumnName("Parent_STORE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.郵便番号)
                .HasColumnName("POSTAL_CODE")
                .HasMaxLength(8)
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.都道府県)
                .HasColumnName("PREFECTURE")
                .HasMaxLength(10)
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.市区町村)
                .HasColumnName("CITY")
                .HasMaxLength(20)
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.番地建物名)
                .HasColumnName("ADDRESS_LINE")
                .HasMaxLength(100)
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
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating診療時間(ModelBuilder modelBuilder) {
        modelBuilder.Entity<診療時間DbEntity>(entity => {
            entity.ToTable("BUSINESS_HOURS");
    
            entity.HasKey(e => new {
                e.Parent_診療科ID,
            })
            .HasName("PK_BUSINESS_HOURS");
    
            entity.Property(e => e.Parent_診療科ID)
                .HasColumnName("Parent_STORE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.開始時間)
                .HasColumnName("OPENING_TIME")
                .HasMaxLength(5)
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.終了時間)
                .HasColumnName("CLOSING_TIME")
                .HasMaxLength(5)
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
        });
    }
}
#endregion Entity Framework Core エンティティ定義

