using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Nijo.CodeGenerator;

/// <summary>
/// ソースコード自動生成のコンテキスト。
/// どのような入力からどのようなソースコードを生成するかは関知せず、
/// ファイルの出力、共有ソースファイルの管理、生成されなかった既存ファイルの削除のみを担う。
/// 生成処理が参照する情報（入力データや出力先のルールなど）や、出力ファイルの形式は派生クラスで定義する。
/// </summary>
/// <typeparam name="TSelf">派生クラス自身の型。共有ソースファイルが派生クラスの情報を参照できるようにするために用いる。</typeparam>
public abstract class CodeRenderingContext<TSelf> where TSelf : CodeRenderingContext<TSelf> {

    /// <param name="managedDirectories">
    /// 自動生成されたファイルのみが置かれるディレクトリ。
    /// 生成処理の中で生成されなかったファイルやディレクトリは、このディレクトリの中から削除される。
    /// </param>
    protected CodeRenderingContext(IEnumerable<string> managedDirectories) {
        _managedDirectories = managedDirectories.ToArray();
    }
    private readonly string[] _managedDirectories;
    private readonly OutputFiles _outputFiles = new();

    /// <summary>
    /// ソースコード自動生成を実行します。
    /// 1つのインスタンスにつき1回だけ実行できます。
    /// </summary>
    /// <param name="define">
    /// ソースコードを生成し、共有ソースファイルの内容を登録する処理。
    /// </param>
    public void Run(Action<TSelf> define) {
        if (_isRun) throw new InvalidOperationException("ソースコード自動生成は1つのインスタンスにつき1回だけ実行できます。");
        _isRun = true;

        var self = (TSelf)this;
        define(self);
        RenderSharedSourceFiles(self);
        _outputFiles.DeleteUnhandled(_managedDirectories);
    }
    private bool _isRun;

    /// <summary>
    /// 指定されたディレクトリにファイルやディレクトリを生成します。
    /// </summary>
    public void Directory(string path, Action<DirectorySetupper> setup) {
        DirectorySetupper.StartSetup(_outputFiles, GetFormat, path, setup);
    }

    /// <summary>
    /// ファイルをどのような形式（文字コード、改行コード、ファイル先頭の注意書きなど）で出力するかを決めます。
    /// </summary>
    protected abstract SourceFileFormat GetFormat(SourceFile sourceFile);


    #region 共有ソースファイル
    /// <summary>
    /// 共有ソースファイルを返します。
    /// 型ごとに初回呼び出し時にインスタンスが作成され、2回目以降は同じインスタンスが返されます。
    /// </summary>
    public T Use<T>() where T : ISharedSourceFile<TSelf>, new() {
        lock (_sharedLock) {
            if (_sharedSourceFilesFixed) {
                throw new InvalidOperationException($"{typeof(T).Name}: 共有ソースファイルのレンダリング開始後に新たな共有ソースファイルを使用することはできません。");
            }
            var instance = _sharedSourceFiles.OfType<T>().SingleOrDefault();
            if (instance == null) {
                instance = new T();
                _sharedSourceFiles.Add(instance);
            }
            return instance;
        }
    }

    private void RenderSharedSourceFiles(TSelf self) {
        // 共有ソースファイルが別の共有ソースファイルに依存することがあるので、
        // 一度でも登場した共有ソースファイルについて、それぞれ必ず1回ずつ依存関係登録メソッドを呼ぶ
        var handled = new HashSet<ISharedSourceFile<TSelf>>();
        while (true) {
            ISharedSourceFile<TSelf>[] unhandled;
            lock (_sharedLock) {
                unhandled = _sharedSourceFiles.Where(src => !handled.Contains(src)).ToArray();
            }
            if (unhandled.Length == 0) break;

            foreach (var src in unhandled) {
                src.RegisterDependencies(self);
                handled.Add(src);
            }
        }

        // レンダリング中に新たな共有ソースファイルが登録されるとそれがレンダリングされないので、以降の登録を禁止する
        lock (_sharedLock) {
            _sharedSourceFilesFixed = true;
        }

        foreach (var src in _sharedSourceFiles) {
            try {
                src.Render(self);
            } catch (Exception ex) {
                throw new InvalidOperationException($"{src.GetType().Name}のレンダリングで例外が発生", ex);
            }
        }
    }

    private readonly Lock _sharedLock = new();
    private readonly List<ISharedSourceFile<TSelf>> _sharedSourceFiles = [];
    private bool _sharedSourceFilesFixed;
    #endregion 共有ソースファイル
}
