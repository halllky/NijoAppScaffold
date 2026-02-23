using Microsoft.EntityFrameworkCore;
using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.CSharp;

/// <summary>
/// EFCoreのナビゲーションプロパティ
/// </summary>
internal abstract class NavigationProperty {
    internal abstract PrincipalOrRelevant Principal { get; }
    internal abstract PrincipalOrRelevant Relevant { get; }

    /// <summary>HasOneWithOneのときだけ設定時に型引数が必要らしいので</summary>
    internal bool IsOneToOne => !Principal.OtherSideIsMany && !Relevant.OtherSideIsMany;
    /// <summary>RDBMS上で主たるエンティティが削除されたときの挙動</summary>
    internal abstract DeleteBehavior PrincipalDeletedBehavior { get; }
    /// <summary>RDBMS上の制約の物理名</summary>
    internal abstract string GetConstraintName();
    /// <summary>外部キー項目を列挙</summary>
    internal abstract IEnumerable<EFCoreEntity.EFCoreEntityColumn> GetRelevantForeignKeys();

    /// <summary>
    /// 集約から適切なインスタンスを取得
    /// </summary>
    protected static IEFCoreEntity GetConcreteClass(AggregateBase aggregate) {
        var root = aggregate.GetRoot();
        if (root.Model is Models.QueryModel && root.IsView) {
            // QueryModelのビューの場合はSearchResultを返す
            if (aggregate is RootAggregate rootAgg) {
                return new Models.QueryModelModules.SearchResult(rootAgg);
            } else if (aggregate is ChildrenAggregate children) {
                return new Models.QueryModelModules.SearchResult.SearchResultChildrenMember(children, false);
            } else {
                throw new InvalidOperationException("SearchResultのナビゲーションプロパティの集約が不正です");
            }
        }
        // それ以外はEFCoreEntityを返す
        return new EFCoreEntity(aggregate);
    }

    public override string ToString() {
        // デバッグ用
        return $"Principal = {Principal.ThisSide}, Relevant = {Relevant.ThisSide}";
    }

    internal class PrincipalOrRelevant : IInstanceStructurePropertyMetadata {
        internal required NavigationProperty NavigationProperty { get; init; }
        internal required AggregateBase ThisSide { get; init; }
        internal required AggregateBase OtherSide { get; init; }
        internal required string OtherSidePhysicalName { get; init; }
        internal required bool OtherSideIsMany { get; init; }

        bool IInstanceStructurePropertyMetadata.IsArray => OtherSideIsMany;
        ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => OtherSide;
        string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => OtherSidePhysicalName;
        string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => GetConcreteClass(OtherSide).CsClassName;
        IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() {
            var otherSideEfCoreEntity = GetConcreteClass(OtherSide);
            var otherSideMetadata = otherSideEfCoreEntity as IInstancePropertyOwnerMetadata
                ?? throw new InvalidOperationException($"インスタンスが必要なインターフェースを実装していません: {otherSideEfCoreEntity.GetType()}");

            foreach (var member in otherSideMetadata.GetMembers()) {
                // 無限ループに陥るのでこのインスタンス自身は列挙しない
                if (member is PrincipalOrRelevant por && por.OtherSide == ThisSide) continue;

                // 無限ループに陥るので被参照ナビゲーションプロパティは列挙しない。
                // IInstancePropertyOwnerMetadata.GetMembers で被参照ナビゲーションプロパティを列挙したい状況もおそらく無いはず……多分
                if (NavigationProperty is NavigationOfRef refNav && refNav.Relation.RefTo == ThisSide) continue;

                yield return member;
            }
        }

        /// <summary>C#型名</summary>
        /// <param name="withNullable">末尾にNull許容演算子をつけるかどうか</param>
        internal string GetOtherSideCsTypeName(bool withNullable = false) {
            var className = GetConcreteClass(OtherSide).CsClassName;
            if (OtherSideIsMany) {
                return $"ICollection<{className}>";
            } else {
                return withNullable
                    ? $"{className}?"
                    : className;
            }
        }
        /// <summary>プロパティ初期化式</summary>
        internal string GetInitializerStatement() {
            if (OtherSideIsMany) {
                return $" = [];";
            } else {
                return string.Empty;
            }
        }

        public override string ToString() {
            // デバッグ用
            return $"({ThisSide.DisplayName}).{OtherSidePhysicalName}";
        }
    }

    /// <summary>
    /// 親子間のナビゲーションプロパティ
    /// </summary>
    internal class NavigationOfParentChild : NavigationProperty {
        internal NavigationOfParentChild(AggregateBase parent, AggregateBase child) {
            Principal = new() {
                NavigationProperty = this,
                ThisSide = parent,
                OtherSide = child,
                OtherSideIsMany = child is ChildrenAggregate,
                OtherSidePhysicalName = child.PhysicalName,
            };
            Relevant = new() {
                NavigationProperty = this,
                ThisSide = child,
                OtherSide = parent,
                OtherSideIsMany = false,
                OtherSidePhysicalName = "Parent",
            };
        }
        internal override PrincipalOrRelevant Principal { get; }
        internal override PrincipalOrRelevant Relevant { get; }

        internal override DeleteBehavior PrincipalDeletedBehavior => DeleteBehavior.Cascade;

        internal override string GetConstraintName() {
            return $"FK_{Principal.ThisSide.DbName}_{Relevant.ThisSide.DbName}";
        }
        internal override IEnumerable<EFCoreEntity.EFCoreEntityColumn> GetRelevantForeignKeys() {
            var child = GetConcreteClass(Relevant.ThisSide);
            var childColumns = child.GetColumns();

            // 子の主キーのうち、親の主キーのいずれかとマッピングキーが合致するものが親子間の外部キー
            var parentKeys = Principal.ThisSide
                .GetKeyVMs()
                .Select(vm => vm.ToMappingKey())
                .ToHashSet();
            return childColumns.Where(c => parentKeys.Contains(c.Member.ToMappingKey()));
        }
    }

    /// <summary>
    /// 外部参照のナビゲーションプロパティ
    /// </summary>
    internal class NavigationOfRef : NavigationProperty {
        public NavigationOfRef(RefToMember relation) {
            Relation = relation;

            var hasUniqueConstraintOnlyForThisRef = relation.Owner
                .GetUniqueConstraints()
                .Any(c => c.IsSingleRefTo(relation));
            var isOneToOne = hasUniqueConstraintOnlyForThisRef
                || relation.RefTo.IsSingleKeyOf(relation.Owner);

            Principal = new() {
                NavigationProperty = this,
                ThisSide = relation.RefTo,
                OtherSide = relation.Owner,
                OtherSideIsMany = !isOneToOne,
                OtherSidePhysicalName = $"RefFrom{relation.Owner.PhysicalName}_{relation.PhysicalName}",
            };
            Relevant = new() {
                NavigationProperty = this,
                ThisSide = relation.Owner,
                OtherSide = relation.RefTo,
                OtherSideIsMany = false,
                OtherSidePhysicalName = relation.PhysicalName,
            };
        }
        internal RefToMember Relation { get; }

        internal override PrincipalOrRelevant Principal { get; }
        internal override PrincipalOrRelevant Relevant { get; }

        internal override DeleteBehavior PrincipalDeletedBehavior => DeleteBehavior.NoAction;

        internal override string GetConstraintName() {
            // 同じテーブルから同じテーブルへ複数の参照経路があるときのための物理名衝突回避用ハッシュ
            var hash = Relation.PhysicalName.ToHashedString().ToUpper().Substring(0, 8);

            return $"FK_{Principal.ThisSide.DbName}_{Relevant.ThisSide.DbName}_{hash}";
        }
        internal override IEnumerable<EFCoreEntity.EFCoreEntityColumn> GetRelevantForeignKeys() {
            var refFrom = GetConcreteClass(Relevant.ThisSide);
            return refFrom
                .GetColumns()
                .Where(col => col is EFCoreEntity.RefKeyMember rm
                           && rm.RefEntry == Relation);
        }
    }
}
