
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
/// 診療履歴の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.LatinName("Treatment History"), NijoAttr.IsStored]
public partial class 診療履歴DbEntity {
    /// <summary>診療ID</summary>
    [NijoAttr.DbName("ORDER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 診療ID { get; set; }
    /// <summary>診療日時</summary>
    [NijoAttr.DbName("ORDER_DATE"), NijoAttr.IsNotNull]
    public DateTime? 診療日時 { get; set; }
    /// <summary>患者ID</summary>
    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 患者_患者ID { get; set; }
    /// <summary>診療科ID</summary>
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 診療科_診療科ID { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 担当医_医療従事者ID { get; set; }
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

    [NijoAttr.DbName("CUSTOMER_ID"), NijoAttr.IsNotNull]
    public virtual 患者マスタDbEntity? 患者 { get; set; }
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsNotNull]
    public virtual 診療科マスタDbEntity? 診療科 { get; set; }
    [NijoAttr.DbName("STAFF_ID")]
    public virtual 医療従事者マスタDbEntity? 担当医 { get; set; }
    [NijoAttr.DbName("ORDER_DETAILS")]
    public virtual ICollection<処置明細DbEntity> 処置明細 { get; set; } = [];
    [NijoAttr.DbName("PAYMENT_INFO")]
    public virtual 会計情報DbEntity? 会計情報 { get; set; }
    [NijoAttr.DbName("SHIPPING_INFO")]
    public virtual 紹介状情報DbEntity? 紹介状情報 { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(診療履歴DbEntity entity) {
        if (entity.診療ID != this.診療ID) return false;
        return true;
    }
}
/// <summary>
/// 処置明細の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("ORDER_DETAILS")]
public partial class 処置明細DbEntity {
    /// <summary>診療ID</summary>
    [NijoAttr.DbName("ORDER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_診療ID { get; set; }
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 医療機器_機器ID { get; set; }
    /// <summary>使用数量</summary>
    [NijoAttr.DbName("QUANTITY"), NijoAttr.IsNotNull]
    public int? 使用数量 { get; set; }
    /// <summary>単価</summary>
    [NijoAttr.DbName("UNIT_PRICE"), NijoAttr.IsNotNull]
    public int? 単価 { get; set; }
    /// <summary>小計</summary>
    [NijoAttr.DbName("SUBTOTAL"), NijoAttr.IsNotNull]
    public int? 小計 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.LatinName("Treatment History"), NijoAttr.IsStored]
    public virtual 診療履歴DbEntity? Parent { get; set; }
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey]
    public virtual 医療機器マスタDbEntity? 医療機器 { get; set; }
    [NijoAttr.DbName("DISCOUNT_INFO")]
    public virtual 保険適用情報DbEntity? 保険適用情報 { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(処置明細DbEntity entity) {
        if (entity.Parent_診療ID != this.Parent_診療ID) return false;
        if (entity.医療機器_機器ID != this.医療機器_機器ID) return false;
        return true;
    }
}
/// <summary>
/// 保険適用情報の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("DISCOUNT_INFO")]
public partial class 保険適用情報DbEntity {
    /// <summary>診療ID</summary>
    [NijoAttr.DbName("ORDER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_Parent_診療ID { get; set; }
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_医療機器_機器ID { get; set; }
    /// <summary>保険コード</summary>
    [NijoAttr.DbName("DISCOUNT_CODE"), NijoAttr.MaxLength(10)]
    public string? 保険コード { get; set; }
    /// <summary>保険適用率</summary>
    [NijoAttr.DbName("DISCOUNT_RATE")]
    public decimal? 保険適用率 { get; set; }
    /// <summary>保険適用額</summary>
    [NijoAttr.DbName("DISCOUNT_AMOUNT")]
    public int? 保険適用額 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.DbName("ORDER_DETAILS")]
    public virtual 処置明細DbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(保険適用情報DbEntity entity) {
        if (entity.Parent_Parent_診療ID != this.Parent_Parent_診療ID) return false;
        if (entity.Parent_医療機器_機器ID != this.Parent_医療機器_機器ID) return false;
        return true;
    }
}
/// <summary>
/// 会計情報の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("PAYMENT_INFO")]
public partial class 会計情報DbEntity {
    /// <summary>診療ID</summary>
    [NijoAttr.DbName("ORDER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_診療ID { get; set; }
    /// <summary>支払方法</summary>
    [NijoAttr.DbName("PAYMENT_TYPE"), NijoAttr.IsNotNull]
    public payment_type? 支払方法 { get; set; }
    /// <summary>支払日</summary>
    [NijoAttr.DbName("PAYMENT_DATE")]
    public DateOnly? 支払日 { get; set; }
    /// <summary>支払状況</summary>
    [NijoAttr.DbName("PAYMENT_STATUS")]
    public payment_status? 支払状況 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.LatinName("Treatment History"), NijoAttr.IsStored]
    public virtual 診療履歴DbEntity? Parent { get; set; }
    [NijoAttr.DbName("CARD_INFO")]
    public virtual カード情報DbEntity? カード情報 { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(会計情報DbEntity entity) {
        if (entity.Parent_診療ID != this.Parent_診療ID) return false;
        return true;
    }
}
/// <summary>
/// カード情報の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("CARD_INFO")]
public partial class カード情報DbEntity {
    /// <summary>診療ID</summary>
    [NijoAttr.DbName("ORDER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_Parent_診療ID { get; set; }
    /// <summary>カード種類</summary>
    [NijoAttr.DbName("CARD_TYPE")]
    public card_type? カード種類 { get; set; }
    /// <summary>下4桁</summary>
    [NijoAttr.DbName("LAST_FOUR_DIGITS"), NijoAttr.MaxLength(4)]
    public string? 下4桁 { get; set; }
    /// <summary>有効期限</summary>
    [NijoAttr.DbName("EXPIRY_DATE")]
    public DateOnly? 有効期限 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.DbName("PAYMENT_INFO")]
    public virtual 会計情報DbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(カード情報DbEntity entity) {
        if (entity.Parent_Parent_診療ID != this.Parent_Parent_診療ID) return false;
        return true;
    }
}
/// <summary>
/// 紹介状情報の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("SHIPPING_INFO")]
public partial class 紹介状情報DbEntity {
    /// <summary>診療ID</summary>
    [NijoAttr.DbName("ORDER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_診療ID { get; set; }
    /// <summary>紹介方法</summary>
    [NijoAttr.DbName("SHIPPING_METHOD"), NijoAttr.IsNotNull]
    public shipping_method? 紹介方法 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.LatinName("Treatment History"), NijoAttr.IsStored]
    public virtual 診療履歴DbEntity? Parent { get; set; }
    [NijoAttr.DbName("SHIPPING_ADDRESS")]
    public virtual 紹介先住所DbEntity? 紹介先住所 { get; set; }
    [NijoAttr.DbName("SHIPPING_STATUS")]
    public virtual ICollection<紹介状況DbEntity> 紹介状況 { get; set; } = [];

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(紹介状情報DbEntity entity) {
        if (entity.Parent_診療ID != this.Parent_診療ID) return false;
        return true;
    }
}
/// <summary>
/// 紹介先住所の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("SHIPPING_ADDRESS")]
public partial class 紹介先住所DbEntity {
    /// <summary>診療ID</summary>
    [NijoAttr.DbName("ORDER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_Parent_診療ID { get; set; }
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

    [NijoAttr.DbName("SHIPPING_INFO")]
    public virtual 紹介状情報DbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(紹介先住所DbEntity entity) {
        if (entity.Parent_Parent_診療ID != this.Parent_Parent_診療ID) return false;
        return true;
    }
}
/// <summary>
/// 紹介状況の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("SHIPPING_STATUS")]
public partial class 紹介状況DbEntity {
    /// <summary>診療ID</summary>
    [NijoAttr.DbName("ORDER_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_Parent_診療ID { get; set; }
    /// <summary>ステータス</summary>
    [NijoAttr.DbName("STATUS"), NijoAttr.IsKey]
    public shipping_status? ステータス { get; set; }
    /// <summary>更新日時</summary>
    [NijoAttr.DbName("UPDATE_DATE")]
    public DateTime? 更新日時 { get; set; }
    /// <summary>備考</summary>
    [NijoAttr.DbName("REMARKS"), NijoAttr.MaxLength(500)]
    public string? 備考 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.DbName("SHIPPING_INFO")]
    public virtual 紹介状情報DbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(紹介状況DbEntity entity) {
        if (entity.Parent_Parent_診療ID != this.Parent_Parent_診療ID) return false;
        if (entity.ステータス != this.ステータス) return false;
        return true;
    }
}

partial class AutoGeneratedDbContext {
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating診療履歴(ModelBuilder modelBuilder) {
        modelBuilder.Entity<診療履歴DbEntity>(entity => {
            entity.ToTable("診療履歴");
    
            entity.HasKey(e => new {
                e.診療ID,
            })
            .HasName("PK_診療履歴");
    
            entity.Property(e => e.診療ID)
                .HasColumnName("ORDER_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.診療日時)
                .HasColumnName("ORDER_DATE")
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.患者_患者ID)
                .HasColumnName("患者_CUSTOMER_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.診療科_診療科ID)
                .HasColumnName("診療科_STORE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(3);
            entity.Property(e => e.担当医_医療従事者ID)
                .HasColumnName("担当医_ID")
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
            entity.Property(e => e.Version)
                .HasColumnName("Version")
                .IsRequired(true)
                .IsConcurrencyToken(true)
                .HasColumnOrder(9);
    
            entity.HasMany(e => e.処置明細)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.Parent_診療ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_診療履歴_ORDER_DETAILS");
    
            entity.HasOne(e => e.会計情報)
                .WithOne(e => e.Parent)
                .HasForeignKey<会計情報DbEntity>(e => new {
                    e.Parent_診療ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_診療履歴_PAYMENT_INFO");
    
            entity.HasOne(e => e.紹介状情報)
                .WithOne(e => e.Parent)
                .HasForeignKey<紹介状情報DbEntity>(e => new {
                    e.Parent_診療ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_診療履歴_SHIPPING_INFO");
    
            entity.HasOne(e => e.患者)
                .WithMany(e => e.RefFrom診療履歴_患者)
                .HasForeignKey(e => new {
                    e.患者_患者ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_患者マスタ_診療履歴_XFB576B4");
    
            entity.HasOne(e => e.診療科)
                .WithMany(e => e.RefFrom診療履歴_診療科)
                .HasForeignKey(e => new {
                    e.診療科_診療科ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_診療科マスタ_診療履歴_X6518C1A");
    
            entity.HasOne(e => e.担当医)
                .WithMany(e => e.RefFrom診療履歴_担当医)
                .HasForeignKey(e => new {
                    e.担当医_医療従事者ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_EMPLOYEE_診療履歴_XDECF289");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating処置明細(ModelBuilder modelBuilder) {
        modelBuilder.Entity<処置明細DbEntity>(entity => {
            entity.ToTable("ORDER_DETAILS");
    
            entity.HasKey(e => new {
                e.Parent_診療ID,
                e.医療機器_機器ID,
            })
            .HasName("PK_ORDER_DETAILS");
    
            entity.Property(e => e.Parent_診療ID)
                .HasColumnName("Parent_ORDER_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.医療機器_機器ID)
                .HasColumnName("医療機器_PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.使用数量)
                .HasColumnName("QUANTITY")
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.単価)
                .HasColumnName("UNIT_PRICE")
                .IsRequired(true)
                .HasColumnOrder(3);
            entity.Property(e => e.小計)
                .HasColumnName("SUBTOTAL")
                .IsRequired(true)
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
    
            entity.HasOne(e => e.保険適用情報)
                .WithOne(e => e.Parent)
                .HasForeignKey<保険適用情報DbEntity>(e => new {
                    e.Parent_Parent_診療ID,
                    e.Parent_医療機器_機器ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ORDER_DETAILS_DISCOUNT_INFO");
    
            entity.HasOne(e => e.医療機器)
                .WithMany(e => e.RefFrom処置明細_医療機器)
                .HasForeignKey(e => new {
                    e.医療機器_機器ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_医療機器マスタ_ORDER_DETAILS_X5D5C85D");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating保険適用情報(ModelBuilder modelBuilder) {
        modelBuilder.Entity<保険適用情報DbEntity>(entity => {
            entity.ToTable("DISCOUNT_INFO");
    
            entity.HasKey(e => new {
                e.Parent_Parent_診療ID,
                e.Parent_医療機器_機器ID,
            })
            .HasName("PK_DISCOUNT_INFO");
    
            entity.Property(e => e.Parent_Parent_診療ID)
                .HasColumnName("Parent_Parent_ORDER_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.Parent_医療機器_機器ID)
                .HasColumnName("Parent_医療機器_PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.保険コード)
                .HasColumnName("DISCOUNT_CODE")
                .HasMaxLength(10)
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.保険適用率)
                .HasColumnName("DISCOUNT_RATE")
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.保険適用額)
                .HasColumnName("DISCOUNT_AMOUNT")
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
    protected virtual void OnModelCreating会計情報(ModelBuilder modelBuilder) {
        modelBuilder.Entity<会計情報DbEntity>(entity => {
            entity.ToTable("PAYMENT_INFO");
    
            entity.HasKey(e => new {
                e.Parent_診療ID,
            })
            .HasName("PK_PAYMENT_INFO");
    
            entity.Property(e => e.Parent_診療ID)
                .HasColumnName("Parent_ORDER_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.支払方法)
                .HasColumnName("PAYMENT_TYPE")
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.支払日)
                .HasColumnName("PAYMENT_DATE")
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.支払状況)
                .HasColumnName("PAYMENT_STATUS")
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
    
            entity.HasOne(e => e.カード情報)
                .WithOne(e => e.Parent)
                .HasForeignKey<カード情報DbEntity>(e => new {
                    e.Parent_Parent_診療ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PAYMENT_INFO_CARD_INFO");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreatingカード情報(ModelBuilder modelBuilder) {
        modelBuilder.Entity<カード情報DbEntity>(entity => {
            entity.ToTable("CARD_INFO");
    
            entity.HasKey(e => new {
                e.Parent_Parent_診療ID,
            })
            .HasName("PK_CARD_INFO");
    
            entity.Property(e => e.Parent_Parent_診療ID)
                .HasColumnName("Parent_Parent_ORDER_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.カード種類)
                .HasColumnName("CARD_TYPE")
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.下4桁)
                .HasColumnName("LAST_FOUR_DIGITS")
                .HasMaxLength(4)
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.有効期限)
                .HasColumnName("EXPIRY_DATE")
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
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating紹介状情報(ModelBuilder modelBuilder) {
        modelBuilder.Entity<紹介状情報DbEntity>(entity => {
            entity.ToTable("SHIPPING_INFO");
    
            entity.HasKey(e => new {
                e.Parent_診療ID,
            })
            .HasName("PK_SHIPPING_INFO");
    
            entity.Property(e => e.Parent_診療ID)
                .HasColumnName("Parent_ORDER_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.紹介方法)
                .HasColumnName("SHIPPING_METHOD")
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.CreateUser)
                .HasColumnName("CreateUser")
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false)
                .HasColumnOrder(4);
            entity.Property(e => e.UpdateUser)
                .HasColumnName("UpdateUser")
                .IsRequired(false)
                .HasColumnOrder(5);
    
            entity.HasOne(e => e.紹介先住所)
                .WithOne(e => e.Parent)
                .HasForeignKey<紹介先住所DbEntity>(e => new {
                    e.Parent_Parent_診療ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SHIPPING_INFO_SHIPPING_ADDRESS");
    
            entity.HasMany(e => e.紹介状況)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.Parent_Parent_診療ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SHIPPING_INFO_SHIPPING_STATUS");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating紹介先住所(ModelBuilder modelBuilder) {
        modelBuilder.Entity<紹介先住所DbEntity>(entity => {
            entity.ToTable("SHIPPING_ADDRESS");
    
            entity.HasKey(e => new {
                e.Parent_Parent_診療ID,
            })
            .HasName("PK_SHIPPING_ADDRESS");
    
            entity.Property(e => e.Parent_Parent_診療ID)
                .HasColumnName("Parent_Parent_ORDER_ID")
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
    protected virtual void OnModelCreating紹介状況(ModelBuilder modelBuilder) {
        modelBuilder.Entity<紹介状況DbEntity>(entity => {
            entity.ToTable("SHIPPING_STATUS");
    
            entity.HasKey(e => new {
                e.Parent_Parent_診療ID,
                e.ステータス,
            })
            .HasName("PK_SHIPPING_STATUS");
    
            entity.Property(e => e.Parent_Parent_診療ID)
                .HasColumnName("Parent_Parent_ORDER_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.ステータス)
                .HasColumnName("STATUS")
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.更新日時)
                .HasColumnName("UPDATE_DATE")
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.備考)
                .HasColumnName("REMARKS")
                .HasMaxLength(500)
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
        });
    }
}
#endregion Entity Framework Core エンティティ定義

