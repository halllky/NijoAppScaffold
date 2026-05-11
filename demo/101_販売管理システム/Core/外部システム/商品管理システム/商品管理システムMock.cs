using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MyApp.Core.外部システム.商品管理システム;

public class 商品管理システムMock : I商品管理システム {

    public 商品管理システムMock(OverridedApplicationService app) {
        _app = app;
    }

    private readonly OverridedApplicationService _app;

    public IEnumerable<ProductImportData> Enumerate商品データ() {
        // 設定ファイルからモックデータのパスを取得
        var mockDataPath = _app.Settings.商品管理システム.MockJsonPath;
        if (string.IsNullOrWhiteSpace(mockDataPath)) {
            throw new InvalidOperationException("商品データ取込のモックデータパスが設定されていません。");
        }

        // パスが相対パスの場合は絶対パスに変換
        if (!Path.IsPathRooted(mockDataPath)) {
            mockDataPath = Path.Combine(Directory.GetCurrentDirectory(), mockDataPath);
        }

        // モックファイルが無い場合はサンプルを作成
        if (!File.Exists(mockDataPath)) {
            File.WriteAllText(mockDataPath, JsonSerializer.Serialize(new MockJsonDataType {
                Read = [
                    new() {
                        ExternalId = "EX12345",
                        Name = "サンプル商品A",
                        Price = 1000,
                        TaxType = "課税対象",
                    },
                    new() {
                        ExternalId = "EX67890",
                        Name = "サンプル商品B",
                        Price = 2000,
                        TaxType = "非課税",
                    },
                ],
                Write = new() {
                    { "EX12345", new() { NewQuantity = 50, UpdatedAt = _app.CurrentTime } },
                    { "EX67890", new() { NewQuantity = 30, UpdatedAt = _app.CurrentTime } },
                },
            }, new JsonSerializerOptions {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
            }));
        }

        _app.Log.LogInformation("商品データ取込を開始します。読み込み元: {mockDataPath}", mockDataPath);

        // JSONファイルを読み込む
        var jsonString = File.ReadAllText(mockDataPath);
        var externalData = JsonSerializer.Deserialize<MockJsonDataType>(jsonString)
            ?? throw new InvalidOperationException("モックデータが空、または不正なフォーマットです。");

        return externalData.Read;
    }

    public void Update在庫数量(string externalProductId, int newStockQuantity) {

        if (_app.CurrentTransaction == null) {
            throw new InvalidOperationException("外部リソース更新はトランザクションスコープ内でのみ実行可能です。");
        }

        _app.CurrentTransaction.Prepare($"商品管理システム（モック） 在庫数更新 ID={externalProductId}", async cancellationToken => {
            // 設定ファイルからモックデータのパスを取得
            var mockDataPath = _app.Settings.商品管理システム.MockJsonPath;
            if (string.IsNullOrWhiteSpace(mockDataPath)) {
                throw new InvalidOperationException("商品データ取込のモックデータパスが設定されていません。");
            }

            // パスが相対パスの場合は絶対パスに変換
            if (!Path.IsPathRooted(mockDataPath)) {
                mockDataPath = Path.Combine(Directory.GetCurrentDirectory(), mockDataPath);
            }

            // ファイルが無い場合は新規作成
            if (!File.Exists(mockDataPath)) {
                await File.WriteAllTextAsync(mockDataPath, JsonSerializer.Serialize(new MockJsonDataType(), new JsonSerializerOptions {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                }), cancellationToken);
            }

            // JSONファイルに在庫数を書き込む。JSON内部の最終更新時刻の方が新しい場合は上書きしない。
            // 実際の外部システムではAPIコールなどを行う想定。
            // 並列での更新に備えて数回のリトライを行う。
            const int MAX_RETRY = 5;
            for (int i = 0; i < MAX_RETRY; i++) {
                try {
                    // 読み書きモードで開く
                    using var fs = new FileStream(mockDataPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    using var reader = new StreamReader(fs); // Leave stream open for writing sequence

                    var jsonString = await reader.ReadToEndAsync(cancellationToken);
                    var externalData = JsonSerializer.Deserialize<MockJsonDataType>(jsonString) ?? new MockJsonDataType();

                    if (externalData.Write.TryGetValue(externalProductId, out var value)) {
                        if (value.UpdatedAt > _app.CurrentTime) {
                            // より新しいデータがある場合はスキップ
                            _app.Log.LogWarning("商品管理システム（モック） より新しいデータがあるので更新をスキップ: ID={externalProductId}, Current={currentTime}, Existing={existingUpdatedAt}",
                                externalProductId,
                                _app.CurrentTime,
                                value.UpdatedAt);
                            return;
                        }
                    } else {
                        value = new QuantityUpdateInterfaceType();
                        externalData.Write[externalProductId] = value;
                    }

                    value.NewQuantity = newStockQuantity;
                    value.UpdatedAt = _app.CurrentTime;

                    // 先頭に戻して書き込み
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);

                    await JsonSerializer.SerializeAsync(fs, externalData, new JsonSerializerOptions {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        WriteIndented = true,
                    }, cancellationToken);

                    _app.Log.LogInformation("商品管理システム（モック） 在庫数を更新しました: ID={externalProductId}, NewQuantity={newStockQuantity}, UpdatedAt={currentTime}",
                        externalProductId,
                        newStockQuantity,
                        _app.CurrentTime);

                    return;

                } catch (IOException) {
                    if (i == MAX_RETRY - 1) throw;
                    await Task.Delay(100, cancellationToken);
                }
            }
        });
    }

    private class MockJsonDataType {
        /// <summary>
        /// 参照側IF
        /// </summary>
        public List<ProductImportData> Read { get; set; } = [];
        /// <summary>
        /// 更新側IF。辞書のキーは外部システム側の商品ID。
        /// </summary>
        public Dictionary<string, QuantityUpdateInterfaceType> Write { get; set; } = [];
    }
}
