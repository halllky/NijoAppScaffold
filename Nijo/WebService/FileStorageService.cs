using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Nijo.WebService;

/// <summary>
/// ファイル操作を安全に行うための共通サービス
/// </summary>
internal class FileStorageService {
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// ファイル識別子として安全な文字列かどうかを検証する
    /// </summary>
    /// <exception cref="ArgumentException">不正な文字が含まれている場合</exception>
    internal static void ThrowIfInvalidFileIdentifier(string identifier, string parameterName) {
        if (string.IsNullOrEmpty(identifier)) {
            throw new ArgumentException($"{parameterName} cannot be null or empty.", parameterName);
        }

        // ファイル名として不正な文字が含まれていないかチェック
        if (identifier.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            throw new ArgumentException($"{parameterName} contains invalid characters for a file name: {identifier}", parameterName);
        }

        // ディレクトリ区切り文字が含まれていないかチェック (ディレクトリトラバーサル対策)
        if (identifier.Contains(Path.DirectorySeparatorChar) || identifier.Contains(Path.AltDirectorySeparatorChar)) {
            throw new ArgumentException($"{parameterName} cannot contain directory separator characters: {identifier}", parameterName);
        }
    }

    /// <summary>
    /// ディレクトリが存在することを保証する（存在しなければ作成）
    /// </summary>
    internal static void EnsureDirectoryExists(string directoryPath) {
        if (!Directory.Exists(directoryPath)) {
            Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// JSONオブジェクトをファイルに保存する
    /// </summary>
    internal static async Task SaveJsonAsync(string filePath, JsonObject data, CancellationToken cancellationToken = default) {
        var directoryPath = Path.GetDirectoryName(filePath);
        if (directoryPath != null) {
            EnsureDirectoryExists(directoryPath);
        }

        var json = JsonSerializer.Serialize(data, _jsonSerializerOptions);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    /// <summary>
    /// ファイルからJSONオブジェクトを読み込む
    /// </summary>
    internal static async Task<JsonObject?> LoadJsonAsync(string filePath, CancellationToken cancellationToken = default) {
        if (!File.Exists(filePath)) {
            return null;
        }

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        return JsonSerializer.Deserialize<JsonObject>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// 指定されたディレクトリ内の.jsonファイル一覧を取得する
    /// </summary>
    internal static async Task<List<JsonObject>> ListJsonFilesAsync(
        string directoryPath,
        Func<string, string> fileNameToId,
        Func<JsonObject, string?> extractName,
        Action<string> onError,
        CancellationToken cancellationToken = default) {

        var result = new List<JsonObject>();

        if (!Directory.Exists(directoryPath)) {
            return result;
        }

        foreach (var filePath in Directory.GetFiles(directoryPath, "*.json")) {
            try {
                var json = await File.ReadAllTextAsync(filePath, cancellationToken);
                var data = JsonSerializer.Deserialize<JsonObject>(json, _jsonSerializerOptions);
                if (data == null) continue;

                var id = fileNameToId(Path.GetFileNameWithoutExtension(filePath));
                var name = extractName(data) ?? id;

                result.Add(new JsonObject {
                    ["id"] = id,
                    ["name"] = name,
                });

            } catch (JsonException ex) {
                onError($"Error deserializing {filePath}: {ex.Message}");
            } catch (Exception ex) {
                onError($"Error processing file {filePath}: {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// 指定されたディレクトリ内の.jsonファイル一覧を取得する（カスタム変換付き）
    /// </summary>
    internal static async Task<List<T>> ListJsonFilesAsync<T>(
        string directoryPath,
        Func<string, JsonObject, T> converter,
        Action<string> onError,
        CancellationToken cancellationToken = default) where T : class {

        var result = new List<T>();

        if (!Directory.Exists(directoryPath)) {
            return result;
        }

        foreach (var filePath in Directory.GetFiles(directoryPath, "*.json")) {
            try {
                var json = await File.ReadAllTextAsync(filePath, cancellationToken);
                var data = JsonSerializer.Deserialize<JsonObject>(json, _jsonSerializerOptions);
                if (data == null) continue;

                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var item = converter(fileName, data);
                result.Add(item);

            } catch (JsonException ex) {
                onError($"Error deserializing {filePath}: {ex.Message}");
            } catch (Exception ex) {
                onError($"Error processing file {filePath}: {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// JSONシリアライズオプション（外部公開用）
    /// </summary>
    internal static JsonSerializerOptions JsonOptions => _jsonSerializerOptions;
}

