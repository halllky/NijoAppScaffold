using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using System;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// EF Core Entity の生成を担当する移植先。
    /// </summary>
    internal class EFCoreEntity {
        internal EFCoreEntity(AggregateBase aggregate) {
            Aggregate = aggregate;
        }

        internal AggregateBase Aggregate { get; }
        internal string ClassName => $"{Aggregate.PhysicalName}DbEntity";
        internal string DbSetName => $"{Aggregate.PhysicalName}DbSet";

        /// <summary>
        /// EF Core エンティティの宣言コードをレンダリングする。
        /// </summary>
        /// <remarks>
        /// 実装方針: 旧版 WriteModel2Features.EFCoreEntity の責務を、
        /// 現行 immutable schema と DbContextClass が扱える形へ変換する。
        /// 実装時は旧版の独自実装をそのまま再現するのではなく、現行 Nijo.Parts.CSharp.EFCoreEntity と差分がある箇所だけを明示して寄せる。
        /// </remarks>
        internal string Render(CodeRenderingContext ctx) {
            if (Aggregate is not RootAggregate rootAggregate) {
                throw new InvalidOperationException($"{nameof(Render)} はルート集約に対してのみ呼び出してください。");
            }

            if (ctx.IsLegacyCompatibilityMode()) {
                return LegacyEFCoreEntity.Render(rootAggregate, ctx);
            }

            return Nijo.Parts.CSharp.EFCoreEntity.RenderClassDeclaring(new Nijo.Parts.CSharp.EFCoreEntity(rootAggregate), ctx);
        }

        /// <summary>
        /// DbContext 登録用の EFCoreEntity メタデータへ変換する。
        /// </summary>
        /// <remarks>
        /// 実装方針: WriteModel2 専用のエンティティ定義から、既存 DbContextClass が受け取れる IEFCoreEntity を返す。
        /// ラップ対象は新実装の自己変換でも、当面は Nijo.Parts.CSharp.EFCoreEntity への委譲でもよいが、差分箇所をコメントで固定する。
        /// </remarks>
        internal IEFCoreEntity AsIEFCoreEntity() {
            // 現時点では DbContextClass が要求する契約を現行実装が満たしているため委譲する。
            return new Nijo.Parts.CSharp.EFCoreEntity(Aggregate);
        }
    }
}
