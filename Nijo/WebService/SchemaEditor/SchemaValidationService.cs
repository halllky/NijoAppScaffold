using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Nijo.CodeGenerating;
using Nijo.SchemaParsing;

namespace Nijo.WebService.SchemaEditor;

/// <summary>
/// スキーマのバリデーション処理を提供する
/// </summary>
internal class SchemaValidationService {

    /// <summary>
    /// XMLとして正しいかを検証し、XDocumentに変換する
    /// </summary>
    internal static bool TryValidateAsXml(
        ApplicationState applicationState,
        XDocument originalXDocument,
        [NotNullWhen(true)] out XDocument? xDocument,
        out IReadOnlyDictionary<XElement, string>? uuidToXmlElement,
        out List<string> errors) {

        errors = new List<string>();
        if (!applicationState.TryConvertToXDocument(originalXDocument, errors, out xDocument, out uuidToXmlElement)) {
            return false;
        }

        return true;
    }

    /// <summary>
    /// スキーマ定義として正しいかを検証する
    /// </summary>
    internal static bool TryValidateAsSchema(
        XDocument xDocument,
        SchemaParseRule rule,
        out IEnumerable<SchemaParseContext.ValidationError> errors) {

        var schemaParseContext = new SchemaParseContext(xDocument, rule, GeneratedProjectOptions.Parse(xDocument, true));
        if (!schemaParseContext.TryBuildSchema(schemaParseContext.Document, out var _, out var xmlErrors)) {
            errors = xmlErrors;
            return false;
        }

        errors = Enumerable.Empty<SchemaParseContext.ValidationError>();
        return true;
    }

    /// <summary>
    /// スキーマ定義のエラーをReactで使うエラー形式に変換する。
    /// エラーを表示したあとに画面側で要素の並び替え、名称変更、などの操作があっても
    /// エラーの内容を変更しないために、IDをキーにしてエラーを管理する。
    /// 具体的には以下のようなJSONオブジェクトを返す。
    /// <code>
    /// {
    ///   "xxxx-xxxx-...": {
    ///     "_own": ["xxxが不正です。", "yyyが不正です。"], // XML要素自体に対するエラー
    ///     "DbName": ["テーブル名が不正です。"], // XMLAttributeに対するエラー
    ///     "MaxLength": ["この項目に最大文字数は設定できません。"], // XMLAttributeに対するエラー
    ///     ...
    ///   },
    ///   "yyyy-yyyy-...": {
    ///     ...
    ///   },
    ///   ...
    /// }
    /// </code>
    /// </summary>
    internal static JsonObject ConvertErrorsToReactFormat(
        IEnumerable<SchemaParseContext.ValidationError> errors,
        IReadOnlyDictionary<XElement, string> mapping) {

        // この名前はReact側と合わせる必要がある
        const string OWN_ERRORS = "_own";

        var result = new JsonObject();
        foreach (var error in errors) {
            var id = mapping.TryGetValue(error.XElement, out var found)
                ? found
                // 表示先が分からない場合はルートに表示する。
                // generate時の場合は保存されたxmlファイルを元に生成を行うのでクライアント側のIDが存在しない
                : "root";

            var thisXmlErrors = new JsonObject();
            result[id] = thisXmlErrors;

            // XML要素自体に対するエラー
            var ownErrors = new JsonArray();
            foreach (var ownError in error.OwnErrors) {
                ownErrors.Add(ownError);
            }

            // XML要素の属性に対するエラー
            thisXmlErrors[OWN_ERRORS] = ownErrors;
            foreach (var attributeError in error.AttributeErrors) {
                var attributeErrors = new JsonArray();
                foreach (var errorMessage in attributeError.Value) {
                    attributeErrors.Add(errorMessage);
                }
                thisXmlErrors[attributeError.Key] = attributeErrors;
            }
        }

        return result;
    }
}

