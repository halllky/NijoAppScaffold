using Nijo.Util.CodeGenerating;

namespace Nijo.Parts {
    internal class RuntimeSettings {

        internal static string ServerSetiingTypeFullName => $"{nameof(RuntimeSettings)}.{SERVER}";

        private const string SERVER = "Server";

        internal const string TO_JSON = "ToJson";
        internal const string GET_DEFAULT = "GetDefault";
        internal const string GET_ACTIVE_CONNSTR = "GetActiveConnectionString";

        internal static SourceFile RenderInterface() {
            return new SourceFile {
                FileName = "IRuntimeSetting.cs",
                RenderContent = ctx => {
                    return $$"""
                        namespace {{ctx.Config.RootNamespace}};

                        /// <summary>
                        /// 自動生成されたソースの中で実行時設定に依存する箇所があるところ、その実行時設定のデータ構造。
                        /// </summary>
                        public interface IRuntimeSetting {
                            /// <summary>
                            /// 現在接続中のDBの名前。 <see cref="DbProfiles"/> のいずれかのキーと一致
                            /// </summary>
                            string? CurrentDb { get; }

                            /// <summary>
                            /// 接続可能なデータベースの接続情報の一覧。
                            /// 開発時はこのリストの中から随時接続先を切り替えながら開発していく。
                            /// </summary>
                            List<DbProfile> DbProfiles { get; }

                            /// <summary>
                            /// <see cref="CurrentDb"/> で設定されている設定から接続文字列を返します。
                            /// </summary>
                            DbProfile? GetCurrentDbProfile() {
                                if (string.IsNullOrWhiteSpace(CurrentDb)) return null;
                                return DbProfiles.FirstOrDefault(profile => profile.Name == CurrentDb);
                            }
                        }

                        /// <summary>
                        /// <see cref="IRuntimeSetting"/> の中のDB接続情報
                        /// </summary>
                        public class DbProfile {
                            public string Name { get; set; } = string.Empty;
                            public string ConnStr { get; set; } = string.Empty;
                            public E_RDB? RDBMS { get; set; }
                            public string DbName { get; set; } = string.Empty;
                        }
                        public enum E_RDB {
                            SQLite,
                            Oracle,
                        }
                        """;
                },
            };
        }

        internal static SourceFile Render(CodeRenderingContext ctx) => new SourceFile {
            FileName = $"RuntimeSettings.cs",
            RenderContent = context => $$"""
                namespace {{ctx.Config.RootNamespace}} {
                    using System.Text.Json;
                    using System.Text.Json.Serialization;

                    public static partial class RuntimeSettings {

                        /// <summary>
                        /// 実行時クライアント側設定
                        /// </summary>
                        public class Client {
                            [JsonPropertyName("server")]
                            public string? ApServerUri { get; set; }
                        }

                        /// <summary>
                        /// 実行時サーバー側設定。機密情報を含んでよい。
                        /// 本番環境ではサーバー管理者のみ閲覧編集可能、デバッグ環境では画面から閲覧編集可能。
                        /// </summary>
                        public partial class {{SERVER}} {

                            /// <summary>
                            /// appsettings.json にはこのアプリケーションの設定値以外にもログライブラリの設定など色々な項目が含まれるところ、
                            /// このアプリケーションの設定値が定義されるセクションの名前
                            /// </summary>
                            public const string APP_SETTINGS_SECTION_NAME = "Nijo";

                            /// <summary>
                            /// ログ出力先ディレクトリ
                            /// </summary>
                            public string? LogDirectory { get; set; }
                            /// <summary>
                            /// アップロードファイル保存先ディレクトリ
                            /// </summary>
                            public string? UploadedFileDir { get; set; }
                            /// <summary>
                            /// バッチ実行結果出力先ディレクトリ
                            /// </summary>
                            public string? JobDirectory { get; set; }

                            /// <summary>
                            /// 再発行時共通パスワード
                            /// </summary>
                            public string? InitPassword { get; set; }

                            #region 通知メール設定
                            /// <summary>
                            /// メールサーバ ホスト名
                            /// </summary>
                            public string? Host { get; set; }
                            /// <summary>
                            /// メールサーバ ポート番号
                            /// </summary>
                            public int? Port { get; set; }

                            /// <summary>
                            /// 基幹連携システムから送信されたメールの送信元アドレス
                            /// </summary>
                            public string? SystemMailAddress { get; set; }
                            /// <summary>
                            /// 基幹連携システムから送信されたメールの送信元名称
                            /// </summary>
                            public string? SystemName { get; set; }
                            #endregion 通知メール設定

                            /// <summary>
                            /// バックグラウンド処理に関する設定
                            /// </summary>
                            public BackgroundTaskSetting BackgroundTask { get; set; } = new();
                            public class BackgroundTaskSetting {
                                /// <summary>
                                /// ポーリング間隔（ミリ秒）
                                /// </summary>
                                public int PollingSpanMilliSeconds { get; set; } = 5000;
                            }


                            public string {{GET_ACTIVE_CONNSTR}}() {
                                if (string.IsNullOrWhiteSpace(CurrentDb))
                                    throw new InvalidOperationException({{MessageConst.CS_CLASS_NAME}}.{{MessageConst.C_INF0051}}());

                                var db = DbProfiles.FirstOrDefault(db => db.Name == CurrentDb);
                                if (db == null) throw new InvalidOperationException({{MessageConst.CS_CLASS_NAME}}.{{MessageConst.C_INF0052}}(CurrentDb));

                                return db.ConnStr;
                            }
                            public string {{TO_JSON}}() {
                                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions {
                                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
                                    WriteIndented = true,
                                });
                                json = json.Replace("\\u0022", "\\\""); // ダブルクォートを\u0022ではなく\"で出力したい

                                return json;
                            }

                            /// <summary>
                            /// 既定の実行時設定を返します。
                            /// </summary>
                            public static {{SERVER}} {{GET_DEFAULT}}() {
                                var connStr = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();
                                connStr.DataSource = "../DEBUG.sqlite3";
                                connStr.Pooling = false; // デバッグ終了時にshm, walファイルが残らないようにするため

                                return new {{SERVER}} {
                                    LogDirectory = "log",
                                    UploadedFileDir = "uploaded-files",
                                    JobDirectory = "job",
                                    InitPassword = "0000",
                                    CurrentDb = "SQLITE",
                                    DbProfiles = new List<DbProfile> {
                                        new DbProfile { RDBMS = 0, Name = "SQLITE", ConnStr = connStr.ToString() },
                                    },
                                };
                            }
                        }
                    }
                }
                """,
        };
    }
}
