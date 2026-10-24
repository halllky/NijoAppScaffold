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
    /// データ構造（IsStored, IsQueryable 属性の有無を問わない）のルート集約を、スキーマ定義上の登場順に返します。
    /// </summary>
    public IEnumerable<RootAggregate> GetDataStructures() {
        return GetRootAggregates(E_Section.DataStructures);
    }
    /// <summary>
    /// コマンドのルート集約を、スキーマ定義上の登場順に返します。
    /// </summary>
    public IEnumerable<RootAggregate> GetCommands() {
        return GetRootAggregates(E_Section.Commands);
    }
    /// <summary>
    /// 静的区分のルート集約を、スキーマ定義上の登場順に返します。
    /// </summary>
    public IEnumerable<RootAggregate> GetStaticEnums() {
        return GetRootAggregates(E_Section.StaticEnums);
    }
    /// <summary>
    /// 値オブジェクトのルート集約を、スキーマ定義上の登場順に返します。
    /// </summary>
    public IEnumerable<RootAggregate> GetValueObjects() {
        return GetRootAggregates(E_Section.ValueObjects);
    }
    /// <summary>
    /// 定数のルート集約を、スキーマ定義上の登場順に返します。
    /// </summary>
    public IEnumerable<RootAggregate> GetConstants() {
        return GetRootAggregates(E_Section.Constants);
    }

    private IEnumerable<RootAggregate> GetRootAggregates(E_Section section) {
        var rootAggregateElements = _xDocument
            .Root
            ?.Element(section.ToXmlElementName())
            ?.Elements()
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
