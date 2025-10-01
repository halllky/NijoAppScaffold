
using System.Collections.Generic;

namespace Nijo.CodeGenerating;

/// <summary>
/// C# と TypeScript の両方で登場する構造体。
///
/// <see cref="Nijo.Parts.Common.PlainPresentationObject"/> と役割重複。
/// </summary>
public interface IPresentationLayerStructure {
    /// <summary>C#クラス名</summary>
    string CsClassName { get; }
    /// <summary>TypeScript型名</summary>
    string TsTypeName { get; }
    /// <summary>構造体のメンバーを取得します。</summary>
    IEnumerable<IInstancePropertyMetadata> GetMembers();
}

/// <summary>
/// <see cref="IPresentationLayerStructure"/> かつ新規オブジェクト作成関数がレンダリングされるもの
/// </summary>
public interface ICreatablePresentationLayerStructure : IPresentationLayerStructure {
    /// <summary>TypeScriptの新規オブジェクト作成関数の名前</summary>
    string TsNewObjectFunction { get; }
    /// <summary>TypeScriptの新規オブジェクト作成関数のリテラル部分をレンダリングします。</summary>
    string RenderTsNewObjectFunctionBody();
}
