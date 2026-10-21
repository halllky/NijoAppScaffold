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
        return GetRootAggregates(E_RootAggregateKind.Stored, E_RootAggregateKind.QueryableOnly, E_RootAggregateKind.Structure);
    }
    /// <summary>
    /// コマンドのルート集約を、スキーマ定義上の登場順に返します。
    /// </summary>
    public IEnumerable<RootAggregate> GetCommands() {
        return GetRootAggregates(E_RootAggregateKind.Command);
    }
    /// <summary>
    /// 静的区分のルート集約を、スキーマ定義上の登場順に返します。
    /// </summary>
    public IEnumerable<RootAggregate> GetStaticEnums() {
        return GetRootAggregates(E_RootAggregateKind.StaticEnum);
    }
    /// <summary>
    /// 値オブジェクトのルート集約を、スキーマ定義上の登場順に返します。
    /// </summary>
    public IEnumerable<RootAggregate> GetValueObjects() {
        return GetRootAggregates(E_RootAggregateKind.ValueObject);
    }
    /// <summary>
    /// 定数のルート集約を、スキーマ定義上の登場順に返します。
    /// </summary>
    public IEnumerable<RootAggregate> GetConstants() {
        return GetRootAggregates(E_RootAggregateKind.Constant);
    }

    private IEnumerable<RootAggregate> GetRootAggregates(params E_RootAggregateKind[] kinds) {
        var rootAggregateElements = _xDocument
            .Root
            ?.Elements()
            .Where(el => el.Name.LocalName != SchemaParseContext.SECTION_CUSTOM_ATTRIBUTES
                      && el.Name.LocalName != SchemaParseContext.SECTION_GENERIC_LOOKUP_TABLES)
            .SelectMany(el => el.Elements())
            .Where(el => el.TryGetRootAggregateKind(out var kind) && kinds.Contains(kind))
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
