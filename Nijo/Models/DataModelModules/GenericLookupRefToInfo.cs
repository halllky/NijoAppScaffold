using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.DataModelModules;

/// <summary>
/// Category 属性付き Generic Lookup Table ref-to の派生情報を扱うヘルパー。
/// </summary>
internal static class GenericLookupRefToInfo {

    internal static bool TryCreate(RefToMember refTo, out Info info) {
        if (refTo.Category == null) {
            info = default!;
            return false;
        }

        var root = refTo.RefTo.GetRoot();
        if (root.XElement.Attribute(BasicNodeOptions.IsGenericLookupTable.AttributeName) == null) {
            info = default!;
            return false;
        }

        var parser = new GenericLookupTableParser(refTo.SchemaParseContext);
        var category = parser
            .GetCategoriesOf(root)
            .FirstOrDefault(candidate => candidate.Name == refTo.Category);
        if (category == null) {
            info = default!;
            return false;
        }

        var hardcodedUniqueIds = category.HardCodedKeys
            .Select(key => key.UniqueId)
            .ToHashSet(StringComparer.Ordinal);
        var keyMembers = refTo.RefTo
            .GetMembers()
            .OfType<ValueMember>()
            .Where(member => member.IsKey)
            .ToArray();

        info = new Info {
            RefTo = refTo,
            RootAggregate = root,
            Category = category,
            HardCodedKeyMembers = keyMembers
                .Where(member => TryGetUniqueId(member, out var uniqueId)
                              && hardcodedUniqueIds.Contains(uniqueId))
                .ToArray(),
            NonHardCodedKeyMembers = keyMembers
                .Where(member => !TryGetUniqueId(member, out var uniqueId)
                              || !hardcodedUniqueIds.Contains(uniqueId))
                .ToArray(),
        };
        return true;
    }

    private static bool TryGetUniqueId(ValueMember member, out string uniqueId) {
        uniqueId = member.XElement.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)?.Value ?? string.Empty;
        return uniqueId.Length > 0;
    }

    internal sealed class Info {
        public required RefToMember RefTo { get; init; }
        public required RootAggregate RootAggregate { get; init; }
        public required GenericLookupTableParser.GenericLookupTableCategory Category { get; init; }
        public required IReadOnlyList<ValueMember> HardCodedKeyMembers { get; init; }
        public required IReadOnlyList<ValueMember> NonHardCodedKeyMembers { get; init; }

        public bool IsHardCoded(ValueMember valueMember) => HardCodedKeyMembers.Contains(valueMember);
        public bool IsNonHardCoded(ValueMember valueMember) => NonHardCodedKeyMembers.Contains(valueMember);
    }
}
