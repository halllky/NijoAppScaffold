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
                ChildAggregate child => new LegacyDisplayDataMetadata(new DisplayData(child)),
                ChildrenAggregate children => new LegacyDisplayDataMetadata(new DisplayData(children)),
                _ => throw new InvalidOperationException(),
            };
        }

        internal new string RenderCSharpDeclaring(CodeRenderingContext ctx) {
            if (!ctx.IsLegacyCompatibilityMode()) {
                return base.RenderCSharpDeclaring(ctx);
                // TODO #43 ADDMODDDELを明示的に指定できるようにする
            }

            var implements = new List<string>();
            if (Aggregate is RootAggregate) implements.Add("DisplayDataClassBase");
            if (HasLifeCycle) implements.Add(ISaveCommandConvertible.INTERFACE_NAME);
            var inheritance = implements.Count == 0 ? string.Empty : $" : {implements.Join(", ")}";
            var childMembers = GetChildMembers().ToArray();
            var legacyValueMembers = GetLegacyValueMembers().ToArray();

            if (childMembers.Length == 0) {
                return $$"""
                    /// <summary>
                    /// {{Aggregate.DisplayName}}の画面表示用データ構造
                    /// </summary>
                    public partial class {{CsClassName}}{{inheritance}} {
                        /// <summary>値</summary>
                        [JsonPropertyName("{{VALUES_TS}}")]
                        public virtual {{ValueCsClassName}} {{VALUES_CS}} { get; set; } = new();
                    {{If(HasLifeCycle, () => $$"""

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
                    """)}}
                    {{If(HasVersion, () => $$"""
                        /// <summary>楽観排他制御用のバージョニング情報</summary>
                        [JsonPropertyName("{{VERSION_TS}}")]
                        public virtual int? {{VERSION_CS}} { get; set; }
                    """)}}
                    {{If(!HasLifeCycle && !HasVersion, () => "")}}
                        /// <summary>どの項目が読み取り専用か</summary>
                        [JsonPropertyName("{{READONLY_TS}}")]
                        public virtual {{ReadOnlyCsClassName}} {{READONLY_CS}} { get; set; } = new();
                    }
                    /// <summary>
                    /// {{Aggregate.DisplayName}}の画面表示用データの値の部分
                    /// </summary>
                    public partial class {{ValueCsClassName}} {
                    {{legacyValueMembers.SelectTextTemplate(member => $$"""
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
                        public virtual bool {{ALL_READONLY_CS}} { get; set; }
                    {{legacyValueMembers.SelectTextTemplate(member => $$"""
                        /// <summary>{{member.GetPropertyName(E_CsTs.CSharp)}}が読み取り専用か否か</summary>
                        public virtual bool {{member.GetPropertyName(E_CsTs.CSharp)}} { get; set; }
                    """)}}
                    }
                    """;
            }

            return $$"""
                /// <summary>
                /// {{Aggregate.DisplayName}}の画面表示用データ構造
                /// </summary>
                public partial class {{CsClassName}}{{inheritance}} {
                    /// <summary>値</summary>
                    [JsonPropertyName("{{VALUES_TS}}")]
                    public virtual {{ValueCsClassName}} {{VALUES_CS}} { get; set; } = new();
                {{childMembers.SelectTextTemplate(c => $$"""
                    {{WithIndent(RenderLegacyChildMember(c), "    ")}}
                """)}}
                {{If(HasLifeCycle, () => $$"""

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
                """)}}
                {{If(HasVersion, () => $$"""
                    /// <summary>楽観排他制御用のバージョニング情報</summary>
                    [JsonPropertyName("{{VERSION_TS}}")]
                    public virtual int? {{VERSION_CS}} { get; set; }
                """)}}
                {{If(!HasLifeCycle && !HasVersion, () => "")}}
                    /// <summary>どの項目が読み取り専用か</summary>
                    [JsonPropertyName("{{READONLY_TS}}")]
                    public virtual {{ReadOnlyCsClassName}} {{READONLY_CS}} { get; set; } = new();
                }
                /// <summary>
                /// {{Aggregate.DisplayName}}の画面表示用データの値の部分
                /// </summary>
                public partial class {{ValueCsClassName}} {
                {{legacyValueMembers.SelectTextTemplate(member => $$"""
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
                    public virtual bool {{ALL_READONLY_CS}} { get; set; }
                {{legacyValueMembers.SelectTextTemplate(member => $$"""
                    /// <summary>{{member.GetPropertyName(E_CsTs.CSharp)}}が読み取り専用か否か</summary>
                    public virtual bool {{member.GetPropertyName(E_CsTs.CSharp)}} { get; set; }
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
                        public virtual {{((IInstanceStructurePropertyMetadata)reference).GetTypeName(E_CsTs.CSharp)}}? {{reference.Member.PhysicalName}} { get; set; }
                        """,
                    _ => throw new InvalidOperationException(),
                };
            }

            static string RenderLegacyChildMember(EditablePresentationObject.EditablePresentationObjectDescendant descendant) {
                return $$"""
                    /// <summary>{{descendant.DisplayName}}</summary>
                    public virtual {{descendant.CsClassNameAsMember}} {{descendant.PhysicalName}} { get; set; } = new();
                    """;
            }

            string RenderLegacyMessageClasses() {
                var members = GetLegacyMessageMembers().ToArray();
                var messageBaseClass = Aggregate is ChildrenAggregate ? "DisplayMessageContainerInGrid" : "DisplayMessageContainerBase";
                var messagePathCtorArgs = Aggregate is ChildrenAggregate ? "IEnumerable<string> path, DisplayMessageContainerBase grid, int rowIndex" : "IEnumerable<string> path";
                var messagePathBaseCall = Aggregate is ChildrenAggregate ? "base(path, grid, rowIndex)" : "base(path)";

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
                    public partial class {{MessageCsClassName}} : {{messageBaseClass}} {
                        public {{MessageCsClassName}}({{messagePathCtorArgs}}) : {{messagePathBaseCall}} {
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
                        /// <summary>{{member.PropertyName}}についてのメッセージ</summary>
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

        internal new string RenderTypeScriptType(CodeRenderingContext ctx) {
            if (!ctx.IsLegacyCompatibilityMode()) {
                return base.RenderTypeScriptType(ctx);
            }

            var childMembers = GetChildMembers().ToArray();
            var legacyValueMembers = GetLegacyValueMembers().ToArray();

            return $$"""
                /** {{Aggregate.DisplayName}}の画面表示用データ構造 */
                export type {{TsTypeName}} = {
                  /** 値 */
                  {{VALUES_TS}}: {{WithIndent(RenderLegacyTsValueType(legacyValueMembers), "  ")}}
                {{childMembers.SelectTextTemplate(member => $$"""
                  /** {{member.DisplayName}} */
                  {{member.PhysicalName}}: {{member.TsTypeNameAsMember}}
                """)}}

                {{If(HasLifeCycle, () => $$"""
                  /** このデータがDBに保存済みかどうか */
                  {{EXISTS_IN_DB_TS}}: boolean
                  /** このデータに更新がかかっているかどうか */
                  {{WILL_BE_CHANGED_TS}}: boolean
                  /** このデータが更新確定時に削除されるかどうか */
                  {{WILL_BE_DELETED_TS}}: boolean
                  /**
                   * 画面操作で使用される一意なID。このインスタンスが作成されたときに発番される。
                   * 画面上で行データの更新や行移動などがなされたりしたときに当該インスタンスを適切に追跡出来るようにするために必要。
                   * このIDは永続化の対象とならない。
                   * インスタンスをnewする場合は明示的にUUIDを設定する ※ UUID.generate()
                   */
                  {{UNIQUE_ID_TS}}: string
                """)}}
                {{If(HasVersion, () => $$"""
                  /** 楽観排他制御用のバージョニング情報 */
                  {{VERSION_TS}}: string | null | undefined
                """)}}
                  /** どの項目が読み取り専用か */
                  {{READONLY_TS}}?: {{WithIndent(RenderLegacyTsReadOnlyType(legacyValueMembers), "  ")}}
                }

                """;
        }

        private IEnumerable<LegacyMessageMember> GetLegacyMessageMembers() {
            foreach (var member in GetLegacyValueMembers()) {
                yield return new LegacyMessageMember(member.GetPropertyName(E_CsTs.CSharp), "IDisplayMessageContainer", true, Aggregate is ChildrenAggregate);
            }

            foreach (var child in GetChildMembers()) {
                var typeName = child is EditablePresentationObjectChildrenDescendant
                    ? $"DisplayMessageContainerList<{child.CsClassName}Messages>"
                    : $"{child.CsClassName}Messages";
                yield return new LegacyMessageMember(child.PhysicalName, typeName, false, false);
            }
        }

        private IEnumerable<IEditablePresentationObjectValueOrRefMember> GetLegacyValueMembers() {
            foreach (var member in Aggregate.GetMembers()) {
                if (member is ValueMember valueMember) {
                    if (!valueMember.OnlySearchCondition || valueMember.Type.CsDomainTypeName == "bool") {
                        yield return new EditablePresentationObjectValueMember(valueMember);
                    }
                } else if (member is RefToMember refToMember) {
                    yield return new EditablePresentationObjectRefMember(refToMember);
                }
            }
        }

        internal string RenderLegacyTypeScriptType() {
            var childMembers = GetChildMembers().ToArray();
            var legacyValueMembers = GetLegacyValueMembers().ToArray();

            return $$"""

                /** {{Aggregate.DisplayName}}の画面表示用データ構造 */
                export type {{TsTypeName}} = {
                  /** 値 */
                  {{VALUES_TS}}: {{WithIndent(RenderLegacyTsValueType(legacyValueMembers), "  ")}}
                {{childMembers.SelectTextTemplate(member => $$"""
                  /** {{member.DisplayName}} */
                  {{member.PhysicalName}}: {{member.TsTypeNameAsMember}}
                """)}}

                {{If(HasLifeCycle, () => $$"""
                  /** このデータがDBに保存済みかどうか */
                  {{EXISTS_IN_DB_TS}}: boolean
                  /** このデータに更新がかかっているかどうか */
                  {{WILL_BE_CHANGED_TS}}: boolean
                  /** このデータが更新確定時に削除されるかどうか */
                  {{WILL_BE_DELETED_TS}}: boolean
                  /**
                   * 画面操作で使用される一意なID。このインスタンスが作成されたときに発番される。
                   * 画面上で行データの更新や行移動などがなされたりしたときに当該インスタンスを適切に追跡出来るようにするために必要。
                   * このIDは永続化の対象とならない。
                   * インスタンスをnewする場合は明示的にUUIDを設定する ※ UUID.generate()
                   */
                  {{UNIQUE_ID_TS}}: string
                """)}}
                {{If(HasVersion, () => $$"""
                  /** 楽観排他制御用のバージョニング情報 */
                  {{VERSION_TS}}: string | null | undefined
                """)}}
                  /** どの項目が読み取り専用か */
                  {{READONLY_TS}}?: {{WithIndent(RenderLegacyTsReadOnlyType(legacyValueMembers), "  ")}}
                }

                """;
        }

        private static string RenderLegacyTsValueType(IEditablePresentationObjectValueOrRefMember[] legacyValueMembers) {
            return $$"""
                {
                {{legacyValueMembers.SelectTextTemplate(member => $$"""
                  {{member.GetPropertyName(E_CsTs.TypeScript)}}?: {{GetLegacyTsType(member)}}
                """)}}
                }
                """;
        }

        private string RenderLegacyTsReadOnlyType(IEditablePresentationObjectValueOrRefMember[] legacyValueMembers) {
            return $$"""
                {
                  /** {{Aggregate.DisplayName}}全体が読み取り専用か否か */
                  {{ALL_READONLY_TS}}?: boolean
                {{legacyValueMembers.SelectTextTemplate(member => $$"""
                  /** {{member.DisplayName}}が読み取り専用か否か */
                  {{member.GetPropertyName(E_CsTs.TypeScript)}}?: boolean
                """)}}
                }
                """;
        }

        private static string GetLegacyTsType(IEditablePresentationObjectValueOrRefMember member) {
            return member switch {
                EditablePresentationObjectValueMember value when value.Member.IsKey && !value.Member.Type.TsTypeName.Contains("null") => $"{value.Member.Type.TsTypeName} | null",
                EditablePresentationObjectValueMember value => value.Member.Type.TsTypeName,
                EditablePresentationObjectRefMember reference => reference.RefEntry.TsTypeName,
                _ => throw new InvalidOperationException(),
            };
        }

        private readonly record struct LegacyMessageMember(string PropertyName, string TypeName, bool IsValueMember, bool IsInGrid) {
            internal string RenderPathConstructor() {
                var path = IsValueMember
                    ? $"[.. path, \"{VALUES_TS}\", \"{PropertyName}\"]"
                    : $"[.. path, \"{PropertyName}\"]";

                if (IsValueMember) {
                    return IsInGrid
                        ? $"{PropertyName} = new DisplayMessageContainerInGrid({path}, grid, rowIndex);"
                        : $"{PropertyName} = new DisplayMessageContainer({path});";
                }

                if (TypeName.StartsWith("DisplayMessageContainerList<", StringComparison.Ordinal)) {
                    var itemType = TypeName["DisplayMessageContainerList<".Length..^1];
                    return $"{PropertyName} = new([.. path, \"{PropertyName}\"], rowIndex => {{\n    return new {itemType}([.. path, \"{PropertyName}\", rowIndex.ToString()], {PropertyName}!, rowIndex);\n}});";
                }

                return $"{PropertyName} = new {TypeName}({path});";
            }

            internal string RenderOriginConstructor() {
                if (IsValueMember) {
                    return $"{PropertyName} = origin;";
                }

                if (TypeName.StartsWith("DisplayMessageContainerList<", StringComparison.Ordinal)) {
                    var itemType = TypeName["DisplayMessageContainerList<".Length..^1];
                    return $"{PropertyName} = new([], rowIndex => {{\n    return new {itemType}(origin);\n}});";
                }

                return $"{PropertyName} = new {TypeName}(origin);";
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
            if (ctx.IsLegacyCompatibilityMode()) {
                var legacyTree = rootAggregate
                    .EnumerateThisAndDescendants()
                    .Select(agg => new DisplayData(agg));

                return $$"""

                    {{legacyTree.SelectTextTemplate(disp => $$"""
                    {{disp.RenderTypeScriptType(ctx)}}
                    """)}}
                    """;
            }

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
            if (ctx.IsLegacyCompatibilityMode()) {
                var childMembers = GetChildMembers().ToArray();

                return $$"""
                    /** {{Aggregate.DisplayName}}の画面表示用オブジェクトを新規作成します。 */
                    export const {{TsNewObjectFunction}} = (): {{TsTypeName}} => ({
                      {{VALUES_TS}}: {
                      },
                    {{childMembers.SelectTextTemplate(c => $$"""
                      {{c.PhysicalName}}: {{c.RenderNewObjectCreation()}},
                    """)}}
                      {{EXISTS_IN_DB_TS}}: false,
                      {{WILL_BE_CHANGED_TS}}: true,
                      {{WILL_BE_DELETED_TS}}: false,
                      {{UNIQUE_ID_TS}}: UUID.generate(),
                    {{If(HasVersion, () => $$"""
                      {{VERSION_TS}}: undefined,
                    """)}}
                    })

                    """;
            }

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
            if (ctx.IsLegacyCompatibilityMode()) {
                return $$"""
                    /** 2つの{{Aggregate.DisplayName}}オブジェクトの値を比較し、一致しているかを返します。 */
                    export const {{DeepEqualFunction}} = (a: {{TsTypeName}}, b: {{TsTypeName}}): boolean => {
                      {{WithIndent(RenderLegacyDeepEqualBody(Aggregate, "a", "b"), "  ")}}
                      if (a.{{WILL_BE_DELETED_TS}} !== b.{{WILL_BE_DELETED_TS}}) return false
                      return true
                    }

                    """;
            }

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
            if (ctx.IsLegacyCompatibilityMode()) {
                return $$"""
                    /** 更新前後の値をディープイコールで判定し、変更があったオブジェクトのwillBeChangedプロパティをtrueに設定して返します。 */
                    export const {{CheckChangesFunction}} = ({ defaultValues, currentValues }: {
                      defaultValues: {{TsTypeName}}
                      currentValues: {{TsTypeName}}
                    }): boolean => {
                      let anyChanged = false

                      if ({{DeepEqualFunction}}(defaultValues, currentValues)) {
                        currentValues.{{WILL_BE_CHANGED_TS}} = false
                      } else {
                        currentValues.{{WILL_BE_CHANGED_TS}} = true
                        anyChanged = true
                      }

                      return anyChanged
                    }

                    """;
            }

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

            var renderedChildMembers = NormalizeEmptyTemplate(GetChildMembers().SelectTextTemplate(child => child switch {
                EditablePresentationObjectChildrenDescendant children => $$"""
                    foreach (var x in displayData.{{children.PhysicalName}}) {
                        {{WithIndent(RenderAggregate(children, "x"), "    ")}}
                    }
                    """,
                EditablePresentationObjectChildDescendant childDescendant => RenderAggregate(childDescendant, $"displayData.{childDescendant.PhysicalName}"),
                _ => throw new InvalidOperationException(),
            }));
            var needsTrailingBlankLine = GetChildMembers().Any() && renderedChildMembers == string.Empty;

            return $$"""
                /// <summary>
                /// {{Aggregate.DisplayName}}の主キー項目を読み取り専用にします。
                /// </summary>
                private void SetKeysReadOnly({{CsClassName}} displayData) {
                    {{WithIndent(RenderAggregate(this, "displayData"), "    ")}}
                {{If(needsTrailingBlankLine, () => "")}}
                }
                """;

            static string RenderAggregate(EditablePresentationObject displayData, string instance) {
                var ownMembers = NormalizeEmptyTemplate(RenderOwnMembers(displayData, instance));
                var childMembers = NormalizeEmptyTemplate(displayData.GetChildMembers().SelectTextTemplate(child => child switch {
                    EditablePresentationObjectChildrenDescendant children => $$"""
                        foreach (var x in {{instance}}.{{children.PhysicalName}}) {
                            {{WithIndent(RenderAggregate(children, "x"), "    ")}}
                        }
                        """,
                    EditablePresentationObjectChildDescendant childDescendant => RenderAggregate(childDescendant, $"{instance}.{childDescendant.PhysicalName}"),
                    _ => throw new InvalidOperationException(),
                }));

                return ownMembers != string.Empty && childMembers != string.Empty
                    ? ownMembers + Environment.NewLine + childMembers
                    : ownMembers + childMembers;
            }

            static string RenderOwnMembers(EditablePresentationObject displayData, string instance) {
                return displayData.Aggregate.GetMembers()
                    .Where(member => member switch {
                        ValueMember valueMember => valueMember.IsKey,
                        RefToMember refToMember => refToMember.IsKey,
                        _ => false,
                    })
                    .SelectTextTemplate(member => member switch {
                        ValueMember valueMember => $"{instance}.{READONLY_CS}.{valueMember.PhysicalName} = true;",
                        RefToMember refToMember => $"{instance}.{READONLY_CS}.{refToMember.PhysicalName} = true;",
                        _ => throw new InvalidOperationException(),
                    });
            }

            static string NormalizeEmptyTemplate(string value) {
                return value == SKIP_MARKER ? string.Empty : value;
            }
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
                    if (member is IInstancePropertyMetadata metadata) {
                        yield return metadata;
                    }
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

        private static string RenderLegacyDeepEqualBody(AggregateBase aggregate, string left, string right) {
            return $$"""
                {{aggregate.GetMembers().SelectTextTemplate(member => member switch {
                ValueMember valueMember when !valueMember.OnlySearchCondition => RenderLegacyValueMember(valueMember, left, right),
                RefToMember refToMember => RenderLegacyRefMember(refToMember, left, right),
                ChildAggregate childAggregate => RenderLegacyChildAggregate(childAggregate, left, right),
                ChildrenAggregate childrenAggregate => RenderLegacyChildrenAggregate(childrenAggregate, left, right),
                _ => string.Empty,
            })}}
                """;

            static string RenderLegacyValueMember(ValueMember valueMember, string left, string right) {
                var leftValue = $"{left}.{VALUES_TS}?.{valueMember.PhysicalName}";
                var rightValue = $"{right}.{VALUES_TS}?.{valueMember.PhysicalName}";

                return valueMember.Type switch {
                    ValueMemberTypes.DecimalMember or ValueMemberTypes.IntMember or ValueMemberTypes.SequenceMember => $$"""
                        if (!Util.strictDecimalEquals({{leftValue}},{{rightValue}})) return false
                        """,
                    ValueMemberTypes.DateMember or ValueMemberTypes.DateTimeMember or ValueMemberTypes.YearMember or ValueMemberTypes.YearMonthMember => $$"""
                        if (Util.toISOStringStrict({{leftValue}}) !== Util.toISOStringStrict({{rightValue}})) return false
                        """,
                    _ => $$"""
                        if (({{leftValue}} ?? undefined) !== ({{rightValue}} ?? undefined)) return false
                        """,
                };
            }

            static string RenderLegacyRefMember(RefToMember refToMember, string left, string right) {
                return $$"""
                    if (JSON.stringify({{left}}.{{VALUES_TS}}?.{{refToMember.PhysicalName}} ?? null) !== JSON.stringify({{right}}.{{VALUES_TS}}?.{{refToMember.PhysicalName}} ?? null)) return false
                    """;
            }

            static string RenderLegacyChildAggregate(ChildAggregate childAggregate, string left, string right) {
                return $$"""
                    if ((({{left}}.{{childAggregate.PhysicalName}} ?? undefined) === undefined) !== ((({{right}}.{{childAggregate.PhysicalName}} ?? undefined) === undefined))) return false
                    if ({{left}}.{{childAggregate.PhysicalName}} && {{right}}.{{childAggregate.PhysicalName}}) {
                      {{WithIndent(RenderLegacyDeepEqualBody(childAggregate, $"{left}.{childAggregate.PhysicalName}", $"{right}.{childAggregate.PhysicalName}"), "  ")}}
                    }
                    """;
            }

            static string RenderLegacyChildrenAggregate(ChildrenAggregate childrenAggregate, string left, string right) {
                var leftItems = $"{left}{'.'}{childrenAggregate.PhysicalName}";
                var rightItems = $"{right}{'.'}{childrenAggregate.PhysicalName}";

                return $$"""
                    const {{childrenAggregate.GetLoopVarName("leftItems")}} = {{leftItems}} ?? []
                    const {{childrenAggregate.GetLoopVarName("rightItems")}} = {{rightItems}} ?? []
                    if ({{childrenAggregate.GetLoopVarName("leftItems")}}.length !== {{childrenAggregate.GetLoopVarName("rightItems")}}.length) return false
                    for (let i = 0; i < {{childrenAggregate.GetLoopVarName("leftItems")}}.length; i++) {
                      const leftItem = {{childrenAggregate.GetLoopVarName("leftItems")}}[i]
                      const rightItem = {{childrenAggregate.GetLoopVarName("rightItems")}}[i]
                      {{WithIndent(RenderLegacyDeepEqualBody(childrenAggregate, "leftItem", "rightItem"), "  ")}}
                    }
                    """;
            }
        }

        private static string RenderUiConstraintMembers(EditablePresentationObject displayData) {
            static string IndentAll(string content, string indent) => indent + WithIndent(content, indent);

            var valueMembers = displayData.GetValueMembers()
                .Select(member => member switch {
                    EditablePresentationObjectValueMember valueMember => $"  {valueMember.PropertyName}: {GetUiConstraintTypeName(valueMember.Member)}",
                    EditablePresentationObjectRefMember refMember => $"  {refMember.PropertyName}: AutoGeneratedUtil.MemberConstraintBase",
                    _ => string.Empty,
                })
                .Where(text => text != string.Empty);

            var childMembers = displayData.GetChildMembers()
                .Select(child => string.Join(Environment.NewLine, new[] {
                            $"  {child.PhysicalName}: {{",
                            IndentAll(RenderUiConstraintMembers(child), "    "),
                            "  }",
                }));

            return string.Join(Environment.NewLine, new[] {
                        "values: {",
                        string.Join(Environment.NewLine, valueMembers),
                        "}",
                        string.Join(Environment.NewLine, childMembers),
                    }.Where(text => text != string.Empty));
        }

        private static string RenderUiConstraintValues(EditablePresentationObject displayData) {
            static string IndentAll(string content, string indent) => indent + WithIndent(content, indent);

            var valueMembers = displayData.GetValueMembers()
                .Select(member => member switch {
                    EditablePresentationObjectValueMember valueMember => IndentAll(RenderUiConstraintValue(valueMember), "  "),
                    EditablePresentationObjectRefMember refMember => IndentAll(RenderUiConstraintValue(refMember), "  "),
                    _ => string.Empty,
                })
                .Where(text => text != string.Empty);

            var childMembers = displayData.GetChildMembers()
                .Select(child => string.Join(Environment.NewLine, new[] {
                            $"  {child.PhysicalName}: {{",
                            IndentAll(RenderUiConstraintValues(child), "    "),
                            "  },",
                }));

            return string.Join(Environment.NewLine, new[] {
                        "values: {",
                        string.Join(Environment.NewLine, valueMembers),
                        "},",
                        string.Join(Environment.NewLine, childMembers),
                    }.Where(text => text != string.Empty));
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
