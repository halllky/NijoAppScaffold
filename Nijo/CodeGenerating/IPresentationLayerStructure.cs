
using System.Collections.Generic;

namespace Nijo.CodeGenerating;

/// <summary>
/// C# と TypeScript の両方で登場する構造体。
/// </summary>
public interface IPresentationLayerStructure {
    /// <summary>C#クラス名</summary>
    string CsClassName { get; }
    /// <summary>
    /// TypeScript型名
    /// TODO: TypeScriptでは必ずしも型に名前をつける必要がない。アドホックな型は型名が無い。
    ///       ITypeScriptNamedStructure のように別のインターフェースが必要
    /// </summary>
    string TsTypeName { get; }
    /// <summary>構造体のメンバーを取得します。</summary>
    IEnumerable<IInstancePropertyMetadata> GetMembers();

    //                 ほんとはGetMembersの戻り値をこっち↓にしたい
    /// <summary>
    /// 構造体のメンバー
    /// </summary>
    public interface IMember : IInstancePropertyMetadata {
        string PhysicalName { get; }
        string GetTypeName(E_CsTs csts);
        string RenderDeclaringCSharp();
        string RenderDeclaringTypeScript();
    }
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
