using Microsoft.EntityFrameworkCore;
using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 旧版互換の EF Core Entity レンダラー。
    /// </summary>
    internal static class LegacyEFCoreEntity {
        internal static string Render(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            var entity = new EFCoreEntity(rootAggregate);
            var sequences = rootAggregate
                .GetMembers()
                .OfType<ValueMember>()
                .Where(vm => vm.Type is ValueMemberTypes.SequenceMember && !string.IsNullOrWhiteSpace(vm.SequenceName))
                .ToArray();

            var allColumns = new Nijo.Parts.CSharp.EFCoreEntity(rootAggregate).GetColumns().ToArray();
            var columnsByMember = allColumns
                .Where(col => col.Member is not null)
                .ToDictionary(col => col.Member!.ToMappingKey());
            var columns = rootAggregate
                .GetMembers()
                .OfType<ValueMember>()
                .Select(member => columnsByMember[member.ToMappingKey()])
                .ToArray();
            var keyColumns = columns.Where(col => col.IsKey).ToArray();
            var refTos = rootAggregate
                .GetMembers()
                .OfType<RefToMember>()
                .ToArray();
            var childAggregates = rootAggregate
                .GetMembers()
                .OfType<ChildAggregate>()
                .ToArray();
            var childrenAggregates = rootAggregate
                .GetMembers()
                .OfType<ChildrenAggregate>()
                .ToArray();
            var referedBy = rootAggregate
                .GetRefFroms()
                .ToArray();

            var columnOrder = 0;
            var columnDefinitions = new List<string>();
            foreach (var col in columns) {
                var valueGeneratedNever = (col.CsType == "int" || col.CsType == "long") && col.IsKey;

                var lines = new List<string> {
                    $"entity.Property(e => e.{col.PhysicalName})",
                    $"    .HasColumnName(\"{col.DbName.Replace("\"", "\\\"")}\")",
                    $"    .HasComment(\"{col.PhysicalName.Replace("\"", "\\\"")}\")",
                };
                if (col.Member?.TotalDigit != null) {
                    lines.Add($"    .HasPrecision({col.Member.TotalDigit}, {col.Member.DecimalPlace?.ToString() ?? "0"})");
                }
                if (valueGeneratedNever) {
                    lines.Add("    .ValueGeneratedNever() // 暗黙的シーケンスの生成をオフにする");
                }
                if (col.Member?.MaxLength != null) {
                    lines.Add($"    .HasMaxLength({col.Member.MaxLength})");
                }
                lines.Add($"    .IsRequired({(col.IsKey || col.IsNotNull ? "true" : "false")})");
                lines.Add($"    .HasColumnOrder({columnOrder++});");
                columnDefinitions.Add(string.Join(Environment.NewLine, lines));
            }

            return $$"""
                /// <summary>
                /// Entity Framework Core のルールに則った{{rootAggregate.DisplayName}}のデータ型
                /// </summary>
                public partial class {{entity.ClassName}} {
                {{columns.SelectTextTemplate(col => $$"""
                    public {{col.CsType}}? {{col.PhysicalName}} { get; set; }
                """)}}
                    /// <summary>楽観排他制御用のバージョニング用カラム</summary>
                    public int? {{Nijo.Parts.CSharp.EFCoreEntity.VERSION}} { get; set; }
                    /// <summary>データが新規作成された日時</summary>
                    public DateTime? {{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT}} { get; set; }
                    /// <summary>データが更新された日時</summary>
                    public DateTime? {{Nijo.Parts.CSharp.EFCoreEntity.UPDATED_AT}} { get; set; }
                    /// <summary>データを新規作成したユーザー</summary>
                    public string? {{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER}} { get; set; }
                    /// <summary>データを更新したユーザー</summary>
                    public string? {{Nijo.Parts.CSharp.EFCoreEntity.UPDATE_USER}} { get; set; }

                {{If(refTos.Length > 0, () => $$"""
                {{refTos.SelectTextTemplate(refTo => $$"""
                    public virtual {{refTo.RefTo.PhysicalName}}DbEntity? {{refTo.PhysicalName}} { get; set; }
                """)}}
                """)}}
                {{childAggregates.SelectTextTemplate(child => $$"""
                    public virtual {{child.PhysicalName}}DbEntity? {{child.PhysicalName}} { get; set; }
                """)}}
                {{childrenAggregates.SelectTextTemplate(children => $$"""
                    public virtual ICollection<{{children.PhysicalName}}DbEntity> {{children.PhysicalName}} { get; set; } = new HashSet<{{children.PhysicalName}}DbEntity>();
                """)}}
                {{referedBy.SelectTextTemplate(refFrom => $$"""
                    public virtual ICollection<{{refFrom.Owner.PhysicalName}}DbEntity> {{GetRefFromPropertyName(refFrom)}} { get; set; } = new HashSet<{{refFrom.Owner.PhysicalName}}DbEntity>();
                """)}}

                    /// <summary>このオブジェクトと比較対象のオブジェクトの主キーが一致するかを返します。</summary>
                    public bool KeyEquals({{entity.ClassName}} entity) {
                {{keyColumns.SelectTextTemplate(col => $$"""
                        if (entity.{{col.PhysicalName}} != this.{{col.PhysicalName}}) return false;
                """)}}
                        return true;
                    }
                    /// <summary>
                    /// このオブジェクトと引数のオブジェクトに、何らかの差分があるかどうかを調べます。
                    /// 登録日時等はこのメソッドの判定から除外されます。
                    /// </summary>
                    public bool HasChanges({{entity.ClassName}} entity) {
                {{columns.SelectTextTemplate(col => $$"""
                        if (this.{{col.PhysicalName}} != entity.{{col.PhysicalName}}) return true;
                """)}}
                        return false;
                    }
                }

                partial class DefaultConfiguration {
                    /// <summary>
                    /// テーブルやカラムの詳細を定義します。
                    /// 参考: "Fluent API" （Entity FrameWork Core の仕組み）
                    /// </summary>
                    public void OnModelCreating{{rootAggregate.PhysicalName}}({{ctx.Config.DbContextName}} dbContext, ModelBuilder modelBuilder) {
                {{If(sequences.Length > 0, () => $$"""
                        // シーケンスを作成
                """)}}
                {{sequences.SelectTextTemplate(s => $$"""
                        modelBuilder.HasSequence<int>("{{s.SequenceName!.Replace("\"", "\\\"")}}")
                            .StartsAt(1)
                            .IncrementsBy(1);
                """)}}
                        modelBuilder.Entity<{{ctx.Config.RootNamespace}}.{{entity.ClassName}}>(entity => {

                            entity.ToTable("{{rootAggregate.DbName.Replace("\"", "\\\"")}}")
                            .HasComment("{{rootAggregate.DisplayName.Replace("\"", "\\\"")}}");

                            entity.HasKey(e => new {
                {{keyColumns.SelectTextTemplate(col => $$"""
                                e.{{col.PhysicalName}},
                """)}}
                            })
                            .HasName("PK_{{GetConstraintToken(rootAggregate)}}");

                            {{WithIndent(columnDefinitions, "            ")}}
                            entity.Property(e => e.{{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT}})
                                .HasColumnName("{{ctx.Config.CreatedAtDbColumnName.Replace("\"", "\\\"")}}")
                                .HasColumnOrder({{columnOrder++}});
                            entity.Property(e => e.{{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER}})
                                .HasColumnName("{{ctx.Config.CreateUserDbColumnName.Replace("\"", "\\\"")}}")
                                .HasColumnOrder({{columnOrder++}});
                            entity.Property(e => e.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATED_AT}})
                                .HasColumnName("{{ctx.Config.UpdatedAtDbColumnName.Replace("\"", "\\\"")}}")
                                .HasColumnOrder({{columnOrder++}});
                            entity.Property(e => e.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATE_USER}})
                                .HasColumnName("{{ctx.Config.UpdateUserDbColumnName.Replace("\"", "\\\"")}}")
                                .HasColumnOrder({{columnOrder++}});
                {{If(!string.IsNullOrEmpty(ctx.Config.VersionDbColumnName), () => $$"""
                            entity.Property(e => e.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}})
                                .HasColumnName("{{ctx.Config.VersionDbColumnName.Replace("\"", "\\\"")}}")
                                .IsRequired(true)
                                .IsConcurrencyToken(true)
                                .HasColumnOrder({{columnOrder++}});
                """).Else(() => $$"""
                            entity.Property(e => e.{{Nijo.Parts.CSharp.EFCoreEntity.VERSION}})
                                .IsRequired(true)
                                .IsConcurrencyToken(true)
                                .HasColumnOrder({{columnOrder++}});
                """)}}

                {{referedBy.Select(RenderRefFromOnModelCreating).SelectTextTemplate(source => $$"""
                            {{WithIndent(source, "            ")}}

                """)}}

                            // このエンティティに対して、自動生成されない初期設定がある場合はこの中で設定される（インデックス、ユニーク制約、デフォルト値など）
                            OnModelCreating(entity);
                        });
                    }

                    /// <summary>
                    /// 自動生成されない初期設定がある場合はこのメソッドをオーバーライドして設定してください。
                    /// （インデックス、ユニーク制約、デフォルト値など）
                    /// </summary>
                    public virtual void OnModelCreating(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<{{entity.ClassName}}> entity) {
                    }
                }
                """;
        }

        private static string GetRefFromPropertyName(RefToMember refFrom) {
            return $"RefferedBy_{refFrom.Owner.PhysicalName}DbEntity_{refFrom.PhysicalName}";
        }

        private static string RenderRefFromOnModelCreating(RefToMember refFrom) {
            var principalAggregate = refFrom.RefTo;
            var relevantAggregate = refFrom.Owner;
            var relationHash = refFrom.PhysicalName.ToHashedString().ToUpperInvariant()[..8];
            var principalId = GetConstraintToken(principalAggregate);
            var relevantId = GetConstraintToken(relevantAggregate);
            var foreignKeys = new Nijo.Parts.CSharp.NavigationProperty.NavigationOfRef(refFrom)
                .GetRelevantForeignKeys()
                .ToArray();

            return $$"""
                entity.HasMany(e => e.{{GetRefFromPropertyName(refFrom)}})
                    .WithOne(e => e.{{refFrom.PhysicalName}})
                    .HasForeignKey(e => new {
                {{foreignKeys.SelectTextTemplate(fk => $$"""
                        e.{{fk.PhysicalName}},
                """)}}
                    })
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_{{principalId}}_{{relationHash}}_{{relevantId}}");
                """;
        }

        private static string GetConstraintToken(AggregateBase aggregate) {
            var raw = aggregate.XElement.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)?.Value;
            if (string.IsNullOrWhiteSpace(raw)) {
                raw = GetLegacyNodeIdPath(aggregate);
            }

            return raw.ToHashedString().ToUpperInvariant()[..8];
        }

        private static string GetLegacyNodeIdPath(AggregateBase aggregate) {
            return string.Concat(aggregate.EnumerateThisAndAncestors().Select(x => $"/{x.PhysicalName}"));
        }

        private static string GetConstraintToken(string? raw) {
            if (string.IsNullOrWhiteSpace(raw)) {
                return "00000000";
            }

            return raw.ToHashedString().ToUpperInvariant()[..8];
        }
    }
}
