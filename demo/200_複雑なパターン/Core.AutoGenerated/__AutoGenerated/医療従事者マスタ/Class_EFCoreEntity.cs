
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
/// 医療従事者マスタの Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("EMPLOYEE"), NijoAttr.IsStored, NijoAttr.IsQueryable]
public partial class 医療従事者マスタDbEntity {
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? 医療従事者ID { get; set; }
    /// <summary>氏名</summary>
    [NijoAttr.DbName("NAME"), NijoAttr.IsNotNull, NijoAttr.MaxLength(40)]
    public string? 氏名 { get; set; }
    /// <summary>氏名カナ</summary>
    [NijoAttr.DbName("NAME_KANA"), NijoAttr.MaxLength(40)]
    public string? 氏名カナ { get; set; }
    /// <summary>退職日</summary>
    [NijoAttr.DbName("TAISHOKU")]
    public DateOnly? 退職日 { get; set; }
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

    [NijoAttr.DbName("SHOZOKU")]
    public virtual ICollection<所属診療科DbEntity> 所属診療科 { get; set; } = [];
    [NijoAttr.DbName("AUTHORITY")]
    public virtual ICollection<権限DbEntity> 権限 { get; set; } = [];
    [NijoAttr.DbName("STAFF_ID")]
    public virtual ICollection<在庫状況履歴DbEntity> RefFrom在庫状況履歴_担当者 { get; set; } = [];
    [NijoAttr.DbName("STAFF_ID")]
    public virtual ICollection<診療履歴DbEntity> RefFrom診療履歴_担当医 { get; set; } = [];
    [NijoAttr.DbName("EMPLOYEE_ID"), NijoAttr.IsKey]
    public virtual 医療従事者プロフィールDbEntity? RefFrom医療従事者プロフィール_医療従事者 { get; set; }
    [NijoAttr.DbName("EMPLOYEE_ID"), NijoAttr.IsKey]
    public virtual ICollection<勤務スケジュールDbEntity> RefFrom勤務スケジュール_医療従事者 { get; set; } = [];
    [NijoAttr.DbName("EMPLOYEE_ID")]
    public virtual ICollection<予約DbEntity> RefFrom予約_担当医 { get; set; } = [];
    [NijoAttr.DbName("MANAGER_ID")]
    public virtual ICollection<保管庫マスタDbEntity> RefFrom保管庫マスタ_管理責任者 { get; set; } = [];
    [NijoAttr.DbName("MANAGER_ID")]
    public virtual ICollection<診療科マスタDbEntity> RefFrom診療科マスタ_科長 { get; set; } = [];
    [NijoAttr.DbName("SURVEYOR_ID"), NijoAttr.IsNotNull]
    public virtual ICollection<機器点検報告DbEntity> RefFrom機器点検報告_点検担当者 { get; set; } = [];
    [NijoAttr.DbName("EXECUTOR_ID")]
    public virtual ICollection<対応措置DbEntity> RefFrom対応措置_実施担当者 { get; set; } = [];
    [NijoAttr.DbName("RESULT_STAFF_ID"), NijoAttr.IsNotNull]
    public virtual ICollection<措置結果DbEntity> RefFrom措置結果_結果担当者 { get; set; } = [];
    [NijoAttr.DbName("NEXT_STAFF_ID")]
    public virtual ICollection<次回措置DbEntity> RefFrom次回措置_担当者 { get; set; } = [];

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(医療従事者マスタDbEntity entity) {
        if (entity.医療従事者ID != this.医療従事者ID) return false;
        return true;
    }
}
/// <summary>
/// 所属診療科の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("SHOZOKU")]
public partial class 所属診療科DbEntity {
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? Parent_医療従事者ID { get; set; }
    /// <summary>年度</summary>
    [NijoAttr.DbName("NENDO"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public int? 年度 { get; set; }
    /// <summary>診療科コード</summary>
    [NijoAttr.DbName("BUSHO_CD"), NijoAttr.IsKey, NijoAttr.IsNotNull, NijoAttr.MaxLength(10)]
    public string? 診療科_診療科コード { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.DbName("EMPLOYEE"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public virtual 医療従事者マスタDbEntity? Parent { get; set; }
    [NijoAttr.DbName("BUSHO_CD"), NijoAttr.IsNotNull]
    public virtual 診療科DbEntity? 診療科 { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(所属診療科DbEntity entity) {
        if (entity.Parent_医療従事者ID != this.Parent_医療従事者ID) return false;
        if (entity.年度 != this.年度) return false;
        return true;
    }
}
/// <summary>
/// 権限の Entity Framework Core エンティティ型。RDBMSのテーブル定義と対応。
/// </summary>
[NijoAttr.DbName("AUTHORITY")]
public partial class 権限DbEntity {
    /// <summary>医療従事者ID</summary>
    [NijoAttr.DbName("ID"), NijoAttr.IsKey, NijoAttr.IsNotNull]
    public string? Parent_医療従事者ID { get; set; }
    /// <summary>権限レベル</summary>
    [NijoAttr.DbName("AUTHORITY_LEVEL"), NijoAttr.IsKey]
    public 権限レベル? 権限レベル { get; set; }
    /// <summary>データが新規作成された日時</summary>
    public DateTime? CreatedAt { get; set; }
    /// <summary>データが更新された日時</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>データを新規作成したユーザー</summary>
    public string? CreateUser { get; set; }
    /// <summary>データを更新したユーザー</summary>
    public string? UpdateUser { get; set; }

    [NijoAttr.DbName("EMPLOYEE"), NijoAttr.IsStored, NijoAttr.IsQueryable]
    public virtual 医療従事者マスタDbEntity? Parent { get; set; }

    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
    public bool KeyEquals(権限DbEntity entity) {
        if (entity.Parent_医療従事者ID != this.Parent_医療従事者ID) return false;
        if (entity.権限レベル != this.権限レベル) return false;
        return true;
    }
}

partial class AutoGeneratedDbContext {
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating医療従事者マスタ(ModelBuilder modelBuilder) {
        modelBuilder.Entity<医療従事者マスタDbEntity>(entity => {
            entity.ToTable("EMPLOYEE");
    
            entity.HasKey(e => new {
                e.医療従事者ID,
            })
            .HasName("PK_EMPLOYEE");
    
            entity.Property(e => e.医療従事者ID)
                .HasColumnName("ID")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.氏名)
                .HasColumnName("NAME")
                .HasMaxLength(40)
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.氏名カナ)
                .HasColumnName("NAME_KANA")
                .HasMaxLength(40)
                .IsRequired(false)
                .HasColumnOrder(2);
            entity.Property(e => e.退職日)
                .HasColumnName("TAISHOKU")
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
    
            entity.HasMany(e => e.所属診療科)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.Parent_医療従事者ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EMPLOYEE_SHOZOKU");
    
            entity.HasMany(e => e.権限)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => new {
                    e.Parent_医療従事者ID,
                })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EMPLOYEE_AUTHORITY");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating所属診療科(ModelBuilder modelBuilder) {
        modelBuilder.Entity<所属診療科DbEntity>(entity => {
            entity.ToTable("SHOZOKU");
    
            entity.HasKey(e => new {
                e.Parent_医療従事者ID,
                e.年度,
            })
            .HasName("PK_SHOZOKU");
    
            entity.Property(e => e.Parent_医療従事者ID)
                .HasColumnName("Parent_ID")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.年度)
                .HasColumnName("NENDO")
                .IsRequired(true)
                .HasColumnOrder(1);
            entity.Property(e => e.診療科_診療科コード)
                .HasColumnName("診療科_BUSHO_CD")
                .HasMaxLength(10)
                .IsRequired(true)
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
    
            entity.HasOne(e => e.診療科)
                .WithMany(e => e.RefFrom所属診療科_診療科)
                .HasForeignKey(e => new {
                    e.診療科_診療科コード,
                })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BUSHO_SHOZOKU_X6518C1A");
        });
    }
    /// <summary>
    /// テーブルやカラムの詳細を定義します。
    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
    /// </summary>
    protected virtual void OnModelCreating権限(ModelBuilder modelBuilder) {
        modelBuilder.Entity<権限DbEntity>(entity => {
            entity.ToTable("AUTHORITY");
    
            entity.HasKey(e => new {
                e.Parent_医療従事者ID,
                e.権限レベル,
            })
            .HasName("PK_AUTHORITY");
    
            entity.Property(e => e.Parent_医療従事者ID)
                .HasColumnName("Parent_ID")
                .IsRequired(true)
                .HasColumnOrder(0);
            entity.Property(e => e.権限レベル)
                .HasColumnName("AUTHORITY_LEVEL")
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
        });
    }
}
#endregion Entity Framework Core エンティティ定義

