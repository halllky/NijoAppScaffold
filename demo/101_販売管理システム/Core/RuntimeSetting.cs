using System.ComponentModel.DataAnnotations;

namespace MyApp;

/// <summary>
/// 実行時設定。 appsettings.json から読み込まれた設定値を使う。
/// 項目の既定値はこのクラスに持たず、設定ファイルに書く。
/// 本番環境の設定ファイルに書き忘れたまま、気付かずにプログラムの既定値で動くのを防ぐため。
/// 省略できない項目には <see cref="RequiredAttribute"/> などの検証の属性を付け、起動のときに <see cref="ThrowIfInvalid"/> で確かめる。
/// </summary>
public partial class RuntimeSetting {

    /// <summary>
    /// appsettings.json には NLog の設定なども含まれているので
    /// どのセクションがこのクラスで定義されている項目と対応するかを表すセクション名
    /// </summary>
    public const string MY_APP_SECTION = "MyApp";

    /// <summary>
    /// NLogのログ出力先ディレクトリ
    /// </summary>
    [Required]
    public string LogDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 省略できない項目が省略されていたり、値が不正だったりしたら、例外を投げる。
    /// 起動の時点で設定の誤りを知らせるため、DI の設定のときに呼ぶ。
    /// </summary>
    public void ThrowIfInvalid() {
        var errors = new List<string>();
        Collect(this, MY_APP_SECTION, errors);
        Collect(ErrorNotification, $"{MY_APP_SECTION}:{nameof(ErrorNotification)}", errors);
        Collect(Job, $"{MY_APP_SECTION}:{nameof(Job)}", errors);
        Collect(Attachment, $"{MY_APP_SECTION}:{nameof(Attachment)}", errors);
        if (errors.Count > 0) {
            throw new InvalidOperationException($"設定に誤りがあります。{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }

        // タイムゾーンの妥当性はアノテーションで検査できないので直接呼び出して検証する
        GetBusinessTimeZone();

        static void Collect(object settings, string sectionPath, List<string> errors) {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(settings, new ValidationContext(settings), results, validateAllProperties: true);
            foreach (var result in results) {
                errors.Add($"{sectionPath}:{string.Join(", ", result.MemberNames)} が指定されていないか、値が不正です。");
            }
        }
    }

    #region データベース接続
    /// <summary>
    /// 現在接続しているデータベースのプロファイル。
    /// <see cref="DbProfiles"/> に設定されたものの名前を指定する。
    /// </summary>
    [Required]
    public string CurrentDbProfileName { get; set; } = string.Empty;
    /// <summary>
    /// 接続先データベース情報の一覧。
    /// 開発中はここに記載されたプロファイルを切り替えながら進める。
    /// </summary>
    public List<DbProfile> DbProfiles { get; set; } = [];

    /// <summary>
    /// マイグレーションスクリプトが格納されているフォルダパス
    /// </summary>
    [Required]
    public string MigrationsScriptFolder { get; set; } = string.Empty;

    public class DbProfile {
        /// <summary>
        /// プロファイル名
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 接続文字列
        /// </summary>
        public string ConnStr { get; set; } = string.Empty;
    }

    /// <summary>
    /// 設定されているDB接続先情報を返します。
    /// 設定値が不正な場合は例外を送出します。
    /// </summary>
    public DbProfile GetCurrentProfile() {
        if (string.IsNullOrWhiteSpace(CurrentDbProfileName)) {
            throw new InvalidOperationException($"{nameof(CurrentDbProfileName)}が指定されていません。");
        }
        var profile = DbProfiles.FirstOrDefault(x => x.Name == CurrentDbProfileName)
            ?? throw new InvalidOperationException($"{nameof(DbProfiles)}に'{CurrentDbProfileName}'という名前の設定がありません。");

        return profile;
    }
    #endregion データベース接続

    /// <summary>
    /// 運用担当へのエラーの通知の設定
    /// </summary>
    public Core.運用担当への通知.ErrorNotificationSettings ErrorNotification { get; set; } = new();

    /// <summary>
    /// ジョブの設定
    /// </summary>
    public Core.ジョブ.JobSettings Job { get; set; } = new();

    /// <summary>
    /// 添付ファイルの設定
    /// </summary>
    public Core.添付ファイル.AttachmentSettings Attachment { get; set; } = new();

    #region タイムゾーン
    /// <summary>
    /// 業務のタイムゾーン。IANA のタイムゾーンの ID（"Asia/Tokyo" など）で指定する。
    /// 日時は、このタイムゾーンの現地時刻として扱う。現在時刻も、サーバーの OS のタイムゾーンによらず、このタイムゾーンの時刻になる。
    /// </summary>
    [Required]
    public string BusinessTimeZone { get; set; } = string.Empty;

    /// <summary>
    /// 業務のタイムゾーンを返す。
    /// 設定値が不正な場合は例外を送出します。
    /// </summary>
    public TimeZoneInfo GetBusinessTimeZone() {
        try {
            return TimeZoneInfo.FindSystemTimeZoneById(BusinessTimeZone);
        } catch (TimeZoneNotFoundException ex) {
            throw new InvalidOperationException($"{nameof(BusinessTimeZone)}に指定されたタイムゾーン'{BusinessTimeZone}'が見つかりません。", ex);
        }
    }
    #endregion タイムゾーン
}
