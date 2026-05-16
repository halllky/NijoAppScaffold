
using System;
using System.Collections.Generic;
using System.Linq;
using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.QueryModelModules;
using Nijo.Parts.Common;
using Nijo.Util.DotnetEx;

namespace Nijo.Parts.JavaScript;

/// <summary>
/// 編集画面の離脱チェックなどに用いられるディープイコール関数と、
/// <see cref="EditablePresentationObject.WILL_BE_CHANGED_TS" /> のフラグ設定用の関数。
/// </summary>
internal class DeepEqualFunction {

    internal DeepEqualFunction(EditablePresentationObject displayData) {
        _displayData = displayData;
    }

    private readonly EditablePresentationObject _displayData;

    internal string FunctionName => $"deepEqual{_displayData.Aggregate.PhysicalName}";

    internal const string JSDOC = $$"""
        /**
         * 2つの画面表示用オブジェクトをディープイコールで比較し、等しい場合はtrueを返す関数。
         *
         * * 以下は差があっても変更なしと判定します。
         *   * ネストされたオブジェクトや配列の参照等価性
         *   * {{EditablePresentationObject.EXISTS_IN_DB_TS}} の差分
         *   * {{EditablePresentationObject.WILL_BE_CHANGED_TS}} の差分
         * * 以下は変更ありと判定します。
         *   * 配列の要素の並び替え。 {{EditablePresentationObject.INSTANCE_ID_TS}} の順番で比較します。
         *   * {{EditablePresentationObject.WILL_BE_DELETED_TS}} の差分
         * * 以下はオプションでルールを変更できます。
         *   * stringやnumberの値比較ルール。
         *     例えばnullとundefinedと空文字を同じとみなすかどうかなど。
         *     未指定の場合は Object.is が使用されます。
         *
         * @param left 比較対象のオブジェクト
         * @param right 比較対象のオブジェクト。オプションを指定すると、差分があるオブジェクトの {{EditablePresentationObject.WILL_BE_CHANGED_TS}} を自動的に変更することもできます。
         * @param option 比較ルールのオプション
         */
        """;

    internal class OptionType : IMultiAggregateSourceFile {
        internal const string TYPENAME = "DisplayDataDeepEqualOption";
        void IMultiAggregateSourceFile.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
            // 特になし
        }

        void IMultiAggregateSourceFile.Render(CodeRenderingContext ctx) {
            ctx.ReactProject(dir => {
                dir.Directory("util", utilDir => {
                    utilDir.Generate(new SourceFile() {
                        FileName = $"{TYPENAME}.ts",
                        Contents = RenderOptionType(ctx),
                    });
                });
            });
        }
        private static string RenderOptionType(CodeRenderingContext ctx) {

            const string TYPE_ENUM = "Enum";
            const string TYPE_VALUE_OBJECT = "ValueObject";
            const string TYPE_BYTE_ARRAY = "ByteArray";

            var valueMemberTypePhysicalNames = ctx.SchemaParser
                .GetValueMemberTypes()
                .Select(t => {
                    if (t is ValueMemberTypes.StaticEnumMember staticEnum) {
                        return new {
                            DisplayName = "列挙体",
                            TypeIdentifier = TYPE_ENUM,
                            t.TsTypeName,
                        };
                    } else if (t is ValueMemberTypes.ValueObjectMember valueObject) {
                        return new {
                            DisplayName = "値オブジェクト",
                            TypeIdentifier = TYPE_VALUE_OBJECT,
                            TsTypeName = t.TsTypeName.Split('.').Last(),
                        };
                    } else if (t is ValueMemberTypes.ByteArrayMember) {
                        return new {
                            t.DisplayName,
                            TypeIdentifier = TYPE_BYTE_ARRAY,
                            TsTypeName = "unknown",
                        };
                    } else {
                        return new {
                            t.DisplayName,
                            TypeIdentifier = t.TypePhysicalName,
                            t.TsTypeName,
                        };
                    }
                })
                .GroupBy(t => t.TypeIdentifier) // Enum, ValueObject が存在する都合でグルーピング
                .Select(group => new {
                    group.First().DisplayName,
                    TypeIdentifier = group.Key,
                    TsTypeName = group.Count() >= 2
                        ? ("\r\n    | " + group.Select(t => t.TsTypeName).Join("\r\n    | "))
                        : group.First().TsTypeName,
                })
                .ToArray();

            // voTypeNames はグルーピング後の valueMemberTypePhysicalNames から取得すると
            // 2個以上あるとき TsTypeName が結合文字列になって import文などが壊れるため、
            // グルーピング前のスキーマから個別に取得する
            var voTypeNames = ctx.SchemaParser
                .GetValueMemberTypes()
                .Where(t => t is ValueMemberTypes.ValueObjectMember)
                .Select(t => t.TsTypeName.Split('.').Last())
                .ToArray();

            return $$"""
                import * as EnumDefs from "../enum-defs"
                {{voTypeNames.SelectTextTemplate(t => $$"""
                import { {{t}} } from "./{{t}}"
                """)}}

                export interface DeepEqualValueMemberTypeMap {
                {{valueMemberTypePhysicalNames.SelectTextTemplate(g => $$"""
                  /** {{g.DisplayName}} */
                  {{g.TypeIdentifier}}: {{g.TsTypeName}}
                """)}}
                }

                /**
                 * DisplayData のディープイコール関数のオプション
                 */
                export type {{TYPENAME}} = {
                  /**
                   * 第2引数のオブジェクトの {{EditablePresentationObject.WILL_BE_CHANGED_TS}} を変更有無に従って true または false に変更します。
                   * 未指定の場合は比較だけを行います。
                   */
                  setRightObjectWillBeChanged?: boolean

                  /**
                   * 値の等価比較を行い、一致する場合にtrueを返す関数。未指定の場合は Object.is が使用されます。
                   */
                  compareFunction?: (...args: CompareFunctionArgs) => boolean
                };

                type CompareFunctionArgs = {
                  [K in keyof DeepEqualValueMemberTypeMap]: [
                    type: K,
                    left: DeepEqualValueMemberTypeMap[K] | null | undefined,
                    right: DeepEqualValueMemberTypeMap[K] | null | undefined,
                    /** type が {{TYPE_ENUM}} の場合に指定される列挙体の型 */
                    enumType?: keyof EnumDefs.EnumTypeMap,
                    /** type が {{TYPE_VALUE_OBJECT}} の場合に指定される値オブジェクトの型 */
                    valueObjectType?: {{(voTypeNames.Length == 0 ? "never" : voTypeNames.Select(t => $"'{t}'").Join(" | "))}}
                  ]
                }[keyof DeepEqualValueMemberTypeMap]
                """;
        }
    }

    internal string Render(CodeRenderingContext ctx) {

        var valueMemberTypePhysicalNames = ctx.SchemaParser
            .GetValueMemberTypes()
            .Select(t => new {
                t.DisplayName,
                t.TypePhysicalName,
                t.TsTypeName,
            });

        var left = new Variable("left", _displayData);
        var right = new Variable("right", _displayData);

        var varNameCounter = 1;

        // 関数内に宣言される比較関数
        static string LocalCompareFunc(EditablePresentationObject disp) {
            return disp.Aggregate is RootAggregate
                ? $"areEqual{disp.Aggregate.PhysicalName}"
                : $"areEqual{disp.Aggregate.EnumerateThisAndAncestors().Skip(1).Select(a => a.PhysicalName).Join("_")}";
        }
        IEnumerable<EditablePresentationObject> CollectDescendants(EditablePresentationObject disp) {
            yield return disp;
            foreach (var child in disp.GetChildMembers()) {
                foreach (var descendant in CollectDescendants(child)) {
                    yield return descendant;
                }
            }
        }

        return $$"""
            //#region ディープイコール関数
            {{JSDOC}}
            export function {{FunctionName}}({{left.Name}}: {{_displayData.TsTypeName}}, {{right.Name}}: {{_displayData.TsTypeName}}, option?: Util.{{OptionType.TYPENAME}}): boolean {
              const compareFunction: Exclude<Util.{{OptionType.TYPENAME}}["compareFunction"], undefined> = option?.compareFunction ?? ((_, left, right, __?, ___?) => Object.is(left, right));
              let areEqual = true;

              {{WithIndent(RenderAggregate(_displayData, left, right))}}

              return areEqual;

              // ------ 値比較関数 --------
            {{CollectDescendants(_displayData).SelectTextTemplate(disp => $$"""

              {{WithIndent(RenderLocalCompareFunction(disp))}}
            """)}}
            }
            //#endregion ディープイコール関数
            """;

        string RenderAggregate(EditablePresentationObject disp, IInstancePropertyOwner leftInstance, IInstancePropertyOwner rightInstance) {

            var leftMembers = leftInstance
                .Create1To1PropertiesRecursively()
                .ToDictionary(p => p.Metadata.SchemaPathNode.ToMappingKey());
            var rightMembers = rightInstance
                .Create1To1PropertiesRecursively()
                .ToDictionary(p => p.Metadata.SchemaPathNode.ToMappingKey());

            // ルート, Child
            if (disp.Aggregate is RootAggregate
             || disp is EditablePresentationObject.EditablePresentationObjectChildDescendant) {
                var leftObj = disp.Aggregate is RootAggregate
                    ? left.Name
                    : leftMembers[disp.Aggregate.ToMappingKey()].GetJoinedPathFromInstance(E_CsTs.TypeScript, ".");
                var rightObj = disp.Aggregate is RootAggregate
                    ? right.Name
                    : rightMembers[disp.Aggregate.ToMappingKey()].GetJoinedPathFromInstance(E_CsTs.TypeScript, ".");

                return $$"""
                    // {{disp.Aggregate.DisplayName}} の等価比較
                    if ({{LocalCompareFunc(disp)}}({{leftObj}}, {{rightObj}}, compareFunction)) {
                      if (option?.setRightObjectWillBeChanged) {{rightObj}}.{{EditablePresentationObject.WILL_BE_CHANGED_TS}} = false;
                    } else {
                      if (option?.setRightObjectWillBeChanged) {{rightObj}}.{{EditablePresentationObject.WILL_BE_CHANGED_TS}} = true;
                      areEqual = false;
                    }
                    {{disp.GetChildMembers().SelectTextTemplate(child => $$"""

                    {{RenderAggregate(child, left, right)}}
                    """)}}
                    """;
            }

            // Children
            else if (disp is EditablePresentationObject.EditablePresentationObjectChildrenDescendant children) {
                var leftArr = leftMembers[children.ChildrenAggregate.ToMappingKey()].GetJoinedPathFromInstance(E_CsTs.TypeScript, "?.");
                var rightArr = rightMembers[children.ChildrenAggregate.ToMappingKey()].GetJoinedPathFromInstance(E_CsTs.TypeScript, "?.");
                var numX = varNameCounter++;
                var x = numX == 1 ? "" : numX.ToString(); // 変数名が被らないように、2回目以降は末尾に数字を付与
                var leftIds = $"leftIds{x}";
                var rightIds = $"rightIds{x}";
                var leftDict = $"leftDict{x}";
                var rightDict = $"rightDict{x}";
                var instanceId = $"instanceId{x}";
                var leftInLoop = new Variable($"x{x}", children);
                var rightInLoop = new Variable($"y{x}", children);

                return $$"""
                    // {{disp.Aggregate.DisplayName}} の等価比較
                    // 以下いずれかに該当すれば変更ありと判定
                    // * 配列の長さが変わっている
                    // * 要素の並び替えが発生している（{{EditablePresentationObject.INSTANCE_ID_TS}} 基準）
                    // * 配列の要素を {{EditablePresentationObject.INSTANCE_ID_TS}} で突合しそれぞれの値を比較
                    const {{leftIds}} = {{leftArr}}?.map(x => x.{{EditablePresentationObject.INSTANCE_ID_TS}}) ?? [];
                    const {{rightIds}} = {{rightArr}}?.map(x => x.{{EditablePresentationObject.INSTANCE_ID_TS}}) ?? [];
                    if ({{leftIds}}.length !== {{rightIds}}.length) {
                      areEqual = false;
                    } else {
                      for (let i = 0; i < {{leftIds}}.length; i++) {
                        if ({{leftIds}}[i] !== {{rightIds}}[i]) {
                          areEqual = false;
                          break;
                        }
                      }
                    }

                    const {{leftDict}} = new Map({{leftArr}}?.map(x => [x.{{EditablePresentationObject.INSTANCE_ID_TS}}, x]) ?? []);
                    const {{rightDict}} = new Map({{rightArr}}?.map(x => [x.{{EditablePresentationObject.INSTANCE_ID_TS}}, x]) ?? []);
                    for (const {{instanceId}} of Array.from(new Set([...{{leftIds}}, ...{{rightIds}}]))) {
                      const {{leftInLoop.Name}} = {{leftDict}}.get({{instanceId}});
                      const {{rightInLoop.Name}} = {{rightDict}}.get({{instanceId}});
                      if (!{{leftInLoop.Name}} && {{rightInLoop.Name}} && option?.setRightObjectWillBeChanged) {{rightInLoop.Name}}.{{EditablePresentationObject.WILL_BE_CHANGED_TS}} = true;
                      if (!{{leftInLoop.Name}} || !{{rightInLoop.Name}}) continue;
                      if ({{LocalCompareFunc(children)}}({{leftInLoop.Name}}, {{rightInLoop.Name}}, compareFunction)) {
                        if (option?.setRightObjectWillBeChanged) {{rightInLoop.Name}}.{{EditablePresentationObject.WILL_BE_CHANGED_TS}} = false;
                      } else {
                        if (option?.setRightObjectWillBeChanged) {{rightInLoop.Name}}.{{EditablePresentationObject.WILL_BE_CHANGED_TS}} = true;
                        areEqual = false;
                      }
                    {{disp.GetChildMembers().SelectTextTemplate(child => $$"""

                      {{WithIndent(RenderAggregate(child, leftInLoop, rightInLoop))}}
                    """)}}
                    }
                    """;
            } else {
                throw new Exception($"Unknown EditablePresentationObject type: {disp.GetType()}");
            }
        }

        string RenderLocalCompareFunction(EditablePresentationObject disp) {
            var left = new Variable("left", disp);
            var right = new Variable("right", disp);

            var leftMembers = left
                .Create1To1PropertiesRecursively()
                .ToDictionary(p => p.Metadata.SchemaPathNode.ToMappingKey());
            var rightMembers = right
                .Create1To1PropertiesRecursively()
                .ToDictionary(p => p.Metadata.SchemaPathNode.ToMappingKey());

            return $$"""
                function {{LocalCompareFunc(disp)}}({{left.Name}}: {{disp.TsTypeName}}, {{right.Name}}: {{disp.TsTypeName}}, equals: Exclude<Util.{{OptionType.TYPENAME}}["compareFunction"], undefined>): boolean {
                {{disp.GetValueMembers().SelectMany(m => RenderMember(m, onlyKey: false)).SelectTextTemplate(sourceCode => $$"""
                  {{WithIndent(sourceCode)}}
                """)}}
                  if (({{left.Name}}.{{EditablePresentationObject.WILL_BE_DELETED_TS}} ?? false) !== ({{right.Name}}.{{EditablePresentationObject.WILL_BE_DELETED_TS}} ?? false)) return false;
                  return true;
                }
                """;

            IEnumerable<string> RenderMember(IInstancePropertyMetadata member, bool onlyKey) {

                if (member is IInstanceValuePropertyMetadata valueMember) {
                    if (onlyKey && member is DisplayDataRef.RefDisplayDataValueMember refValueMember && !refValueMember.Member.IsKey) {
                        yield break; // 外部参照オブジェクトはキー以外を比較しない
                    }

                    var leftProp = leftMembers[valueMember.SchemaPathNode.ToMappingKey()]
                        .GetJoinedPathFromInstance(E_CsTs.TypeScript, "?.");
                    var rightProp = rightMembers[valueMember.SchemaPathNode.ToMappingKey()]
                        .GetJoinedPathFromInstance(E_CsTs.TypeScript, "?.");

                    var valueTypeName = valueMember.Type switch {
                        ValueMemberTypes.StaticEnumMember => "Enum",
                        ValueMemberTypes.ValueObjectMember => "ValueObject",
                        _ => valueMember.Type.TypePhysicalName,
                    };
                    var valueTypeAdditionalInfo = valueMember.Type switch {
                        ValueMemberTypes.StaticEnumMember => $", '{valueMember.Type.TsTypeName.Split('.').Last()}'",
                        ValueMemberTypes.ValueObjectMember => $", undefined, '{valueMember.Type.TsTypeName.Split('.').Last()}'",
                        _ => "",
                    };

                    yield return $$"""
                        if (!equals('{{valueTypeName}}', {{leftProp}}, {{rightProp}}{{valueTypeAdditionalInfo}})) return false;
                        """;

                } else if (member is IInstanceStructurePropertyMetadata structureMember) {
                    // Childはローカル比較関数で登場するので割愛
                    if (structureMember.SchemaPathNode is AggregateBase agg && agg.IsDescendantOf(disp.Aggregate)) yield break;

                    // RefTo の配列要素の差分はこの関数では追跡しない。
                    // ローカル比較関数の外でやっているChildrenの比較と同じようにすれば可能ではあるが、
                    // キーのみを比較対象とする方向性で仕様変更すれば、参照先の子配列はキーになりえないので比較しなくて済む。
                    if (structureMember.IsArray) {
                        yield return $$"""
                            // {{structureMember.SchemaPathNode.GetPathFromEntry().Select(x => x.XElement.Name.LocalName).Join(".")}} はディープイコールの対象となりません。
                            """;
                        yield break;
                    }

                    // 外部参照先のキー以外の項目もすべてディープイコールの比較対象とする。
                    // * キーのみにしようと思うと Structure, Query モデルでキー属性を定義できるようにする必要があり、
                    //   それらのモデルでは（ビューにマッピングされるQueryModelを除き）キーの役割がここしかないため、
                    //   仕様の複雑化を招くのを避けた。
                    // * ただ「RefToされる Query / Structure はキー必須」を nijo.xml 定義時のバリデーションに含めれば
                    //   利用者がそれほど迷うこともないか？
                    // var isRefEntry = structureMember is EditablePresentationObject.EditablePresentationObjectRefMember;

                    foreach (var member2 in structureMember.GetMembers()) {
                        foreach (var sourceCode in RenderMember(member2, onlyKey /*|| isRefEntry */)) {
                            yield return sourceCode;
                        }
                    }
                }
            }
        }
    }
}
