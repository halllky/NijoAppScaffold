#if DEBUG

using System.Data.Common;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MyApp.WebApi.Debugging.DbViewer;

/// <summary>
/// DBビューア（ER図とテーブルデータのプレビュー）のデバッグ用エンドポイント。
/// </summary>
[ApiController]
[Route("api/debug/db-viewer")]
[AllowAnonymous]
public class DbViewerController : ControllerBase {

    /// <summary>
    /// 全テーブルの定義を返す。
    /// </summary>
    [HttpGet("schema")]
    public IActionResult GetSchema([FromServices] OverridedApplicationService app) {
        var model = app.DbContext.GetService<IDesignTimeModel>().Model;
        return Ok(DbSchema.FromModel(model));
    }

    /// <summary>
    /// 画面の設定を返す。まだ保存されていない場合、設定は null。
    /// </summary>
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings([FromServices] IWebHostEnvironment env) {
        var file = DbViewerSettingsFile.Locate(env.ContentRootPath);
        if (file == null) return Problem(NIJO_XML_NOT_FOUND);

        return Ok(new SettingsResponse {
            Settings = await file.ReadAsync(),
            FilePath = file.FilePath,
        });
    }

    /// <summary>
    /// 画面の設定を保存する。
    /// </summary>
    [HttpPut("settings")]
    public async Task<IActionResult> SaveSettings([FromBody] JsonObject settings, [FromServices] IWebHostEnvironment env) {
        if (!env.IsDevelopment()) return Forbid();

        var file = DbViewerSettingsFile.Locate(env.ContentRootPath);
        if (file == null) return Problem(NIJO_XML_NOT_FOUND);

        await file.WriteAsync(settings);
        return Ok();
    }

    /// <summary>
    /// テーブルの中身を1ページ分返す。
    /// </summary>
    [HttpPost("table-data")]
    public async Task<IActionResult> GetTableData([FromBody] TableDataQuery query, [FromServices] OverridedApplicationService app) {
        var schema = DbSchema.FromModel(app.DbContext.GetService<IDesignTimeModel>().Model);
        var errors = query.Validate(schema);
        if (errors.Count > 0) return BadRequest(string.Join(Environment.NewLine, errors));

        // 利用者が入力したWHERE句・ORDER BY句の誤りは想定内の失敗なので、例外ではなく入力エラーとして返す
        try {
            return Ok(await query.ExecuteAsync(app.DbContext));
        } catch (DbException ex) {
            return BadRequest(ex.Message);
        }
    }

    private const string NIJO_XML_NOT_FOUND = "nijo.xml が見つからないため、設定ファイルの場所を決められません。";

    private class SettingsResponse {
        [JsonPropertyName("settings")]
        public JsonNode? Settings { get; init; }
        [JsonPropertyName("filePath")]
        public required string FilePath { get; init; }
    }
}

#endif
