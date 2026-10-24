using Nijo.Commands;
using Nijo.SchemaParsing;
using Nijo.StaticEnums;
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
        return GetRootElements(E_Section.DataStructures).Select(el => (RootAggregate)_parseContext.ToAggregateBase(el, null));
    }
    /// <summary>
    /// コマンドを、スキーマ定義上の登場順に返します。
    /// </summary>
    public IEnumerable<CommandRoot> GetCommands() {
        return GetRootElements(E_Section.Commands).Select(el => new CommandRoot(el, _parseContext));
    }
    /// <summary>
    /// 静的区分の種類を、スキーマ定義上の登場順に返します。
    /// </summary>
    public IEnumerable<StaticEnumRoot> GetStaticEnums() {
        return GetRootElements(E_Section.StaticEnums).Select(el => new StaticEnumRoot(el, _parseContext));
    }
    /// <summary>
    /// 値オブジェクトを、スキーマ定義上の登場順に返します。
    /// </summary>
    public IEnumerable<SectionRoot> GetValueObjects() {
        return GetRootElements(E_Section.ValueObjects).Select(el => new SectionRoot(el, _parseContext));
    }
    /// <summary>
    /// 定数のグループを、スキーマ定義上の登場順に返します。
    /// </summary>
    public IEnumerable<SectionRoot> GetConstants() {
        return GetRootElements(E_Section.Constants).Select(el => new SectionRoot(el, _parseContext));
    }

    private IEnumerable<XElement> GetRootElements(E_Section section) {
        return _xDocument.Root?.Element(section.ToXmlElementName())?.Elements() ?? [];
    }

}
