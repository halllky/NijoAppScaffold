using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MyApp.WebApi.Debugging;

[ApiController]
[Route("/debug/er-diagram")]
public class ERDiagramController : ControllerBase {

    [HttpGet]
    public IActionResult GetERDiagram([FromServices] OverridedApplicationService app) {
        var model = app.DbContext.Model;
        var entityTypes = model.GetEntityTypes().ToList();

        return Ok(new ERDiagramResponse {
            LogicalNameDataSet = BuildDataSet(entityTypes, useLogicalName: true),
            PhysicalNameDataSet = BuildDataSet(entityTypes, useLogicalName: false),
        });
    }

    private static GraphView2DataSet BuildDataSet(IReadOnlyList<IReadOnlyEntityType> entityTypes, bool useLogicalName) {
        var nodes = new List<GraphView2Node>();
        var edges = new List<GraphView2Edge>();

        foreach (var entityType in entityTypes) {
            var isView = entityType.GetTableName() == null && entityType.GetViewName() != null;
            var physicalName = entityType.GetTableName() ?? entityType.GetViewName() ?? entityType.DisplayName();
            var shortName = entityType.ClrType?.Name ?? entityType.DisplayName();
            // "入荷DbEntity" → "入荷"
            var logicalName = shortName.EndsWith("DbEntity")
                ? shortName[..^"DbEntity".Length]
                : shortName;

            var nodeLabel = useLogicalName ? logicalName : physicalName;
            if (isView) nodeLabel += " (ビュー)";

            var pk = entityType.FindPrimaryKey();
            var pkProps = pk?.Properties.ToHashSet() ?? [];
            var uniqueIndexProps = entityType.GetIndexes()
                .Where(i => i.IsUnique)
                .SelectMany(i => i.Properties)
                .ToHashSet();

            var headers = new[] {
                useLogicalName ? "カラム名(論理)" : "カラム名(物理)",
                "型", "PK", "NOT NULL", "ユニーク",
            };
            var rows = entityType.GetProperties()
                .OrderBy(p => p.GetColumnOrder() ?? int.MaxValue)
                .Select(p => new[] {
                    useLogicalName ? p.Name : (p.GetColumnName() ?? p.Name),
                    GetTypeDisplay(p),
                    pkProps.Contains(p) ? "○" : "",
                    p.IsNullable ? "" : "○",
                    uniqueIndexProps.Contains(p) ? "○" : "",
                })
                .ToArray();

            nodes.Add(new GraphView2Node {
                Id = entityType.Name,
                Label = nodeLabel,
                Table = new TableData { Headers = headers, Rows = rows },
            });

            // 外部キーをエッジとして追加（このエンティティが依存側）
            foreach (var fk in entityType.GetForeignKeys()) {
                edges.Add(new GraphView2Edge {
                    Source = entityType.Name,
                    Target = fk.PrincipalEntityType.Name,
                    TargetEndShape = "triangle",
                });
            }
        }

        return new GraphView2DataSet { Nodes = nodes, Edges = edges };
    }

    private static string GetTypeDisplay(IReadOnlyProperty property) {
        var clrType = property.ClrType;
        var underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;
        var maxLength = property.GetMaxLength();
        var precision = property.GetPrecision();
        var scale = property.GetScale();

        return underlyingType.Name switch {
            "String" => maxLength.HasValue ? $"string({maxLength})" : "string",
            "Int32" => "int",
            "Int64" => "long",
            "Int16" => "short",
            "Byte" => "byte",
            "Decimal" => precision.HasValue && scale.HasValue ? $"decimal({precision},{scale})"
                                : precision.HasValue ? $"decimal({precision})" : "decimal",
            "Double" => "double",
            "Single" => "float",
            "DateTime" => "datetime",
            "DateTimeOffset" => "datetimeoffset",
            "DateOnly" => "date",
            "TimeOnly" => "time",
            "Boolean" => "bool",
            "Byte[]" => maxLength.HasValue ? $"binary({maxLength})" : "binary",
            "Guid" => "guid",
            _ => underlyingType.Name,
        };
    }

    private class ERDiagramResponse {
        public GraphView2DataSet LogicalNameDataSet { get; set; } = new();
        public GraphView2DataSet PhysicalNameDataSet { get; set; } = new();
    }

    private class GraphView2DataSet {
        [JsonPropertyName("nodes")]
        public List<GraphView2Node> Nodes { get; set; } = [];
        [JsonPropertyName("edges")]
        public List<GraphView2Edge> Edges { get; set; } = [];
    }

    /// <summary>
    /// GraphView2 コンポーネントで使用するノードのデータ構造。
    /// ER図で言うとテーブルまたはビュー1個分。
    /// </summary>
    private class GraphView2Node {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;
        [JsonPropertyName("label")]
        public string? Label { get; set; }
        [JsonPropertyName("table")]
        public TableData? Table { get; set; }
    }
    /// <summary>
    /// GraphView2 コンポーネントで使用するテーブルデータの構造。
    /// ER図で言うとテーブルのカラム名、NOT NULL などの情報。
    /// </summary>
    private class TableData {
        [JsonPropertyName("headers")]
        public string[] Headers { get; set; } = default!;
        [JsonPropertyName("rows")]
        public string[][] Rows { get; set; } = default!;
    }
    /// <summary>
    /// GraphView2 コンポーネントで使用するエッジのデータ構造。
    /// ER図で言うとテーブル同士のリレーションシップ（外部キー制約など）を表す。
    /// </summary>
    private class GraphView2Edge {
        [JsonPropertyName("label")]
        public string? Label { get; set; }
        [JsonPropertyName("source")]
        public string Source { get; set; } = default!;
        [JsonPropertyName("target")]
        public string Target { get; set; } = default!;

        /// <summary>cytoscape.Css.ArrowShape で利用できる値のみ指定可能</summary>
        [JsonPropertyName("sourceEndShape")]
        public string? SourceEndShape { get; set; }
        /// <summary>cytoscape.Css.ArrowShape で利用できる値のみ指定可能</summary>
        [JsonPropertyName("targetEndShape")]
        public string? TargetEndShape { get; set; }
    }
}
