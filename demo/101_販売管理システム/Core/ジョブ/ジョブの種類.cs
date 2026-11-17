using System.Reflection;
using System.Text.Json;

namespace MyApp.Core.ジョブ;

/// <summary>
/// ジョブの種類。種類の名前と、ジョブの本体と、定期的に実行する種類ならその予定をもつ。
/// 種類は <see cref="OverridedApplicationService"/> の public static readonly のフィールドとして定義する。
/// 業務のフォルダを削除すれば、その業務のジョブの種類も消えるように、種類の一覧をどこかに列挙することはしない。
/// 引数をとる種類は <see cref="ジョブの種類{TArgument}"/> で定義する。
/// </summary>
/// <example>
/// <code>
/// partial class OverridedApplicationService {
///     public static readonly ジョブの種類 日次集計 = new("日次集計", (app, job) => app.Run日次集計Async(job), ジョブの予定.毎日(new(2, 0)));
/// }
/// </code>
/// </example>
public class ジョブの種類 {

    /// <param name="名前">種類の名前。ジョブのテーブルに保存し、ログの処理名にもなる。種類の間で重複しないこと</param>
    /// <param name="本体">ジョブの本体。正常終了なら何も返さず、異常終了なら例外を投げる</param>
    /// <param name="予定">定期的に実行する場合の予定。画面などから登録したときだけ実行する種類では null</param>
    public ジョブの種類(string 名前, Func<OverridedApplicationService, 実行中のジョブ, Task> 本体, ジョブの予定? 予定 = null) {
        if (string.IsNullOrWhiteSpace(名前)) throw new ArgumentException("ジョブの種類の名前を指定してください。", nameof(名前));
        Name = 名前;
        _本体 = 本体;
        this.予定 = 予定;
    }
    private readonly Func<OverridedApplicationService, 実行中のジョブ, Task> _本体;

    /// <summary>種類の名前</summary>
    public string Name { get; }
    /// <summary>定期的に実行する場合の予定。定期的に実行しない種類では null</summary>
    public ジョブの予定? 予定 { get; }

    /// <summary>
    /// ジョブの本体を実行する。
    /// </summary>
    internal Task ExecuteAsync(OverridedApplicationService app, 実行中のジョブ job) => _本体(app, job);

    /// <summary>
    /// 登録のときに引数を渡す必要がある種類なら true。
    /// </summary>
    internal virtual bool Has引数 => false;

    /// <summary>
    /// <see cref="OverridedApplicationService"/> の static フィールドに定義された、すべての種類を集める。
    /// </summary>
    internal static ジョブの種類[] Collect定義された種類() {
        return typeof(OverridedApplicationService)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => typeof(ジョブの種類).IsAssignableFrom(f.FieldType))
            .Select(f => (ジョブの種類)f.GetValue(null)!)
            .ToArray();
    }
}

/// <summary>
/// 引数をとるジョブの種類。引数は JSON にしてジョブのテーブルに保存し、実行するときに元の型に戻して本体に渡す。
/// 引数は登録から実行までの間にプログラムが変わっても読めるよう、単純な値だけをもつ型にすること。
/// 定期的なジョブは引数を渡す者がいないので、この種類では予定を指定できない。
/// </summary>
/// <typeparam name="TArgument">引数の型</typeparam>
public sealed class ジョブの種類<TArgument> : ジョブの種類 {

    /// <param name="名前">種類の名前。ジョブのテーブルに保存し、ログの処理名にもなる。種類の間で重複しないこと</param>
    /// <param name="本体">ジョブの本体。正常終了なら何も返さず、異常終了なら例外を投げる</param>
    public ジョブの種類(string 名前, Func<OverridedApplicationService, TArgument, 実行中のジョブ, Task> 本体)
        : base(名前, (app, job) => 本体(app, DeserializeArgument(job.引数), job)) {
    }

    internal override bool Has引数 => true;

    /// <summary>
    /// 引数をジョブのテーブルに保存する形にする。
    /// </summary>
    internal static string SerializeArgument(TArgument argument) {
        return JsonSerializer.Serialize(argument, JSON_OPTIONS);
    }

    private static TArgument DeserializeArgument(string? json) {
        if (json == null) throw new InvalidOperationException("ジョブの引数が保存されていません。");
        return JsonSerializer.Deserialize<TArgument>(json, JSON_OPTIONS)!;
    }

    private static readonly JsonSerializerOptions JSON_OPTIONS = new JsonSerializerOptions().EditDefaultJsonSerializerOptions();
}
