
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : ConstantsSection.cs
// ※ このファイルを生成したメソッド: RenderRootAggregate

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
    /// メッセージ
    /// </summary>
    public static class Messages {
        /// <summary>
        /// 成功メッセージ
        /// </summary>
        public static string Success() => $"正常に処理されました";
        /// <summary>
        /// エラーメッセージ
        /// <para>
        /// コロンの後ろには開発者向けの情報ではなくアプリケーションのユーザーに向けた文字にすること。
        /// </para>
        /// </summary>
        public static string Error(string arg0) => $"エラーが発生しました: {arg0}";
        /// <summary>
        /// バリデーションエラー
        /// <para>
        /// プレースホルダーが複数ある場合のテスト
        /// </para>
        /// </summary>
        public static string ValidationError(string arg0, string arg1, string arg2) => $"{arg0}フィールドで{arg1}エラーが発生しました。詳細: {arg2}";
        /// <summary>
        /// メンテナンスメッセージ
        /// <para>
        /// プレースホルダーが0個の場合のテスト
        /// </para>
        /// </summary>
        public static string SystemMaintenance() => $"システムメンテナンス中です。しばらくお待ちください。";
    }
    /// <summary>
    /// API設定
    /// </summary>
    public static class API {
        /// <summary>
        /// APIベースURL
        /// </summary>
        public const string BaseUrl = "https://api.example-medical-test.local.example.com";
        /// <summary>
        /// 認証ヘッダー
        /// <para>
        /// 特殊文字のテスト
        /// </para>
        /// </summary>
        public static string AuthHeader(string arg0) => $"Bearer {arg0}";
        /// <summary>
        /// JSONフォーマット
        /// <para>
        /// 引用符を含む文字列のテスト
        /// </para>
        /// </summary>
        public static string JsonFormat(string arg0, string arg1) => $"{{\"message\": \"{arg0}\", \"timestamp\": \"{arg1}\"}}";
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
