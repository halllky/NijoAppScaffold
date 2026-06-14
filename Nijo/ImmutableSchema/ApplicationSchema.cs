using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.ImmutableSchema;

/// <summary>
/// アプリケーションスキーマ。
/// </summary>
public class ApplicationSchema {

    internal ApplicationSchema(XDocument xDocument, SchemaParseContext parseContext) {
        _xDocument = xDocument;
        _parseContext = parseContext;
    }
    private readonly XDocument _xDocument;
    private readonly SchemaParseContext _parseContext;

    /// <summary>
    /// このスキーマで定義されているルート集約を返します。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<RootAggregate> GetRootAggregates() {
        var rootAggregateElements = _xDocument
            .Root
            ?.Elements()
            .Where(el => el.Name.LocalName != SchemaParseContext.SECTION_CUSTOM_ATTRIBUTES
                      && el.Name.LocalName != SchemaParseContext.SECTION_GENERIC_LOOKUP_TABLES)
            .SelectMany(el => el.Elements())
            ?? [];

        foreach (var xElement in rootAggregateElements) {
            var aggregate = _parseContext.ToAggregateBase(xElement, null);
            if (aggregate is not RootAggregate rootAggregate) {
                throw new InvalidOperationException();
            }
            yield return rootAggregate;
        }
    }

}
