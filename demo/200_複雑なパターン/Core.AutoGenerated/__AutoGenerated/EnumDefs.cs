
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : EnumFile.cs
// ※ このファイルを生成したメソッド: Render

using System.ComponentModel.DataAnnotations;

namespace MyApp;

/// <summary>
/// action_type
/// </summary>
public enum action_type {
    [Display(Name = "機器補充")]
    補充 = 1,
    [Display(Name = "設定調整")]
    調整 = 2,
    [Display(Name = "機器廃棄")]
    破棄 = 3,
    [Display(Name = "機器移動")]
    移動 = 4,
    [Display(Name = "再点検")]
    確認 = 5,
}

/// <summary>
/// card_type
/// </summary>
public enum card_type {
    [Display(Name = "VISA Card")]
    VISA = 1,
    [Display(Name = "Master Card")]
    MasterCard = 2,
    [Display(Name = "JCB Card")]
    JCB = 3,
    [Display(Name = "American Express")]
    AmericanExpress = 4,
    [Display(Name = "Diners Club")]
    DinersClub = 5,
}

/// <summary>
/// document_type
/// </summary>
public enum document_type {
    [Display(Name = "写真")]
    写真 = 1,
    [Display(Name = "報告書")]
    報告書 = 2,
    [Display(Name = "請求書")]
    請求書 = 3,
    [Display(Name = "納品書")]
    納品書 = 4,
    [Display(Name = "その他")]
    その他 = 5,
}

/// <summary>
/// gender_type
/// </summary>
public enum gender_type {
    [Display(Name = "男性")]
    男性 = 1,
    [Display(Name = "女性")]
    女性 = 2,
    [Display(Name = "その他")]
    その他 = 3,
    [Display(Name = "回答しない")]
    回答しない = 4,
}

/// <summary>
/// member_rank
/// </summary>
public enum member_rank {
    [Display(Name = "一般患者")]
    一般 = 1,
    [Display(Name = "高齢者")]
    シルバー = 2,
    [Display(Name = "重篤患者")]
    ゴールド = 3,
    [Display(Name = "VIP患者")]
    プラチナ = 4,
}

/// <summary>
/// payment_status
/// </summary>
public enum payment_status {
    [Display(Name = "未払い")]
    未払い = 1,
    [Display(Name = "支払済")]
    支払済 = 2,
    [Display(Name = "一部支払い")]
    一部支払い = 3,
    [Display(Name = "キャンセル")]
    キャンセル = 4,
}

/// <summary>
/// payment_type
/// </summary>
public enum payment_type {
    [Display(Name = "現金決済")]
    現金 = 1,
    [Display(Name = "クレジットカード決済")]
    クレジットカード = 2,
    [Display(Name = "電子マネー決済")]
    電子マネー = 3,
    [Display(Name = "保険適用")]
    保険 = 4,
    [Display(Name = "銀行振込")]
    銀行振込 = 5,
}

/// <summary>
/// position_type
/// </summary>
public enum position_type {
    [Display(Name = "看護師")]
    一般社員 = 1,
    [Display(Name = "主任看護師")]
    主任 = 2,
    [Display(Name = "医師")]
    課長 = 3,
    [Display(Name = "主任医師")]
    部長 = 4,
    [Display(Name = "院長")]
    役員 = 5,
}

/// <summary>
/// reservation_type
/// </summary>
public enum reservation_type {
    [Display(Name = "初診（しょしん）")]
    初診 = 1,
    [Display(Name = "再診（さいしん）")]
    再診 = 2,
    [Display(Name = "検査（けんさ）")]
    検査 = 3,
}

/// <summary>
/// result_status
/// </summary>
public enum result_status {
    [Display(Name = "成功")]
    成功 = 1,
    [Display(Name = "部分的成功")]
    部分的成功 = 2,
    [Display(Name = "失敗")]
    失敗 = 3,
    [Display(Name = "保留")]
    保留 = 4,
}

/// <summary>
/// shipping_method
/// </summary>
public enum shipping_method {
    [Display(Name = "紹介状郵送")]
    紹介状送付 = 1,
    [Display(Name = "FAX送信")]
    FAX送信 = 2,
    [Display(Name = "院内受取")]
    院内受取 = 3,
}

/// <summary>
/// shipping_status
/// </summary>
public enum shipping_status {
    [Display(Name = "準備中")]
    準備中 = 1,
    [Display(Name = "送信済")]
    送信済 = 2,
    [Display(Name = "確認中")]
    確認中 = 3,
    [Display(Name = "受理完了")]
    受理完了 = 4,
}

/// <summary>
/// status_type
/// </summary>
public enum status_type {
    [Display(Name = "未実施")]
    未実施 = 1,
    [Display(Name = "実施中")]
    実施中 = 2,
    [Display(Name = "完了")]
    完了 = 3,
    [Display(Name = "中止")]
    中止 = 4,
}

/// <summary>
/// 権限レベル
/// </summary>
public enum 権限レベル {
    [Display(Name = "管理者")]
    管理者 = 1,
    [Display(Name = "一般")]
    一般 = 2,
}

/// <summary>action_typeの検索条件クラス</summary>
public class action_typeSearchCondition {
    public bool 補充 { get; set; }
    public bool 調整 { get; set; }
    public bool 破棄 { get; set; }
    public bool 移動 { get; set; }
    public bool 確認 { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (補充) return true;
        if (調整) return true;
        if (破棄) return true;
        if (移動) return true;
        if (確認) return true;
        return false;
    }
}

/// <summary>card_typeの検索条件クラス</summary>
public class card_typeSearchCondition {
    public bool VISA { get; set; }
    public bool MasterCard { get; set; }
    public bool JCB { get; set; }
    public bool AmericanExpress { get; set; }
    public bool DinersClub { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (VISA) return true;
        if (MasterCard) return true;
        if (JCB) return true;
        if (AmericanExpress) return true;
        if (DinersClub) return true;
        return false;
    }
}

/// <summary>document_typeの検索条件クラス</summary>
public class document_typeSearchCondition {
    public bool 写真 { get; set; }
    public bool 報告書 { get; set; }
    public bool 請求書 { get; set; }
    public bool 納品書 { get; set; }
    public bool その他 { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (写真) return true;
        if (報告書) return true;
        if (請求書) return true;
        if (納品書) return true;
        if (その他) return true;
        return false;
    }
}

/// <summary>gender_typeの検索条件クラス</summary>
public class gender_typeSearchCondition {
    public bool 男性 { get; set; }
    public bool 女性 { get; set; }
    public bool その他 { get; set; }
    public bool 回答しない { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (男性) return true;
        if (女性) return true;
        if (その他) return true;
        if (回答しない) return true;
        return false;
    }
}

/// <summary>member_rankの検索条件クラス</summary>
public class member_rankSearchCondition {
    public bool 一般 { get; set; }
    public bool シルバー { get; set; }
    public bool ゴールド { get; set; }
    public bool プラチナ { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (一般) return true;
        if (シルバー) return true;
        if (ゴールド) return true;
        if (プラチナ) return true;
        return false;
    }
}

/// <summary>payment_statusの検索条件クラス</summary>
public class payment_statusSearchCondition {
    public bool 未払い { get; set; }
    public bool 支払済 { get; set; }
    public bool 一部支払い { get; set; }
    public bool キャンセル { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (未払い) return true;
        if (支払済) return true;
        if (一部支払い) return true;
        if (キャンセル) return true;
        return false;
    }
}

/// <summary>payment_typeの検索条件クラス</summary>
public class payment_typeSearchCondition {
    public bool 現金 { get; set; }
    public bool クレジットカード { get; set; }
    public bool 電子マネー { get; set; }
    public bool 保険 { get; set; }
    public bool 銀行振込 { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (現金) return true;
        if (クレジットカード) return true;
        if (電子マネー) return true;
        if (保険) return true;
        if (銀行振込) return true;
        return false;
    }
}

/// <summary>position_typeの検索条件クラス</summary>
public class position_typeSearchCondition {
    public bool 一般社員 { get; set; }
    public bool 主任 { get; set; }
    public bool 課長 { get; set; }
    public bool 部長 { get; set; }
    public bool 役員 { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (一般社員) return true;
        if (主任) return true;
        if (課長) return true;
        if (部長) return true;
        if (役員) return true;
        return false;
    }
}

/// <summary>reservation_typeの検索条件クラス</summary>
public class reservation_typeSearchCondition {
    public bool 初診 { get; set; }
    public bool 再診 { get; set; }
    public bool 検査 { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (初診) return true;
        if (再診) return true;
        if (検査) return true;
        return false;
    }
}

/// <summary>result_statusの検索条件クラス</summary>
public class result_statusSearchCondition {
    public bool 成功 { get; set; }
    public bool 部分的成功 { get; set; }
    public bool 失敗 { get; set; }
    public bool 保留 { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (成功) return true;
        if (部分的成功) return true;
        if (失敗) return true;
        if (保留) return true;
        return false;
    }
}

/// <summary>shipping_methodの検索条件クラス</summary>
public class shipping_methodSearchCondition {
    public bool 紹介状送付 { get; set; }
    public bool FAX送信 { get; set; }
    public bool 院内受取 { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (紹介状送付) return true;
        if (FAX送信) return true;
        if (院内受取) return true;
        return false;
    }
}

/// <summary>shipping_statusの検索条件クラス</summary>
public class shipping_statusSearchCondition {
    public bool 準備中 { get; set; }
    public bool 送信済 { get; set; }
    public bool 確認中 { get; set; }
    public bool 受理完了 { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (準備中) return true;
        if (送信済) return true;
        if (確認中) return true;
        if (受理完了) return true;
        return false;
    }
}

/// <summary>status_typeの検索条件クラス</summary>
public class status_typeSearchCondition {
    public bool 未実施 { get; set; }
    public bool 実施中 { get; set; }
    public bool 完了 { get; set; }
    public bool 中止 { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (未実施) return true;
        if (実施中) return true;
        if (完了) return true;
        if (中止) return true;
        return false;
    }
}

/// <summary>権限レベルの検索条件クラス</summary>
public class 権限レベルSearchCondition {
    public bool 管理者 { get; set; }
    public bool 一般 { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (管理者) return true;
        if (一般) return true;
        return false;
    }
}

