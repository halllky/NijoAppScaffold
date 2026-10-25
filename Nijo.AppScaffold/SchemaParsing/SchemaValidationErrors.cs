using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.SchemaParsing;

/// <summary>
/// スキーマ定義の検証で見つかったエラーを、XML要素ごとに集める。
/// XML要素自体に対するエラーと、XML要素の特定の属性に対するエラーを区別する。
/// </summary>
internal sealed class SchemaValidationErrors {
    private readonly List<(XElement Element, string Message)> _ownErrors = [];
    private readonly List<(XElement Element, string AttributeName, string Message)> _attributeErrors = [];

    /// <summary>XML要素自体に対するエラーを追加します。</summary>
    internal void Add(XElement element, string message) {
        _ownErrors.Add((element, message));
    }
    /// <summary>XML要素の特定の属性に対するエラーを追加します。</summary>
    internal void AddAttributeError(XElement element, string attributeName, string message) {
        _attributeErrors.Add((element, attributeName, message));
    }

    /// <summary>
    /// 集めたエラーを、エラーのあったXML要素の単位にまとめて返します。
    /// </summary>
    internal SchemaParseContext.ValidationError[] ToArray() {
        return _ownErrors
            .Select(x => x.Element)
            .Concat(_attributeErrors.Select(x => x.Element))
            .Distinct()
            .Select(element => new SchemaParseContext.ValidationError {
                XElement = element,
                OwnErrors = _ownErrors
                    .Where(x => x.Element == element)
                    .Select(x => x.Message)
                    .ToArray(),
                AttributeErrors = _attributeErrors
                    .Where(x => x.Element == element)
                    .GroupBy(x => x.AttributeName)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Message).ToArray()),
            })
            .ToArray();
    }
}
