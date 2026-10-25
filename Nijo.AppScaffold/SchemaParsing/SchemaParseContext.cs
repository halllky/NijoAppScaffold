using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures.ValueMemberTypes;
using Nijo.DataStructures;

namespace Nijo.SchemaParsing;

/// <summary>
/// XML要素をこのアプリケーションのルールに従って解釈する
/// </summary>
public class SchemaParseContext {
    public SchemaParseContext(XDocument xDocument, SchemaParseRule rule, GeneratedProjectOptions projectOptions) {
        // ルールの検証
        rule.ThrowIfInvalid();

        Document = xDocument;
        ProjectOptions = projectOptions;

        _rule = rule;
        _nodeOptionsByName = new(() => rule.NodeOptions.ToDictionary(opt => opt.AttributeName));
        _customAttributes = new(() => NijoXmlCustomAttribute.FromXDocument(Document).ToArray());
        _customAttributesByUniqueId = new(() => CustomAttributes
            .Where(attr => !string.IsNullOrWhiteSpace(attr.UniqueId))
            .GroupBy(attr => attr.UniqueId!)
            .ToDictionary(group => group.Key, group => group.First()));
        _valueMemberTypes = rule.ValueMemberTypes.ToDictionary(m => m.SchemaTypeName);
        _staticEnumMembers = new(() => Document.Root
            ?.Element(SECTION_STATIC_ENUMS)
            ?.Elements()
            .ToDictionary(GetPhysicalName, el => new StaticEnumMember(el, this))
            ?? []);
        _valueObjectMembers = new(() => Document.Root
            ?.Element(SECTION_VALUE_OBJECTS)
            ?.Elements()
            .ToDictionary(GetPhysicalName, el => new ValueObjectMember(el, this))
            ?? []);
        _elementCountByLocalName = new Lazy<IReadOnlyDictionary<string, int>>(() => Document
            .Descendants()
            .GroupBy(el => el.Name.LocalName)
            .ToDictionary(group => group.Key, group => group.Count()));
    }

    public XDocument Document { get; }
    public GeneratedProjectOptions ProjectOptions { get; }
    private readonly SchemaParseRule _rule;
    private readonly IReadOnlyDictionary<string, IValueMemberType> _valueMemberTypes;
    /// <summary>単語型など予め登録された値型の一覧。静的区分と値オブジェクトは含まない。</summary>
    public IEnumerable<IValueMemberType> PredefinedValueMemberTypes => _rule.ValueMemberTypes;
    /// <inheritdoc cref="SchemaParseRule.Sections"/>
    internal IEnumerable<Section> Sections => _rule.Sections;

    /// <summary>
    /// nijo.xml のルート要素直下の要素のうち、データ構造が格納されるXML要素の名前。
    /// このセクションのルート集約からどういったソースコードが生成されるかは IsStored, IsQueryable 属性の組み合わせで決まる。
    /// </summary>
    internal const string SECTION_DATA_STRUCTURES = "DataStructures";
    /// <summary>
    /// nijo.xml のルート要素直下の要素のうち、 Command Model が格納されるXML要素の名前
    /// </summary>
    internal const string SECTION_COMMANDS = "Commands";
    /// <summary>
    /// nijo.xml のルート要素直下の要素のうち、静的区分が格納されるXML要素の名前
    /// </summary>
    internal const string SECTION_STATIC_ENUMS = "StaticEnums";
    /// <summary>
    /// nijo.xml のルート要素直下の要素のうち、値オブジェクトが格納されるXML要素の名前
    /// </summary>
    internal const string SECTION_VALUE_OBJECTS = "ValueObjects";
    /// <summary>
    /// nijo.xml のルート要素直下の要素のうち、定数が格納されるXML要素の名前
    /// </summary>
    internal const string SECTION_CONSTANTS = "Constants";
    /// <summary>
    /// nijo.xml のルート要素直下の要素のうち、カスタム属性が格納されるXML要素の名前
    /// </summary>
    internal const string SECTION_CUSTOM_ATTRIBUTES = "CustomAttributes";
    /// <summary>
    /// nijo.xml のルート要素直下の要素のうち、汎用参照テーブルのカテゴリが格納されるXML要素の名前
    /// </summary>
    internal const string SECTION_GENERIC_LOOKUP_TABLES = "GenericLookupTableCategories";

    /// <summary>
    /// 要素を一意に識別するための属性名。
    /// GUI上で新たに作成されて以降決して変更されない前提。
    /// </summary>
    internal const string ATTR_UNIQUE_ID = "UniqueId";
    internal const string ATTR_NODE_TYPE = "Type";

    internal const string NODE_TYPE_CHILD = "child";
    internal const string NODE_TYPE_CHILDREN = "children";
    internal const string NODE_TYPE_REFTO = "ref-to";

    /// <summary>
    /// 物理名。スキーマ内での物理名の衝突を考慮した値を返す。
    /// </summary>
    internal string GetPhysicalName(XElement xElement) {
        return _physicalNameCache.GetOrAdd(xElement, el => {
            var nodeType = GetNodeType(el);

            // ルート集約の場合は単純に名前を返す。
            // ルート集約の物理名の衝突はスキーマの検証時にエラーになるため、ここでは考えなくてよい
            if (nodeType == E_NodeType.RootAggregate) {
                return el.Name.LocalName;
            }

            // Child型またはChildren型、かつ名前衝突がある場合、「（直近の親のPhysicalName）の（LocalName）」
            if (nodeType == E_NodeType.ChildAggregate || nodeType == E_NodeType.ChildrenAggregate) {
                var duplicates = _elementCountByLocalName.Value.TryGetValue(el.Name.LocalName, out var count)
                    && count > 1;
                if (duplicates) {
                    // 「（直近の親のPhysicalName）の（LocalName）」
                    return GetPhysicalName(el.Parent!) + "の" + el.Name.LocalName;
                }
            }

            // それ以外の場合は単純にLocalNameを返す
            return el.Name.LocalName;
        });
    }
    private readonly ConcurrentDictionary<XElement, string> _physicalNameCache = new();
    private readonly Lazy<IReadOnlyDictionary<string, int>> _elementCountByLocalName;


    /// <summary>
    /// XML要素の種類を判定する。
    /// 編集途中の不正な状態であっても入力内容検査等に使う必要があるため、
    /// XML要素が不正であっても例外を出さない。
    /// </summary>
    internal E_NodeType GetNodeType(XElement xElement) {
        return _nodeTypeCache.GetOrAdd(xElement, el => {
            // ルート集約, Child, Children
            if (el.TryGetAggregateNodeType(out var aggregateNodeType)) {
                return aggregateNodeType.Value;
            }

            // 親がenumセクションの直下にあるなら静的区分の値
            var xElementParent = el.Parent;
            if (xElementParent != null
                && xElementParent.Parent?.Parent == Document.Root
                && xElementParent.Parent?.Name.LocalName == SECTION_STATIC_ENUMS) {
                return E_NodeType.StaticEnumValue;
            }

            // 以降はType属性の値で区別
            var type = el.Attribute(ATTR_NODE_TYPE);
            if (type == null) {
                return E_NodeType.Unknown;
            }

            // RefTo
            if (type.Value.StartsWith(NODE_TYPE_REFTO)) {
                return E_NodeType.Ref;
            }
            // ValueMember
            if (TryResolveMemberType(el, out _)) {
                return E_NodeType.ValueMember;
            }
            return E_NodeType.Unknown;
        });
    }
    private readonly ConcurrentDictionary<XElement, E_NodeType> _nodeTypeCache = new();


    #region オプション属性
    /// <summary>
    /// XML要素に定義されているオプション属性を返します。
    /// </summary>
    public IEnumerable<NodeOption> GetOptions(XElement xElement) {
        var attrs = xElement.Attributes().Select(attr => attr.Name.LocalName).ToHashSet();
        return _rule.NodeOptions.Where(opt => attrs.Contains(opt.AttributeName));
    }
    /// <summary>
    /// このルールで定義されているすべてのオプション属性
    /// </summary>
    public IEnumerable<NodeOption> NodeOptions => _rule.NodeOptions;

    /// <summary>
    /// CustomAttributes セクションに定義されているカスタム属性の一覧
    /// </summary>
    internal IReadOnlyList<NijoXmlCustomAttribute> CustomAttributes => _customAttributes.Value;
    private readonly Lazy<NijoXmlCustomAttribute[]> _customAttributes;

    /// <summary>
    /// XML要素に定義されているカスタム属性を返します。
    /// </summary>
    public IEnumerable<NijoXmlCustomAttribute> GetCustomAttributes(XElement xElement) {
        var specifiedAttrCustomIds = xElement
            .Attributes()
            .Select(attr => attr.Name.LocalName)
            .ToHashSet();
        return CustomAttributes
            .Where(def => specifiedAttrCustomIds.Contains(def.UniqueId!));
    }
    #endregion オプション属性


    #region Aggregate
    /// <summary>
    /// ルート集約や子集約を表すXML要素を <see cref="AggregateBase"/> のインスタンスに変換します。
    /// XML要素が集約を表すもので無かった場合は例外を送出します。
    /// </summary>
    internal AggregateBase ToAggregateBase(XElement xElement, ISchemaPathNode? previous) {
        var nodeType = GetNodeType(xElement);
        if (nodeType == E_NodeType.RootAggregate) {
            return new RootAggregate(xElement, this, previous);
        }
        if (nodeType == E_NodeType.ChildAggregate) {
            return new ChildAggregate(xElement, this, previous);
        }
        if (nodeType == E_NodeType.ChildrenAggregate) {
            return new ChildrenAggregate(xElement, this, previous);
        }
        throw new InvalidOperationException($"集約ではありません: {xElement}");
    }
    #endregion Aggregate


    #region ValueMember
    /// <summary>
    /// このスキーマで定義されている静的区分の種類を返します。
    /// </summary>
    /// <returns></returns>
    internal IReadOnlyDictionary<string, StaticEnumMember> GetStaticEnumMembers() => _staticEnumMembers.Value;
    private readonly Lazy<Dictionary<string, StaticEnumMember>> _staticEnumMembers;

    /// <summary>
    /// このスキーマで定義されている値オブジェクト型を返します。
    /// </summary>
    /// <returns></returns>
    internal IReadOnlyDictionary<string, ValueObjectMember> GetValueObjectMembers() => _valueObjectMembers.Value;
    private readonly Lazy<Dictionary<string, ValueObjectMember>> _valueObjectMembers;

    /// <summary>
    /// スキーマ解釈ルールとしてあらかじめ定められた値種別および静的区分の種類の一覧を返します。
    /// </summary>
    public IEnumerable<IValueMemberType> GetValueMemberTypes() {
        // 単語型など予め登録された型
        foreach (var type in _valueMemberTypes.Values) {
            yield return type;
        }
        // 列挙体
        foreach (var type in GetStaticEnumMembers().Values) {
            yield return type;
        }
        // 値オブジェクト型
        foreach (var type in GetValueObjectMembers().Values) {
            yield return type;
        }
    }

    /// <summary>
    /// ValueMemberを表すXML要素の種別（日付, 数値, ...等）を判別して返します。
    /// </summary>
    internal bool TryResolveMemberType(XElement xElement, out IValueMemberType valueMemberType) {
        var type = xElement.Attribute(ATTR_NODE_TYPE);
        if (type == null) {
            valueMemberType = null!;
            return false;
        }

        // 単語型など予め登録された型
        if (_valueMemberTypes.TryGetValue(type.Value, out var vmType)) {
            valueMemberType = vmType;
            return true;
        }

        // 列挙体
        if (GetStaticEnumMembers().TryGetValue(type.Value, out var enumMember)) {
            valueMemberType = enumMember;
            return true;
        }

        // 値オブジェクト型
        if (GetValueObjectMembers().TryGetValue(type.Value, out var valueObjectMember)) {
            valueMemberType = valueObjectMember;
            return true;
        }

        // 解決できなかった
        valueMemberType = null!;
        return false;
    }
    #endregion ValueMember


    #region RefTo
    /// <summary>
    /// 参照先のXML要素を返します。
    /// </summary>
    internal XElement? FindRefTo(XElement xElement) {
        var type = xElement.Attribute(ATTR_NODE_TYPE) ?? throw new InvalidOperationException();
        var splitted = type.Value.Split(':');
        if (splitted.Length < 2) return null;
        var xPath = $"//{SECTION_DATA_STRUCTURES}/{splitted[1]}";
        return _refToCache.GetOrAdd(xPath, path => {
            return Document.Root?.XPathSelectElement(path);
        });
    }
    /// <summary>
    /// 引数の集約を参照している集約を探して返します。
    /// </summary>
    internal IEnumerable<XElement> FindRefFrom(XElement xElement) {
        return _refFromCache.GetOrAdd(xElement, el => {
            var fullPath = string.Join("/", el.AncestorsAndSelf().Reverse().Skip(2).Select(GetPhysicalName));
            return Document.XPathSelectElements($"//{SECTION_DATA_STRUCTURES}//*[@{ATTR_NODE_TYPE}='{NODE_TYPE_REFTO}:{fullPath}']")?.ToArray() ?? [];
        });
    }
    /// <summary>
    /// ルート集約とその子孫に定義された外部参照のうち、参照先が存在し、かつ自身のツリー以外を指しているものを列挙します。
    /// 参照先が存在しない外部参照や自身のツリーを指す外部参照は、それ自体が検証エラーになるため含めません。
    /// </summary>
    internal IEnumerable<(XElement RefElement, XElement RefTo, XElement RefToRoot)> EnumerateRefsToOtherTrees(XElement rootAggregateElement) {
        foreach (var refElement in rootAggregateElement.Descendants()) {
            if (GetNodeType(refElement) != E_NodeType.Ref) continue;
            var refTo = FindRefTo(refElement);
            if (refTo == null) continue;
            var refToRoot = refTo.GetRootAggregateElement();
            if (refToRoot == rootAggregateElement) continue;
            yield return (refElement, refTo, refToRoot);
        }
    }
    private readonly ConcurrentDictionary<string, XElement?> _refToCache = new();
    private readonly ConcurrentDictionary<XElement, IReadOnlyCollection<XElement>> _refFromCache = new();
    #endregion RefTo


    #region 検証
    /// <summary>
    /// XMLドキュメントがスキーマ定義として不正な状態を持たないかを検証し、
    /// 検証に成功した場合はアプリケーションスキーマのインスタンスを返します。
    /// </summary>
    /// <param name="xDocument">XMLドキュメント</param>
    /// <param name="schema">作成完了後のスキーマ</param>
    /// <param name="errors">エラー</param>
    /// <returns>スキーマの作成に成功したかどうか</returns>
    public bool TryBuildSchema(XDocument xDocument, out ApplicationSchema schema, out ValidationError[] errors) {
        schema = new ApplicationSchema(xDocument, this);
        var validationErrors = new SchemaValidationErrors();

        ValidateCustomAttributeDefinitions(xDocument, validationErrors);

        var rootsBySection = Enum.GetValues<E_Section>().ToDictionary(
            section => section,
            section => (IReadOnlyList<XElement>)(xDocument.Root?.Element(section.ToXmlElementName())?.Elements().ToArray() ?? []));

        // ルート集約の物理名はセクションをまたいで一意でなければならない
        var rootPhysicalNames = new HashSet<string>();
        foreach (var root in rootsBySection.Values.SelectMany(roots => roots)) {
            var rootName = GetPhysicalName(root);
            if (!rootPhysicalNames.Add(rootName)) {
                validationErrors.Add(root, $"ルート集約の物理名'{rootName}'が重複しています。セクションをまたいでの重複もできません。");
            }

            // ルート集約の種類は属するセクションと IsStored, IsQueryable 属性で決まるため、Type 属性は使わない
            if (root.Attribute(ATTR_NODE_TYPE) != null) {
                validationErrors.AddAttributeError(root, ATTR_NODE_TYPE, $"ルート集約には {ATTR_NODE_TYPE} 属性を指定できません。");
            }
        }

        // セクション固有の検証
        foreach (var section in _rule.Sections) {
            section.Validate(rootsBySection[section.Kind], this, validationErrors);
        }

        // 属性単位の検証
        foreach (var element in rootsBySection.Values.SelectMany(roots => roots).SelectMany(root => root.DescendantsAndSelf())) {
            ValidateAttributes(element, validationErrors);
        }

        errors = validationErrors.ToArray();
        return errors.Length == 0;
    }

    /// <summary>
    /// CustomAttributes セクションに定義されたカスタム属性の定義自体を検証します。
    /// </summary>
    private void ValidateCustomAttributeDefinitions(XDocument xDocument, SchemaValidationErrors errors) {
        var customAttributeElements = xDocument.Root?.Element(SECTION_CUSTOM_ATTRIBUTES)?.Elements().ToArray() ?? [];
        var customAttributes = NijoXmlCustomAttribute.FromXDocument(xDocument).ToArray();
        if (customAttributeElements.Length != customAttributes.Length) return;

        for (int i = 0; i < customAttributes.Length; i++) {
            foreach (var error in customAttributes[i].ValidateThis(this, _rule)) {
                errors.Add(customAttributeElements[i], error);
            }
        }

        foreach (var attr in customAttributes.Where(a => a.IsValidation)) {
            if (string.IsNullOrWhiteSpace(attr.PhysicalName)) continue;
            if (!attr.PhysicalName.IsCSharpSafe()) {
                var el = customAttributeElements.FirstOrDefault(e => e.Name.LocalName == attr.UniqueId) ?? xDocument.Root ?? new XElement("root");
                errors.Add(el, $"カスタム属性 '{attr.PhysicalName}' の {nameof(attr.PhysicalName)} はC#の識別子として不正です。英数字とアンダースコアのみを使用し、先頭は英字またはアンダースコアにしてください。");
            }
        }
    }

    /// <summary>
    /// XML要素に指定されている属性それぞれについて、その場所に指定可能か、値が妥当かを検証します。
    /// </summary>
    private void ValidateAttributes(XElement element, SchemaValidationErrors errors) {
        if (!NodeOptionTarget.TryCreate(element, GetNodeType(element), out var target)) return;

        foreach (var attr in element.Attributes()) {
            // 名前空間宣言はスキーマのルールで定義された属性ではないのでスキップ
            if (attr.IsNamespaceDeclaration) continue;

            var attrName = attr.Name.LocalName;
            if (attrName == ATTR_NODE_TYPE || attrName == ATTR_UNIQUE_ID) continue;

            if (_nodeOptionsByName.Value.TryGetValue(attrName, out var opt)) {
                if (!opt.IsAvailable(target)) {
                    errors.AddAttributeError(element, attrName, "この属性はこの要素には指定できません。");
                    continue;
                }
                opt.Validate(new() {
                    Value = attr.Value,
                    XElement = element,
                    Target = target,
                    AddError = err => errors.AddAttributeError(element, attrName, err),
                    SchemaParseContext = this,
                });

            } else if (_customAttributesByUniqueId.Value.TryGetValue(attrName, out var customAttr)) {
                if (!customAttr.IsAvailableFor(target)) {
                    errors.AddAttributeError(element, attrName, $"カスタム属性 '{customAttr.PhysicalName}' はこの要素では使用できません。");
                    continue;
                }
                foreach (var error in customAttr.ValidateModelElement(element)) {
                    errors.AddAttributeError(element, attrName, error);
                }

            } else {
                errors.AddAttributeError(element, attrName, "この属性は定義されていません。");
            }
        }
    }
    private readonly Lazy<Dictionary<string, NodeOption>> _nodeOptionsByName;
    private readonly Lazy<Dictionary<string, NijoXmlCustomAttribute>> _customAttributesByUniqueId;

    public class ValidationError {
        public required XElement XElement { get; init; }
        /// <summary>
        /// XML要素自体に対するエラー
        /// </summary>
        public required string[] OwnErrors { get; init; }
        /// <summary>
        /// 属性に対するエラー
        /// </summary>
        public required IReadOnlyDictionary<string, string[]> AttributeErrors { get; init; }

        public override string ToString() {
            return $"{XElement.AncestorsAndSelf().Reverse().Select(el => el.Name.LocalName).Join("/")}: {OwnErrors.Join(", ")}";
        }
    }
    #endregion 検証
}
