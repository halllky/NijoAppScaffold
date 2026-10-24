using Nijo.CodeGenerating;
using Nijo.DataStructures.Structure;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Nijo.DataStructures.Aggregates;

namespace Nijo.Commands;

/// <summary>
/// Commands セクションに定義されたコマンド1個。
/// 引数と戻り値の型は、<see cref="ParameterOption"/>, <see cref="ReturnValueOption"/> 属性で指定されたデータ構造から決まる。
/// </summary>
public sealed class CommandRoot : SectionRoot {
    internal CommandRoot(XElement xElement, SchemaParseContext ctx) : base(xElement, ctx) { }

    /// <summary>
    /// このコマンドの引数の型を返します。引数が無い場合は null を返します。
    /// </summary>
    public ICreatablePresentationLayerStructure? GetParameterStructure() {
        var parameter = XElement.Attribute(ParameterOption.ATTRIBUTE_NAME);
        return parameter == null ? null : GetTargetStructure(parameter);
    }
    /// <summary>
    /// このコマンドの戻り値の型を返します。戻り値が無い場合は null を返します。
    /// </summary>
    public ICreatablePresentationLayerStructure? GetReturnValueStructure() {
        var returnValue = XElement.Attribute(ReturnValueOption.ATTRIBUTE_NAME);
        return returnValue == null ? null : GetTargetStructure(returnValue);
    }

    private ICreatablePresentationLayerStructure GetTargetStructure(XAttribute attribute) {
        var splitted = attribute.Value.Split(':');
        var targetPhysicalName = splitted[0];
        var targetXElement = _ctx.Document.Root
            ?.Element(SchemaParseContext.SECTION_DATA_STRUCTURES)
            ?.Element(targetPhysicalName)
            ?? throw new InvalidOperationException($"対象の集約が見つかりません: {targetPhysicalName}");

        var targetRootAggregate = (RootAggregate)_ctx.ToAggregateBase(targetXElement, null);
        if (splitted.Length == 1) {
            // コマンドの典型的な利用シーンとして「詳細画面の初期表示データ取得」があるので
            // 戻り値であっても常にDisplayDataを返したほうがよいのか？うーん…
            // return isParameter
            //     ? new StructureDisplayData(targetRootAggregate)
            //     : new PlainStructure(targetRootAggregate);

            return new StructureDisplayData(targetRootAggregate);

        } else if (CommandTypeSpecificationOption.AvailableFromCommandToQuery.TryGetValue(splitted[1], out var factory)) {
            return factory(targetRootAggregate);

        } else {
            throw new InvalidOperationException($"不正な参照先の種類: {splitted[1]}");
        }
    }

    /// <summary>
    /// 引数のデータ構造が、いずれかのコマンドの <see cref="ParameterOption"/> に指定されているかどうかを返します。
    /// </summary>
    internal static bool IsReferredAsParameter(RootAggregate dataStructure) {
        return IsReferredBy(dataStructure, ParameterOption.ATTRIBUTE_NAME);
    }
    /// <summary>
    /// 引数のデータ構造が、いずれかのコマンドの <see cref="ReturnValueOption"/> に指定されているかどうかを返します。
    /// </summary>
    internal static bool IsReferredAsReturnValue(RootAggregate dataStructure) {
        return IsReferredBy(dataStructure, ReturnValueOption.ATTRIBUTE_NAME);
    }
    private static bool IsReferredBy(RootAggregate dataStructure, string attributeName) {
        var commandElements = dataStructure.XElement.Document?.Root
            ?.Element(SchemaParseContext.SECTION_COMMANDS)
            ?.Elements()
            ?? [];
        return commandElements.Any(command => command.Attribute(attributeName)?.Value.Split(':')[0] == dataStructure.PhysicalName);
    }
}
