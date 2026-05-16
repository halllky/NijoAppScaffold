using Nijo.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models {
    internal static class QuerySourceProjectionHelper {
        internal static Dictionary<SchemaNodeIdentity, List<IInstanceProperty>> BuildCandidateDictionary(IInstancePropertyOwner rightOwner) {
            var rightMembers = new Dictionary<SchemaNodeIdentity, List<IInstanceProperty>>();
            foreach (var prop in rightOwner.Create1To1PropertiesRecursively()) {
                var key = prop.Metadata.SchemaPathNode.ToMappingKey();
                if (!rightMembers.TryGetValue(key, out var candidates)) {
                    candidates = [];
                    rightMembers[key] = candidates;
                }
                candidates.Add(prop);
            }
            return rightMembers;
        }

        internal static IEnumerable<string> RenderProjectionMembers(
            IInstancePropertyOwner left,
            IReadOnlyDictionary<SchemaNodeIdentity, List<IInstanceProperty>> rightMembers,
            Func<InstanceValueProperty, bool> preferDeepNavigation,
            Func<InstanceValueProperty, IInstanceProperty, string?> renderSpecialValuePath,
            Func<InstanceStructureProperty, Variable> createArrayItemVariable,
            Func<IInstanceProperty, string>? renderArrayPath = null,
            Func<InstanceStructureProperty, bool>? renderExplicitConstructorInvocation = null) {

            return RenderMembers(left, rightMembers, string.Empty);

            IEnumerable<string> RenderMembers(
                IInstancePropertyOwner currentLeft,
                IReadOnlyDictionary<SchemaNodeIdentity, List<IInstanceProperty>> currentRightMembers,
                string indent) {

                foreach (var prop in currentLeft.CreateProperties()) {
                    if (prop is InstanceValueProperty valueProp) {
                        var right = ResolveRightMember(valueProp, currentRightMembers, preferDeepNavigation(valueProp));
                        var rightPath = renderSpecialValuePath(valueProp, right)
                            ?? right.GetJoinedPathFromInstance(E_CsTs.CSharp, "!.");

                        yield return $$"""{{indent}}{{valueProp.Metadata.GetPropertyName(E_CsTs.CSharp)}} = {{rightPath}},""";

                    } else if (prop is InstanceStructureProperty structureProp) {
                        if (!structureProp.Metadata.IsArray) {
                            yield return $$"""{{indent}}{{structureProp.Metadata.GetPropertyName(E_CsTs.CSharp)}} = new() {""";
                            foreach (var line in RenderMembers(structureProp, currentRightMembers, indent + "    ")) {
                                yield return line;
                            }
                            yield return $$"""{{indent}}},""";
                        } else {
                            var loopVar = createArrayItemVariable(structureProp);
                            var overridedDict = CloneCandidateDictionary(currentRightMembers);
                            foreach (var member in loopVar.Create1To1PropertiesRecursively()) {
                                var key = member.Metadata.SchemaPathNode.ToMappingKey();
                                if (!overridedDict.TryGetValue(key, out var candidates)) {
                                    candidates = [];
                                    overridedDict[key] = candidates;
                                }
                                candidates.Add(member);
                            }

                            var arrayPath = ResolveRightMember(structureProp, currentRightMembers);
                            var renderedArrayPath = renderArrayPath?.Invoke(arrayPath)
                                ?? arrayPath.GetJoinedPathFromInstance(E_CsTs.CSharp, "!.");

                            var constructor = (renderExplicitConstructorInvocation?.Invoke(structureProp) ?? false)
                                ? $"new {structureProp.Metadata.GetTypeName(E_CsTs.CSharp)}()"
                                : $"new {structureProp.Metadata.GetTypeName(E_CsTs.CSharp)}";
                            yield return $$"""{{indent}}{{structureProp.Metadata.GetPropertyName(E_CsTs.CSharp)}} = {{renderedArrayPath}}.Select({{loopVar.Name}} => {{constructor}} {""";
                            foreach (var line in RenderMembers(structureProp, overridedDict, indent + "    ")) {
                                yield return line;
                            }
                            yield return $$"""{{indent}}}).ToList(),""";
                        }
                    }
                }
            }
        }

        internal static IInstanceProperty ResolveRightMember(IInstanceProperty left, IReadOnlyDictionary<SchemaNodeIdentity, List<IInstanceProperty>> rightMembers, bool preferDeepNavigation = false) {
            var key = left.Metadata.SchemaPathNode.ToMappingKey();
            if (!rightMembers.TryGetValue(key, out var candidates) || candidates.Count == 0) {
                candidates = rightMembers.Values
                    .SelectMany(props => props)
                    .Where(candidate => GetPropertyNamePath(candidate).SequenceEqual(GetPropertyNamePath(left)))
                    .ToList();

                if (candidates.Count == 0) {
                    throw new InvalidOperationException($"右辺に対応するプロパティが見つかりません: {key}");
                }
            }
            if (candidates.Count == 1) {
                return candidates[0];
            }

            var leftPath = left.GetPathFromInstance()
                .Select(p => p.Metadata.SchemaPathNode.ToMappingKey())
                .ToArray();

            return candidates
                .OrderByDescending(candidate => preferDeepNavigation ? candidate.GetPathFromInstance().Count() : 0)
                .ThenByDescending(candidate => CountCommonPathSegments(leftPath, candidate))
                .ThenBy(candidate => candidate.Metadata.GetPropertyName(E_CsTs.CSharp).Length)
                .ThenBy(candidate => candidate.GetJoinedPathFromInstance(E_CsTs.CSharp, "!.").Length)
                .First();

            static int CountCommonPathSegments(IReadOnlyList<SchemaNodeIdentity> leftPath, IInstanceProperty candidate) {
                var rightPath = candidate.GetPathFromInstance()
                    .Select(p => p.Metadata.SchemaPathNode.ToMappingKey())
                    .ToArray();
                var common = 0;
                var max = Math.Min(leftPath.Count, rightPath.Length);
                for (var i = 0; i < max; i++) {
                    if (leftPath[i] != rightPath[i]) break;
                    common++;
                }
                return common;
            }

            static IEnumerable<string> GetPropertyNamePath(IInstanceProperty property) {
                return property.GetPathFromInstance()
                    .Select(p => p.Metadata.GetPropertyName(E_CsTs.CSharp));
            }
        }

        internal static string RenderViewValuePath(IInstanceProperty right, string actualValueMemberName) {
            if (right.Owner is IInstanceProperty ownerProperty) {
                return $"{ownerProperty.GetJoinedPathFromInstance(E_CsTs.CSharp, "!.")}!.{actualValueMemberName}";
            }
            return $"{right.Root.Name}.{actualValueMemberName}";
        }

        private static Dictionary<SchemaNodeIdentity, List<IInstanceProperty>> CloneCandidateDictionary(IReadOnlyDictionary<SchemaNodeIdentity, List<IInstanceProperty>> rightMembers) {
            return rightMembers.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());
        }
    }
}
