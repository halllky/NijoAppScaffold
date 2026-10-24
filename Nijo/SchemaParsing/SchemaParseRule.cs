using Nijo.ImmutableSchema;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Nijo.Commands;
using Nijo.Constants;
using Nijo.DataStructures;
using Nijo.DataStructures.IsGenericLookupTable;
using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures.IsStored;
using Nijo.DataStructures.Members;
using Nijo.DataStructures.UseSoftDelete;
using Nijo.SchemaParsing.CommonOptions;
using Nijo.StaticEnums;
using Nijo.ValueObjects;

namespace Nijo.SchemaParsing;

/// <summary>
/// nijo.xml の定義。
/// どういったセクション・属性・値の型が存在し、それぞれからどういったソースコードが生成されるかの一覧。
/// </summary>
public class SchemaParseRule {
    /// <summary>
    /// セクションの種類。ソースコードはこの順番で生成される。
    /// </summary>
    internal Section[] Sections { get; init; } = [];
    /// <summary>
    /// 値メンバーの種類
    /// </summary>
    public required IValueMemberType[] ValueMemberTypes { get; init; }
    /// <summary>
    /// オプション項目の種類
    /// </summary>
    public required NodeOption[] NodeOptions { get; init; }


    /// <summary>
    /// 既定の型解釈ルール
    /// </summary>
    public static SchemaParseRule Default() {
        var valueMemberTypes = new IValueMemberType[] {
            // 文字列系
            new ValueMemberTypes.Word(),
            new ValueMemberTypes.Description(),
            // 数値系
            new ValueMemberTypes.IntMember(),
            new ValueMemberTypes.DecimalMember(),
            new ValueMemberTypes.SequenceMember(),
            // 日付系
            new ValueMemberTypes.DateTimeMember(),
            new ValueMemberTypes.DateMember(),
            new ValueMemberTypes.YearMonthMember(),
            new ValueMemberTypes.YearMember(),
            // その他
            new ValueMemberTypes.BoolMember(),
            new ValueMemberTypes.ByteArrayMember(),
        };
        var nodeOptions = new NodeOption[] {
            new DisplayNameOption(),
            new DisplayNameIsEmptyOption(),
            new DbNameOption(),
            new LatinNameOption(),
            new IsKeyOption(),
            new IsNotNullOption(),
            new IsStoredOption(),
            new IsQueryableOption(),
            new UseSoftDeleteOption(),
            new UniqueConstraintsOption(),
            new IsGenericLookupTableOption(),
            new IsHardCodedPrimaryKeyOption(),
            new GenericLookupCategoryOption(),
            new MapToViewOption(),
            new OnlySearchConditionOption(),
            new ParameterOption(),
            new ReturnValueOption(),
            new RefToObjectOption(),
            new MaxLengthOption(),
            new TotalDigitOption(),
            new DecimalPlaceOption(),
            new SequenceNameOption(),
            new ConstantTypeOption(),
            new ConstantValueOption(),
            new StringSearchBehaviorOption(),
        };
        var sections = new Section[] {
            new DataStructuresSection(nodeOptions.OfType<FeatureOption>()),
            new CommandsSection(),
            new StaticEnumsSection(),
            new ValueObjectsSection(),
            new ConstantsSection(),
        };
        return new() {
            Sections = sections,
            ValueMemberTypes = valueMemberTypes,
            NodeOptions = nodeOptions,
        };
    }

    /// <summary>
    /// このルールの整合性を検証します。
    /// </summary>
    /// <exception cref="InvalidOperationException">ルールに問題がある場合</exception>
    public void ThrowIfInvalid() {
        // スキーマ定義名重複チェック
        var appearedName = new HashSet<string>();
        var duplicates = new HashSet<string>();
        foreach (var name in ValueMemberTypes.Select(t => t.SchemaTypeName)) {
            if (appearedName.Contains(name)) {
                duplicates.Add(name);
            } else {
                appearedName.Add(name);
            }
        }
        if (duplicates.Count > 0) {
            throw new InvalidOperationException($"型名 {string.Join(", ", duplicates)} が重複しています。");
        }

        // オプション属性のキー重複チェック
        var groupedOptions = NodeOptions
            .GroupBy(opt => opt.AttributeName)
            .Where(group => group.Count() >= 2)
            .ToArray();
        if (groupedOptions.Length > 0) {
            throw new InvalidOperationException($"オプション属性名 {groupedOptions.Select(g => g.Key).Join(", ")} が重複しています。");
        }

        // 予約語
        if (NodeOptions.Any(opt => opt.AttributeName == SchemaParseContext.ATTR_NODE_TYPE)) {
            throw new InvalidOperationException($"{SchemaParseContext.ATTR_NODE_TYPE} という名前のオプション属性は定義できません。");
        }
    }

}
