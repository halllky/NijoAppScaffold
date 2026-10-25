using Nijo.CodeGenerating;
using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures.IsStored;
using Nijo.DataStructures.Members;
using Nijo.DataStructures.Structure;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System;
using Nijo.Commands;
using Nijo.DataStructures.Aggregates;

namespace Nijo.DataStructures;

/// <summary>
/// DataStructures セクション。
/// ルート集約に指定された <see cref="FeatureOption"/> ごとに検証とソースコード生成を行う。
/// いずれの <see cref="FeatureOption"/> も指定されていない（IsStored 属性も IsQueryable 属性も指定されていない）ルート集約は、
/// Webサーバーとクライアントの間で共有される構造体として扱う。
/// </summary>
internal sealed class DataStructuresSection : Section {
    /// <param name="features">ソースコードを生成する順番に並んだ <see cref="FeatureOption"/></param>
    internal DataStructuresSection(IEnumerable<FeatureOption> features) {
        _features = features.ToArray();
    }

    private readonly FeatureOption[] _features;

    public override E_Section Kind => E_Section.DataStructures;

    public override void Validate(IReadOnlyList<XElement> rootAggregateElements, SchemaParseContext context, SchemaValidationErrors errors) {
        ValidateTableNames(rootAggregateElements, context, errors);

        foreach (var rootAggregateElement in rootAggregateElements) {
            foreach (var aggregate in rootAggregateElement.DescendantsAndSelf().Where(el => context.GetNodeType(el).HasFlag(E_NodeType.Aggregate))) {
                ValidateMemberNames(aggregate, context, errors);
            }
            foreach (var member in rootAggregateElement.Descendants()) {
                ValidateNodeType(member, context, errors);
            }

            foreach (var feature in GetFeaturesOf(rootAggregateElement)) {
                feature.ValidateRootAggregate(rootAggregateElement, context, errors);
            }
            if (IsStructure(rootAggregateElement)) {
                ValidateStructureRefTo(rootAggregateElement, context, errors);
            }
        }
    }

    /// <summary>
    /// 永続化されるデータ構造のテーブル名は、データ構造全体で一意でなければならない
    /// </summary>
    private static void ValidateTableNames(IEnumerable<XElement> rootAggregateElements, SchemaParseContext context, SchemaValidationErrors errors) {
        var tableNameGroups = rootAggregateElements
            .Where(root => root.IsStored())
            .SelectMany(root => root.DescendantsAndSelf())
            .Where(el => context.GetNodeType(el).HasFlag(E_NodeType.Aggregate))
            .GroupBy(el => DbNameOption.GetDbName(el));
        foreach (var group in tableNameGroups) {
            if (group.Count() == 1) continue;
            foreach (var el in group) {
                errors.Add(el, $"同じテーブル名'{group.Key}'を複数の集約で定義することはできません。");
            }
        }
    }

    /// <summary>
    /// 同じ集約の直下のメンバー同士で物理名が重複していてはならない
    /// </summary>
    private static void ValidateMemberNames(XElement aggregate, SchemaParseContext context, SchemaValidationErrors errors) {
        var appeared = new HashSet<string>();
        foreach (var member in aggregate.Elements()) {
            var physicalName = context.GetPhysicalName(member);
            if (!appeared.Add(physicalName)) {
                errors.Add(member, $"同じ親の下で物理名'{physicalName}'が重複しています。");
            }
        }
    }

    /// <summary>
    /// ルート集約の子孫のXML要素が、集約・値メンバー・外部参照のいずれかとして解釈できるかを検証します。
    /// </summary>
    private static void ValidateNodeType(XElement member, SchemaParseContext context, SchemaValidationErrors errors) {
        switch (context.GetNodeType(member)) {
            case E_NodeType.ValueMember:
                if (context.TryResolveMemberType(member, out var vmType)) {
                    vmType.Validate(member, context, errors.Add);
                }
                break;

            case E_NodeType.Ref:
                var typeAttrValue = member.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value;
                var refTo = context.FindRefTo(member);
                if (refTo == null) {
                    errors.Add(member, $"参照先が見つかりません: {typeAttrValue}");
                } else if (refTo.GetRootAggregateElement() == member.GetRootAggregateElement()) {
                    errors.Add(member, "自身のツリーの集約を参照することはできません。");
                }
                break;

            case E_NodeType.Unknown:
                var typeName = member.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value;
                errors.AddAttributeError(member, SchemaParseContext.ATTR_NODE_TYPE, string.IsNullOrEmpty(typeName)
                    ? $"ノードの種類が不明です。{SchemaParseContext.ATTR_NODE_TYPE}属性が指定されているか確認してください。"
                    : $"ノードの種類 '{typeName}' は有効ではありません。");
                break;
        }
    }

    public override void Render(CodeRenderingContext ctx) {
        var rootAggregates = ctx.Schema.GetDataStructures().ToArray();

        RenderEach(ctx, rootAggregates, rootAggregate => {
            var aggregateFile = new SourceFileByAggregate(rootAggregate);
            foreach (var feature in GetFeaturesOf(rootAggregate.XElement)) {
                feature.Render(ctx, rootAggregate, aggregateFile);
            }
            if (IsStructure(rootAggregate.XElement)) {
                RenderStructure(ctx, rootAggregate, aggregateFile);
            }
            aggregateFile.ExecuteRendering(ctx);
        });

        // ルート集約1個と対応しない、全ルート集約共通のソースコード
        foreach (var feature in _features) {
            if (rootAggregates.Any(root => GetFeaturesOf(root.XElement).Contains(feature))) {
                feature.RenderCommon(ctx);
            }
        }
        if (rootAggregates.Any(root => IsStructure(root.XElement))) {
            ctx.Use<DeepEqualFunction.OptionType>();
        }

        // データ構造が1個も無くても生成されるソースコード
        ctx.Use<AggregateMetadata>();
        ctx.Use<DbContextClass>();
        ctx.CoreLibrary(dir => {
            dir.Directory("Util", utilDir => {
                utilDir.Generate(NijoAttr.RenderDeclaration(ctx));
                utilDir.Generate(FromTo.Render(ctx));
            });
        });

        // 値メンバーの型ごとのソースコード
        foreach (var vmType in ctx.SchemaParser.GetValueMemberTypes()) {
            vmType.RegisterDependencies(ctx);
            vmType.RenderStaticSources(ctx);
        }
    }

    /// <summary>
    /// 引数の属性が、データ構造のいずれかの場所に指定できるかどうかを返します。
    /// </summary>
    internal static bool IsAvailableInDataStructures(NodeOption option) {
        return NodeOptionTarget
            .EnumerateAll()
            .Any(target => target.Section == E_Section.DataStructures && option.IsAvailable(target));
    }

    /// <summary>
    /// ルート集約に指定されている <see cref="FeatureOption"/> を列挙します。
    /// その場所に指定できない属性は、属性単位の検証でエラーになるため、ここでは除外します。
    /// </summary>
    private IEnumerable<FeatureOption> GetFeaturesOf(XElement rootAggregateElement) {
        var target = new NodeOptionTarget(Kind, rootAggregateElement.IsStored(), rootAggregateElement.IsQueryable(), E_NodeType.RootAggregate);
        return _features.Where(feature => rootAggregateElement.Attribute(feature.AttributeName) != null && feature.IsAvailable(target));
    }

    private static bool IsStructure(XElement rootAggregateElement) {
        return !rootAggregateElement.IsStored() && !rootAggregateElement.IsQueryable();
    }

    /// <summary>
    /// IsStored 属性も IsQueryable 属性も指定されていないデータ構造のルート集約1個と対応するソースコードを生成します。
    /// サーバー(C#)とクライアント(TypeScript)の双方で共有するための入れ子可能なオブジェクトの定義が生成されます。
    /// </summary>
    private static void RenderStructure(CodeRenderingContext ctx, RootAggregate rootAggregate, SourceFileByAggregate aggregateFile) {
        // C# クラス定義（ルートと子孫）
        aggregateFile.AddCSharpClass(PlainStructure.RenderCSharpRecursively(rootAggregate, ctx), "Class_Structure");

        // TypeScript 型定義（ルートと子孫）
        aggregateFile.AddTypeScriptTypeDef(PlainStructure.RenderTypeScriptRecursively(rootAggregate, ctx));

        // TypeScript 新規オブジェクト作成関数（ルートと子孫）
        aggregateFile.AddTypeScriptTypeDef(PlainStructure.RenderTsNewObjectFunctionRecursively(rootAggregate, ctx));

        // この構造体がいずれかのコマンドの引数or戻り値として参照されている場合、
        // プレゼンテーション層での編集用のオブジェクト等を生成する。
        var refferdAsParameter = CommandRoot.IsReferredAsParameter(rootAggregate);
        var refferdAsReturnValue = CommandRoot.IsReferredAsReturnValue(rootAggregate);
        StructureDisplayData? displayData = null;
        if (refferdAsParameter || refferdAsReturnValue) {
            // 画面表示用データ
            displayData = new StructureDisplayData(rootAggregate);
            aggregateFile.AddCSharpClass(StructureDisplayData.RenderCSharpRecursively(rootAggregate, ctx), "Class_DisplayData");
            aggregateFile.AddTypeScriptTypeDef(StructureDisplayData.RenderTypeScriptRecursively(rootAggregate, ctx));
            aggregateFile.AddTypeScriptFunction(EditablePresentationObject.RenderTsNewObjectFunctionRecursively(displayData, ctx));
            aggregateFile.AddTypeScriptFunction(new DeepEqualFunction(displayData).Render(ctx));
        }
        if (refferdAsParameter) {
            // メッセージコンテナ
            var messageContainer = new StructureDisplayDataMessageContainer(rootAggregate);
            aggregateFile.AddCSharpClass(StructureDisplayDataMessageContainer.RenderCSharpRecursively(rootAggregate), "Class_DisplayDataMessage");
            ctx.Use<Parts.CSharp.MessageContainer.BaseClass>().Register(messageContainer.CsClassName, messageContainer.CsClassName);
        }

        // TypeScriptのマッピングファイルへの登録
        ctx.Use<CommandQueryMappings>().AddStructureModel(rootAggregate);

        // 定数: メタデータ
        aggregateFile.AddTypeScriptFunction(AggregateMetadata.RenderTsConstant(rootAggregate, displayData, null, ctx));
    }

    /// <summary>
    /// IsStored 属性も IsQueryable 属性も指定されていないデータ構造からの外部参照の制約を検証します。
    /// </summary>
    private static void ValidateStructureRefTo(XElement rootAggregateElement, SchemaParseContext context, SchemaValidationErrors errors) {
        foreach (var (refElement, _, refToRoot) in context.EnumerateRefsToOtherTrees(rootAggregateElement)) {
            var refToIsQueryable = refToRoot.IsQueryable();

            // 検索可能なデータ構造または構造体しか参照できない
            if (refToRoot.IsStored() && !refToIsQueryable) {
                errors.Add(refElement, $"{IsStoredOption.ATTRIBUTE_NAME}属性も{IsQueryableOption.ATTRIBUTE_NAME}属性も指定されていないデータ構造からは、{IsQueryableOption.ATTRIBUTE_NAME}属性が指定されたデータ構造、または{IsStoredOption.ATTRIBUTE_NAME}属性も{IsQueryableOption.ATTRIBUTE_NAME}属性も指定されていないデータ構造しか参照できません。");
                continue;
            }

            // 検索可能なデータ構造からはどのオブジェクトを参照するかが一意に決まらないため、明示が必要
            if (refToIsQueryable && refElement.Attribute(RefToObjectOption.ATTRIBUTE_NAME) == null) {
                errors.Add(refElement, $"{IsQueryableOption.ATTRIBUTE_NAME}属性が指定されたデータ構造を外部参照する場合、{RefToObjectOption.ATTRIBUTE_NAME}属性を指定する必要があります。");
            }
        }
    }
}
