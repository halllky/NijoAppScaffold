#if DEBUG

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MyApp.WebApi.Debugging.DbViewer;

/// <summary>
/// DBビューアの画面の設定（サブジェクトエリアの一覧など）が保存されるJSONファイル。
/// nijo.xml と同じディレクトリに置かれ、ソースコードと一緒にバージョン管理される想定。
/// 設定の中身の構造はクライアント側が定義しており、このクラスは中身を解釈せずそのまま読み書きする。
/// </summary>
public class DbViewerSettingsFile {
    private DbViewerSettingsFile(string filePath) {
        FilePath = filePath;
    }

    /// <summary>ファイルの絶対パス。</summary>
    public string FilePath { get; }

    private const string NIJO_XML = "nijo.xml";
    private const string FILE_NAME = "nijo.dbViewer.json";

    /// <summary>
    /// 指定のディレクトリから親方向へ nijo.xml を探し、見つかったディレクトリにある設定ファイルを返す。
    /// nijo.xml が見つからない場合は null を返す。
    /// </summary>
    public static DbViewerSettingsFile? Locate(string startDirectory) {
        for (var dir = new DirectoryInfo(startDirectory); dir != null; dir = dir.Parent) {
            if (File.Exists(Path.Combine(dir.FullName, NIJO_XML))) {
                return new DbViewerSettingsFile(Path.Combine(dir.FullName, FILE_NAME));
            }
        }
        return null;
    }

    /// <summary>
    /// 設定を読み込む。ファイルが存在しない場合は null を返す。
    /// </summary>
    public async Task<JsonNode?> ReadAsync() {
        if (!File.Exists(FilePath)) return null;

        await using var stream = File.OpenRead(FilePath);
        return await JsonNode.ParseAsync(stream);
    }

    /// <summary>
    /// 設定を書き込む。ファイルが存在する場合は上書きする。
    /// </summary>
    public async Task WriteAsync(JsonNode settings) {
        await using var stream = File.Create(FilePath);
        await JsonSerializer.SerializeAsync(stream, settings, WRITE_OPTIONS);
    }

    private static readonly JsonSerializerOptions WRITE_OPTIONS = new() {
        WriteIndented = true,
        // 差分を人間が読めるよう、日本語のテーブル名などをUnicodeエスケープしない
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}

#endif
