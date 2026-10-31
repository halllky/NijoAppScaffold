using Nijo.CodeGenerating;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures.UseSoftDelete;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.DataStructures.IsStored;

/// <summary>
/// 検証時点の値をもとに監査項目と楽観排他のバージョンを設定し、エンティティの変更追跡を開始する処理。
/// </summary>
internal static class SaveCommandTracking {

    private const string COMMAND = ValidatedCommand.FIELD_COMMAND;
    private const string SNAPSHOT = ValidatedCommand.FIELD_SNAPSHOT;
    private const string DBCONTEXT = $"{COMMAND}.{SaveCommand.FIELD_DBCONTEXT}";
    private const string LOADED = $"{COMMAND}.{SaveCommand.FIELD_LOADED}";
    private const string EXPECTED_VERSION = $"{COMMAND}.{SaveCommand.FIELD_EXPECTED_VERSION}";
    private const string CURRENT_TIME = $"{DBCONTEXT}.AuditInfo.CurrentTime";
    private const string CURRENT_USER = $"{DBCONTEXT}.AuditInfo.CurrentUser";

    /// <summary>新規登録</summary>
    internal static string RenderCreate(RootAggregate rootAggregate) {
        var paths = CreateDescendantPaths(rootAggregate);

        return $$"""
            private protected override void {{ValidatedCommand.START_TRACKING_CORE}}(List<object> tracked) {
                var dbEntity = {{SNAPSHOT}};
                dbEntity.{{EFCoreEntity.VERSION}} = 0;
                dbEntity.{{EFCoreEntity.CREATED_AT}} = {{CURRENT_TIME}};
                dbEntity.{{EFCoreEntity.UPDATED_AT}} = {{CURRENT_TIME}};
                dbEntity.{{EFCoreEntity.CREATE_USER}} = {{CURRENT_USER}};
                dbEntity.{{EFCoreEntity.UPDATE_USER}} = {{CURRENT_USER}};
                tracked.Add(dbEntity);
                {{WithIndent(RenderForEachDescendant(rootAggregate, paths, "dbEntity", "new", entity => $$"""
                {{entity}}.{{EFCoreEntity.CREATED_AT}} = {{CURRENT_TIME}};
                {{entity}}.{{EFCoreEntity.UPDATED_AT}} = {{CURRENT_TIME}};
                {{entity}}.{{EFCoreEntity.CREATE_USER}} = {{CURRENT_USER}};
                {{entity}}.{{EFCoreEntity.UPDATE_USER}} = {{CURRENT_USER}};
                tracked.Add({{entity}});
                """))}}

                {{DBCONTEXT}}.Add(dbEntity);
            }
            """;
    }

    /// <summary>更新</summary>
    internal static string RenderUpdate(RootAggregate rootAggregate) {
        var paths = CreateDescendantPaths(rootAggregate);

        return $$"""
            private protected override void {{ValidatedCommand.START_TRACKING_CORE}}(List<object> tracked) {
                var beforeDbEntity = {{LOADED}};
                var afterDbEntity = {{SNAPSHOT}};
                afterDbEntity.{{EFCoreEntity.VERSION}} = {{EXPECTED_VERSION}} + 1;
                afterDbEntity.{{EFCoreEntity.CREATED_AT}} = beforeDbEntity.{{EFCoreEntity.CREATED_AT}};
                afterDbEntity.{{EFCoreEntity.UPDATED_AT}} = {{CURRENT_TIME}};
                afterDbEntity.{{EFCoreEntity.CREATE_USER}} = beforeDbEntity.{{EFCoreEntity.CREATE_USER}};
                afterDbEntity.{{EFCoreEntity.UPDATE_USER}} = {{CURRENT_USER}};

                tracked.Add(afterDbEntity);
                var entry = {{DBCONTEXT}}.Entry(afterDbEntity);
                entry.State = EntityState.Modified;
                entry.Property(e => e.{{EFCoreEntity.VERSION}}).OriginalValue = {{EXPECTED_VERSION}};

            {{RenderDescendantAttaching(rootAggregate, paths).SelectTextTemplate(source => $$"""
                {{WithIndent(source)}}

            """)}}
            }
            """;
    }

    /// <summary>削除。論理削除の場合は削除前に退避テーブルへ移送する。</summary>
    internal static string RenderDelete(RootAggregate rootAggregate, CodeRenderingContext ctx) {
        var paths = CreateDescendantPaths(rootAggregate);

        return $$"""
            private protected override void {{ValidatedCommand.START_TRACKING_CORE}}(List<object> tracked) {
                var dbEntity = {{LOADED}};
            {{If(rootAggregate.UseSoftDelete, () => $$"""

                {{WithIndent(DeletedTableInsertion.Render(rootAggregate, "dbEntity", CURRENT_TIME, CURRENT_USER, DBCONTEXT))}}

            """)}}
                tracked.Add(dbEntity);
                var entry = {{DBCONTEXT}}.Entry(dbEntity);
                entry.State = EntityState.Deleted;
                entry.Property(e => e.{{EFCoreEntity.VERSION}}).OriginalValue = {{EXPECTED_VERSION}};
                entry.Property(e => e.{{EFCoreEntity.VERSION}}).CurrentValue = {{EXPECTED_VERSION}};
                {{WithIndent(RenderForEachDescendant(rootAggregate, paths, "dbEntity", "deleting", entity => $$"""
                tracked.Add({{entity}});
                {{DBCONTEXT}}.Entry({{entity}}).State = EntityState.Deleted;
                """))}}
            }
            """;
    }

    private static Dictionary<SchemaNodeIdentity, InstancePropertyPath> CreateDescendantPaths(RootAggregate rootAggregate) {
        return new EFCoreEntity(rootAggregate)
            .CreateStructurePathsRecursively()
            .ToDictionary(x => x.Metadata.SchemaPathNode.ToMappingKey());
    }

    /// <summary>
    /// 子孫要素をDbContextにアタッチし、あわせて監査列を設定するソースをレンダリングする。
    /// </summary>
    private static IEnumerable<string> RenderDescendantAttaching(RootAggregate rootAggregate, IReadOnlyDictionary<SchemaNodeIdentity, InstancePropertyPath> variablePathInfo) {
        var descendantDbEntities = rootAggregate.EnumerateDescendants().ToArray();

        for (int i = 0; i < descendantDbEntities.Length; i++) {
            var descAggregate = descendantDbEntities[i];
            var descDbEntity = new EFCoreEntity(descAggregate);
            var tempBefore = $"before{descAggregate.PhysicalName}_{i}";
            var tempAfter = $"after{descAggregate.PhysicalName}_{i}";

            // ChangeState変更。Child型の場合は、オプショナル（親テーブル1に対しChildは0または1）の考慮を行う。
            // 1. 子テーブルが元々存在し、更新後も存在する場合 → UPDATE
            // 2. 子テーブルが元々存在せず、更新後に存在する場合 → INSERT
            // 3. 子テーブルが元々存在するが、更新後に存在しない場合 → DELETE
            var arrayPath = variablePathInfo[descAggregate.ToMappingKey()].GetFlattenArrayPath(E_CsTs.CSharp, out var isMany);
            if (isMany) {
                yield return $$"""
                    var {{tempBefore}} = beforeDbEntity.{{arrayPath.Join("?.")}}?.OfType<{{descDbEntity.CsClassName}}>() ?? [];
                    var {{tempAfter}} = afterDbEntity.{{arrayPath.Join("?.")}}?.OfType<{{descDbEntity.CsClassName}}>() ?? [];
                    foreach (var a in {{tempAfter}}) {
                        var b = {{tempBefore}}.SingleOrDefault(b => b.{{EFCoreEntity.KEYEQUALS}}(a));
                        tracked.Add(a);
                        if (b == null) {
                            a.{{EFCoreEntity.CREATED_AT}} = {{CURRENT_TIME}};
                            a.{{EFCoreEntity.UPDATED_AT}} = {{CURRENT_TIME}};
                            a.{{EFCoreEntity.CREATE_USER}} = {{CURRENT_USER}};
                            a.{{EFCoreEntity.UPDATE_USER}} = {{CURRENT_USER}};
                            {{DBCONTEXT}}.Entry(a).State = EntityState.Added; // 子テーブルが元々存在せず、更新後に存在する場合 → INSERT
                        } else {
                            a.{{EFCoreEntity.CREATED_AT}} = b.{{EFCoreEntity.CREATED_AT}};
                            a.{{EFCoreEntity.UPDATED_AT}} = {{CURRENT_TIME}};
                            a.{{EFCoreEntity.CREATE_USER}} = b.{{EFCoreEntity.CREATE_USER}};
                            a.{{EFCoreEntity.UPDATE_USER}} = {{CURRENT_USER}};
                            {{DBCONTEXT}}.Entry(a).State = EntityState.Modified; // 子テーブルが元々存在し、更新後も存在する場合 → UPDATE
                        }
                    }
                    foreach (var b in {{tempBefore}}) {
                        var a = {{tempAfter}}.SingleOrDefault(a => a.{{EFCoreEntity.KEYEQUALS}}(b));
                        if (a == null) {
                            tracked.Add(b);
                            {{DBCONTEXT}}.Entry(b).State = EntityState.Deleted; // 子テーブルが元々存在するが、更新後に存在しない場合 → DELETE
                        }
                    }
                    """;

            } else {
                yield return $$"""
                    var {{tempBefore}} = beforeDbEntity.{{arrayPath.Join("?.")}};
                    var {{tempAfter}} = afterDbEntity.{{arrayPath.Join("?.")}};
                    if ({{tempAfter}} != null) {
                        tracked.Add({{tempAfter}});
                        if ({{tempBefore}} != null) {
                            {{tempAfter}}.{{EFCoreEntity.CREATED_AT}} = {{tempBefore}}.{{EFCoreEntity.CREATED_AT}};
                            {{tempAfter}}.{{EFCoreEntity.UPDATED_AT}} = {{CURRENT_TIME}};
                            {{tempAfter}}.{{EFCoreEntity.CREATE_USER}} = {{tempBefore}}.{{EFCoreEntity.CREATE_USER}};
                            {{tempAfter}}.{{EFCoreEntity.UPDATE_USER}} = {{CURRENT_USER}};
                            {{DBCONTEXT}}.Entry({{tempAfter}}).State = EntityState.Modified; // 子テーブルが元々存在し、更新後も存在する場合 → UPDATE
                        } else {
                            {{tempAfter}}.{{EFCoreEntity.CREATED_AT}} = {{CURRENT_TIME}};
                            {{tempAfter}}.{{EFCoreEntity.UPDATED_AT}} = {{CURRENT_TIME}};
                            {{tempAfter}}.{{EFCoreEntity.CREATE_USER}} = {{CURRENT_USER}};
                            {{tempAfter}}.{{EFCoreEntity.UPDATE_USER}} = {{CURRENT_USER}};
                            {{DBCONTEXT}}.Entry({{tempAfter}}).State = EntityState.Added; // 子テーブルが元々存在せず、更新後に存在する場合 → INSERT
                        }
                    } else if ({{tempBefore}} != null) {
                        tracked.Add({{tempBefore}});
                        {{DBCONTEXT}}.Entry({{tempBefore}}).State = EntityState.Deleted; // 子テーブルが元々存在するが、更新後に存在しない場合 → DELETE
                    }
                    """;
            }
        }
    }

    /// <summary>
    /// 子孫要素のエンティティそれぞれに対して同じ処理を行うソースをレンダリングする。
    /// 子孫要素は階層の深さにかかわらずループを入れ子にせず列挙され、存在しない（null の）子孫要素は処理されない。
    /// </summary>
    /// <param name="tempVarPrefix">子孫要素を受ける一時変数名の接頭辞。一時変数はレンダリング結果の挿入先のスコープに宣言されるため、同じメソッド内の他の変数と衝突しないものを指定すること。</param>
    /// <param name="renderStatement">エンティティを表す式を受け取り、それに対する処理をレンダリングする。式は子孫要素の多重度によって一時変数にもループ変数にもなるため、特定の変数名を前提にしないこと。</param>
    private static IEnumerable<string> RenderForEachDescendant(RootAggregate rootAggregate, IReadOnlyDictionary<SchemaNodeIdentity, InstancePropertyPath> variablePathInfo, string rootEntityName, string tempVarPrefix, Func<string, string> renderStatement) {
        var descendantDbEntities = rootAggregate.EnumerateDescendants().ToArray();

        for (int i = 0; i < descendantDbEntities.Length; i++) {
            var descAggregate = descendantDbEntities[i];
            var descDbEntity = new EFCoreEntity(descAggregate);
            var temp = $"{tempVarPrefix}{descAggregate.PhysicalName}_{i}";
            var arrayPath = variablePathInfo[descAggregate.ToMappingKey()].GetFlattenArrayPath(E_CsTs.CSharp, out var isMany);
            if (isMany) {
                yield return $$"""
                    var {{temp}} = {{rootEntityName}}.{{arrayPath.Join("?.")}}?.OfType<{{descDbEntity.CsClassName}}>() ?? [];
                    foreach (var a in {{temp}}) {
                        {{WithIndent(renderStatement("a"))}}
                    }
                    """;
            } else {
                yield return $$"""
                    var {{temp}} = {{rootEntityName}}.{{arrayPath.Join("?.")}};
                    if ({{temp}} != null) {
                        {{WithIndent(renderStatement(temp))}}
                    }
                    """;
            }
        }
    }
}
