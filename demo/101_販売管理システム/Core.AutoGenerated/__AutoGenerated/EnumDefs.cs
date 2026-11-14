
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : EnumFile.cs
// ※ このファイルを生成したメソッド: Render

using System.ComponentModel.DataAnnotations;

namespace MyApp;

/// <summary>
/// 売上明細区分
/// </summary>
public enum 売上明細区分 {
    [Display(Name = "売上")]
    売上 = 0,
    [Display(Name = "取消")]
    取消 = 1,
}

/// <summary>
/// 権限区分
/// </summary>
public enum 権限区分 {
    [Display(Name = "システム管理者")]
    システム管理者 = 0,
    [Display(Name = "入荷担当")]
    入荷担当 = 1,
    [Display(Name = "販売担当")]
    販売担当 = 2,
}

/// <summary>
/// 消費税区分
/// </summary>
public enum 消費税区分 {
    [Display(Name = "非課税")]
    非課税 = 0,
    [Display(Name = "一般税率")]
    一般税率 = 1,
    [Display(Name = "軽減税率")]
    軽減税率 = 2,
}

/// <summary>売上明細区分の検索条件クラス</summary>
public class 売上明細区分SearchCondition {
    public bool 売上 { get; set; }
    public bool 取消 { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (売上) return true;
        if (取消) return true;
        return false;
    }
}

/// <summary>権限区分の検索条件クラス</summary>
public class 権限区分SearchCondition {
    public bool システム管理者 { get; set; }
    public bool 入荷担当 { get; set; }
    public bool 販売担当 { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (システム管理者) return true;
        if (入荷担当) return true;
        if (販売担当) return true;
        return false;
    }
}

/// <summary>消費税区分の検索条件クラス</summary>
public class 消費税区分SearchCondition {
    public bool 非課税 { get; set; }
    public bool 一般税率 { get; set; }
    public bool 軽減税率 { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (非課税) return true;
        if (一般税率) return true;
        if (軽減税率) return true;
        return false;
    }
}

