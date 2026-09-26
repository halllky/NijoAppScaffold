#if DEBUG

using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MyApp.WebApi.Debugging.DbViewer;

/// <summary>
/// EF Core のモデルから読み取った、データベースのテーブル定義の一覧。
/// </summary>
public class DbSchema {
    [JsonPropertyName("tables")]
    public required IReadOnlyList<DbSchemaTable> Tables { get; init; }

    /// <summary>
    /// EF Core のモデルからテーブル定義の一覧を作成する。
    /// テーブルもビューも無いエンティティ型は対象外。
    /// 1つのテーブルに複数のエンティティ型が割り当てられている場合は、最初のエンティティ型の定義を採用する。
    /// </summary>
    public static DbSchema FromModel(IReadOnlyModel model) {
        var tables = model.GetEntityTypes()
            .Where(entityType => entityType.GetTableName() != null || entityType.GetViewName() != null)
            .Select(ToTable)
            .DistinctBy(table => table.TableName)
            .OrderBy(table => table.TableName)
            .ToArray();

        return new DbSchema { Tables = tables };
    }

    /// <summary>
    /// 指定の名前のテーブルまたはビューが存在するかどうか。
    /// </summary>
    public bool ContainsTable(string tableName) {
        return Tables.Any(table => table.TableName == tableName);
    }

    private static DbSchemaTable ToTable(IReadOnlyEntityType entityType) {
        var isView = entityType.GetTableName() == null;
        var tableName = entityType.GetTableName() ?? entityType.GetViewName()!;

        // "入荷DbEntity" → "入荷"
        var clrName = entityType.ClrType.Name;
        var logicalName = clrName.EndsWith("DbEntity") ? clrName[..^"DbEntity".Length] : clrName;

        var pkProps = entityType.FindPrimaryKey()?.Properties.ToHashSet() ?? [];
        var uniqueProps = entityType.GetIndexes()
            .Where(index => index.IsUnique)
            .SelectMany(index => index.Properties)
            .ToHashSet();

        var columns = entityType.GetProperties()
            .OrderBy(p => p.GetColumnOrder() ?? int.MaxValue)
            .Select(p => new DbSchemaColumn {
                PhysicalName = p.GetColumnName(),
                LogicalName = p.Name,
                Type = GetStoreType(p),
                IsPrimaryKey = pkProps.Contains(p),
                IsNotNull = !p.IsNullable,
                IsUnique = uniqueProps.Contains(p),
                Comment = p.GetComment(),
            })
            .ToArray();

        var foreignKeys = entityType.GetForeignKeys()
            .Select(fk => new DbSchemaForeignKey {
                PrincipalTable = fk.PrincipalEntityType.GetTableName() ?? fk.PrincipalEntityType.GetViewName() ?? fk.PrincipalEntityType.DisplayName(),
                Columns = fk.Properties.Select(p => p.GetColumnName()).ToArray(),
            })
            .ToArray();

        return new DbSchemaTable {
            TableName = tableName,
            LogicalName = logicalName,
            Comment = entityType.GetComment(),
            IsView = isView,
            Columns = columns,
            ForeignKeys = foreignKeys,
        };
    }

    private static string GetStoreType(IReadOnlyProperty property) {
        var storeObject = StoreObjectIdentifier.Create(property.DeclaringType, StoreObjectType.Table)
            ?? StoreObjectIdentifier.Create(property.DeclaringType, StoreObjectType.View);

        var storeType = storeObject.HasValue
            ? property.GetColumnType(storeObject.Value)
            : property.GetColumnType();

        return storeType ?? property.GetRelationalTypeMapping().StoreType;
    }
}

/// <summary>
/// テーブルまたはビュー1個分の定義。
/// </summary>
public class DbSchemaTable {
    /// <summary>物理名。テーブルを一意に識別するキーとしても使われる。</summary>
    [JsonPropertyName("tableName")]
    public required string TableName { get; init; }
    [JsonPropertyName("logicalName")]
    public required string LogicalName { get; init; }
    [JsonPropertyName("comment")]
    public required string? Comment { get; init; }
    [JsonPropertyName("isView")]
    public required bool IsView { get; init; }
    [JsonPropertyName("columns")]
    public required IReadOnlyList<DbSchemaColumn> Columns { get; init; }
    /// <summary>このテーブルが依存側となる外部キー。</summary>
    [JsonPropertyName("foreignKeys")]
    public required IReadOnlyList<DbSchemaForeignKey> ForeignKeys { get; init; }
}

/// <summary>
/// カラム1個分の定義。
/// </summary>
public class DbSchemaColumn {
    [JsonPropertyName("physicalName")]
    public required string PhysicalName { get; init; }
    [JsonPropertyName("logicalName")]
    public required string LogicalName { get; init; }
    [JsonPropertyName("type")]
    public required string Type { get; init; }
    [JsonPropertyName("isPrimaryKey")]
    public required bool IsPrimaryKey { get; init; }
    [JsonPropertyName("isNotNull")]
    public required bool IsNotNull { get; init; }
    [JsonPropertyName("isUnique")]
    public required bool IsUnique { get; init; }
    [JsonPropertyName("comment")]
    public required string? Comment { get; init; }
}

/// <summary>
/// 外部キー1個分の定義。依存側のテーブルは、この外部キーを持つ <see cref="DbSchemaTable"/> 自身。
/// </summary>
public class DbSchemaForeignKey {
    /// <summary>主側のテーブルの物理名。</summary>
    [JsonPropertyName("principalTable")]
    public required string PrincipalTable { get; init; }
    /// <summary>依存側のテーブルにある、外部キーを構成するカラムの物理名。</summary>
    [JsonPropertyName("columns")]
    public required IReadOnlyList<string> Columns { get; init; }
}

#endif
