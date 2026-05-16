using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.DataModelModules;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.WriteModel2Modules {
    /// <summary>
    /// 旧版互換の子孫要素アタッチ・デタッチ処理レンダラー。
    /// </summary>
    internal static class LegacyDescendantState {
        internal static IEnumerable<string> RenderDescendantAttaching(RootAggregate rootAggregate) {
            var descendantDbEntities = rootAggregate.EnumerateDescendants().ToArray();

            var rootDbEntity = new Nijo.Parts.CSharp.EFCoreEntity(rootAggregate);
            var variablePathInfo = new Variable("※この変数名は使用されない※", rootDbEntity)
                .CreatePropertiesRecursively()
                .Where(p => p is InstanceStructureProperty)
                .ToDictionary(x => x.Metadata.SchemaPathNode.ToMappingKey());

            for (int i = 0; i < descendantDbEntities.Length; i++) {
                var descAggregate = descendantDbEntities[i];
                var descDbEntity = new Nijo.Parts.CSharp.EFCoreEntity(descAggregate);
                var tempBefore = $"before{descAggregate.PhysicalName}_{i}";
                var tempAfter = $"after{descAggregate.PhysicalName}_{i}";
                var arrayPath = variablePathInfo[descAggregate.ToMappingKey()].GetFlattenArrayPath(E_CsTs.CSharp, out var isMany);

                if (isMany) {
                    yield return $$"""
                        var {{tempBefore}} = beforeDbEntity.{{arrayPath.Join("?.")}}?.OfType<{{descDbEntity.CsClassName}}>() ?? Enumerable.Empty<{{descDbEntity.CsClassName}}>();
                        var {{tempAfter}}  =  afterDbEntity.{{arrayPath.Join("?.")}}?.OfType<{{descDbEntity.CsClassName}}>() ?? Enumerable.Empty<{{descDbEntity.CsClassName}}>();
                        foreach (var a in {{tempAfter}}) {
                            var b = {{tempBefore}}.SingleOrDefault(b => b.{{Nijo.Parts.CSharp.EFCoreEntity.KEYEQUALS}}(a));
                            if (b == null) {
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT}} = CurrentTime;
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATED_AT}} = CurrentTime;
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER}} = CurrentUser;
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATE_USER}} = CurrentUser;
                                DbContext.Entry(a).State = EntityState.Added;
                            } else {
                                var hasChanges = a.HasChanges(b);
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT}} = b.{{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT}};
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER}} = b.{{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER}};
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATED_AT}} = hasChanges ? CurrentTime : b.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATED_AT}};
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATE_USER}} = hasChanges ? CurrentUser : b.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATE_USER}};
                                DbContext.Entry(a).State = hasChanges ? EntityState.Modified : EntityState.Unchanged;
                            }
                        }
                        foreach (var b in {{tempBefore}}) {
                            var a = {{tempAfter}}.SingleOrDefault(a => a.{{Nijo.Parts.CSharp.EFCoreEntity.KEYEQUALS}}(b));
                            if (a == null) {
                                DbContext.Entry(b).State = EntityState.Deleted;
                            }
                        }
                        """;
                } else {
                    yield return $$"""
                        var {{tempBefore}} = new {{descDbEntity.CsClassName}}?[] {
                            beforeDbEntity.{{arrayPath.Join("?.")}},
                        }.OfType<{{descDbEntity.CsClassName}}>().ToArray();
                        var {{tempAfter}} = new {{descDbEntity.CsClassName}}?[] {
                            afterDbEntity.{{arrayPath.Join("?.")}},
                        }.OfType<{{descDbEntity.CsClassName}}>().ToArray();
                        foreach (var a in {{tempAfter}}) {
                            var b = {{tempBefore}}.SingleOrDefault(b => b.{{Nijo.Parts.CSharp.EFCoreEntity.KEYEQUALS}}(a));
                            if (b == null) {
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT}} = CurrentTime;
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATED_AT}} = CurrentTime;
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER}} = CurrentUser;
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATE_USER}} = CurrentUser;
                                DbContext.Entry(a).State = EntityState.Added;
                            } else {
                                var hasChanges = a.HasChanges(b);
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT}} = b.{{Nijo.Parts.CSharp.EFCoreEntity.CREATED_AT}};
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER}} = b.{{Nijo.Parts.CSharp.EFCoreEntity.CREATE_USER}};
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATED_AT}} = hasChanges ? CurrentTime : b.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATED_AT}};
                                a.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATE_USER}} = hasChanges ? CurrentUser : b.{{Nijo.Parts.CSharp.EFCoreEntity.UPDATE_USER}};
                                DbContext.Entry(a).State = hasChanges ? EntityState.Modified : EntityState.Unchanged;
                            }
                        }
                        foreach (var b in {{tempBefore}}) {
                            var a = {{tempAfter}}.SingleOrDefault(a => a.{{Nijo.Parts.CSharp.EFCoreEntity.KEYEQUALS}}(b));
                            if (a == null) {
                                DbContext.Entry(b).State = EntityState.Deleted;
                            }
                        }
                        """;
                }
            }
        }

        internal static IEnumerable<string> RenderDescendantDetaching(RootAggregate rootAggregate, string rootEntityName) {
            var descendantDbEntities = rootAggregate.EnumerateDescendants().ToArray();

            var rootDbEntity = new Nijo.Parts.CSharp.EFCoreEntity(rootAggregate);
            var variablePathInfo = new Variable("※この変数名は使用されない※", rootDbEntity)
                .CreatePropertiesRecursively()
                .Where(p => p is InstanceStructureProperty)
                .ToDictionary(x => x.Metadata.SchemaPathNode.ToMappingKey());

            for (int i = 0; i < descendantDbEntities.Length; i++) {
                var descAggregate = descendantDbEntities[i];
                var descDbEntity = new Nijo.Parts.CSharp.EFCoreEntity(descAggregate);
                var temp = $"after{descAggregate.PhysicalName}_{i}";
                var arrayPath = variablePathInfo[descAggregate.ToMappingKey()].GetFlattenArrayPath(E_CsTs.CSharp, out var isMany);

                if (isMany) {
                    yield return $$"""
                        var {{temp}} = {{rootEntityName}}.{{arrayPath.Join("?.")}}?.OfType<{{descDbEntity.CsClassName}}>() ?? Enumerable.Empty<{{descDbEntity.CsClassName}}>();
                        foreach (var a in {{temp}}) {
                            DbContext.Entry(a).State = EntityState.Detached;
                        }
                        """;
                } else {
                    yield return $$"""
                        var {{temp}} = new {{descDbEntity.CsClassName}}?[] {
                            {{rootEntityName}}.{{arrayPath.Join("?.")}},
                        }.OfType<{{descDbEntity.CsClassName}}>().ToArray();
                        foreach (var a in {{temp}}) {
                            DbContext.Entry(a).State = EntityState.Detached;
                        }
                        """;
                }
            }
        }
    }
}
