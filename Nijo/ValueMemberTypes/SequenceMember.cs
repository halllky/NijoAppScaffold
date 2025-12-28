using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.QueryModelModules;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using Nijo.Parts.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.ValueMemberTypes {
    /// <summary>
    /// シーケンス。RDBMS登録時にデータベース側で採番処理がなされる整数型。
    /// </summary>
    internal class SequenceMember : IValueMemberType {
        string IValueMemberType.TypePhysicalName => "Sequence";
        string IValueMemberType.SchemaTypeName => "sequence";
        string IValueMemberType.CsDomainTypeName => "int";
        string IValueMemberType.CsPrimitiveTypeName => "int";
        string IValueMemberType.TsTypeName => "string";
        string IValueMemberType.DisplayName => "シーケンス型";

        string IValueMemberType.RenderSpecificationMarkdown() {
            return $$"""
                自動採番される整数値を格納する型です。
                データベース側で自動的に連番が付与されます。
                主キーや管理番号など、一意性が必要な数値データに適しています。
                検索時の挙動は範囲検索（以上・以下）が可能です。
                """;
        }

        void IValueMemberType.Validate(XElement element, SchemaParseContext context, Action<XElement, string> addError) {
            var sequenceName = element.Attribute(BasicNodeOptions.SequenceName.AttributeName);
            if (sequenceName is null || string.IsNullOrWhiteSpace(sequenceName.Value)) {
                addError(element, "シーケンス名が指定されていません。");
            }
        }

        ValueMemberSearchBehavior? IValueMemberType.SearchBehavior => new() {
            FilterCsTypeName = $"{FromTo.CS_CLASS_NAME}<int?>",
            FilterTsTypeName = "{ from?: string | null; to?: string | null }",
            RenderTsNewObjectFunctionValue = () => "{ from: '', to: '' }",
            RenderFiltering = ctx => RangeSearchRenderer.RenderRangeSearchFiltering(ctx),
        };

        UiConstraint.E_Type IValueMemberType.UiConstraintType => UiConstraint.E_Type.NumberMemberConstraint;

        void IValueMemberType.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
            // シーケンスが1個以上定義されている場合、シーケンス採番の構文をユーザーに定義させる
            var hasSequence = ctx.Schema
                .GetRootAggregates()
                .Where(root => root.Model is Models.DataModel)
                .SelectMany(root => root.EnumerateThisAndDescendants())
                .SelectMany(agg => agg.GetMembers())
                .Any(member => member is ValueMember vm && vm.Type is SequenceMember);
            if (hasSequence) {
                ctx.Use<ApplicationConfigure>().AddCoreMethod(
                    $$"""
                    /// <summary>
                    /// EFCoreのモデル定義処理で呼ばれます。
                    /// シーケンス作成定義、採番構文定義をしてください。
                    /// <code>
                    /// // SQL Server の場合
                    /// modelBuilder.HasSequence<int>(sequenceName).StartsAt(1).IncrementsBy(1);
                    /// property.HasDefaultValueSql($"NEXT VALUE FOR {sequenceName}");
                    ///
                    /// // Oracle の場合
                    /// modelBuilder.HasSequence<int>(sequenceName).StartsAt(1).IncrementsBy(1);
                    /// property.HasDefaultValueSql($"NEXTVAL('{sequenceName}')");
                    ///
                    /// // SQLite の場合（SQLiteにはシーケンスがないため、AUTO_INCREMENTを使用）
                    /// property.HasAnnotation("Sqlite:Autoincrement", true);
                    /// </code>
                    /// </summary>
                    protected abstract void {{CONFIGURE_MEMBER}}(
                        Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder,
                        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder entity,
                        Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<int?> property,
                        string sequenceName);
                    """);
            }
        }
        void IValueMemberType.RenderStaticSources(CodeRenderingContext ctx) {
            // 特になし
        }

        string IValueMemberType.RenderCreateDummyDataValueBody(CodeRenderingContext ctx) {
            return $$"""
                return member.IsKey
                    ? context.GetNextSequence()
                    : context.Random.Next(0, 1000);
                """;
        }

        #region 採番処理
        /// <summary>
        /// <see cref="ApplicationConfigure"/> で使用するメソッド名。
        /// 具体的なシーケンス設定方法はRDBMSにより異なるため、自動生成はせずにユーザーに定義させる。
        /// </summary>
        internal const string CONFIGURE_MEMBER = "ConfigureSequenceMember";
        #endregion 採番処理
    }
}
