
using System.Collections.Generic;

namespace Nijo.CodeGenerating;

/// <summary>
/// C# と TypeScript の両方で登場する構造体。
/// </summary>
public interface IPresentationLayerStructure {
    /// <summary>C#クラス名</summary>
    string CsClassName { get; }
    /// <summary>TypeScript型名</summary>
    string TsTypeName { get; }
    /// <summary>構造体のメンバーを取得します。</summary>
    IEnumerable<IInstancePropertyMetadata> GetMembers();
    /// <summary>TypeScriptの新規オブジェクト作成関数のリテラル部分をレンダリングします。</summary>
    string RenderTsNewObjectFunctionBody();
}

/// <summary>
/// <see cref="IPresentationLayerStructure"/> かつ新規オブジェクト作成関数がレンダリングされるもの
/// </summary>
public interface ICreatablePresentationLayerStructure : IPresentationLayerStructure {
    /// <summary>TypeScriptの新規オブジェクト作成関数の名前</summary>
    string TsNewObjectFunction { get; }
}
