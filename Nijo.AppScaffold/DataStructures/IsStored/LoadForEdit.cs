using Nijo.CodeGenerating;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures.ValueMemberTypes;
using Nijo.Util.DotnetEx;
using System.Linq;

namespace Nijo.DataStructures.IsStored;

/// <summary>
/// コマンドの入手経路。DbContext のメソッドとしてレンダリングされる。
/// EFCore の公開APIには DbSet から DbContext を取得する手段がないので、DbSet の拡張メソッドにはしていない。
/// </summary>
internal static class LoadForEdit {

    /// <summary>複数キーで読み込むときの1回のSQLあたりのキーの数の上限。SQLのパラメータ数の上限を超えないようにするためのもの。</summary>
    private const int KEY_CHUNK_SIZE = 100;

    private const string PREDICATE_CLASS = "LoadForEditPredicate";

    internal static string Render(RootAggregate rootAggregate, CodeRenderingContext ctx) {
        var dbEntity = new EFCoreEntity(rootAggregate);
        var createCommand = new SaveCommand(rootAggregate, SaveCommand.E_Type.Create);
        var updateCommand = new SaveCommand(rootAggregate, SaveCommand.E_Type.Update);
        var key = new KeyClass.KeyClassEntry(rootAggregate);
        var keyWithVersion = key.ClassNameWithVersion;
        var physicalName = rootAggregate.PhysicalName;

        var selectKeysLeft = new Variable("e", dbEntity)
            .CreateProperties()
            .ToArray();
        var keys = rootAggregate
            .GetKeyVMs()
            .Select((vm, i) => new {
                TempVarName = $"searchKey{i + 1}",
                VmType = vm.Type,
                KeyPath = vm.GetPathFromEntry().AsSaveCommand().ToArray(),
                EntityPath = selectKeysLeft
                    .Single(x => x.Metadata.SchemaPathNode.ToMappingKey() == vm.ToMappingKey())
                    .GetJoinedPathFromInstance(E_CsTs.CSharp, "!."),
            })
            .ToArray();

        var keyVariables = (string keyName) => keys.SelectTextTemplate(k => $$"""
            var {{k.TempVarName}} = {{k.VmType.RenderCastToPrimitiveType()}}{{keyName}}.{{k.KeyPath.Join("?.")}};
            """);
        var predicate = keys
            .Select(k => $"{k.EntityPath} == {k.TempVarName}")
            .Join(" && ");
        var includes = dbEntity.RenderInclude().ToArray();

        return $$"""
            partial class {{ctx.Config.DbContextName}} {
                #region {{rootAggregate.DisplayName}} のコマンドの入手
                /// <summary>
                /// {{rootAggregate.DisplayName}} の新規登録を開始します。
                /// 保存する内容を設定して <see cref="{{createCommand.CsClassName}}.{{SaveCommand.GET_VALIDATION_ERRORS}}"/> で検証し、保存処理に渡してください。
                /// </summary>
                public {{createCommand.CsClassName}} StartCreating{{physicalName}}() => new(this);

                /// <summary>
                /// {{rootAggregate.DisplayName}} をキーで1件読み込みます。
                /// 変更追跡は行いません。
                /// </summary>
                /// <param name="key">読み込むデータのキーと、画面表示時のバージョン</param>
                /// <returns>読み込まれたデータ。見つからなかった場合は null</returns>
                public async Task<{{updateCommand.CsClassName}}?> LoadForEdit{{physicalName}}Async({{keyWithVersion}} key, CancellationToken ct = default) {
                    {{WithIndent(keyVariables("key"))}}

                    var dbEntity = await {{dbEntity.DbSetName}}
                        .AsNoTracking()
            {{includes.SelectTextTemplate(source => $$"""
                        {{source}}
            """)}}
                        .SingleOrDefaultAsync(e => {{predicate}}, ct)
                        .ConfigureAwait(false);

                    return dbEntity == null
                        ? null
                        : {{updateCommand.CsClassName}}.{{SaveCommand.FROM_DBENTITY}}(this, dbEntity, key.Version);
                }

                /// <summary>
                /// {{rootAggregate.DisplayName}} をキーで複数件読み込みます。
                /// 変更追跡は行いません。
                /// </summary>
                /// <param name="keys">読み込むデータのキーと、画面表示時のバージョン</param>
                /// <returns>読み込まれたデータ。見つからなかったものは結果に含まれません。</returns>
                public async Task<IReadOnlyDictionary<{{key.ClassName}}, {{updateCommand.CsClassName}}>> LoadForEdit{{physicalName}}Async(IEnumerable<{{keyWithVersion}}> keys, CancellationToken ct = default) {
                    var expectedVersions = new Dictionary<{{key.ClassName}}, int>();
                    foreach (var key in keys) {
                        expectedVersions[key] = key.Version;
                    }

                    var result = new Dictionary<{{key.ClassName}}, {{updateCommand.CsClassName}}>();
                    foreach (var chunk in expectedVersions.Keys.Chunk({{KEY_CHUNK_SIZE}})) {
                        var predicates = chunk.Select(key => {
                            {{WithIndent(keyVariables("key"))}}
                            return (Expression<Func<{{dbEntity.CsClassName}}, bool>>)(e => {{predicate}});
                        });
                        var dbEntities = await {{dbEntity.DbSetName}}
                            .AsNoTracking()
            {{includes.SelectTextTemplate(source => $$"""
                            {{source}}
            """)}}
                            .Where({{PREDICATE_CLASS}}.AnyOf(predicates))
                            .ToListAsync(ct)
                            .ConfigureAwait(false);

                        foreach (var dbEntity in dbEntities) {
                            var loadedKey = {{key.ClassName}}.{{SaveCommand.FROM_DBENTITY}}(dbEntity);

                            // RDBMSの照合順序などの理由でC#上は別のキーになる場合は読み込めなかったものとして扱う
                            if (!expectedVersions.TryGetValue(loadedKey, out var expectedVersion)) continue;

                            result[loadedKey] = {{updateCommand.CsClassName}}.{{SaveCommand.FROM_DBENTITY}}(this, dbEntity, expectedVersion);
                        }
                    }
                    return result;
                }

                /// <summary>
                /// {{rootAggregate.DisplayName}} を任意の条件で読み込みます。更新開始時点で対象データのキーが分からない更新で使用します。
                /// 変更追跡は行いません。
                /// 読み込んだ時点のバージョンが楽観排他制御で期待するバージョンになるので、読み込みから保存までの間に他者が更新した場合は楽観排他エラーになります。
                /// </summary>
                /// <param name="query">読み込む条件。子孫の Include は適用済みなので指定不要です。</param>
                public async Task<IReadOnlyList<{{updateCommand.CsClassName}}>> LoadForEdit{{physicalName}}Async(Func<IQueryable<{{dbEntity.CsClassName}}>, IQueryable<{{dbEntity.CsClassName}}>> query, CancellationToken ct = default) {
                    var source = {{dbEntity.DbSetName}}
                        .AsNoTracking()
            {{includes.SelectTextTemplate(source => $$"""
                        {{source}}
            """)}}
                        ;
                    var dbEntities = await query(source)
                        .ToListAsync(ct)
                        .ConfigureAwait(false);

                    return dbEntities
                        .Select(dbEntity => {{updateCommand.CsClassName}}.{{SaveCommand.FROM_DBENTITY}}(this, dbEntity, dbEntity.{{EFCoreEntity.VERSION}}!.Value))
                        .ToArray();
                }
                #endregion {{rootAggregate.DisplayName}} のコマンドの入手
            }
            """;
    }

    /// <summary>
    /// 複数のキーでの読み込みの検索条件を組み立てる処理をレンダリングします。
    /// </summary>
    internal static SourceFile RenderPredicate(CodeRenderingContext ctx) {
        return new SourceFile {
            FileName = $"{PREDICATE_CLASS}.cs",
            Contents = $$"""
                using System.Linq.Expressions;

                namespace {{ctx.Config.RootNamespace}};

                /// <summary>
                /// 複数のキーでデータを読み込むときの検索条件を組み立てる。
                /// </summary>
                internal static class {{PREDICATE_CLASS}} {
                    /// <summary>引数の条件のいずれかに一致することを表す条件を返します。</summary>
                    internal static Expression<Func<T, bool>> AnyOf<T>(IEnumerable<Expression<Func<T, bool>>> predicates) {
                        var parameter = Expression.Parameter(typeof(T), "e");
                        var body = predicates
                            .Select(predicate => new ParameterReplacer(predicate.Parameters[0], parameter).Visit(predicate.Body))
                            .Aggregate(Expression.OrElse);
                        return Expression.Lambda<Func<T, bool>>(body, parameter);
                    }

                    private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor {
                        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : node;
                    }
                }
                """,
        };
    }
}
