using System.Linq;
using Nijo.ImmutableSchema;
using Nijo.Util.DotnetEx;

namespace Nijo.Parts.JavaScript;

/// <summary>
/// ルート集約1つにつき1ファイル生成される TypeScript のモジュール。
/// 利用側では "import * as ルート集約名 from '...'" の形で読み込まれることを前提とし、
/// モジュール内で export されるシンボルの名前にはルート集約の名前を含めない。
/// </summary>
internal class TypeScriptAggregateModule {
    internal TypeScriptAggregateModule(AggregateBase aggregate) {
        _rootAggregate = aggregate.GetRoot();
    }
    private readonly RootAggregate _rootAggregate;

    /// <summary>このモジュールが出力されるディレクトリの名前</summary>
    internal const string DIRECTORY = "models";

    /// <summary>このモジュールのファイル名（拡張子なし）</summary>
    internal string FileNameWithoutExtension => _rootAggregate.PhysicalName.ToFileNameSafe();
    /// <summary>"import * as" で読み込む際の別名</summary>
    internal string ImportAlias => _rootAggregate.PhysicalName;

    /// <summary>
    /// このモジュールを、同じディレクトリに出力される別のモジュールから読み込むimport文をレンダリングします。
    /// </summary>
    internal string RenderImportFromSibling(bool typeOnly = false) {
        return $"import {(typeOnly ? "type " : "")}* as {ImportAlias} from \"./{FileNameWithoutExtension}\"";
    }
    /// <summary>
    /// このモジュールを、自動生成ディレクトリ直下のモジュールから読み込むimport文をレンダリングします。
    /// </summary>
    internal string RenderImportFromRoot(bool typeOnly = false) {
        return $"import {(typeOnly ? "type " : "")}* as {ImportAlias} from \"./{DIRECTORY}/{FileNameWithoutExtension}\"";
    }

    /// <summary>
    /// このモジュールのシンボルを他のモジュールから参照するときの名前を返します。
    /// </summary>
    internal string Qualify(string exportName) {
        return $"{ImportAlias}.{exportName}";
    }
    /// <summary>
    /// このモジュールのシンボルを、指定の集約のモジュールから参照するときの名前を返します。
    /// 同じモジュール内からの参照であれば修飾しません。
    /// </summary>
    internal string QualifyFrom(AggregateBase referrer, string exportName) {
        return referrer.GetRoot() == _rootAggregate
            ? exportName
            : Qualify(exportName);
    }

    /// <summary>
    /// 集約ごとに定義されるシンボルの、モジュール内での名前を返します。
    /// ルート集約ならば基本名そのまま、子孫集約ならば基本名の後ろにルート集約からのパスを付したものになります。
    /// </summary>
    /// <param name="aggregate">集約</param>
    /// <param name="baseName">基本名</param>
    internal static string GetExportName(AggregateBase aggregate, string baseName) {
        if (aggregate is RootAggregate) return baseName;

        var path = aggregate
            .EnumerateThisAndAncestors()
            .Where(agg => agg is not RootAggregate)
            .Select(agg => agg.PhysicalName);
        return $"{baseName}_{path.Join("_")}";
    }
}
