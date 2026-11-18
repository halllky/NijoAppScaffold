using System.Text;
using Microsoft.Extensions.Logging;
using MyApp.Core.ジョブ;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// 外部システム（商品管理システム）から商品データを取り込むジョブ。毎日実行し、画面からも起動できる。
    /// 取込結果は、商品ごとに1行の CSV ファイルとしてスプールに出力する。
    /// </summary>
    public static readonly ジョブの種類 商品データ取込のジョブ = new("商品データ取込", (app, job) => app.Run商品データ取込Async(job), ジョブの予定.毎日(new TimeOnly(2, 0)));

    /// <summary>
    /// 商品データ取込のジョブを登録する。取込の完了を待たずに応答を返し、完了したら通知で知らせる。
    /// </summary>
    protected override async Task OnExecute商品データ取込Async(MessageSetter messages, OperationContext context) {
        context.Confirms.Add(MSG_DEMO101.ProductImportConfirm());
        if (context.ValidationOnly) return;

        await using var tran = await BeginTransactionAsync();
        await RegisterジョブAsync(商品データ取込のジョブ);
        await tran.CommitAsync();
    }

    /// <summary>
    /// 商品データを取り込む。取り込めない商品があっても残りの商品の取込は続け、最後に異常終了とする。
    /// トランザクションは商品1件ごとなので、異常終了しても、取り込めた商品はロールバックされない。
    /// </summary>
    private async Task Run商品データ取込Async(実行中のジョブ job) {
        var results = new List<(Core.外部システム.商品管理システム.ProductImportData Item, string Result)>();
        foreach (var item in 商品管理システム.Enumerate商品データ()) {
            job.停止の合図.ThrowIfCancellationRequested();
            results.Add((item, await Import商品Async(item)));
        }

        await Write取込結果Async(Path.Combine(job.スプールフォルダ, "取込結果.csv"), results);

        var failedCount = results.Count(r => r.Result != 取込結果_登録 && r.Result != 取込結果_更新);
        job.結果メッセージ = failedCount > 0
            ? MSG_DEMO101.ProductImportPartiallyFailedResult(results.Count, results.Count - failedCount, failedCount)
            : MSG_DEMO101.ProductImportResult(results.Count, results.Count - failedCount);
        Log.LogInformation("商品データ取込が完了しました。{Message}", job.結果メッセージ);

        if (failedCount > 0) throw new InvalidOperationException($"取り込めない商品データがありました。{job.結果メッセージ}");
    }

    private const string 取込結果_登録 = "登録";
    private const string 取込結果_更新 = "更新";

    /// <summary>
    /// 商品1件を、外部システム側IDで検索し、新規登録または更新する。
    /// </summary>
    /// <returns>取込の結果。登録・更新、または取り込めなかった理由</returns>
    private async Task<string> Import商品Async(Core.外部システム.商品管理システム.ProductImportData item) {
        Log.LogInformation("処理開始: {itemName} ({itemExternalId})", item.Name, item.ExternalId);

        // 消費税区分の変換 (文字列 -> Enum)
        if (!Enum.TryParse<消費税区分>(item.TaxType, out var taxType)) {
            Log.LogWarning("不正な消費税区分です: {itemTaxType} (商品: {itemName})", item.TaxType, item.Name);
            return MSG_DEMO101.InvalidTaxTypeReason(item.TaxType);
        }

        // 外部システムIDで検索。
        // 読み込み時点のバージョンで楽観排他をかける。
        var loaded = await DbContext.LoadForEdit商品Async(query => query.Where(x => x.外部システム側ID == item.ExternalId));
        var existing = loaded.SingleOrDefault();

        ValidatedCommand<商品UpdateCommand>? validated;
        商品SaveCommandMessages errors;
        if (existing == null) {
            // 新規登録
            var createCommand = DbContext.StartCreating商品();
            createCommand.外部システム側ID = item.ExternalId;
            createCommand.商品名 = item.Name;
            createCommand.売値単価_税抜 = item.Price;
            createCommand.消費税区分 = taxType;
            createCommand.TryValidate(out validated, out errors);

        } else {
            // 更新
            existing.商品名 = item.Name;
            existing.売値単価_税抜 = item.Price;
            existing.消費税区分 = taxType;
            existing.TryValidate(out validated, out errors);
        }
        if (validated == null) return string.Join(" ", errors.GetAllMessages());

        // トランザクションの範囲は商品1件
        await using var tran = await BeginTransactionAsync();

        var result = await DbContext.TrySaveAsync(validated);
        if (!result.Success) return string.Join(" ", result.Errors.GetAllMessages());

        await tran.CommitAsync();
        return existing == null ? 取込結果_登録 : 取込結果_更新;
    }

    /// <summary>
    /// 取込結果を CSV ファイルに出力する。Excel で開いても文字化けしないよう、BOM 付きの UTF-8 にする。
    /// </summary>
    private static async Task Write取込結果Async(string path, IEnumerable<(Core.外部システム.商品管理システム.ProductImportData Item, string Result)> results) {
        var csv = new StringBuilder();
        csv.AppendLine("外部システム側ID,商品名,結果");
        foreach (var (item, result) in results) {
            csv.AppendLine(string.Join(",", Escape(item.ExternalId), Escape(item.Name), Escape(result)));
        }
        await File.WriteAllTextAsync(path, csv.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        static string Escape(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
