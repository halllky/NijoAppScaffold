using Microsoft.EntityFrameworkCore;
using Nijo.Core;
using Nijo.Core.AggregateMemberTypes;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.WriteModel2Features {
    /// <summary>
    /// Entity Framework Core のエンティティ
    /// </summary>
    internal class EFCoreEntity {
        internal EFCoreEntity(GraphNode<Aggregate> agg) {
            _aggregate = agg;
        }
        internal GraphNode<Aggregate> Aggregate => _aggregate;
        private readonly GraphNode<Aggregate> _aggregate;

        internal string ClassName => _aggregate.Item.EFCoreEntityClassName;
        internal string DbSetName => $"{_aggregate.Item.PhysicalName}DbSet";

        /// <summary>楽観排他制御用のバージョニング用カラムの名前</summary>
        internal const string VERSION = "Version";

        /// <summary>データが新規作成された日時</summary>
        internal const string CREATED_AT = "CreatedAt";
        /// <summary>データが更新された日時</summary>
        internal const string UPDATED_AT = "UpdatedAt";

        /// <summary>データを新規作成したユーザー</summary>
        internal const string CREATE_USER = "CreateUser";
        /// <summary>データを更新したユーザー</summary>
        internal const string UPDATE_USER = "UpdateUser";

        /// <summary>主キーが一致するかどうかを調べるメソッドの名前</summary>
        internal const string KEYEQUALS = "KeyEquals";
        /// <summary>登録日時など以外の項目に何らかの差異があるかどうかを調べるメソッドの名前</summary>
        internal const string HASCHANDES = "HasChanges";

        /// <summary>
        /// このエンティティに関するテーブルやカラムの詳細を定義する処理（"Fluent API  Entity FrameWork Core" で調べて）を
        /// エンティティクラス内にstaticメソッドで記述することにしているが、そのstaticメソッドの名前
        /// </summary>
        private string OnModelCreating => $"OnModelCreating{_aggregate.Item.PhysicalName}";

        /// <summary>
        /// このエンティティのテーブルに属するカラムと対応するメンバーを列挙します。
        /// </summary>
        internal IEnumerable<AggregateMember.ValueMember> GetTableColumnMembers() {
            foreach (var member in _aggregate.GetMembers()) {
                if (member is not AggregateMember.ValueMember vm) continue;
                if (vm.Inherits?.GetRefForeignKeyProxy() != null) continue;

                yield return vm;
            }
        }

        /// <summary>
        /// このエンティティがもつナビゲーションプロパティを、
        /// このエンティティがPrincipal側かRelevant側かを考慮しつつ列挙します。
        /// </summary>
        internal IEnumerable<NavigationProperty.PrincipalOrRelevant> GetNavigationPropertiesThisSide() {
            foreach (var nav in GetNavigationProperties()) {
                if (nav.Principal.Owner == _aggregate) yield return nav.Principal;
                if (nav.Relevant.Owner == _aggregate) yield return nav.Relevant;
            }
        }
        /// <summary>
        /// このエンティティがもつナビゲーションプロパティを列挙します。
        /// このエンティティがPrincipal側かRelevant側かは考慮しません。
        /// </summary>
        internal IEnumerable<NavigationProperty> GetNavigationProperties() {
            foreach (var member in _aggregate.GetMembers()) {
                if (member is not AggregateMember.RelationMember relationMember) continue;
                yield return relationMember.GetNavigationProperty();
            }

            foreach (var refered in _aggregate.GetReferedEdges()) {
                if (!refered.Initial.IsStored()) continue;
                yield return new NavigationProperty(refered);
            }
        }

        /// <summary>
        /// エンティティクラス定義をレンダリングします。
        /// </summary>
        internal string Render(CodeRenderingContext context) {
            var sequences = _aggregate
                .GetMembers()
                .OfType<AggregateMember.ValueMember>()
                .Where(vm => vm.Options.MemberType is SequenceMember)
                .ToArray();

            // 各カラムの定義を生成
            var columnOrder = 0;
            var membersSourceCode = new List<string>();
            foreach (var member in _aggregate.GetMembersOrderByStrict()) {

                if (member is AggregateMember.ValueMember vm
                    && vm.Inherits?.GetRefForeignKeyProxy() == null) {

                    // EFCoreは、C#側の型が int または long で、かつその項目が主キーの場合、
                    // 暗黙的にシーケンスオブジェクトを作成してしまうが、これは意図した挙動でないため、オフにする
                    var csType = vm.Options.MemberType.GetCSharpTypeName();
                    var valueGeneratedNever = (csType == "int" || csType == "long") && vm.IsKey;

                    var sourceCode = $$"""
                        entity.Property(e => e.{{vm.MemberName}})
                            .HasColumnName("{{vm.DbColumnName}}")
                            .HasComment("{{vm.MemberName}}")
                        {{If(vm.Options.TotalDigits != null, () => $$"""
                            .HasPrecision({{vm.Options.TotalDigits}}, {{(vm.Options.FractionalDigits?.ToString() ?? "0")}})
                        """)}}
                        {{If(valueGeneratedNever, () => $$"""
                            .ValueGeneratedNever() // 暗黙的シーケンスの生成をオフにする
                        """)}}
                        {{If(vm.Options.MaxLength != null, () => $$"""
                            .HasMaxLength({{vm.Options.MaxLength}})
                        """)}}
                            .IsRequired({{(vm.IsRequired ? "true" : "false")}})
                            .HasColumnOrder({{columnOrder++}});
                        """;

                    membersSourceCode.Add(sourceCode);

                } else if (member is AggregateMember.RelationMember rm) {

                    var nav = new NavigationProperty(rm.Relation);
                    if (nav.Principal.Owner != _aggregate) continue;

                    membersSourceCode.Add(RenderNavigationPropertyOnModelCreating(nav));
                }
            }

            // このエンティティが参照される側である関係性のナビゲーションプロパティを作成する
            var referedSourceCode = new List<string>();
            foreach (var refered in _aggregate.GetReferedEdges()) {
                if (!refered.Initial.IsStored()) continue;

                var nav = new NavigationProperty(refered);
                if (nav.Principal.Owner != _aggregate) continue;

                referedSourceCode.Add(RenderNavigationPropertyOnModelCreating(nav));
            }


            return $$"""
                /// <summary>
                /// Entity Framework Core のルールに則った{{_aggregate.Item.DisplayName}}のデータ型
                /// </summary>
                public partial class {{_aggregate.Item.EFCoreEntityClassName}} {
                {{GetTableColumnMembers().SelectTextTemplate(col => $$"""
                    public {{col.Options.MemberType.GetCSharpTypeName()}}? {{col.MemberName}} { get; set; }
                """)}}
                {{If(_aggregate.IsRoot(), () => $$"""
                    /// <summary>楽観排他制御用のバージョニング用カラム</summary>
                    public int? {{VERSION}} { get; set; }
                """)}}
                    /// <summary>データが新規作成された日時</summary>
                    public DateTime? {{CREATED_AT}} { get; set; }
                    /// <summary>データが更新された日時</summary>
                    public DateTime? {{UPDATED_AT}} { get; set; }
                    /// <summary>データを新規作成したユーザー</summary>
                    public string? {{CREATE_USER}} { get; set; }
                    /// <summary>データを更新したユーザー</summary>
                    public string? {{UPDATE_USER}} { get; set; }

                {{GetNavigationPropertiesThisSide().SelectTextTemplate(nav => $$"""
                    public virtual {{nav.CSharpTypeName}} {{nav.PropertyName}} { get; set; }{{nav.Initializer}}
                """)}}

                    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
                    public bool {{KEYEQUALS}}({{ClassName}} entity) {
                {{_aggregate.GetKeys().OfType<AggregateMember.ValueMember>().Where(col => col.Inherits?.GetRefForeignKeyProxy() == null).SelectTextTemplate(col => $$"""
                        if (entity.{{col.MemberName}} != this.{{col.MemberName}}) return false;
                """)}}
                        return true;
                    }
                    /// <summary>
                    /// このオブジェクトと引数のオブジェクトに、何らかの差分があるかどうかを調べます。
                    /// 登録日時等はこのメソッドの判定から除外されます。
                    /// </summary>
                    public bool {{HASCHANDES}}({{ClassName}} entity) {
                {{GetTableColumnMembers().SelectTextTemplate(col => $$"""
                        if (this.{{col.MemberName}} != entity.{{col.MemberName}}) return true;
                """)}}
                        return false;
                    }
                }

                partial class {{Parts.Configure.ABSTRACT_CLASS_NAME}} {
                    /// <summary>
                    /// テーブルやカラムの詳細を定義します。
                    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
                    /// </summary>
                    public void {{OnModelCreating}}({{context.Config.DbContextName}} dbContext, ModelBuilder modelBuilder) {
                {{If(sequences.Length > 0, () => $$"""
                        // シーケンスを作成
                """)}}  
                {{sequences.SelectTextTemplate(s => $$"""
                        modelBuilder.HasSequence<int>("{{s.Options.SeqName}}")
                            .StartsAt(1)
                            .IncrementsBy(1);
                """)}}
                        modelBuilder.Entity<{{context.Config.EntityNamespace}}.{{_aggregate.Item.EFCoreEntityClassName}}>(entity => {

                            entity.ToTable("{{_aggregate.Item.Options.DbName ?? _aggregate.Item.PhysicalName}}")
                            .HasComment("{{_aggregate.Item.DisplayName}}");

                            entity.HasKey(e => new {
                {{_aggregate.GetKeys().OfType<AggregateMember.ValueMember>().Where(pk => pk.Inherits?.GetRefForeignKeyProxy() == null).SelectTextTemplate(pk => $$"""
                                e.{{pk.MemberName}},
                """)}}
                            })
                            .HasName("PK_{{_aggregate.Item.UniqueId.ToUpper().Substring(0, 8)}}");

                            {{WithIndent(membersSourceCode, "            ")}}
                            entity.Property(e => e.{{CREATED_AT}})
                                .HasColumnName("{{context.Config.CreatedAtDbColumnName?.Replace("\"", "\\\"")}}")
                                .HasColumnOrder({{columnOrder++}});
                            entity.Property(e => e.{{CREATE_USER}})
                                .HasColumnName("{{context.Config.CreateUserDbColumnName?.Replace("\"", "\\\"")}}")
                                .HasColumnOrder({{columnOrder++}});
                            entity.Property(e => e.{{UPDATED_AT}})
                                .HasColumnName("{{context.Config.UpdatedAtDbColumnName?.Replace("\"", "\\\"")}}")
                                .HasColumnOrder({{columnOrder++}});
                            entity.Property(e => e.{{UPDATE_USER}})
                                .HasColumnName("{{context.Config.UpdateUserDbColumnName?.Replace("\"", "\\\"")}}")
                                .HasColumnOrder({{columnOrder++}});
                {{If(_aggregate.IsRoot(), () => $$"""
                            entity.Property(e => e.{{VERSION}})
                {{If(context.Config.VersionDbColumnName != null, () => $$"""
                                .HasColumnName("{{context.Config.VersionDbColumnName?.Replace("\"", "\\\"")}}")
                """)}}
                                .IsRequired(true)
                                .IsConcurrencyToken(true)
                                .HasColumnOrder({{columnOrder++}});
                """)}}

                            {{WithIndent(referedSourceCode, "            ")}}

                            // このエンティティに対して、自動生成されない初期設定がある場合はこの中で設定される（インデックス、ユニーク制約、デフォルト値など）
                            OnModelCreating(entity);
                        });
                    }

                    /// <summary>
                    /// 自動生成されない初期設定がある場合はこのメソッドをオーバーライドして設定してください。
                    /// （インデックス、ユニーク制約、デフォルト値など）
                    /// </summary>
                    public virtual void OnModelCreating(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<{{_aggregate.Item.EFCoreEntityClassName}}> entity) {
                    }
                }
                """;
        }

        /// <summary>
        /// ナビゲーションプロパティの Fluent API 定義
        /// </summary>
        private string RenderNavigationPropertyOnModelCreating(NavigationProperty nav) {

            var sourceCode = new StringBuilder();

            // Has
            if (nav.Principal.OppositeIsMany) {
                sourceCode.AppendLine($"entity.HasMany(e => e.{nav.Principal.PropertyName})");
            } else {
                sourceCode.AppendLine($"entity.HasOne(e => e.{nav.Principal.PropertyName})");
            }

            // With
            if (nav.Relevant.OppositeIsMany) {
                sourceCode.AppendLine($"    .WithMany(e => e.{nav.Relevant.PropertyName})");
            } else {
                sourceCode.AppendLine($"    .WithOne(e => e.{nav.Relevant.PropertyName})");
            }

            // FK
            if (!nav.Principal.OppositeIsMany && !nav.Relevant.OppositeIsMany) {
                // HasOneWithOneのときは型引数が要るらしい
                sourceCode.AppendLine($"    .HasForeignKey<{nav.Relevant.Owner.Item.EFCoreEntityClassName}>(e => new {{");
            } else {
                sourceCode.AppendLine($"    .HasForeignKey(e => new {{");
            }
            foreach (var fk in nav.Relevant.GetForeignKeys()) {
                var memberName = fk.Inherits?.GetRefForeignKeyProxy()?.GetProxyMember().MemberName ?? fk.MemberName;
                sourceCode.AppendLine($"        e.{memberName},");
            }
            sourceCode.AppendLine($"    }})");

            // OnDelete
            sourceCode.AppendLine($"    .OnDelete({nameof(DeleteBehavior)}.{nav.OnPrincipalDeleted})");

            // 外部キーの制約名
            var me = nav.Principal.Owner.Item.UniqueId.ToUpper().Substring(0, 8);
            var you = nav.Relevant.Owner.Item.UniqueId.ToUpper().Substring(0, 8);
            var relation = nav.Relation.RelationName.ToHashedString().ToUpper().Substring(0, 8);
            sourceCode.AppendLine($$"""
                    .HasConstraintName("FK_{{me}}_{{relation}}_{{you}}");
                """);

            return sourceCode.ToString();
        }

        /// <summary>
        /// <see cref="ON_MODEL_CREATING"/> メソッドを呼び出す
        /// </summary>
        internal Func<string, string> RenderCallingOnModelCreating(CodeRenderingContext context) {
            return modelBuilder => $$"""
                _nijoConfig.{{OnModelCreating}}(this, {{modelBuilder}});
                """;
        }

        /// <summary>
        /// 子孫要素をIncludeする処理をレンダリングします。
        /// 必要に応じて AsSplitQuery もレンダリングします。
        /// </summary>
        internal string RenderIncludeAndAsSplitQuery(bool includeRefs) {
            var includeEntities = _aggregate
                .EnumerateThisAndDescendants()
                .ToList();
            if (includeRefs) {
                var refEntities = _aggregate
                    .EnumerateThisAndDescendants()
                    .SelectMany(agg => agg.GetMembers())
                    .Select(m => m.DeclaringAggregate);
                foreach (var entity in refEntities) {
                    includeEntities.Add(entity);
                }
            }

            // 同じ集約内に1対多のリレーションが複数ある場合は分割クエリによる読み込みを行う。
            // パフォーマンス障害防止のため。（例えば子テーブル2個に親と紐づくデータがそれぞれ100件ずつあるとき
            // 分割クエリなら 親1 + 子100 + 子100 の計201件だけがフェッチされるが、
            // 分割クエリにしないと 親1 * 子100 * 子100 の10000件がやり取りされることになる）
            var shouldSplitQuery = includeEntities.Count(agg => agg.IsChildrenMember()) >= 2;

            // 直下の子はInclude, 子がさらに子をもつ場合はThenInclude
            var paths = includeEntities
                .Select(entity => entity.PathFromEntry())
                .Distinct()
                .SelectMany(edge => edge)
                .Select(edge => edge.As<Aggregate>())
                .Select(edge => {
                    var source = edge.Source.As<Aggregate>();
                    var nav = new NavigationProperty(edge);
                    var prop = edge.Source.As<Aggregate>() == nav.Principal.Owner
                        ? nav.Principal.PropertyName
                        : nav.Relevant.PropertyName;
                    return new { source, prop };
                });

            return $$"""
                {{If(shouldSplitQuery, () => $$"""
                .AsSplitQuery()
                """)}}
                {{paths.SelectTextTemplate(path => path.source == _aggregate ? $$"""
                .Include(x => x.{{path.prop}})
                """ : $$"""
                .ThenInclude(x => x.{{path.prop}})
                """)}}
                """;
        }
    }

    internal static partial class GetFullPathExtensions {

        /// <summary>
        /// エントリーからのパスを <see cref="EFCoreEntity"/> のインスタンスの型のルールにあわせて返す。
        /// </summary>
        internal static IEnumerable<string> GetFullPathAsDbEntity(this GraphNode<Aggregate> aggregate, GraphNode<Aggregate>? since = null, GraphNode<Aggregate>? until = null) {
            var path = aggregate.PathFromEntry();
            if (since != null) path = path.Since(since);
            if (until != null) path = path.Until(until);

            foreach (var edge in path) {
                var navigation = new NavigationProperty(edge.As<Aggregate>());
                if (navigation.Principal.Owner == edge.Source.As<Aggregate>()) {
                    yield return navigation.Principal.PropertyName;
                } else {
                    yield return navigation.Relevant.PropertyName;
                }
            }
        }

        /// <summary>
        /// エントリーからのパスを <see cref="EFCoreEntity"/> のインスタンスの型のルールにあわせて返す。
        /// </summary>
        internal static IEnumerable<string> GetFullPathAsDbEntity(this AggregateMember.AggregateMemberBase member, GraphNode<Aggregate>? since = null, GraphNode<Aggregate>? until = null) {
            foreach (var path in member.Owner.GetFullPathAsDbEntity(since, until)) {
                yield return path;
            }
            yield return member.MemberName;
        }

        /// <summary>
        /// フルパスの途中で配列が出てきた場合はSelectやmapをかける
        /// </summary>
        internal static IEnumerable<string> GetFullPathAsDbEntity(this AggregateMember.AggregateMemberBase member, E_CsTs csts, out bool isArray, GraphNode<Aggregate>? since = null, GraphNode<Aggregate>? until = null) {
            var path = member.Owner.PathFromEntry();
            if (since != null) path = path.Since(since);
            if (until != null) path = path.Until(until);

            isArray = false;
            var edges = path.ToArray();
            var result = new List<string>();
            for (int i = 0; i < edges.Length; i++) {
                var edge = edges[i];

                var navigation = new NavigationProperty(edge.As<Aggregate>());
                var relationName = navigation.Principal.Owner == edge.Source.As<Aggregate>()
                    ? navigation.Principal.PropertyName
                    : navigation.Relevant.PropertyName;

                var isMany = false;
                if (edge.IsParentChild()
                    && edge.Source == edge.Initial
                    && edge.Terminal.As<Aggregate>().IsChildrenMember()) {
                    isMany = true;
                }

                if (isMany) {
                    result.Add(isArray
                        ? (csts == E_CsTs.CSharp
                            ? $"SelectMany(x => x.{relationName})"
                            : $"flatMap(x => x.{relationName})")
                        : relationName);
                    isArray = true;

                } else {
                    result.Add(isArray
                        ? (csts == E_CsTs.CSharp
                            ? $"Select(x => x.{relationName})"
                            : $"map(x => x.{relationName})")
                        : relationName);
                }
            }

            result.Add(member.MemberName);
            return result;
        }
    }
}
