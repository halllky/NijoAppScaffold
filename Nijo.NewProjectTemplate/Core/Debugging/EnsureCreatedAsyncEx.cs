using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Debugging;

/// <summary>
/// DbContextの拡張メソッド
/// </summary>
public static class DbContextExtensions {
    /// <summary>
    /// データベースを作成し、指定されたフォルダ内のSQLスクリプトを実行します。
    /// マイグレーションSQLが事前にファイルとして存在していることを前提としています。
    /// マイグレーションSQLはTaskフォルダにある「ADD_MIGRATION.bat」を実行して作成してください（Windowsの場合）。
    /// </summary>
    /// <param name="context">DbContextのインスタンス</param>
    /// <param name="runtimeSetting">実行時設定</param>
    /// <returns>データベースが新規作成されたかどうか</returns>
    public static async Task<bool> EnsureCreatedAsyncEx<T>(this T context, RuntimeSetting runtimeSetting) where T : DbContext {
        // データベースファイルの存在確認と作成（テーブルは作成しない）
        // デモでは SQLite を前提としているので、以下は SQLite 専用の処理である点に注意。
        var created = false;
        var connectionString = new SqliteConnectionStringBuilder(context.Database.GetConnectionString());
        if (connectionString.DataSource is null) {
            throw new InvalidOperationException("設定ファイルでSQLiteのファイルパスが指定されていません。");
        }

        if (!File.Exists(connectionString.DataSource)) {
            // 空のデータベースファイルを作成。
            // SQLiteでは0バイトのファイルがあればデータベースとして認識される。
            File.WriteAllBytes(connectionString.DataSource, new byte[0]);
        }

        // SQLスクリプトのフォルダパスを取得
        string scriptFolder = Path.GetFullPath(runtimeSetting.MigrationsScriptFolder);
        if (string.IsNullOrEmpty(scriptFolder) || !Directory.Exists(scriptFolder)) {
            // フォルダが存在しない場合は処理を中断
            return created;
        }

        // .sqlファイルをファイル名の昇順で取得
        var sqlFiles = Directory.GetFiles(scriptFolder, "*.sql")
            .OrderBy(file => Path.GetFileName(file))
            .ToList();

        // 各SQLファイルを実行
        foreach (var sqlFile in sqlFiles) {
            // ファイルの内容を読み込む（エンコード：BOMありUTF-8）
            string sql = await File.ReadAllTextAsync(sqlFile, new UTF8Encoding(true));

            // SQLを実行
            await context.Database.ExecuteSqlRawAsync(sql);
        }

        return created;
    }
}
