
namespace Nijo.CodeGenerating;

/// <summary>
/// C# と TypeScript の両方で登場する構造体。
/// </summary>
public interface IPresentationLayerStructure {
    /// <summary>C#クラス名</summary>
    string CsClassName { get; }
    /// <summary>TypeScript型名</summary>
    string TsTypeName { get; }
    /// <summary>TypeScriptの新規オブジェクト作成関数の名前</summary>
    string TsNewObjectFunction { get; }
}
