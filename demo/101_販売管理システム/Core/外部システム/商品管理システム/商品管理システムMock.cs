using System.Text.Json;

namespace MyApp.Core.外部システム.商品管理システム;

public class 商品管理システムMock : I商品管理システム {

    public 商品管理システムMock(RuntimeSetting settings, NLog.ILogger log, OverridedApplicationConfigure config) {
        Settings = settings;
        Log = log;
        Config = config;
    }

    private RuntimeSetting Settings { get; }
    private NLog.ILogger Log { get; }
    private OverridedApplicationConfigure Config { get; }

    public IEnumerable<ProductImportData> Enumerate商品データ() {
        // 設定ファイルからモックデータのパスを取得
        var mockDataPath = Settings.商品管理システム.MockJsonPath;
        if (string.IsNullOrWhiteSpace(mockDataPath)) {
            throw new InvalidOperationException("商品データ取込のモックデータパスが設定されていません。");
        }

        // パスが相対パスの場合は絶対パスに変換
        if (!Path.IsPathRooted(mockDataPath)) {
            mockDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, mockDataPath);
        }

        // モックファイルが無い場合はサンプルを作成
        if (!File.Exists(mockDataPath)) {
            File.WriteAllText(mockDataPath, Config.ToJson(new[] {
                new ProductImportData {
                    ExternalId = "EX12345",
                    Name = "サンプル商品A",
                    Price = 1000,
                    TaxType = "課税対象",
                },
                new ProductImportData {
                    ExternalId = "EX67890",
                    Name = "サンプル商品B",
                    Price = 2000,
                    TaxType = "非課税",
                },
            }));
        }

        Log.Info($"商品データ取込を開始します。読み込み元: {mockDataPath}");

        // JSONファイルを読み込む
        var jsonString = File.ReadAllText(mockDataPath);
        var externalData = JsonSerializer.Deserialize<List<ProductImportData>>(jsonString);

        if (externalData == null) {
            throw new InvalidOperationException("モックデータが空、または不正なフォーマットです。");
        }

        return externalData;
    }
}
