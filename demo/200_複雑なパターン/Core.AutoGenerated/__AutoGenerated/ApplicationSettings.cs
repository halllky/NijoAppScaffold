
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : ConstantsSection.cs
// ※ このファイルを生成したメソッド: RenderRootAggregate
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

namespace MyApp;

/// <summary>
/// アプリケーション設定
/// <para>
/// アプリケーション設定の定数定義
/// </para>
/// </summary>
public static class ApplicationSettings {
    /// <summary>
    /// アプリケーション名
    /// <para>
    /// クライアント側のトップページの表示に利用。
    /// </para>
    /// </summary>
    public const string AppName = "医療機器管理システム";
    /// <summary>
    /// バージョン
    /// <para>
    /// csprojファイルのバージョン番号と合わせること。
    /// </para>
    /// </summary>
    public const string Version = "1.0.0";
    /// <summary>
    /// 最大リトライ回数
    /// </summary>
    public const int MaxRetryCount = 3;
    /// <summary>
    /// タイムアウト秒数
    /// </summary>
    public const decimal TimeoutSeconds = 30.5m;
    /// <summary>
    /// データベース設定
    /// </summary>
    public static class Database {
        /// <summary>
        /// 接続タイムアウト
        /// </summary>
        public const int ConnectionTimeout = 30;
        /// <summary>
        /// コマンドタイムアウト
        /// </summary>
        public const int CommandTimeout = 60;
        /// <summary>
        /// 最大接続数
        /// </summary>
        public const int MaxConnections = 100;
    }
    /// <summary>
    /// API設定
    /// </summary>
    public static class API {
        /// <summary>
        /// APIベースURL
        /// </summary>
        public const string BaseUrl = "https://api.example-medical-test.local.example.com";
    }
    /// <summary>
    /// UI設定
    /// </summary>
    public static class UI {
        /// <summary>
        /// ページサイズ
        /// </summary>
        public const int PageSize = 20;
        /// <summary>
        /// 最大ファイルサイズ(MB)
        /// </summary>
        public const decimal MaxFileSize = 10.5m;
        /// <summary>
        /// カラー設定
        /// </summary>
        public static class Colors {
            /// <summary>
            /// プライマリカラー
            /// </summary>
            public const string Primary = "#007bff";
            /// <summary>
            /// セカンダリカラー
            /// </summary>
            public const string Secondary = "#6c757d";
            /// <summary>
            /// 成功カラー
            /// </summary>
            public const string Success = "#28a745";
            /// <summary>
            /// 警告カラー
            /// </summary>
            public const string Warning = "#ffc107";
            /// <summary>
            /// エラーカラー
            /// </summary>
            public const string Error = "#dc3545";
        }
    }
}
