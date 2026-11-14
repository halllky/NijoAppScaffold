
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


#region 検索クエリ型
/// <summary>
/// 診療収益分析の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.DbName("V_診療収益分析"), NijoAttr.LatinName("Medical Revenue Analysis"), NijoAttr.IsQueryable, NijoAttr.MapToView]
public partial class 診療収益分析SearchResult {
    /// <summary>年月</summary>
    [NijoAttr.IsKey]
    public int? 年月 { get; set; }
    /// <summary>診療科ID</summary>
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 診療科_診療科ID { get; set; }
    /// <summary>診療科名</summary>
    [NijoAttr.DbName("STORE_NAME"), NijoAttr.MaxLength(50)]
    public string? 診療科_診療科名 { get; set; }
    /// <summary>電話番号</summary>
    [NijoAttr.DbName("PHONE"), NijoAttr.MaxLength(20)]
    public string? 診療科_電話番号 { get; set; }
    /// <summary>郵便番号</summary>
    [NijoAttr.DbName("POSTAL_CODE"), NijoAttr.MaxLength(8), NijoAttr.AvailableCharacter(NijoAttr.AvailableCharacterAttribute.E_Value.半角数字および半角ハイフンのみ)]
    public string? 診療科_住所_郵便番号 { get; set; }
    /// <summary>都道府県</summary>
    [NijoAttr.DbName("PREFECTURE"), NijoAttr.MaxLength(10)]
    public string? 診療科_住所_都道府県 { get; set; }
    /// <summary>市区町村</summary>
    [NijoAttr.DbName("CITY"), NijoAttr.MaxLength(20)]
    public string? 診療科_住所_市区町村 { get; set; }
    /// <summary>番地建物名</summary>
    [NijoAttr.DbName("ADDRESS_LINE"), NijoAttr.MaxLength(100)]
    public string? 診療科_住所_番地建物名 { get; set; }
    /// <summary>開始時間</summary>
    [NijoAttr.DbName("OPENING_TIME"), NijoAttr.MaxLength(5)]
    public string? 診療科_診療時間_開始時間 { get; set; }
    /// <summary>終了時間</summary>
    [NijoAttr.DbName("CLOSING_TIME"), NijoAttr.MaxLength(5)]
    public string? 診療科_診療時間_終了時間 { get; set; }
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 診療科_科長_医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 診療科_科長_氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 診療科_科長_氏名カナ { get; set; }
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 診療科_科長_退職日 { get; set; }
    /// <summary>診療収益合計</summary>
    public int? 診療収益合計 { get; set; }
    /// <summary>患者数</summary>
    public int? 患者数 { get; set; }
    /// <summary>患者単価</summary>
    public int? 患者単価 { get; set; }
    /// <summary>機器分類別収益</summary>
    [NijoAttr.DbName("V_機器分類別収益")]
    public virtual ICollection<機器分類別収益SearchResult> 機器分類別収益 { get; set; } = [];
    /// <summary>時間帯別収益</summary>
    [NijoAttr.DbName("V_時間帯別収益")]
    public virtual ICollection<時間帯別収益SearchResult> 時間帯別収益 { get; set; } = [];
    /// <summary>診療科</summary>
    [NijoAttr.IsKey]
    public virtual 診療科マスタDbEntity? 診療科 { get; set; }
}
/// <summary>
/// 機器分類別収益の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.DbName("V_機器分類別収益")]
public partial class 機器分類別収益SearchResult {
    /// <summary>年月</summary>
    [NijoAttr.IsKey]
    public int? 年月 { get; set; }
    /// <summary>診療科ID</summary>
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 診療科_診療科ID { get; set; }
    /// <summary>機器分類ID</summary>
    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 機器分類_機器分類ID { get; set; }
    /// <summary>機器分類名</summary>
    [NijoAttr.DbName("CATEGORY_NAME"), NijoAttr.MaxLength(50)]
    public string? 機器分類_機器分類名 { get; set; }
    /// <summary>診療収益</summary>
    public int? 診療収益 { get; set; }
    /// <summary>収益構成比</summary>
    public decimal? 収益構成比 { get; set; }
    /// <summary>機器別収益</summary>
    [NijoAttr.DbName("V_機器別収益")]
    public virtual ICollection<機器別収益SearchResult> 機器別収益 { get; set; } = [];
    /// <summary>機器分類別収益</summary>
    [NijoAttr.DbName("V_機器分類別収益")]
    public virtual 診療収益分析SearchResult? Parent { get; set; }
    /// <summary>機器分類</summary>
    [NijoAttr.IsKey]
    public virtual 機器分類マスタDbEntity? 機器分類 { get; set; }
}
/// <summary>
/// 機器別収益の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.DbName("V_機器別収益")]
public partial class 機器別収益SearchResult {
    /// <summary>年月</summary>
    [NijoAttr.IsKey]
    public int? 年月 { get; set; }
    /// <summary>診療科ID</summary>
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 診療科_診療科ID { get; set; }
    /// <summary>機器分類ID</summary>
    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 機器分類_機器分類ID { get; set; }
    /// <summary>機器ID</summary>
    [NijoAttr.DbName("PRODUCT_ID"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(20)]
    public string? 医療機器_機器ID { get; set; }
    /// <summary>機器名</summary>
    [NijoAttr.DbName("PRODUCT_NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(100)]
    public string? 医療機器_機器名 { get; set; }
    /// <summary>単価</summary>
    [NijoAttr.DbName("PRICE"), NijoAttr.IsNotNull]
    public int? 医療機器_単価 { get; set; }
    /// <summary>機器分類ID</summary>
    [NijoAttr.DbName("CATEGORY_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 医療機器_機器分類_機器分類ID { get; set; }
    /// <summary>機器分類名</summary>
    [NijoAttr.DbName("CATEGORY_NAME"), NijoAttr.MaxLength(50)]
    public string? 医療機器_機器分類_機器分類名 { get; set; }
    /// <summary>供給業者ID</summary>
    [NijoAttr.DbName("SUPPLIER_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 医療機器_供給業者_供給業者ID { get; set; }
    /// <summary>供給業者名</summary>
    [NijoAttr.DbName("SUPPLIER_NAME"), NijoAttr.MaxLength(100)]
    public string? 医療機器_供給業者_供給業者名 { get; set; }
    /// <summary>担当者名</summary>
    [NijoAttr.DbName("CONTACT_PERSON"), NijoAttr.MaxLength(50)]
    public string? 医療機器_供給業者_担当者名 { get; set; }
    /// <summary>電話番号</summary>
    [NijoAttr.DbName("PHONE"), NijoAttr.MaxLength(20)]
    public string? 医療機器_供給業者_電話番号 { get; set; }
    /// <summary>メールアドレス</summary>
    [NijoAttr.DbName("EMAIL"), NijoAttr.MaxLength(100)]
    public string? 医療機器_供給業者_メールアドレス { get; set; }
    /// <summary>機器説明</summary>
    [NijoAttr.DbName("DESCRIPTION"), NijoAttr.MaxLength(1000)]
    public string? 医療機器_機器詳細_機器説明 { get; set; }
    /// <summary>重量</summary>
    [NijoAttr.DbName("WEIGHT")]
    public int? 医療機器_機器詳細_機器仕様_重量 { get; set; }
    /// <summary>幅</summary>
    [NijoAttr.DbName("WIDTH"), NijoAttr.TotalDigit(16)]
    public int? 医療機器_機器詳細_機器仕様_サイズ_幅 { get; set; }
    /// <summary>高さ</summary>
    [NijoAttr.DbName("HEIGHT"), NijoAttr.TotalDigit(16)]
    public int? 医療機器_機器詳細_機器仕様_サイズ_高さ { get; set; }
    /// <summary>奥行</summary>
    [NijoAttr.DbName("DEPTH"), NijoAttr.TotalDigit(16)]
    public int? 医療機器_機器詳細_機器仕様_サイズ_奥行 { get; set; }
    /// <summary>診療収益</summary>
    public int? 診療収益 { get; set; }
    /// <summary>使用回数</summary>
    public int? 使用回数 { get; set; }
    /// <summary>平均単価</summary>
    public int? 平均単価 { get; set; }
    /// <summary>機器別収益</summary>
    [NijoAttr.DbName("V_機器別収益")]
    public virtual 機器分類別収益SearchResult? Parent { get; set; }
    /// <summary>医療機器</summary>
    [NijoAttr.IsKey]
    public virtual 医療機器マスタDbEntity? 医療機器 { get; set; }
}
/// <summary>
/// 時間帯別収益の検索結果型。
/// SQLのSELECT句の形と対応する。
/// </summary>
[NijoAttr.DbName("V_時間帯別収益")]
public partial class 時間帯別収益SearchResult {
    /// <summary>年月</summary>
    [NijoAttr.IsKey]
    public int? 年月 { get; set; }
    /// <summary>診療科ID</summary>
    [NijoAttr.DbName("STORE_ID"), NijoAttr.IsKey, NijoAttr.MaxLength(10)]
    public string? 診療科_診療科ID { get; set; }
    /// <summary>時間帯</summary>
    [NijoAttr.IsKey]
    public string? 時間帯 { get; set; }
    /// <summary>診療収益</summary>
    public int? 診療収益 { get; set; }
    /// <summary>診療件数</summary>
    public int? 診療件数 { get; set; }
    /// <summary>平均患者単価</summary>
    public int? 平均患者単価 { get; set; }
    /// <summary>時間帯別収益</summary>
    [NijoAttr.DbName("V_時間帯別収益")]
    public virtual 診療収益分析SearchResult? Parent { get; set; }
}
#endregion 検索クエリ型


#region Entity Framework Core エンティティ定義
partial class AutoGeneratedDbContext {
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating診療収益分析(ModelBuilder modelBuilder) {
        modelBuilder.Entity<診療収益分析SearchResult>(entity => {
            entity.ToView("V_診療収益分析");
    
            entity.HasKey(e => new {
                e.年月,
                e.診療科_診療科ID,
            })
            .HasName("PK_V_診療収益分析");
    
            entity.Property(e => e.年月)
                .HasColumnName("年月")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.診療科_診療科ID)
                .HasColumnName("診療科_STORE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.診療科_診療科名)
                .HasColumnName("診療科_STORE_NAME")
                .HasMaxLength(50)
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.診療科_電話番号)
                .HasColumnName("診療科_PHONE")
                .HasMaxLength(20)
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.診療科_住所_郵便番号)
                .HasColumnName("診療科_住所_POSTAL_CODE")
                .HasMaxLength(8)
                .IsRequired(false)
                .HasColumnOrder(4);
            entity.Property(e => e.診療科_住所_都道府県)
                .HasColumnName("診療科_住所_PREFECTURE")
                .HasMaxLength(10)
                .IsRequired(false)
                .HasColumnOrder(5);
            entity.Property(e => e.診療科_住所_市区町村)
                .HasColumnName("診療科_住所_CITY")
                .HasMaxLength(20)
                .IsRequired(false)
                .HasColumnOrder(6);
            entity.Property(e => e.診療科_住所_番地建物名)
                .HasColumnName("診療科_住所_ADDRESS_LINE")
                .HasMaxLength(100)
                .IsRequired(false)
                .HasColumnOrder(7);
            entity.Property(e => e.診療科_診療時間_開始時間)
                .HasColumnName("診療科_診療時間_OPENING_TIME")
                .HasMaxLength(5)
                .IsRequired(false)
                .HasColumnOrder(8);
            entity.Property(e => e.診療科_診療時間_終了時間)
                .HasColumnName("診療科_診療時間_CLOSING_TIME")
                .HasMaxLength(5)
                .IsRequired(false)
                .HasColumnOrder(9);
            entity.Property(e => e.診療科_科長_医療従事者ID)
                .HasColumnName("診療科_MANAGER_ID_ID")
                .IsRequired(true)
                .HasColumnOrder(10);
            entity.Property(e => e.診療科_科長_氏名)
                .HasColumnName("診療科_MANAGER_ID_NAME")
                .HasMaxLength(40)
                .IsRequired(true)
                .HasColumnOrder(11);
            entity.Property(e => e.診療科_科長_氏名カナ)
                .HasColumnName("診療科_MANAGER_ID_NAME_KANA")
                .HasMaxLength(40)
                .IsRequired(false)
                .HasColumnOrder(12);
            entity.Property(e => e.診療科_科長_退職日)
                .HasColumnName("診療科_MANAGER_ID_TAISHOKU")
                .IsRequired(false)
                .HasColumnOrder(13);
            entity.Property(e => e.診療収益合計)
                .HasColumnName("診療収益合計")
                .IsRequired(false)
                .HasColumnOrder(14);
            entity.Property(e => e.患者数)
                .HasColumnName("患者数")
                .IsRequired(false)
                .HasColumnOrder(15);
            entity.Property(e => e.患者単価)
                .HasColumnName("患者単価")
                .IsRequired(false)
                .HasColumnOrder(16);
    
            entity.HasMany(e => e.機器分類別収益)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.年月,
                    e.診療科_診療科ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_V_診療収益分析_V_機器分類別収益");
    
            entity.HasMany(e => e.時間帯別収益)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.年月,
                    e.診療科_診療科ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_V_診療収益分析_V_時間帯別収益");
    
            entity.HasOne(e => e.診療科)
                .WithMany(e => e.RefFrom診療収益分析_診療科)
                .HasForeignKey(e => new {
                    e.診療科_診療科ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_診療科マスタ_V_診療収益分析_X6518C1A");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating機器分類別収益(ModelBuilder modelBuilder) {
        modelBuilder.Entity<機器分類別収益SearchResult>(entity => {
            entity.ToView("V_機器分類別収益");
    
            entity.HasKey(e => new {
                e.年月,
                e.診療科_診療科ID,
                e.機器分類_機器分類ID,
            })
            .HasName("PK_V_機器分類別収益");
    
            entity.Property(e => e.年月)
                .HasColumnName("年月")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.診療科_診療科ID)
                .HasColumnName("診療科_STORE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.機器分類_機器分類ID)
                .HasColumnName("機器分類_CATEGORY_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.機器分類_機器分類名)
                .HasColumnName("機器分類_CATEGORY_NAME")
                .HasMaxLength(50)
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.診療収益)
                .HasColumnName("診療収益")
                .IsRequired(false)
                .HasColumnOrder(4);
            entity.Property(e => e.収益構成比)
                .HasColumnName("収益構成比")
                .IsRequired(false)
                .HasColumnOrder(5);
    
            entity.HasMany(e => e.機器別収益)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.年月,
                    e.診療科_診療科ID,
                    e.機器分類_機器分類ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_V_機器分類別収益_V_機器別収益");
    
            entity.HasOne(e => e.機器分類)
                .WithMany(e => e.RefFrom機器分類別収益_機器分類)
                .HasForeignKey(e => new {
                    e.機器分類_機器分類ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_機器分類マスタ_V_機器分類別収益_X7D2BEB8");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating機器別収益(ModelBuilder modelBuilder) {
        modelBuilder.Entity<機器別収益SearchResult>(entity => {
            entity.ToView("V_機器別収益");
    
            entity.HasKey(e => new {
                e.年月,
                e.診療科_診療科ID,
                e.機器分類_機器分類ID,
                e.医療機器_機器ID,
            })
            .HasName("PK_V_機器別収益");
    
            entity.Property(e => e.年月)
                .HasColumnName("年月")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.診療科_診療科ID)
                .HasColumnName("診療科_STORE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.機器分類_機器分類ID)
                .HasColumnName("機器分類_CATEGORY_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.医療機器_機器ID)
                .HasColumnName("医療機器_PRODUCT_ID")
                .HasMaxLength(20)
                .IsRequired(true)
                .HasColumnOrder(3);
            entity.Property(e => e.医療機器_機器名)
                .HasColumnName("医療機器_PRODUCT_NAME")
                .HasMaxLength(100)
                .IsRequired(true)
                .HasColumnOrder(4);
            entity.Property(e => e.医療機器_単価)
                .HasColumnName("医療機器_PRICE")
                .IsRequired(true)
                .HasColumnOrder(5);
            entity.Property(e => e.医療機器_機器分類_機器分類ID)
                .HasColumnName("医療機器_CATEGORY_ID_CATEGORY_ID")
                .HasMaxLength(10)
                .IsRequired(false)
                .HasColumnOrder(6);
            entity.Property(e => e.医療機器_機器分類_機器分類名)
                .HasColumnName("医療機器_CATEGORY_ID_CATEGORY_NAME")
                .HasMaxLength(50)
                .IsRequired(false)
                .HasColumnOrder(7);
            entity.Property(e => e.医療機器_供給業者_供給業者ID)
                .HasColumnName("医療機器_SUPPLIER_ID_SUPPLIER_ID")
                .HasMaxLength(10)
                .IsRequired(false)
                .HasColumnOrder(8);
            entity.Property(e => e.医療機器_供給業者_供給業者名)
                .HasColumnName("医療機器_SUPPLIER_ID_SUPPLIER_NAME")
                .HasMaxLength(100)
                .IsRequired(false)
                .HasColumnOrder(9);
            entity.Property(e => e.医療機器_供給業者_担当者名)
                .HasColumnName("医療機器_SUPPLIER_ID_CONTACT_PERSON")
                .HasMaxLength(50)
                .IsRequired(false)
                .HasColumnOrder(10);
            entity.Property(e => e.医療機器_供給業者_電話番号)
                .HasColumnName("医療機器_SUPPLIER_ID_PHONE")
                .HasMaxLength(20)
                .IsRequired(false)
                .HasColumnOrder(11);
            entity.Property(e => e.医療機器_供給業者_メールアドレス)
                .HasColumnName("医療機器_SUPPLIER_ID_EMAIL")
                .HasMaxLength(100)
                .IsRequired(false)
                .HasColumnOrder(12);
            entity.Property(e => e.医療機器_機器詳細_機器説明)
                .HasColumnName("医療機器_機器詳細_DESCRIPTION")
                .HasMaxLength(1000)
                .IsRequired(false)
                .HasColumnOrder(13);
            entity.Property(e => e.医療機器_機器詳細_機器仕様_重量)
                .HasColumnName("医療機器_機器詳細_機器仕様_WEIGHT")
                .IsRequired(false)
                .HasColumnOrder(14);
            entity.Property(e => e.医療機器_機器詳細_機器仕様_サイズ_幅)
                .HasColumnName("医療機器_機器詳細_機器仕様_サイズ_WIDTH")
                .HasPrecision(16, 0)
                .IsRequired(false)
                .HasColumnOrder(15);
            entity.Property(e => e.医療機器_機器詳細_機器仕様_サイズ_高さ)
                .HasColumnName("医療機器_機器詳細_機器仕様_サイズ_HEIGHT")
                .HasPrecision(16, 0)
                .IsRequired(false)
                .HasColumnOrder(16);
            entity.Property(e => e.医療機器_機器詳細_機器仕様_サイズ_奥行)
                .HasColumnName("医療機器_機器詳細_機器仕様_サイズ_DEPTH")
                .HasPrecision(16, 0)
                .IsRequired(false)
                .HasColumnOrder(17);
            entity.Property(e => e.診療収益)
                .HasColumnName("診療収益")
                .IsRequired(false)
                .HasColumnOrder(18);
            entity.Property(e => e.使用回数)
                .HasColumnName("使用回数")
                .IsRequired(false)
                .HasColumnOrder(19);
            entity.Property(e => e.平均単価)
                .HasColumnName("平均単価")
                .IsRequired(false)
                .HasColumnOrder(20);
    
            entity.HasOne(e => e.医療機器)
                .WithMany(e => e.RefFrom機器別収益_医療機器)
                .HasForeignKey(e => new {
                    e.医療機器_機器ID,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_医療機器マスタ_V_機器別収益_X5D5C85D");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating時間帯別収益(ModelBuilder modelBuilder) {
        modelBuilder.Entity<時間帯別収益SearchResult>(entity => {
            entity.ToView("V_時間帯別収益");
    
            entity.HasKey(e => new {
                e.年月,
                e.診療科_診療科ID,
                e.時間帯,
            })
            .HasName("PK_V_時間帯別収益");
    
            entity.Property(e => e.年月)
                .HasColumnName("年月")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.診療科_診療科ID)
                .HasColumnName("診療科_STORE_ID")
                .HasMaxLength(10)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.時間帯)
                .HasColumnName("時間帯")
                .IsRequired(true)
                .HasColumnOrder(2);
            entity.Property(e => e.診療収益)
                .HasColumnName("診療収益")
                .IsRequired(false)
                .HasColumnOrder(3);
            entity.Property(e => e.診療件数)
                .HasColumnName("診療件数")
                .IsRequired(false)
                .HasColumnOrder(4);
            entity.Property(e => e.平均患者単価)
                .HasColumnName("平均患者単価")
                .IsRequired(false)
                .HasColumnOrder(5);
        });
    }
}
#endregion Entity Framework Core エンティティ定義

