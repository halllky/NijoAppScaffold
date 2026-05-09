using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.WriteModel2Modules;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using Nijo.SchemaParsing;
using System;
using System.Linq;
using System.Xml.Linq;
using WriteModel2EFCoreEntity = Nijo.Models.WriteModel2Modules.EFCoreEntity;

namespace Nijo.Models {
    /// <summary>
    /// 旧版互換の更新系モデル。
    /// 入力の解釈は現行の parser / immutable schema に揃え、
    /// 旧版互換の出力は WriteModel2 専用モジュール群で段階的に実装する。
    /// </summary>
    internal class WriteModel2 : IModel {
        internal const string SCHEMA_NAME = "write-model-2";

        public string SchemaName => SCHEMA_NAME;

        public void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            foreach (var aggregateElement in rootAggregateElement.DescendantsAndSelf().Where(IsRootOrChildrenAggregate)) {
                var hasOwnKey = aggregateElement.Elements().Any(member => member.Attribute(BasicNodeOptions.IsKey.AttributeName) != null);
                if (!hasOwnKey) {
                    addError(aggregateElement, $"{aggregateElement.GetDisplayName()}にキーが1つもありません。");
                }
            }

            foreach (var refElement in rootAggregateElement.Descendants().Where(IsRefToElement)) {
                var refTo = context.FindRefTo(refElement);
                if (refTo == null) continue;

                if (!context.TryGetModel(refTo.GetRootAggregateElement(), out var refToModel)) continue;
                if (refToModel.SchemaName == SCHEMA_NAME) continue;

                var ownerAggregate = refElement.Parent ?? rootAggregateElement;
                addError(refElement, $"{ownerAggregate.GetDisplayName()}.{refElement.GetDisplayName()}: {nameof(WriteModel2)}の参照先は{nameof(WriteModel2)}である必要があります。");
            }

            static bool IsRootOrChildrenAggregate(XElement element) {
                return element.Parent?.Parent == element.Document?.Root
                    || element.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILDREN;
            }

            static bool IsRefToElement(XElement element) {
                return element.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value?.StartsWith($"{SchemaParseContext.NODE_TYPE_REFTO}:", StringComparison.Ordinal) == true;
            }
        }

        public void GenerateCode(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            if (!ctx.IsLegacyCompatibilityMode()) throw new InvalidOperationException("旧版互換モードでのみ利用可能");

            var aggregateFile = new SourceFileByAggregate(rootAggregate);

            // 集約横断で使う保存用共通型
            ctx.Use<DataClassForSaveBase>().Register(rootAggregate);

            ctx.Use<LegacyDbContextClass>().AddEntities(rootAggregate
                .EnumerateThisAndDescendants()
                .Select(aggregate => new WriteModel2EFCoreEntity(aggregate).AsIEFCoreEntity()));

            foreach (var aggregate in rootAggregate.EnumerateThisAndDescendants()) {
                // EF Core Entity
                var efCoreEntity = new WriteModel2EFCoreEntity(aggregate);
                aggregateFile.AddCSharpClass(efCoreEntity.Render(ctx), "Class_EFCoreEntity");

                // 保存用 DTO 群
                var createDataClass = new DataClassForSave(aggregate, DataClassForSave.E_Type.Create);
                aggregateFile.AddCSharpClass(createDataClass.RenderCSharp(ctx), "Class_DataClassForCreate");
                aggregateFile.AddCSharpClass(createDataClass.RenderCSharpReadOnlyStructure(ctx), "Class_DataClassForCreateReadOnly");
                aggregateFile.AddTypeScriptTypeDef(createDataClass.RenderTypeScript(ctx));
                aggregateFile.AddTypeScriptTypeDef(createDataClass.RenderTypeScriptReadOnlyStructure(ctx));

                var saveDataClass = new DataClassForSave(aggregate, DataClassForSave.E_Type.UpdateOrDelete);
                aggregateFile.AddCSharpClass(saveDataClass.RenderCSharp(ctx), "Class_DataClassForSave");
                aggregateFile.AddCSharpClass(saveDataClass.RenderCSharpMessageStructure(ctx), "Class_DataClassForSaveMessage");
                aggregateFile.AddCSharpClass(saveDataClass.RenderCSharpReadOnlyStructure(ctx), "Class_DataClassForSaveReadOnly");
                aggregateFile.AddTypeScriptTypeDef(saveDataClass.RenderTypeScript(ctx));
                aggregateFile.AddTypeScriptTypeDef(saveDataClass.RenderTypeScriptReadOnlyStructure(ctx));

                if (aggregate is RootAggregate or ChildrenAggregate) {
                    aggregateFile.AddTypeScriptFunction(createDataClass.RenderTsNewObjectFunction(ctx));
                    aggregateFile.AddTypeScriptFunction(saveDataClass.RenderTsNewObjectFunction(ctx));
                }

                // 他集約から参照されるときのキー型
                var refTargetKeys = new DataClassForRefTargetKeys(aggregate, aggregate);
                aggregateFile.AddCSharpClass(refTargetKeys.RenderCSharpDeclaringRecursively(ctx), "Class_RefTargetKeys");
                aggregateFile.AddTypeScriptTypeDef(refTargetKeys.RenderTypeScriptDeclaringRecursively(ctx));
            }

            // 一括更新系メッセージ
            ctx.Use<SaveContext>().AddWriteModel(rootAggregate);

            // 一括更新処理
            if (rootAggregate.GenerateBatchUpdateCommand) {
                ctx.Use<BatchUpdateWriteModel>().Register(rootAggregate);
            }

            var hasSequenceMember = rootAggregate
                .EnumerateThisAndDescendants()
                .SelectMany(aggregate => aggregate.GetMembers())
                .OfType<ValueMember>()
                .Any(member => member.Type is ValueMemberTypes.SequenceMember && !string.IsNullOrWhiteSpace(member.SequenceName));

            // 登録・更新・削除 AppSrv
            var legacyAppSrvSections = new[] {
                LegacyUpdateMethod.Render(rootAggregate, ctx),
                LegacyDeleteMethod.Render(rootAggregate, ctx),
                RequiredCheck.Render(rootAggregate, ctx),
                MaxLengthCheck.Render(rootAggregate, ctx),
                NotNegativeCheck.Render(rootAggregate, ctx),
                CharacterTypeCheck.Render(rootAggregate, ctx),
                DigitsCheck.Render(rootAggregate, ctx),
                CharacterTypeCheck.RenderLegacyKbnType(rootAggregate),
            }
            .Concat(hasSequenceMember ? [new GenerateAndSetSequenceMethod(rootAggregate).RenderAppSrvMethod(ctx)] : [])
            .ToArray();
            var legacyAppSrvMethods = string.Join(Environment.NewLine, legacyAppSrvSections.Select(source => source.TrimEnd('\r', '\n')));
            aggregateFile.AddAppSrvMethod(LegacyCreateMethod.Render(rootAggregate, ctx), "新規登録処理");
            aggregateFile.AddAppSrvMethod(legacyAppSrvMethods, "更新処理");

            // WriteModel2 と同形の QueryModel 生成
            if (rootAggregate.GenerateDefaultQueryModel) {
                QueryModel.GenerateCode(ctx, rootAggregate, aggregateFile);
            }

            Nijo.Models.DataModelModules.GenericLookupTableFeature.GenerateCode(ctx, rootAggregate, aggregateFile);

            // ダミーデータ、メタデータ、既存の共通マッピング
            ctx.Use<DummyDataGenerator>().Add(rootAggregate);
            ctx.Use<WriteModelMetadata>().Register(rootAggregate);

            aggregateFile.ExecuteRendering(ctx);
        }

        public void GenerateCode(CodeRenderingContext ctx) {
        }
    }
}
