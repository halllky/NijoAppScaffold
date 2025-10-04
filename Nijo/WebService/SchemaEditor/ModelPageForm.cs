using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nijo.WebService.SchemaEditor;

/// <summary>
/// ルート集約1個分の編集画面のデータ型定義
/// </summary>
public class ModelPageForm {
    [JsonPropertyName("xmlElements")]
    public List<XmlElementItem> XmlElements { get; set; } = [];
}

