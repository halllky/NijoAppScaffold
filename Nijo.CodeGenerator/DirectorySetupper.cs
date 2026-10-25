using System;

namespace Nijo.CodeGenerator;

/// <summary>
/// ソースコード自動生成処理を、生成されるディレクトリ構造と同じ入れ子構造で直感的に書けるようにするためのクラス
/// </summary>
public class DirectorySetupper {
    internal static void StartSetup(OutputFiles outputFiles, Func<SourceFile, SourceFileFormat> getFormat, string absolutePath, Action<DirectorySetupper> fn) {
        var setupper = new DirectorySetupper(outputFiles, getFormat, absolutePath);
        setupper.Directory("", fn);
    }
    private DirectorySetupper(OutputFiles outputFiles, Func<SourceFile, SourceFileFormat> getFormat, string path) {
        Path = path;
        _outputFiles = outputFiles;
        _getFormat = getFormat;
    }

    internal string Path { get; }

    private readonly OutputFiles _outputFiles;
    private readonly Func<SourceFile, SourceFileFormat> _getFormat;

    /// <summary>
    /// このディレクトリからの相対パスで指定されたディレクトリを作成し、その中にファイルやディレクトリを生成します。
    /// </summary>
    public void Directory(string relativePath, Action<DirectorySetupper> fn) {
        var fullpath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Path, relativePath));
        if (!System.IO.Directory.Exists(fullpath))
            System.IO.Directory.CreateDirectory(fullpath);
        _outputFiles.Handle(fullpath);

        fn(new DirectorySetupper(_outputFiles, _getFormat, fullpath));
    }

    /// <summary>
    /// このディレクトリにファイルを生成します。
    /// 1回のソースコード自動生成の中で同じファイルを2回以上生成することはできません。
    /// 既存のファイルと内容が全く同じ場合、そのファイルは更新されません。
    /// </summary>
    public void Generate(SourceFile sourceFile) {
        var fullpath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Path, sourceFile.FileName));

        if (!_outputFiles.TryHandle(fullpath)) {
            throw new InvalidOperationException($"同じファイルを2回以上レンダリングしようとしました: {fullpath}");
        }

        var dir = System.IO.Path.GetDirectoryName(fullpath);
        if (dir != null && !System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

        sourceFile.Render(fullpath, _getFormat(sourceFile));
    }
}
