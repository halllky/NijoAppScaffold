using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Nijo.CodeGenerator;

/// <summary>
/// 1回のソースコード自動生成の中で生成されたファイルやディレクトリの記録。
/// </summary>
internal sealed class OutputFiles {
    private readonly HashSet<string> _handled = [];
    private readonly Lock _lock = new();

    /// <summary>
    /// 生成されたことを記録します。
    /// </summary>
    internal void Handle(string fullpath) {
        lock (_lock) {
            _handled.Add(Path.GetFullPath(fullpath));
        }
    }
    /// <summary>
    /// 生成されたことを記録します。既に記録されていた場合は false を返します。
    /// </summary>
    internal bool TryHandle(string fullpath) {
        lock (_lock) {
            return _handled.Add(Path.GetFullPath(fullpath));
        }
    }

    /// <summary>
    /// 特定のディレクトリ直下に生成されたファイルの一覧を返します。
    /// </summary>
    internal IEnumerable<string> GetFilesIn(string directory) {
        lock (_lock) {
            return _handled.Where(path => directory == Path.GetDirectoryName(path)).ToArray();
        }
    }

    /// <summary>
    /// 生成されていないファイルやディレクトリを、指定されたディレクトリの中から削除します。
    /// 指定されたディレクトリ自体は削除しません。
    /// </summary>
    internal void DeleteUnhandled(IEnumerable<string> directories) {
        var existingDirectories = directories.Where(Directory.Exists).ToArray();

        foreach (var file in existingDirectories.SelectMany(dir => Directory.GetFiles(dir, "*", SearchOption.AllDirectories)).ToArray()) {
            if (_handled.Contains(Path.GetFullPath(file))) continue;
            if (!File.Exists(file)) continue;
            File.Delete(file);
        }

        foreach (var dir in existingDirectories.SelectMany(dir => Directory.GetDirectories(dir, "*", SearchOption.AllDirectories)).ToArray()) {
            if (_handled.Contains(Path.GetFullPath(dir))) continue;
            if (!Directory.Exists(dir)) continue;
            Directory.Delete(dir, true);
        }
    }
}
