
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
/// 患者マスタの Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.LatinName("Patient Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 患者マスタDbEntity {
    /// <summary>患者ID</summary>
    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 患者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("CUSTOMER_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(50)]
    public string? 氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("CUSTOMER_KANA"), NijoAttr.MaxLength(50)]
    public string? 氏名カナ { get; set; }
    /// <summary>生年月日</summary>
    [NijoAttr.DbName("BIRTH_DATE")]
    public DateOnly? 生年月日 { get; set; }
    /// <summary>性別</summary>
    [NijoAttr.DbName("GENDER")]
    public gender_type? 性別 { get; set; }
    /// <summary>メールアドレス</summary>
    [NijoAttr.DbName("EMAIL"), NijoAttr.MaxLength(100)]
    public string? メールアドレス { get; set; }
    /// <summary>電話番号</summary>
    [NijoAttr.DbName("PHONE"), NijoAttr.MaxLength(20)]
    public string? 電話番号 { get; set; }
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

    [NijoAttr.DbName("CUSTOMER_ADDRESS")]
    public virtual 患者マスタの住所DbEntity? 患者マスタの住所 { get; set; }
    [NijoAttr.DbName("MEMBERSHIP")]
    public virtual 患者情報DbEntity? 患者情報 { get; set; }
    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsNotNull]
    public virtual ICollection<診療履歴DbEntity> RefFrom診療履歴_患者 { get; set; } = [];
    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsNotNull]
    public virtual ICollection<予約DbEntity> RefFrom予約_患者 { get; set; } = [];

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(患者マスタDbEntity entity) {
        if (entity.患者ID != this.患者ID) return false;
        return true;
    }
}
/// <summary>
/// 住所の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("CUSTOMER_ADDRESS")]
public partial class 患者マスタの住所DbEntity {
    /// <summary>患者ID</summary>
    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_患者ID { get; set; }
    /// <summary>郵便番号</summary>
    [NijoAttr.DbName("POSTAL_CODE"), NijoAttr.MaxLength(8)]
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

    [NijoAttr.LatinName("Patient Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public virtual 患者マスタDbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(患者マスタの住所DbEntity entity) {
        if (entity.Parent_患者ID != this.Parent_患者ID) return false;
        return true;
    }
}
/// <summary>
/// 患者情報の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("MEMBERSHIP")]
public partial class 患者情報DbEntity {
    /// <summary>患者ID</summary>
    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_患者ID { get; set; }
    /// <summary>患者分類</summary>
    [NijoAttr.DbName("RANK")]
    public member_rank? 患者分類 { get; set; }
    /// <summary>初診日</summary>
    [NijoAttr.DbName("JOIN_DATE")]
    public DateOnly? 初診日 { get; set; }
    /// <summary>最終受診日</summary>
    [NijoAttr.DbName("LAST_VISIT")]
    public DateOnly? 最終受診日 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.LatinName("Patient Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public virtual 患者マスタDbEntity? Parent { get; set; }
    [NijoAttr.DbName("POINT_HISTORY")]
    public virtual ICollection<患者情報の診療履歴DbEntity> 患者情報の診療履歴 { get; set; } = [];

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(患者情報DbEntity entity) {
        if (entity.Parent_患者ID != this.Parent_患者ID) return false;
        return true;
    }
}
/// <summary>
/// 診療履歴の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("POINT_HISTORY")]
public partial class 患者情報の診療履歴DbEntity {
    /// <summary>患者ID</summary>
    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_Parent_患者ID { get; set; }
    /// <summary>履歴ID</summary>
    [NijoAttr.DbName("HISTORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(36)]
    public string? 履歴ID { get; set; }
    /// <summary>日付</summary>
    [NijoAttr.DbName("DATE")]
    public DateOnly? 日付 { get; set; }
    /// <summary>診療点数</summary>
    [NijoAttr.DbName("POINTS")]
    public int? 診療点数 { get; set; }
    /// <summary>診療内容</summary>
    [NijoAttr.DbName("REASON"), NijoAttr.MaxLength(100)]
    public string? 診療内容 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.DbName("MEMBERSHIP")]
    public virtual 患者情報DbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(患者情報の診療履歴DbEntity entity) {
        if (entity.Parent_Parent_患者ID != this.Parent_Parent_患者ID) return false;
        if (entity.履歴ID != this.履歴ID) return false;
        return true;
    }
}

partial class AutoGeneratedDbContext {
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating患者マスタ(ModelBuilder modelBuilder) {
        modelBuilder.Entity<患者マスタDbEntity>(entity => {
            entity.ToTable("患者マスタ");
    
            entity.HasKey(e => new {
                e.患者ID,
            })
            .HasName("PK_患者マスタ");
    
            entity.Property(e => e.患者ID)
                .HasColumnName("CUSTOMER_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.氏名)
                .HasColumnName("CUSTOMER_NAME")
                .HasMaxLength(50)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.氏名カナ)
                .HasColumnName("CUSTOMER_KANA")
                .HasMaxLength(50)
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.生年月日)
                .HasColumnName("BIRTH_DATE")
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.性別)
                .HasColumnName("GENDER")
                .IsRequired(false)
                .HasColumnOrder(4);
            entity.Property(e => e.メールアドレス)
                .HasColumnName("EMAIL")
                .HasMaxLength(100)
                .IsRequired(false)
                .HasColumnOrder(5);
            entity.Property(e => e.電話番号)
                .HasColumnName("PHONE")
                .HasMaxLength(20)
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
            entity.Property(e => e.Version)
                .HasColumnName("Version")
                .IsRequired(true)
                .IsConcurrencyToken(true)
                .HasColumnOrder(11);
    
            entity.HasOne(e => e.患者マスタの住所)
                .WithOne(e => e.Parent)
                .HasForeignKey<患者マスタの住所DbEntity>(e => new {
                    e.Parent_患者ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_患者マスタ_CUSTOMER_ADDRESS");
    
            entity.HasOne(e => e.患者情報)
                .WithOne(e => e.Parent)
                .HasForeignKey<患者情報DbEntity>(e => new {
                    e.Parent_患者ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_患者マスタ_MEMBERSHIP");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating患者マスタの住所(ModelBuilder modelBuilder) {
        modelBuilder.Entity<患者マスタの住所DbEntity>(entity => {
            entity.ToTable("CUSTOMER_ADDRESS");
    
            entity.HasKey(e => new {
                e.Parent_患者ID,
            })
            .HasName("PK_CUSTOMER_ADDRESS");
    
            entity.Property(e => e.Parent_患者ID)
                .HasColumnName("Parent_CUSTOMER_ID")
                .HasMaxLength(20)
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
    protected virtual void OnModelCreating患者情報(ModelBuilder modelBuilder) {
        modelBuilder.Entity<患者情報DbEntity>(entity => {
            entity.ToTable("MEMBERSHIP");
    
            entity.HasKey(e => new {
                e.Parent_患者ID,
            })
            .HasName("PK_MEMBERSHIP");
    
            entity.Property(e => e.Parent_患者ID)
                .HasColumnName("Parent_CUSTOMER_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.患者分類)
                .HasColumnName("RANK")
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.初診日)
                .HasColumnName("JOIN_DATE")
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.最終受診日)
                .HasColumnName("LAST_VISIT")
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
    
            entity.HasMany(e => e.患者情報の診療履歴)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.Parent_Parent_患者ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_MEMBERSHIP_POINT_HISTORY");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating患者情報の診療履歴(ModelBuilder modelBuilder) {
        modelBuilder.Entity<患者情報の診療履歴DbEntity>(entity => {
            entity.ToTable("POINT_HISTORY");
    
            entity.HasKey(e => new {
                e.Parent_Parent_患者ID,
                e.履歴ID,
            })
            .HasName("PK_POINT_HISTORY");
    
            entity.Property(e => e.Parent_Parent_患者ID)
                .HasColumnName("Parent_Parent_CUSTOMER_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.履歴ID)
                .HasColumnName("HISTORY_ID")
                .HasMaxLength(36)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.日付)
                .HasColumnName("DATE")
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.診療点数)
                .HasColumnName("POINTS")
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.診療内容)
                .HasColumnName("REASON")
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
}
#endregion Entity Framework Core エンティティ定義

