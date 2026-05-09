using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Models.ReadModel2Modules {
    internal class DisplayData : EditablePresentationObject {
        internal DisplayData(AggregateBase aggregate) : base(aggregate) {
        }

        internal override string CsClassName => $"{Aggregate.PhysicalName}DisplayData";
        internal override string TsTypeName => $"{Aggregate.PhysicalName}DisplayData";
        internal override bool HasVersion => Aggregate is RootAggregate
                                          || Aggregate.XElement.Attribute(BasicNodeOptions.HasLifecycle.AttributeName) != null;
        internal bool HasLifeCycle => Aggregate is RootAggregate
                 || Aggregate is ChildrenAggregate
                 || Aggregate.XElement.Attribute(BasicNodeOptions.HasLifecycle.AttributeName) != null;
        internal const string VALUES_CS = "Values";
        internal const string VALUES_TS = "values";
        internal string ValueCsClassName => $"{CsClassName}Values";
        internal const string READONLY_CS = "ReadOnly";
        internal const string READONLY_TS = "readOnly";
        internal const string ALL_READONLY_CS = "AllReadOnly";
        internal const string ALL_READONLY_TS = "allReadOnly";
        internal string ReadOnlyCsClassName => $"{CsClassName}ReadOnly";
        internal string MessageCsClassName => $"{CsClassName}Messages";
        internal string MessageListCsClassName => $"{CsClassName}MessagesList";
        internal const string UNIQUE_ID_CS = "UniqueId";
        internal const string UNIQUE_ID_TS = "uniqueId";

        internal static IInstancePropertyOwnerMetadata GetLegacyCompatibleInstanceApiMetadata(AggregateBase aggregate) {
            return aggregate switch {
                RootAggregate root => new LegacyDisplayDataMetadata(new DisplayData(root)),
                ChildAggregate child => new EditablePresentationObjectChildDescendant(child),
                ChildrenAggregate children => new EditablePresentationObjectChildrenDescendant(children),
                _ => throw new InvalidOperationException(),
            };
        }

        internal new string RenderCSharpDeclaring(CodeRenderingContext ctx) {
            if (!ctx.IsLegacyCompatibilityMode()) {
                return base.RenderCSharpDeclaring(ctx);
            }

            var implements = new List<string>();
            if (Aggregate is RootAggregate) implements.Add("DisplayDataClassBase");
            if (HasLifeCycle) implements.Add(ISaveCommandConvertible.INTERFACE_NAME);
            var inheritance = implements.Count == 0 ? string.Empty : $" : {implements.Join(", ")}";

            return $$"""
            /// <summary>
            /// {{Aggregate.DisplayName}}の画面表示用データ構造
            /// </summary>
            public partial class {{CsClassName}}{{inheritance}} {
                /// <summary>値</summary>
                [JsonPropertyName("{{VALUES_TS}}")]
                public virtual {{ValueCsClassName}} {{VALUES_CS}} { get; set; } = new();

            {{GetChildMembers().SelectTextTemplate(c => $$"""
                [JsonPropertyName("{{c.PhysicalName}}")]
                public {{WithIndent(c.CsClassNameAsMember, "    ")}} {{c.PhysicalName}} { get; set; } = new();
              """)}}
                // TODO #43 ADDMODDDELを明示的に指定できるようにする
                /// <summary>このデータがDBに保存済みかどうか</summary>
                [JsonPropertyName("{{EXISTS_IN_DB_TS}}")]
                public virtual bool {{EXISTS_IN_DB_CS}} { get; set; }
                /// <summary>このデータに更新がかかっているかどうか</summary>
                [JsonPropertyName("{{WILL_BE_CHANGED_TS}}")]
                public virtual bool {{WILL_BE_CHANGED_CS}} { get; set; }
                /// <summary>このデータが更新確定時に削除されるかどうか</summary>
                [JsonPropertyName("{{WILL_BE_DELETED_TS}}")]
                public virtual bool {{WILL_BE_DELETED_CS}} { get; set; }
                /// <summary>
                /// 画面操作で使用される一意なID。このインスタンスが作成されたときに発番される。
                /// 画面上で行データの更新や行移動などがなされたりしたときに当該インスタンスを適切に追跡出来るようにするために必要。
                /// このIDは永続化の対象とならない。
                /// インスタンスをnewする場合は明示的にGUIDを設定する ※Guid.NewGuid().ToString()
                /// </summary>
                [JsonPropertyName("{{UNIQUE_ID_TS}}")]
                public virtual required string {{UNIQUE_ID_CS}} { get; set; }
            {{If(HasVersion, () => $$"""
                /// <summary>楽観排他制御用のバージョニング情報</summary>
                [JsonPropertyName("{{VERSION_TS}}")]
                public virtual int? {{VERSION_CS}} { get; set; }
            """)}}
                /// <summary>どの項目が読み取り専用か</summary>
                [JsonPropertyName("{{READONLY_TS}}")]
                public virtual {{ReadOnlyCsClassName}} {{READONLY_CS}} { get; set; } = new();
            }
            /// <summary>
            /// {{Aggregate.DisplayName}}の画面表示用データの値の部分
            /// </summary>
            public partial class {{ValueCsClassName}} {
            {{GetValueMembers().SelectTextTemplate(member => $$"""
                {{WithIndent(RenderLegacyValueMember(member), "    ")}}
            """)}}
            }
            {{RenderLegacyMessageClasses()}}
            /// <summary>
            /// {{Aggregate.DisplayName}}の画面表示用データの読み取り専用情報格納部分
            /// </summary>
            public partial class {{ReadOnlyCsClassName}} {
                /// <summary>{{Aggregate.DisplayName}}全体が読み取り専用か否か</summary>
                [JsonPropertyName("{{ALL_READONLY_TS}}")]
                public bool {{ALL_READONLY_CS}} { get; set; }
            {{GetValueMembers().SelectTextTemplate(member => $$"""
                /// <summary>{{member.GetPropertyName(E_CsTs.CSharp)}}が読み取り専用か否か</summary>
                public bool {{member.GetPropertyName(E_CsTs.CSharp)}} { get; set; }
            """)}}
            }
            """;

            static string RenderLegacyValueMember(IEditablePresentationObjectValueOrRefMember member) {
                return member switch {
                    EditablePresentationObjectValueMember value => $$"""
                        /// <summary>{{value.Member.DisplayName}}</summary>
                        public virtual {{value.Member.Type.CsDomainTypeName.Replace("DateOnly", "Date")}}? {{value.Member.PhysicalName}} { get; set; }
                        """,
                    EditablePresentationObjectRefMember reference => $$"""
                        /// <summary>{{reference.Member.DisplayName}}</summary>
                        public virtual {{((IInstanceStructurePropertyMetadata)reference).GetTypeName(E_CsTs.CSharp)}} {{reference.Member.PhysicalName}} { get; set; } = new();
                        """,
                    _ => throw new InvalidOperationException(),
                };
            }

            string RenderLegacyMessageClasses() {
                var members = GetLegacyMessageMembers().ToArray();

                return $$"""
                    {{If(Aggregate is RootAggregate, () => $$"""
                    /// <summary>
                    /// {{Aggregate.DisplayName}}の画面表示用データのメッセージ情報格納部分。
                    /// （クライアント側からWebサーバー側に一度に複数件のデータが送られてくる場合のためのもの）
                    /// </summary>
                    public partial class {{MessageListCsClassName}} : DisplayMessageContainerList<{{MessageCsClassName}}> {
                        public {{MessageListCsClassName}}(IEnumerable<string> path) : base(path, i => new([.. path, i.ToString()])) { }

                        /// <summary>すべてのメッセージを画面ルートに転送する場合に用いられるコンストラクタ</summary>
                        public {{MessageListCsClassName}}(IDisplayMessageContainer origin) : base(origin, _ => new(origin)) { }
                    }
                    """)}}

                    /// <summary>
                    /// {{Aggregate.DisplayName}}の画面表示用データのメッセージ情報格納部分。
                    /// WriteModelのデータとのマッピングが必要になる場合、このクラスを使用せず、
                    /// 別途当該WriteModelのエラーデータのインターフェースを実装したクラスを新規作成して、そちらを使用してください。
                    /// </summary>
                    public partial class {{MessageCsClassName}} : DisplayMessageContainerBase {
                        public {{MessageCsClassName}}(IEnumerable<string> path) : base(path) {
                    {{members.SelectTextTemplate(member => $$"""
                        {{WithIndent(member.RenderPathConstructor(), "        ")}}
                    """)}}
                        }
                        /// <summary>すべてのメッセージを画面ルートに転送する場合に用いられるコンストラクタ</summary>
                        public {{MessageCsClassName}}(IDisplayMessageContainer origin) : base(origin) {
                    {{members.SelectTextTemplate(member => $$"""
                        {{WithIndent(member.RenderOriginConstructor(), "        ")}}
                    """)}}
                        }

                    {{members.SelectTextTemplate(member => $$"""
                    public {{member.TypeName}} {{member.PropertyName}} { get; }
                    """)}}

                        public override IEnumerable<IDisplayMessageContainer> EnumerateChildren() {
                    {{members.SelectTextTemplate(member => $$"""
                        yield return {{member.PropertyName}};
                    """)}}
                        }
                    }
                    """;
            }
        }

        private IEnumerable<LegacyMessageMember> GetLegacyMessageMembers() {
            foreach (var member in GetValueMembers()) {
                yield return new LegacyMessageMember(member.GetPropertyName(E_CsTs.CSharp), "DisplayMessageContainer");
            }

            foreach (var child in GetChildMembers()) {
                var typeName = child is EditablePresentationObjectChildrenDescendant
                    ? $"DisplayMessageContainerList<{child.CsClassName}Messages>"
                    : $"{child.CsClassName}Messages";
                yield return new LegacyMessageMember(child.PhysicalName, typeName);
            }
        }

        private readonly record struct LegacyMessageMember(string PropertyName, string TypeName) {
            internal string RenderPathConstructor() {
                return TypeName.StartsWith("DisplayMessageContainerList<", StringComparison.Ordinal)
                  ? $"{PropertyName} = new([.. path, \"{PropertyName}\"], i => new([.. path, \"{PropertyName}\", i.ToString()]));"
                  : $"{PropertyName} = new([.. path, \"{PropertyName}\"]);";
            }

            internal string RenderOriginConstructor() {
                return TypeName.StartsWith("DisplayMessageContainerList<", StringComparison.Ordinal)
                  ? $"{PropertyName} = new(origin, _ => new(origin));"
                  : $"{PropertyName} = new(origin);";
            }
        }

        internal static SourceFile RenderBaseClass() => new() {
            FileName = "DisplayDataClassBase.cs",
            Contents = CodeRenderingContext.CurrentContext.IsLegacyCompatibilityMode()
            ? """
              /// <summary>
              /// 画面表示用データの基底クラス
              /// </summary>
              public abstract partial class DisplayDataClassBase {
              }
              """
            : $$"""
              namespace {{CodeRenderingContext.CurrentContext.Config.RootNamespace}};

              /// <summary>
              /// 画面表示用データの基底クラス
              /// </summary>
              public abstract partial class DisplayDataClassBase {
              }
              """,
        };
        internal static string RenderCSharpRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            var tree = rootAggregate
                .EnumerateThisAndDescendants()
                .Select<AggregateBase, EditablePresentationObject>(agg => agg switch {
                    RootAggregate root => new DisplayData(root),
                    ChildAggregate child => new EditablePresentationObjectChildDescendant(child),
                    ChildrenAggregate children => new EditablePresentationObjectChildrenDescendant(children),
                    _ => throw new InvalidOperationException(),
                });

            return $$"""
                #region 画面表示用データ
                {{tree.SelectTextTemplate(disp => $$"""
                {{disp.RenderCSharpDeclaring(ctx)}}
                """)}}
                #endregion 画面表示用データ
                """;
        }
        internal static string RenderTypeScriptRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
            var tree = rootAggregate
                .EnumerateThisAndDescendants()
                .Select<AggregateBase, EditablePresentationObject>(agg => agg switch {
                    RootAggregate root => new DisplayData(root),
                    ChildAggregate child => new EditablePresentationObjectChildDescendant(child),
                    ChildrenAggregate children => new EditablePresentationObjectChildrenDescendant(children),
                    _ => throw new InvalidOperationException(),
                });

            return $$"""
                //#region 画面表示用データ
                {{tree.SelectTextTemplate(disp => $$"""
                {{disp.RenderTypeScriptType(ctx)}}
                """)}}
                //#endregion 画面表示用データ
                """;
        }

        internal string RenderTsNewObjectFunction(CodeRenderingContext ctx) {
            return $$"""
                /** {{Aggregate.DisplayName}}の画面表示用データの新しいインスタンスを作成します。 */
                export const {{TsNewObjectFunction}} = (): {{TsTypeName}} => {
                  // タイムスタンプ(ミリ秒) + ランダム文字列 で一意のIDを生成
                  function generateRandomUniqueId(): string {
                    return [
                      Date.now().toString(36).substring(0, 8),
                      Math.random().toString(36).substring(2, 6),
                      Math.random().toString(36).substring(2, 6),
                      Math.random().toString(36).substring(2, 6),
                      Math.random().toString(36).replace('.', ''),
                    ].join('-')
                  }

                  return {{WithIndent(RenderTsNewObjectFunctionBody(), "  ")}}
                }
                """;
        }
        internal string RenderExtractPrimaryKey() {
            var keys = Aggregate.GetKeyVMs().ToArray();
            var dataProperties = new Variable("data", this)
                .Create1To1PropertiesRecursively()
                .ToDictionary(p => p.Metadata.SchemaPathNode.ToMappingKey());

            return $$"""
                /** {{Aggregate.DisplayName}}の画面表示用データのインスタンスから主キーを抽出して配列にします。 */
                export const {{PkExtractFunctionName}} = (data: {{TsTypeName}}): [{{keys.Select(k => $"{k.PhysicalName}: {k.Type.TsTypeName} | null | undefined").Join(", ")}}] => {
                  return [
                {{keys.SelectTextTemplate(k => $$"""
                    {{dataProperties[k.ToMappingKey()].GetJoinedPathFromInstance(E_CsTs.TypeScript, ".")}},
                """)}}
                  ]
                }
                """;
        }
        internal string RenderAssignPrimaryKey() {
            var keys = Aggregate.GetKeyVMs().ToArray();
            var dataProperties = new Variable("data", this)
                    .Create1To1PropertiesRecursively()
                    .ToDictionary(p => p.Metadata.SchemaPathNode.ToMappingKey());

            return $$"""
                /** {{Aggregate.DisplayName}}の画面表示用データのインスタンスに主キーを設定します。 */
                export const {{PkAssignFunctionName}} = (data: {{TsTypeName}}, keys: [{{keys.Select(k => $"{k.PhysicalName}: {k.Type.TsTypeName} | undefined").Join(", ")}}]): void => {
                  if (keys.length !== {{keys.Length}}) {
                    console.error(`主キーの数が一致しません。個数は{{keys.Length}}であるべきところ${keys.length}個です。`)
                    return
                  }
                {{keys.SelectTextTemplate((k, i) => $$"""
                  {{dataProperties[k.ToMappingKey()].GetJoinedPathFromInstance(E_CsTs.TypeScript, ".")}} = keys[{{i}}]
                """)}}
                }
                """;
        }
        internal string RenderDeepEqualFunctionRecursively(CodeRenderingContext ctx) {
            return $$"""
                /** 2つの{{Aggregate.DisplayName}}オブジェクトの値を比較し、一致しているかを返します。 */
                export const {{DeepEqualFunction}} = (a: {{TsTypeName}}, b: {{TsTypeName}}): boolean => {
                  {{WithIndent(RenderDeepEqualBody(Aggregate, "a", "b"), "  ")}}
                  if (a.{{WILL_BE_DELETED_TS}} !== b.{{WILL_BE_DELETED_TS}}) return false
                  return true
                }
                """;
        }
        internal string RenderCheckChangesFunction(CodeRenderingContext ctx) {
            return $$"""
                /** 更新前後の値をディープイコールで判定し、変更があったオブジェクトのwillBeChangedプロパティをtrueに設定して返します。 */
                export const {{CheckChangesFunction}} = ({ defaultValues, currentValues }: {
                  defaultValues: {{TsTypeName}}
                  currentValues: {{TsTypeName}}
                }): boolean => {
                  const changed = !{{DeepEqualFunction}}(defaultValues, currentValues)
                  currentValues.{{WILL_BE_CHANGED_TS}} = changed
                  return changed
                }
                """;
        }
        internal string RenderSetKeysReadOnly(CodeRenderingContext ctx) {
            var keys = Aggregate.GetKeyVMs().ToArray();

            if (!ctx.IsLegacyCompatibilityMode()) {
                return $$"""
              /// <summary>
              /// {{Aggregate.DisplayName}}の主キー項目を読み取り専用にします。
              /// 現行の ReadModel2 移植途中では読み取り専用メタデータ構造をまだ持たないため no-op とする。
              /// </summary>
              private void SetKeysReadOnly({{CsClassName}} displayData) {
              }
              """;
            }

            return $$"""
                /// <summary>
                /// {{Aggregate.DisplayName}}の主キー項目を読み取り専用にします。
                /// </summary>
                private void SetKeysReadOnly({{CsClassName}} displayData) {
                {{keys.SelectTextTemplate(key => $$"""
                    displayData.{{READONLY_CS}}.{{key.PhysicalName}} = true;
                """)}}
                }
                """;
        }

        internal string PkExtractFunctionName => $"extract{Aggregate.PhysicalName}Keys";
        internal string PkAssignFunctionName => $"assign{Aggregate.PhysicalName}Keys";
        internal string DeepEqualFunction => $"deepEquals{TsTypeName}";
        internal string CheckChangesFunction => $"checkChanges{TsTypeName}";
        internal string UiConstraintTypeName => $"{Aggregate.PhysicalName}ConstraintType";
        internal string UiConstraingValueName => $"{Aggregate.PhysicalName}Constraints";

        private sealed class LegacyDisplayDataMetadata : IInstancePropertyOwnerMetadata {
            internal LegacyDisplayDataMetadata(DisplayData displayData) {
                _displayData = displayData;
                _values = new LegacyDisplayDataValuesMember(displayData);
            }

            private readonly DisplayData _displayData;
            private readonly LegacyDisplayDataValuesMember _values;

            public IEnumerable<IInstancePropertyMetadata> GetMembers() {
                yield return _values;

                foreach (var child in _displayData.GetChildMembers()) {
                    yield return (IInstancePropertyMetadata)child;
                }
            }
        }

        private sealed class LegacyDisplayDataValuesMember : IInstanceStructurePropertyMetadata {
            internal LegacyDisplayDataValuesMember(DisplayData displayData) {
                _displayData = displayData;
            }

            private readonly DisplayData _displayData;

            public ISchemaPathNode SchemaPathNode => _displayData.Aggregate;
            public bool IsArray => false;
            public string GetPropertyName(E_CsTs csts) => csts == E_CsTs.CSharp ? VALUES_CS : VALUES_TS;
            public string GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? _displayData.ValueCsClassName : _displayData.ValueCsClassName;
            public string DisplayName => "値";

            public IEnumerable<IInstancePropertyMetadata> GetMembers() {
                foreach (var member in _displayData.GetValueMembers()) {
                    yield return member;
                }
            }
        }

        internal string RenderUiConstraintType(CodeRenderingContext ctx) {
            return $$"""
                /** {{Aggregate.DisplayName}}の各メンバーの制約の型 */
                type {{UiConstraintTypeName}} = {
                  {{WithIndent(RenderUiConstraintMembers(this), "  ")}}
                }
                """;
        }

        internal string RenderUiConstraintValue(CodeRenderingContext ctx) {
            return $$"""
                /** {{Aggregate.DisplayName}}の各メンバーの制約の具体的な値 */
                export const {{UiConstraingValueName}}: {{UiConstraintTypeName}} = {
                  {{WithIndent(RenderUiConstraintValues(this), "  ")}}
                }
                """;
        }

        private static string RenderDeepEqualBody(AggregateBase aggregate, string left, string right) {
            return $$"""
                {{aggregate.GetMembers().SelectTextTemplate(member => member switch {
                ValueMember valueMember when !valueMember.OnlySearchCondition => RenderValueMember(valueMember, left, right),
                RefToMember refToMember => RenderRefMember(refToMember, left, right),
                ChildAggregate childAggregate => RenderChildAggregate(childAggregate, left, right),
                ChildrenAggregate childrenAggregate => RenderChildrenAggregate(childrenAggregate, left, right),
                _ => string.Empty,
            })}}
                """;

            static string RenderValueMember(ValueMember valueMember, string left, string right) {
                return $$"""
                    if (({{left}}.{{valueMember.PhysicalName}} ?? undefined) !== ({{right}}.{{valueMember.PhysicalName}} ?? undefined)) return false
                    """;
            }

            static string RenderRefMember(RefToMember refToMember, string left, string right) {
                return $$"""
                    if (JSON.stringify({{left}}.{{refToMember.PhysicalName}} ?? null) !== JSON.stringify({{right}}.{{refToMember.PhysicalName}} ?? null)) return false
                    """;
            }

            static string RenderChildAggregate(ChildAggregate childAggregate, string left, string right) {
                return $$"""
                    if ((({{left}}.{{childAggregate.PhysicalName}} ?? undefined) === undefined) !== ((({{right}}.{{childAggregate.PhysicalName}} ?? undefined) === undefined))) return false
                    if ({{left}}.{{childAggregate.PhysicalName}} && {{right}}.{{childAggregate.PhysicalName}}) {
                      {{WithIndent(RenderDeepEqualBody(childAggregate, $"{left}.{childAggregate.PhysicalName}", $"{right}.{childAggregate.PhysicalName}"), "  ")}}
                    }
                    """;
            }

            static string RenderChildrenAggregate(ChildrenAggregate childrenAggregate, string left, string right) {
                var leftItems = $"{left}{'.'}{childrenAggregate.PhysicalName}";
                var rightItems = $"{right}{'.'}{childrenAggregate.PhysicalName}";

                return $$"""
                    const {{childrenAggregate.GetLoopVarName("leftItems")}} = {{leftItems}} ?? []
                    const {{childrenAggregate.GetLoopVarName("rightItems")}} = {{rightItems}} ?? []
                    if ({{childrenAggregate.GetLoopVarName("leftItems")}}.length !== {{childrenAggregate.GetLoopVarName("rightItems")}}.length) return false
                    for (let i = 0; i < {{childrenAggregate.GetLoopVarName("leftItems")}}.length; i++) {
                      const leftItem = {{childrenAggregate.GetLoopVarName("leftItems")}}[i]
                      const rightItem = {{childrenAggregate.GetLoopVarName("rightItems")}}[i]
                      {{WithIndent(RenderDeepEqualBody(childrenAggregate, "leftItem", "rightItem"), "  ")}}
                    }
                    """;
            }
        }

        private static string RenderUiConstraintMembers(EditablePresentationObject displayData) {
            return $$"""
                values: {
                {{displayData.GetValueMembers().SelectTextTemplate(member => member switch {
                EditablePresentationObjectValueMember valueMember => $$"""
                  {{valueMember.PropertyName}}: {{GetUiConstraintTypeName(valueMember.Member)}}
                """,
                EditablePresentationObjectRefMember refMember => $$"""
                  {{refMember.PropertyName}}: AutoGeneratedUtil.MemberConstraintBase
                """,
                _ => string.Empty,
            })}}
                }
                {{displayData.GetChildMembers().SelectTextTemplate(child => $$"""
                {{child.PhysicalName}}: {
                  {{WithIndent(RenderUiConstraintMembers(child), "  ")}}
                }
                """)}}
                """;
        }

        private static string RenderUiConstraintValues(EditablePresentationObject displayData) {
            return $$"""
                values: {
                {{displayData.GetValueMembers().SelectTextTemplate(member => member switch {
                EditablePresentationObjectValueMember valueMember => RenderUiConstraintValue(valueMember),
                EditablePresentationObjectRefMember refMember => RenderUiConstraintValue(refMember),
                _ => string.Empty,
            })}}
                },
                {{displayData.GetChildMembers().SelectTextTemplate(child => $$"""
                {{child.PhysicalName}}: {
                  {{WithIndent(RenderUiConstraintValues(child), "  ")}}
                },
                """)}}
                """;
        }

        private static string GetUiConstraintTypeName(ValueMember valueMember) {
            return valueMember.Type.CsDomainTypeName switch {
                "string" => "AutoGeneratedUtil.StringMemberConstraint",
                "int" or "decimal" => "AutoGeneratedUtil.NumberMemberConstraint",
                _ => "AutoGeneratedUtil.MemberConstraintBase",
            };
        }

        private static string RenderUiConstraintValue(EditablePresentationObjectValueMember valueMember) {
            var valueLines = new System.Collections.Generic.List<string>();
            if (valueMember.Member.IsKey || valueMember.Member.IsNotNull) valueLines.Add("required: true,");
            if (valueMember.Member.MaxLength is int maxLength) valueLines.Add($"maxLength: {maxLength},");
            if (!string.IsNullOrWhiteSpace(valueMember.Member.CharacterType)) valueLines.Add($"characterType: '{valueMember.Member.CharacterType}',");
            if (valueMember.Member.TotalDigit is int totalDigit) valueLines.Add($"totalDigit: {totalDigit},");
            if (valueMember.Member.DecimalPlace is int decimalPlace) valueLines.Add($"decimalPlace: {decimalPlace},");
            if (valueMember.Member.IsNotNegative) valueLines.Add("notNegative: true,");

            return $$"""
                {{valueMember.PropertyName}}: {
                {{valueLines.SelectTextTemplate(line => $$"""
                  {{line}}
                """)}}
                },
                """;
        }

        private static string RenderUiConstraintValue(EditablePresentationObjectRefMember refMember) {
            var required = refMember.Member.IsKey || refMember.Member.IsNotNull;
            return $$"""
                {{refMember.PropertyName}}: {
                {{If(required, () => $$"""
                  required: true,
                """)}}
                },
                """;
        }
    }
}
