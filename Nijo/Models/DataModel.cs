using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.Models {

    /// <summary>
    /// データモデル。
    /// アプリケーションに永続化されるデータの形を表す。
    /// トランザクションの境界の単位（より強い整合性の範囲）で区切られる。
    /// データモデルの境界を跨ぐエラーチェックは、一時的に整合性が崩れる可能性がある。
    /// DDD（ドメイン駆動設計）における集約ルートの概念とほぼ同じ。
    /// IsStored 属性が指定されたデータ構造から生成される。
    /// </summary>
    internal static class DataModel {

        /// <summary>
        /// ルート集約1個と対応するソースコードを生成します。
        /// </summary>
        internal static void Render(CodeRenderingContext ctx, RootAggregate rootAggregate, SourceFileByAggregate aggregateFile) {
            // データ型: EFCore Entity
            var efCoreEntity = new EFCoreEntity(rootAggregate);
            aggregateFile.AddCSharpClass(EFCoreEntity.RenderClassDeclaring(efCoreEntity, ctx), "Class_EFCoreEntity");
            ctx.Use<DbContextClass>().AddEntities(efCoreEntity.EnumerateThisAndDescendants());

            // データ型: EFCore Entity（論理削除用）
            if (rootAggregate.UseSoftDelete) {
                var deletedEfCoreEntity = new EFCoreEntity(rootAggregate, isDeletedTable: true);
                aggregateFile.AddCSharpClass(EFCoreEntity.RenderClassDeclaring(deletedEfCoreEntity, ctx), "Class_EFCoreEntity_Deleted");
                ctx.Use<DbContextClass>().AddEntities(deletedEfCoreEntity.EnumerateThisAndDescendants());
            }

            // 汎用参照テーブル: カテゴリごとのビューエンティティ・ユーティリティ・静的メソッドを生成
            GenericLookupTableFeature.GenerateCode(ctx, rootAggregate, aggregateFile);

            // データ型: SaveCommand
            aggregateFile.AddCSharpClass(SaveCommand.RenderAll(rootAggregate, ctx), "Class_SaveCommand");

            // データ型: SaveCommandメッセージ
            var saveCommandMessage = new SaveCommandMessageContainer(rootAggregate);
            aggregateFile.AddCSharpClass(SaveCommandMessageContainer.RenderTree(rootAggregate), "Class_SaveCommandMessage");
            ctx.Use<MessageContainer.BaseClass>()
                .Register(saveCommandMessage.InterfaceName, saveCommandMessage.CsClassName)
                .Register(saveCommandMessage.CsClassName, saveCommandMessage.CsClassName);

            // 処理: 新規登録、更新
            var descendantStructurePaths = efCoreEntity
                .CreateStructurePathsRecursively()
                .ToDictionary(x => x.Metadata.SchemaPathNode.ToMappingKey());
            var create = new CreateMethod(rootAggregate);
            var update = new UpdateMethod(rootAggregate);
            aggregateFile.AddAppSrvMethod(create.Render(ctx, descendantStructurePaths), "新規登録処理");
            aggregateFile.AddAppSrvMethod(update.Render(ctx, descendantStructurePaths), "更新処理");

            // 処理: 削除
            if (rootAggregate.UseSoftDelete) {
                var softDelete = new SoftDeleteMethods(rootAggregate);
                aggregateFile.AddAppSrvMethod(softDelete.Render(ctx, descendantStructurePaths), "論理削除処理");
            } else {
                var delete = new DeleteMethod(rootAggregate);
                aggregateFile.AddAppSrvMethod(delete.Render(ctx, descendantStructurePaths), "物理削除処理");
            }

            // 処理: 自動生成されるバリデーションエラーチェック
            aggregateFile.AddAppSrvMethod($$"""
                #region 自動生成されるバリデーション処理
                {{GetValidators(ctx).SelectTextTemplate(validator => $$"""
                {{validator.RenderDeclaring(rootAggregate, ctx)}}
                """)}}
                #endregion 自動生成されるバリデーション処理
                """, "バリデーション処理");

            // データ型: ほかの集約から参照されるときのキー
            aggregateFile.AddCSharpClass(KeyClass.KeyClassEntry.RenderClassDeclaringRecursively(rootAggregate, ctx), "Class_KeyClass");

            // カスタムロジック用モジュール
            ctx.Use<CommandQueryMappings>().AddDataModel(rootAggregate);
        }

        /// <summary>
        /// すべてのデータモデルで共通して使われる、ルート集約1個と対応しないソースコードを生成します。
        /// </summary>
        internal static void RenderCommonModules(CodeRenderingContext ctx) {
            // 保存処理の結果クラス
            ctx.CoreLibrary(dir => {
                dir.Directory("Util", utilDir => {
                    utilDir.Generate(DataModelSaveResult.Render(ctx));
                });
            });

            // メッセージコンテナ
            UpdateMethod.RegisterCommonParts(ctx);

            // バリデーターチェック違反時のメッセージ
            var msgFactory = ctx.Use<MsgFactory>();
            var validatorsForMessage = GetValidators(ctx)
                .GroupBy(v => v.MsgId)
                .Select(g => g.First());

            foreach (var validator in validatorsForMessage) {
                msgFactory.AddMessage(
                    validator.MsgId,
                    $"入力エラー時メッセージ（{validator.CommentName}）",
                    validator.MsgTemplate);
            }
        }

        /// <summary>
        /// データモデルのバリデーターを列挙します。
        /// </summary>
        internal static IEnumerable<ValidatorBase> GetValidators(CodeRenderingContext ctx) {
            yield return new ValidateRequired();
            yield return new ValidateGenericLookupExists();
            yield return new ValidateMaxLength();
            yield return new ValidateDigitsAndScales();

            var customValidationAttributes = NijoXmlCustomAttribute
                .FromXDocument(ctx.SchemaParser.Document)
                .Where(attr => attr.IsValidation
                            && !string.IsNullOrWhiteSpace(attr.UniqueId)
                            && !string.IsNullOrWhiteSpace(attr.PhysicalName));
            foreach (var attr in customValidationAttributes) {
                yield return new ValidateCustom(attr);
            }
        }
    }
}
