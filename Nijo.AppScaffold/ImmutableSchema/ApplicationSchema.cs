using Nijo.Commands;
using Nijo.SchemaParsing;
using Nijo.StaticEnums;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Nijo.DataStructures.Aggregates;

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

    private readonly Lock _orderByDfLock = new();
    /// <summary>
    /// このルート要素の、ref-toによる外部参照の関係性に従った、
    /// 参照される方を先、参照する方を後、としたときの順番。
    /// 永続化されるデータ構造、それ以外のデータ構造、コマンドの順に並ぶ。
    /// </summary>
    public int GetIndexOfDataFlow(ISectionRoot root) {
        lock (_orderByDfLock) {
            if (_rootAggregateOrderCache == null) {
                _rootAggregateOrderCache = new();
                var dataStructures = GetDataStructures().ToList();

                // 1) 永続化されるデータ構造（外部参照の依存関係に基づく順序）
                // 列挙するたびにこのリストから集約をクリアしていき、
                // このリストから全ての集約が無くなったら列挙完了
                var rest = dataStructures
                    .Where(root => root.IsStored)
                    .Select(root => new {
                        root,
                        refTargets = root
                            .EnumerateThisAndDescendants()
                            .SelectMany(agg => agg.GetMembers())
                            .OfType<RefToMember>()
                            .Select(refTo => refTo.RefTo.GetRoot())
                            .ToHashSet(),
                    })
                    .ToList();

                var indexForDataModels = 0;
                while (true) {
                    if (rest.Count == 0) break;

                    var next = rest[indexForDataModels];

                    // 参照先集約が未処理ならば後回し
                    var notEnumerated = rest.Where(agg => next.refTargets.Contains(agg.root));
                    if (notEnumerated.Any()) {
                        // 集約間の循環参照が存在するなどの場合は無限ループが発生するので例外。
                        // なお循環参照はスキーマ作成時にエラーとする想定
                        if (indexForDataModels + 1 >= rest.Count) throw new InvalidOperationException("集約間のデータの流れを決定できません。");

                        indexForDataModels++;
                        continue;
                    }

                    // 参照先集約が無い == nextは現在のrestの中で最上流の集約
                    _rootAggregateOrderCache.Add(next.root.XElement, _rootAggregateOrderCache.Count);

                    rest.Remove(next);
                    indexForDataModels = 0;
                }

                // 2) 永続化されないデータ構造、コマンドの順に、それぞれスキーマ定義上の登場順
                var notStoredRoots = dataStructures
                    .Where(root => !root.IsStored)
                    .Select(root => root.XElement)
                    .Concat(GetCommands().Select(command => command.XElement));
                foreach (var element in notStoredRoots) {
                    _rootAggregateOrderCache.Add(element, _rootAggregateOrderCache.Count);
                }
            }
            return _rootAggregateOrderCache[root.XElement];
        }
    }
    private Dictionary<XElement, int>? _rootAggregateOrderCache;
}

public static class ApplicationSchemaExtensions {
    /// <inheritdoc cref="ApplicationSchema.GetIndexOfDataFlow"/>
    public static int GetIndexOfDataFlow(this ISectionRoot root, ApplicationSchema schema) {
        return schema.GetIndexOfDataFlow(root);
    }
    /// <summary>
    /// ルート要素を、ref-toによる外部参照の関係性に従い、
    /// 参照される方を先、参照する方を後、とする順番に並び替えた新しいインスタンスを返します。
    /// </summary>
    public static IEnumerable<T> OrderByDataFlow<T>(this IEnumerable<T> roots, ApplicationSchema schema) where T : ISectionRoot {
        return roots.OrderBy(r => schema.GetIndexOfDataFlow(r));
    }
}
