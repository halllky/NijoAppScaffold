#if DEBUG

using System.Data.Common;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace MyApp.WebApi.Debugging.DbViewer;

/// <summary>
/// 1つのテーブルの中身を1ページ分取得する <c>SELECT * FROM テーブル</c> クエリ。
/// WHERE句・ORDER BY句は利用者が入力した文字列をそのままSQLに埋め込む。
/// 開発者専用の画面から使われる前提のため、SQLインジェクション対策は複文やコメントの混入を拒否する程度にとどめている。
/// </summary>
public class TableDataQuery {
    /// <summary>対象テーブルの物理名。</summary>
    [JsonPropertyName("tableName")]
    public string TableName { get; init; } = "";
    /// <summary>WHERE句の条件部分（WHERE キーワードは含まない）。空の場合は絞り込まない。</summary>
    [JsonPropertyName("where")]
    public string? Where { get; init; }
    /// <summary>ORDER BY句の並び順部分（ORDER BY キーワードは含まない）。空の場合は並び順を指定しない。</summary>
    [JsonPropertyName("orderBy")]
    public string? OrderBy { get; init; }
    /// <summary>0始まりのページ番号。</summary>
    [JsonPropertyName("pageIndex")]
    public int PageIndex { get; init; }
    /// <summary>1ページあたりの件数。</summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; init; }

    private const int MAX_PAGE_SIZE = 1000;
    private static readonly string[] FORBIDDEN_TOKENS = [";", "--", "/*", "*/"];

    /// <summary>
    /// クエリを実行してよいかを検証し、問題の一覧を返す。問題が無ければ空。
    /// </summary>
    public IReadOnlyList<string> Validate(DbSchema schema) {
        var errors = new List<string>();

        if (!schema.ContainsTable(TableName)) {
            errors.Add($"テーブル '{TableName}' は存在しません。");
        }
        if (PageIndex < 0) {
            errors.Add("ページ番号は0以上を指定してください。");
        }
        if (PageSize < 1 || PageSize > MAX_PAGE_SIZE) {
            errors.Add($"1ページあたりの件数は1～{MAX_PAGE_SIZE}の範囲で指定してください。");
        }
        foreach (var (clauseName, clause) in new[] { ("WHERE", Where), ("ORDER BY", OrderBy) }) {
            var token = FORBIDDEN_TOKENS.FirstOrDefault(t => clause?.Contains(t) == true);
            if (token != null) {
                errors.Add($"{clauseName}句に '{token}' を含めることはできません。");
            }
        }

        return errors;
    }

    /// <summary>
    /// クエリを実行する。
    /// 条件に合致する全件数と、指定ページの行を返す。値はすべて表示用の文字列に変換される。
    /// WHERE句・ORDER BY句の誤りなど、SQLの実行時エラーは <see cref="DbException"/> として送出される。
    /// </summary>
    public async Task<TableDataQueryResult> ExecuteAsync(DbContext dbContext) {
        var sqlHelper = dbContext.GetService<ISqlGenerationHelper>();
        var from = $"FROM {sqlHelper.DelimitIdentifier(TableName)}";
        var where = string.IsNullOrWhiteSpace(Where) ? "" : $"WHERE {Where}";
        var orderBy = string.IsNullOrWhiteSpace(OrderBy) ? "" : $"ORDER BY {OrderBy}";

        var connection = dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();
        try {
            // 全件数
            await using var countCommand = connection.CreateCommand();
            countCommand.CommandText = $"SELECT COUNT(*) {from} {where}";
            var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

            // 指定ページの行。
            // ページングの構文はデータベースの種類によって異なる。ここでは LIMIT / OFFSET 構文を使う。
            await using var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = $"SELECT * {from} {where} {orderBy} LIMIT @limit OFFSET @offset";
            AddParameter(selectCommand, "@limit", PageSize);
            AddParameter(selectCommand, "@offset", PageIndex * PageSize);

            await using var reader = await selectCommand.ExecuteReaderAsync();
            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
            var rows = new List<string?[]>();
            while (await reader.ReadAsync()) {
                var row = new string?[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; i++) {
                    row[i] = reader.IsDBNull(i) ? null : Convert.ToString(reader.GetValue(i));
                }
                rows.Add(row);
            }

            return new TableDataQueryResult {
                Columns = columns,
                Rows = rows,
                TotalCount = totalCount,
            };
        } finally {
            if (shouldClose) await connection.CloseAsync();
        }
    }

    private static void AddParameter(DbCommand command, string name, object value) {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}

/// <summary>
/// <see cref="TableDataQuery"/> の実行結果。
/// </summary>
public class TableDataQueryResult {
    /// <summary>列名。</summary>
    [JsonPropertyName("columns")]
    public required IReadOnlyList<string> Columns { get; init; }
    /// <summary>行。各行の値の並びは <see cref="Columns"/> と対応する。NULLは null。</summary>
    [JsonPropertyName("rows")]
    public required IReadOnlyList<string?[]> Rows { get; init; }
    /// <summary>条件に合致する全件数。</summary>
    [JsonPropertyName("totalCount")]
    public required int TotalCount { get; init; }
}

#endif
