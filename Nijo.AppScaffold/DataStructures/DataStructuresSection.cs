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

    public override void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        foreach (var feature in GetFeaturesOf(rootAggregateElement)) {
            feature.ValidateRootAggregate(rootAggregateElement, context, addError);
        }
        if (IsStructure(rootAggregateElement)) {
            ValidateStructureRefTo(rootAggregateElement, context, addError);
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
    /// IsStored 属性も IsQueryable 属性も指定されていないデータ構造の外部参照をチェックします
    /// </summary>
    private static void ValidateStructureRefTo(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // 自身を起点とするすべての外部参照を取得
        var refElements = rootAggregateElement
            .Descendants()
            .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value?.StartsWith(SchemaParseContext.NODE_TYPE_REFTO + ":") == true)
            .ToList();

        if (!refElements.Any()) return;

        foreach (var refElement in refElements) {
            var refTo = context.FindRefTo(refElement);
            if (refTo == null) {
                addError(refElement, $"参照先の要素が見つかりません: {refElement.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value}");
                continue;
            }

            // 自身のツリーの集約を参照していないかチェック
            var rootElement = refElement.GetRootAggregateElement();
            var refToRoot = refTo.GetRootAggregateElement();

            if (rootElement == refToRoot) {
                addError(refElement, "自身のツリーの集約を参照することはできません。");
                continue;
            }

            // 参照先が検索可能なデータ構造、または構造体か確認
            var isQueryable = refToRoot.IsQueryable();
            var isStructure = !refToRoot.IsStored() && !isQueryable;

            if (!isQueryable && !isStructure) {
                addError(refElement, $"{IsStoredOption.ATTRIBUTE_NAME}属性も{IsQueryableOption.ATTRIBUTE_NAME}属性も指定されていないデータ構造からは、{IsQueryableOption.ATTRIBUTE_NAME}属性が指定されたデータ構造、または{IsStoredOption.ATTRIBUTE_NAME}属性も{IsQueryableOption.ATTRIBUTE_NAME}属性も指定されていないデータ構造しか参照できません。");
                continue;
            }

            // 検索可能なデータ構造を参照する場合はRefToObjectの指定が必須
            var refToObject = refElement.Attribute(RefToObjectOption.ATTRIBUTE_NAME);
            if (isQueryable && refToObject == null) {
                addError(refElement, $"{IsQueryableOption.ATTRIBUTE_NAME}属性が指定されたデータ構造を外部参照する場合、{RefToObjectOption.ATTRIBUTE_NAME}属性を指定する必要があります。");
            }

            // RefToObjectの値が不正かチェック
            if (isQueryable
                    && refToObject != null
                    && !RefToObjectOption.AvailableFromStructureToQuery.Contains(refToObject.Value)) {
                addError(refElement, $"{RefToObjectOption.ATTRIBUTE_NAME}属性の値は {string.Join(" または ", RefToObjectOption.AvailableFromStructureToQuery)} のいずれかを指定してください。");
            }

            // 検索条件を参照する場合はルート集約のみ参照可能
            if (refToObject?.Value == RefToObjectOption.REF_TO_OBJECT_SEARCH_CONDITION
                    && refTo != refToRoot) {
                addError(refElement, $"検索条件を参照する場合はルート集約のみ指定可能です。");
            }

            // 構造体を参照する場合はRefToObjectの指定は不要（指定されていても無視）
        }
    }
}
