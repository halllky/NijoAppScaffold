
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
/// 医療機器マスタの Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.LatinName("Medical Equipment Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 医療機器マスタDbEntity {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 機器ID { get; set; }
    /// <summary>機器名</summary>
    [NijoAttr.DbName("PRODUCT_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(100)]
    public string? 機器名 { get; set; }
    /// <summary>単価</summary>
    [NijoAttr.DbName("PRICE"), NijoAttr.IsNotNull]
    public int? 単価 { get; set; }
    /// <summary>機器分類ID</summary>
    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 機器分類_機器分類ID { get; set; }
    /// <summary>供給業者ID</summary>
    [NijoAttr.DbName("SUPPLIER_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 供給業者_供給業者ID { get; set; }
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

    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsNotNull]
    public virtual 機器分類マスタDbEntity? 機器分類 { get; set; }
    [NijoAttr.DbName("SUPPLIER_ID")]
    public virtual 供給業者マスタDbEntity? 供給業者 { get; set; }
    [NijoAttr.DbName("PRODUCT_DETAIL")]
    public virtual 機器詳細DbEntity? 機器詳細 { get; set; }
    [NijoAttr.DbName("INVENTORY")]
    public virtual ICollection<在庫情報DbEntity> 在庫情報 { get; set; } = [];
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey]
    public virtual ICollection<処置明細DbEntity> RefFrom処置明細_医療機器 { get; set; } = [];
    [NijoAttr.IsKey]
    public virtual ICollection<機器別収益SearchResult> RefFrom機器別収益_医療機器 { get; set; } = [];

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(医療機器マスタDbEntity entity) {
        if (entity.機器ID != this.機器ID) return false;
        return true;
    }
}
/// <summary>
/// 機器詳細の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("PRODUCT_DETAIL")]
public partial class 機器詳細DbEntity {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_機器ID { get; set; }
    /// <summary>機器説明</summary>
    [NijoAttr.DbName("DESCRIPTION"), NijoAttr.MaxLength(1000)]
    public string? 機器説明 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.LatinName("Medical Equipment Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public virtual 医療機器マスタDbEntity? Parent { get; set; }
    [NijoAttr.DbName("PRODUCT_SPEC")]
    public virtual 機器仕様DbEntity? 機器仕様 { get; set; }
    [NijoAttr.DbName("ACCESSORIES")]
    public virtual ICollection<付属品DbEntity> 付属品 { get; set; } = [];

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(機器詳細DbEntity entity) {
        if (entity.Parent_機器ID != this.Parent_機器ID) return false;
        return true;
    }
}
/// <summary>
/// 機器仕様の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("PRODUCT_SPEC")]
public partial class 機器仕様DbEntity {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_Parent_機器ID { get; set; }
    /// <summary>重量</summary>
    [NijoAttr.DbName("WEIGHT")]
    public int? 重量 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.DbName("PRODUCT_DETAIL")]
    public virtual 機器詳細DbEntity? Parent { get; set; }
    [NijoAttr.DbName("SIZE")]
    public virtual サイズDbEntity? サイズ { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(機器仕様DbEntity entity) {
        if (entity.Parent_Parent_機器ID != this.Parent_Parent_機器ID) return false;
        return true;
    }
}
/// <summary>
/// サイズの Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("SIZE")]
public partial class サイズDbEntity {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_Parent_Parent_機器ID { get; set; }
    /// <summary>幅</summary>
    [NijoAttr.DbName("WIDTH"), NijoAttr.TotalDigit(16)]
    public int? 幅 { get; set; }
    /// <summary>高さ</summary>
    [NijoAttr.DbName("HEIGHT"), NijoAttr.TotalDigit(16)]
    public int? 高さ { get; set; }
    /// <summary>奥行</summary>
    [NijoAttr.DbName("DEPTH"), NijoAttr.TotalDigit(16)]
    public int? 奥行 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.DbName("PRODUCT_SPEC")]
    public virtual 機器仕様DbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(サイズDbEntity entity) {
        if (entity.Parent_Parent_Parent_機器ID != this.Parent_Parent_Parent_機器ID) return false;
        return true;
    }
}
/// <summary>
/// 付属品の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("ACCESSORIES")]
public partial class 付属品DbEntity {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_Parent_機器ID { get; set; }
    /// <summary>付属品ID</summary>
    [NijoAttr.DbName("ACCESSORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(20)]
    public string? 付属品ID { get; set; }
    /// <summary>付属品名</summary>
    [NijoAttr.DbName("ACCESSORY_NAME"), NijoAttr.MaxLength(50)]
    public string? 付属品名 { get; set; }
    /// <summary>数量</summary>
    [NijoAttr.DbName("QUANTITY")]
    public int? 数量 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.DbName("PRODUCT_DETAIL")]
    public virtual 機器詳細DbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(付属品DbEntity entity) {
        if (entity.Parent_Parent_機器ID != this.Parent_Parent_機器ID) return false;
        if (entity.付属品ID != this.付属品ID) return false;
        return true;
    }
}
/// <summary>
/// 在庫情報の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("INVENTORY")]
public partial class 在庫情報DbEntity {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_機器ID { get; set; }
    /// <summary>保管庫ID</summary>
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 保管庫_保管庫ID { get; set; }
    /// <summary>在庫数</summary>
    [NijoAttr.DbName("STOCK_QUANTITY")]
    public int? 在庫数 { get; set; }
    /// <summary>棚卸日時</summary>
    [NijoAttr.DbName("INVENTORY_DATE")]
    public DateTime? 棚卸日時 { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.LatinName("Medical Equipment Master"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public virtual 医療機器マスタDbEntity? Parent { get; set; }
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey]
    public virtual 保管庫マスタDbEntity? 保管庫 { get; set; }
    [NijoAttr.DbName("STOCK_HISTORY")]
    public virtual ICollection<在庫状況履歴DbEntity> 在庫状況履歴 { get; set; } = [];
    [NijoAttr.DbName("INVENTORY_REF"), NijoAttr.IsKey]
    public virtual 機器点検報告DbEntity? RefFrom機器点検報告_対象機器 { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(在庫情報DbEntity entity) {
        if (entity.Parent_機器ID != this.Parent_機器ID) return false;
        if (entity.保管庫_保管庫ID != this.保管庫_保管庫ID) return false;
        return true;
    }
}
/// <summary>
/// 在庫状況履歴の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("STOCK_HISTORY")]
public partial class 在庫状況履歴DbEntity {
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? Parent_Parent_機器ID { get; set; }
    /// <summary>保管庫ID</summary>
    [NijoAttr.DbName("WAREHOUSE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? Parent_保管庫_保管庫ID { get; set; }
    /// <summary>履歴ID</summary>
    [NijoAttr.DbName("HISTORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(36)]
    public string? 履歴ID { get; set; }
    /// <summary>変更日時</summary>
    [NijoAttr.DbName("CHANGE_DATE")]
    public DateTime? 変更日時 { get; set; }
    /// <summary>変更前在庫数</summary>
    [NijoAttr.DbName("PREVIOUS_QUANTITY")]
    public int? 変更前在庫数 { get; set; }
    /// <summary>変更後在庫数</summary>
    [NijoAttr.DbName("CURRENT_QUANTITY")]
    public int? 変更後在庫数 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 担当者_医療従事者ID { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.DbName("INVENTORY")]
    public virtual 在庫情報DbEntity? Parent { get; set; }
    [NijoAttr.DbName("STAFF_ID")]
    public virtual 医療従事者マスタDbEntity? 担当者 { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(在庫状況履歴DbEntity entity) {
        if (entity.Parent_Parent_機器ID != this.Parent_Parent_機器ID) return false;
        if (entity.Parent_保管庫_保管庫ID != this.Parent_保管庫_保管庫ID) return false;
        if (entity.履歴ID != this.履歴ID) return false;
        return true;
    }
}

partial class AutoGeneratedDbContext {
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating医療機器マスタ(ModelBuilder modelBuilder) {
        modelBuilder.Entity<医療機器マスタDbEntity>(entity => {
            entity.ToTable("医療機器マスタ");
    
            entity.HasKey(e => new {
                e.機器ID,
            })
            .HasName("PK_医療機器マスタ");
    
            entity.Property(e => e.機器ID)
                .HasColumnName("PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.機器名)
                .HasColumnName("PRODUCT_NAME")
                .HasMaxLength(100)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.単価)
                .HasColumnName("PRICE")
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.機器分類_機器分類ID)
                .HasColumnName("機器分類_CATEGORY_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(3);
            entity.Property(e => e.供給業者_供給業者ID)
                .HasColumnName("供給業者_SUPPLIER_ID")
                .HasMaxLength(10)
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
    
            entity.HasOne(e => e.機器詳細)
                .WithOne(e => e.Parent)
                .HasForeignKey<機器詳細DbEntity>(e => new {
                    e.Parent_機器ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_医療機器マスタ_PRODUCT_DETAIL");
    
            entity.HasMany(e => e.在庫情報)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.Parent_機器ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_医療機器マスタ_INVENTORY");
    
            entity.HasOne(e => e.機器分類)
                .WithMany(e => e.RefFrom医療機器マスタ_機器分類)
                .HasForeignKey(e => new {
                    e.機器分類_機器分類ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_機器分類マスタ_医療機器マスタ_X7D2BEB8");
    
            entity.HasOne(e => e.供給業者)
                .WithMany(e => e.RefFrom医療機器マスタ_供給業者)
                .HasForeignKey(e => new {
                    e.供給業者_供給業者ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_供給業者マスタ_医療機器マスタ_X89DCA22");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating機器詳細(ModelBuilder modelBuilder) {
        modelBuilder.Entity<機器詳細DbEntity>(entity => {
            entity.ToTable("PRODUCT_DETAIL");
    
            entity.HasKey(e => new {
                e.Parent_機器ID,
            })
            .HasName("PK_PRODUCT_DETAIL");
    
            entity.Property(e => e.Parent_機器ID)
                .HasColumnName("Parent_PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.機器説明)
                .HasColumnName("DESCRIPTION")
                .HasMaxLength(1000)
                .IsRequired(false)
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
    
            entity.HasOne(e => e.機器仕様)
                .WithOne(e => e.Parent)
                .HasForeignKey<機器仕様DbEntity>(e => new {
                    e.Parent_Parent_機器ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PRODUCT_DETAIL_PRODUCT_SPEC");
    
            entity.HasMany(e => e.付属品)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.Parent_Parent_機器ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PRODUCT_DETAIL_ACCESSORIES");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating機器仕様(ModelBuilder modelBuilder) {
        modelBuilder.Entity<機器仕様DbEntity>(entity => {
            entity.ToTable("PRODUCT_SPEC");
    
            entity.HasKey(e => new {
                e.Parent_Parent_機器ID,
            })
            .HasName("PK_PRODUCT_SPEC");
    
            entity.Property(e => e.Parent_Parent_機器ID)
                .HasColumnName("Parent_Parent_PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.重量)
                .HasColumnName("WEIGHT")
                .IsRequired(false)
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
    
            entity.HasOne(e => e.サイズ)
                .WithOne(e => e.Parent)
                .HasForeignKey<サイズDbEntity>(e => new {
                    e.Parent_Parent_Parent_機器ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PRODUCT_SPEC_SIZE");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreatingサイズ(ModelBuilder modelBuilder) {
        modelBuilder.Entity<サイズDbEntity>(entity => {
            entity.ToTable("SIZE");
    
            entity.HasKey(e => new {
                e.Parent_Parent_Parent_機器ID,
            })
            .HasName("PK_SIZE");
    
            entity.Property(e => e.Parent_Parent_Parent_機器ID)
                .HasColumnName("Parent_Parent_Parent_PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.幅)
                .HasColumnName("WIDTH")
                .HasPrecision(16, 0)
                .IsRequired(false)
                .HasColumnOrder(1);
            entity.Property(e => e.高さ)
                .HasColumnName("HEIGHT")
                .HasPrecision(16, 0)
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.奥行)
                .HasColumnName("DEPTH")
                .HasPrecision(16, 0)
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
    protected virtual void OnModelCreating付属品(ModelBuilder modelBuilder) {
        modelBuilder.Entity<付属品DbEntity>(entity => {
            entity.ToTable("ACCESSORIES");
    
            entity.HasKey(e => new {
                e.Parent_Parent_機器ID,
                e.付属品ID,
            })
            .HasName("PK_ACCESSORIES");
    
            entity.Property(e => e.Parent_Parent_機器ID)
                .HasColumnName("Parent_Parent_PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.付属品ID)
                .HasColumnName("ACCESSORY_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.付属品名)
                .HasColumnName("ACCESSORY_NAME")
                .HasMaxLength(50)
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.数量)
                .HasColumnName("QUANTITY")
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
    protected virtual void OnModelCreating在庫情報(ModelBuilder modelBuilder) {
        modelBuilder.Entity<在庫情報DbEntity>(entity => {
            entity.ToTable("INVENTORY");
    
            entity.HasKey(e => new {
                e.Parent_機器ID,
                e.保管庫_保管庫ID,
            })
            .HasName("PK_INVENTORY");
    
            entity.Property(e => e.Parent_機器ID)
                .HasColumnName("Parent_PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.保管庫_保管庫ID)
                .HasColumnName("保管庫_WAREHOUSE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.在庫数)
                .HasColumnName("STOCK_QUANTITY")
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.棚卸日時)
                .HasColumnName("INVENTORY_DATE")
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
    
            entity.HasMany(e => e.在庫状況履歴)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.Parent_Parent_機器ID,
                    e.Parent_保管庫_保管庫ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_INVENTORY_STOCK_HISTORY");
    
            entity.HasOne(e => e.保管庫)
                .WithMany(e => e.RefFrom在庫情報_保管庫)
                .HasForeignKey(e => new {
                    e.保管庫_保管庫ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_保管庫マスタ_INVENTORY_X48C69C7");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating在庫状況履歴(ModelBuilder modelBuilder) {
        modelBuilder.Entity<在庫状況履歴DbEntity>(entity => {
            entity.ToTable("STOCK_HISTORY");
    
            entity.HasKey(e => new {
                e.Parent_Parent_機器ID,
                e.Parent_保管庫_保管庫ID,
                e.履歴ID,
            })
            .HasName("PK_STOCK_HISTORY");
    
            entity.Property(e => e.Parent_Parent_機器ID)
                .HasColumnName("Parent_Parent_PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.Parent_保管庫_保管庫ID)
                .HasColumnName("Parent_保管庫_WAREHOUSE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.履歴ID)
                .HasColumnName("HISTORY_ID")
                .HasMaxLength(36)
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.変更日時)
                .HasColumnName("CHANGE_DATE")
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.変更前在庫数)
                .HasColumnName("PREVIOUS_QUANTITY")
                .IsRequired(false)
                .HasColumnOrder(4);
            entity.Property(e => e.変更後在庫数)
                .HasColumnName("CURRENT_QUANTITY")
                .IsRequired(false)
                .HasColumnOrder(5);
            entity.Property(e => e.担当者_医療従事者ID)
                .HasColumnName("担当者_ID")
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
                .WithMany(e => e.RefFrom在庫状況履歴_担当者)
                .HasForeignKey(e => new {
                    e.担当者_医療従事者ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_EMPLOYEE_STOCK_HISTORY_XB332A41");
        });
    }
}
#endregion Entity Framework Core エンティティ定義

