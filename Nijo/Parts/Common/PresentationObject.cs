using System;
using System.Collections.Generic;
using System.Linq;
using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.QueryModelModules;

namespace Nijo.Parts.Common;

/// <summary>
/// ユーザーが画面上で編集するオブジェクトをレンダリングするための基底クラス。
/// ユーザーが編集するという特徴から、以下の情報がレンダリングされる。
///
/// <list type="bullet">
/// <item>保存時に新規追加・更新・削除・変更なしのどの処理が実行されるかを表すフラグ</item>
/// <item>（TODO: あとで実装する）どの項目にどういったエラーメッセージ等が発生しているか</item>
/// <item>（TODO: あとで実装する）インスタンスごとのユニークなID</item>
/// </list>
/// </summary>
internal abstract class PresentationObject : IInstancePropertyOwnerMetadata, ICreatablePresentationLayerStructure {

    /// <summary>値が格納されるプロパティの名前（C#）</summary>
    internal const string VALUES_CS = "Values";
    /// <summary>値が格納されるプロパティの名前（TypeScript）</summary>
    internal const string VALUES_TS = "values";

    /// <summary>このデータがDBに保存済みかどうか（C#）。つまり新規作成のときはfalse, 閲覧・更新・削除のときはtrue</summary>
    internal const string EXISTS_IN_DB_CS = "ExistsInDatabase";
    /// <summary>このデータがDBに保存済みかどうか（TypeScript）。つまり新規作成のときはfalse, 閲覧・更新・削除のときはtrue</summary>
    internal const string EXISTS_IN_DB_TS = "existsInDatabase";

    /// <summary>画面上で何らかの変更が加えられてから、保存処理の実行でその変更が確定するまでの間、trueになる（C#）</summary>
    internal const string WILL_BE_CHANGED_CS = "WillBeChanged";
    /// <summary>画面上で何らかの変更が加えられてから、保存処理の実行でその変更が確定するまでの間、trueになる（TypeScript）</summary>
    internal const string WILL_BE_CHANGED_TS = "willBeChanged";

    /// <summary>画面上で削除が指示されてから、保存処理の実行でその削除が確定するまでの間、trueになる（C#）</summary>
    internal const string WILL_BE_DELETED_CS = "WillBeDeleted";
    /// <summary>画面上で削除が指示されてから、保存処理の実行でその削除が確定するまでの間、trueになる（TypeScript）</summary>
    internal const string WILL_BE_DELETED_TS = "willBeDeleted";

    /// <summary>楽観排他制御用のバージョニング情報をもつプロパティの名前（C#側）</summary>
    internal const string VERSION_CS = "Version";
    /// <summary>楽観排他制御用のバージョニング情報をもつプロパティの名前（TypeScript側）</summary>
    internal const string VERSION_TS = "version";

    internal const string TO_CREATE_COMMAND = "ToCreateCommand";
    internal const string ASSIGN_TO_UPDATE_COMMAND = "AssignToUpdateCommand";
    internal const string TO_DELETE_COMMAND = "ToDeleteCommand";

    protected PresentationObject(AggregateBase aggregate) {
        Aggregate = aggregate;
    }
    internal AggregateBase Aggregate { get; }

    /// <summary>C#クラス名</summary>
    internal abstract string CsClassName { get; }
    string IPresentationLayerStructure.CsClassName => CsClassName;
    /// <summary>C#クラス名（values）</summary>
    internal abstract string CsValuesClassName { get; }
    /// <summary>TypeScript型名</summary>
    internal abstract string TsTypeName { get; }
    string IPresentationLayerStructure.TsTypeName => TsTypeName;

    /// <summary>画面上で独自の追加削除のライフサイクルを持つかどうか</summary>
    internal abstract bool HasLifeCycle { get; }
    /// <summary>楽観排他制御用のバージョンを持つかどうか</summary>
    internal abstract bool HasVersion { get; }

    /// <summary>C#側に追加のメソッドをレンダリングする場合は指定</summary>
    protected virtual string RenderAdditionalMethodToCSharp() => string.Empty;

    /// <summary>
    /// 子孫要素でなく自身のメンバーはこのオブジェクトの中に列挙される
    /// </summary>
    internal ValuesContainer Values => _values ??= new ValuesContainer(Aggregate);
    private ValuesContainer? _values;

    /// <summary>
    /// 子要素を列挙する。
    /// </summary>
    internal IEnumerable<DisplayDataDescendant> GetChildMembers() {
        foreach (var member in Aggregate.GetMembers()) {
            if (member is ChildAggregate child) {
                yield return new DisplayDataChildDescendant(child);

            } else if (member is ChildrenAggregate children) {
                yield return new DisplayDataChildrenDescendant(children);

            }
        }
    }

    IEnumerable<IInstancePropertyMetadata> IPresentationLayerStructure.GetMembers() {
        return ((IInstancePropertyOwnerMetadata)this).GetMembers();
    }
    IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() {
        yield return Values;

        foreach (var childMember in GetChildMembers()) {
            yield return (IInstancePropertyMetadata)childMember;
        }
    }

    #region レンダリング
    protected string RenderCSharpDeclaring(CodeRenderingContext ctx) {

        return $$"""
            /// <summary>
            /// {{Aggregate.DisplayName}}の画面表示用データ。
            /// </summary>
            public partial class {{CsClassName}} {
                /// <summary>{{Aggregate.DisplayName}}自身が持つ値</summary>
                [JsonPropertyName("{{VALUES_TS}}")]
                public {{CsValuesClassName}} {{VALUES_CS}} { get; set; } = new();
            {{GetChildMembers().SelectTextTemplate(c => $$"""
                [JsonPropertyName("{{c.PhysicalName}}")]
                public {{WithIndent(c.CsClassNameAsMember, "    ")}} {{c.PhysicalName}} { get; set; } = new();
            """)}}
            {{If(HasLifeCycle, () => $$"""

                /// <summary>このデータがDBに保存済みかどうか</summary>
                [JsonPropertyName("{{EXISTS_IN_DB_TS}}")]
                public bool {{EXISTS_IN_DB_CS}} { get; set; }
                /// <summary>このデータに更新がかかっているかどうか</summary>
                [JsonPropertyName("{{WILL_BE_CHANGED_TS}}")]
                public bool {{WILL_BE_CHANGED_CS}} { get; set; }
                /// <summary>このデータが更新確定時に削除されるかどうか</summary>
                [JsonPropertyName("{{WILL_BE_DELETED_TS}}")]
                public bool {{WILL_BE_DELETED_CS}} { get; set; }
            """)}}
            {{If(HasVersion, () => $$"""
                /// <summary>楽観排他制御用のバージョニング情報</summary>
                [JsonPropertyName("{{VERSION_TS}}")]
                public int? {{VERSION_CS}} { get; set; }
            """)}}
                {{WithIndent(RenderAdditionalMethodToCSharp(), "    ")}}
            }

            /// <summary>
            /// <see cref="{{CsClassName}}/> の{{VALUES_CS}}の型
            /// </summary>
            public partial class {{CsValuesClassName}} {
            {{Values.GetMembers().SelectTextTemplate(m => $$"""
                {{WithIndent(m.RenderCsDeclaration(), "    ")}}
            """)}}
            }
            """;
    }

    protected string RenderTypeScriptType(CodeRenderingContext ctx) {
        return $$"""
            /** {{Aggregate.DisplayName}}の画面表示用データ。 */
            export type {{TsTypeName}} = {
              /** 値 */
              {{VALUES_TS}}: {
            {{Values.GetMembers().SelectTextTemplate(m => $$"""
                {{WithIndent(m.RenderTsDeclaration(), "    ")}}
            """)}}
              }
            {{GetChildMembers().SelectTextTemplate(member => $$"""
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
            """)}}
            {{If(HasVersion, () => $$"""
              /** 楽観排他制御用のバージョニング情報 */
              {{VERSION_TS}}: number | undefined
            """)}}
            }
            """;
    }
    #endregion レンダリング


    #region Values
    /// <summary>
    /// Valuesオブジェクトそのもの
    /// </summary>
    internal class ValuesContainer : IInstanceStructurePropertyMetadata {
        public ValuesContainer(AggregateBase aggregate) {
            _aggregate = aggregate;
        }
        private readonly AggregateBase _aggregate;

        ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => ISchemaPathNode.Empty;
        string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => csts == E_CsTs.CSharp ? VALUES_CS : VALUES_TS;
        bool IInstanceStructurePropertyMetadata.IsArray => false;
        string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp
            ? new DisplayData(_aggregate).CsValuesClassName
            : throw new InvalidOperationException("この分岐にくることは無いはず");

        IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => GetMembers();

        internal IEnumerable<IDisplayDataMemberInValues> GetMembers() {
            foreach (var member in _aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    yield return new DisplayDataValueMember(vm);

                } else if (member is RefToMember refTo) {
                    yield return new DisplayDataRefMember(refTo);

                }
            }
        }
    }
    /// <summary>
    /// Valuesオブジェクトの中のメンバー
    /// </summary>
    internal interface IDisplayDataMemberInValues : IUiConstraintValue, IInstancePropertyMetadata {
        UiConstraint.E_Type UiConstraintType { get; }

        string RenderCsDeclaration();
        string RenderTsDeclaration();

        string RenderNewObjectCreation();
    }
    /// <summary>
    /// Valuesオブジェクトの中のValueMember
    /// </summary>
    internal class DisplayDataValueMember : IDisplayDataMemberInValues, IInstanceValuePropertyMetadata {
        internal DisplayDataValueMember(ValueMember vm) {
            Member = vm;
        }
        internal ValueMember Member { get; }

        public string PropertyName => Member.PhysicalName;
        public string DisplayName => Member.DisplayName;
        public UiConstraint.E_Type UiConstraintType => Member.Type.UiConstraintType;

        IValueMemberType IInstanceValuePropertyMetadata.Type => Member.Type;
        ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => Member;
        string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => PropertyName;

        public bool IsRequired => Member.IsKey || Member.IsRequired;
        public string? CharacterType => Member.CharacterType;
        public int? MaxLength => Member.MaxLength;
        public int? TotalDigit => Member.TotalDigit;
        public int? DecimalPlace => Member.DecimalPlace;

        public string RenderCsDeclaration() {
            return $$"""
                /// <summary>{{Member.DisplayName}}</summary>
                public {{Member.Type.CsDomainTypeName}}? {{PropertyName}} { get; set; }
                """;
        }
        public string RenderTsDeclaration() {
            return $$"""
                {{PropertyName}}?: {{Member.Type.TsTypeName}}
                """;
        }

        public string RenderNewObjectCreation() {
            return "undefined";
        }
    }
    /// <summary>
    /// Valuesオブジェクトの中のRefTo
    /// </summary>
    internal class DisplayDataRefMember : IDisplayDataMemberInValues, IInstanceStructurePropertyMetadata {
        internal DisplayDataRefMember(RefToMember refTo) {
            Member = refTo;
            RefEntry = new DisplayDataRef.Entry(refTo.RefTo);
        }
        internal RefToMember Member { get; }
        internal DisplayDataRef.Entry RefEntry;

        public string PropertyName => Member.PhysicalName;
        public string DisplayName => Member.DisplayName;
        public UiConstraint.E_Type UiConstraintType => UiConstraint.E_Type.MemberConstraintBase;

        public bool IsRequired => Member.IsKey || Member.IsRequired;
        public string? CharacterType => null;
        public int? MaxLength => null;
        public int? TotalDigit => null;
        public int? DecimalPlace => null;

        public string RenderCsDeclaration() {
            return $$"""
                /// <summary>{{Member.DisplayName}}</summary>
                public {{RefEntry.CsClassName}} {{PropertyName}} { get; set; } = new();
                """;
        }
        public string RenderTsDeclaration() {
            return $$"""
                {{PropertyName}}: {{RefEntry.TsTypeName}}
                """;
        }

        public string RenderNewObjectCreation() {
            return $"{RefEntry.TsNewObjectFunction}()";
        }

        internal IEnumerable<DisplayDataRef.IRefDisplayDataMember> GetMembers() {
            return RefEntry.GetMembers();
        }

        ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => Member;
        bool IInstanceStructurePropertyMetadata.IsArray => false;
        string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => PropertyName;
        string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp
            ? RefEntry.CsClassName
            : RefEntry.TsTypeName;
        IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => GetMembers();
    }
    #endregion Values


    #region UI用の制約定義
    internal string UiConstraintTypeName => $"{Aggregate.PhysicalName}ConstraintType";
    internal string UiConstraingValueName => $"{Aggregate.PhysicalName}Constraints";
    internal string RenderUiConstraintType(CodeRenderingContext ctx) {
        if (Aggregate is not RootAggregate) throw new InvalidOperationException();

        return $$"""
            /** {{Aggregate.DisplayName}}の各メンバーの制約の型 */
            type {{UiConstraintTypeName}} = {
              {{WithIndent(RenderMembers(this), "  ")}}
            }
            """;

        static string RenderMembers(PresentationObject displayData) {
            return $$"""
                {{VALUES_TS}}: {
                {{displayData.Values.GetMembers().SelectTextTemplate(m => $$"""
                  {{m.GetPropertyName(E_CsTs.TypeScript)}}: Util.{{m.UiConstraintType}}
                """)}}
                }
                {{displayData.GetChildMembers().SelectTextTemplate(desc => $$"""
                {{desc.PhysicalName}}: {
                  {{WithIndent(RenderMembers(desc), "  ")}}
                }
                """)}}
                """;
        }
    }
    internal string RenderUiConstraintValue(CodeRenderingContext ctx) {
        if (Aggregate is not RootAggregate) throw new InvalidOperationException();

        return $$"""
            /** {{Aggregate.DisplayName}}の各メンバーの制約の具体的な値 */
            export const {{UiConstraingValueName}}: {{UiConstraintTypeName}} = {
              {{WithIndent(RenderMembers(this), "  ")}}
            }
            """;

        static string RenderMembers(PresentationObject displayData) {
            return $$"""
                {{VALUES_TS}}: {
                {{displayData.Values.GetMembers().SelectTextTemplate(m => $$"""
                  {{m.GetPropertyName(E_CsTs.TypeScript)}}: {
                {{If(m.IsRequired, () => $$"""
                    {{UiConstraint.MEMBER_REQUIRED}}: true,
                """)}}
                {{If(m.CharacterType != null, () => $$"""
                    {{UiConstraint.MEMBER_CHARACTER_TYPE}}: {{m.CharacterType}},
                """)}}
                {{If(m.MaxLength != null, () => $$"""
                    {{UiConstraint.MEMBER_MAX_LENGTH}}: {{m.MaxLength}},
                """)}}
                {{If(m.TotalDigit != null, () => $$"""
                    {{UiConstraint.MEMBER_TOTAL_DIGIT}}: {{m.TotalDigit}},
                """)}}
                {{If(m.DecimalPlace != null, () => $$"""
                    {{UiConstraint.MEMBER_DECIMAL_PLACE}}: {{m.DecimalPlace}},
                """)}}
                  },
                """)}}
                },
                {{displayData.GetChildMembers().SelectTextTemplate(desc => $$"""
                {{desc.PhysicalName}}: {
                  {{WithIndent(RenderMembers(desc), "  ")}}
                },
                """)}}
                """;
        }
    }
    #endregion UI用の制約定義


    #region TypeScript新規オブジェクト作成関数
    /// <summary>
    /// TypeScriptの新規オブジェクト作成関数の名前
    /// </summary>
    public string TsNewObjectFunction => $"createNew{TsTypeName}";

    internal static string RenderTsNewObjectFunctionRecursively(RootAggregate rootAggregate, CodeRenderingContext ctx) {
        var tree = rootAggregate
            .EnumerateThisAndDescendants()
            .Select(agg => agg switch {
                RootAggregate root => new DisplayData(root),
                ChildAggregate child => new DisplayDataChildDescendant(child),
                ChildrenAggregate children => new DisplayDataChildrenDescendant(children),
                _ => throw new InvalidOperationException(),
            });

        return $$"""
            //#region 画面表示用データ新規作成用関数
            {{tree.SelectTextTemplate(disp => $$"""
            {{disp.RenderTypeScriptObjectCreationFunction(ctx)}}
            """)}}
            //#endregion 画面表示用データ新規作成用関数
            """;
    }
    private string RenderTypeScriptObjectCreationFunction(CodeRenderingContext ctx) {
        return $$"""
            /** {{Aggregate.DisplayName}}の画面表示用データの新しいインスタンスを作成します。 */
            export const {{TsNewObjectFunction}} = (): {{TsTypeName}} => ({{RenderTsNewObjectFunctionBody()}})
            """;
    }
    public string RenderTsNewObjectFunctionBody() {
        return $$"""
            {
              {{VALUES_TS}}: {
            {{Values.GetMembers().SelectTextTemplate(m => $$"""
                {{m.GetPropertyName(E_CsTs.TypeScript)}}: {{m.RenderNewObjectCreation()}},
            """)}}
              },
            {{If(HasLifeCycle, () => $$"""
              {{EXISTS_IN_DB_TS}}: false,
              {{WILL_BE_CHANGED_TS}}: true,
              {{WILL_BE_DELETED_TS}}: false,
            """)}}
            {{If(HasVersion, () => $$"""
              {{VERSION_TS}}: undefined,
            """)}}
            {{GetChildMembers().SelectTextTemplate(c => $$"""
              {{c.PhysicalName}}: {{c.RenderNewObjectCreation()}},
            """)}}
            }
            """;
    }
    #endregion TypeScript新規オブジェクト作成関数


    #region Valuesの外に定義されるメンバー（Child, Children）
    internal abstract class DisplayDataDescendant : DisplayData {
        internal DisplayDataDescendant(AggregateBase aggregate) : base(aggregate) { }

        internal string PhysicalName => Aggregate.PhysicalName;
        internal string DisplayName => Aggregate.DisplayName;
        internal abstract string CsClassNameAsMember { get; }
        internal abstract string TsTypeNameAsMember { get; }

        internal abstract string RenderNewObjectCreation();
    }

    internal class DisplayDataChildDescendant : DisplayDataDescendant, IInstanceStructurePropertyMetadata {
        internal DisplayDataChildDescendant(ChildAggregate child) : base(child) {
            _child = child;
        }
        private readonly ChildAggregate _child;

        internal override string CsClassNameAsMember => CsClassName;
        internal override string TsTypeNameAsMember => TsTypeName;
        internal override bool HasLifeCycle => _child.HasLifeCycle;

        internal override string RenderNewObjectCreation() {
            return $"{TsNewObjectFunction}()";
        }

        ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => _child;
        bool IInstanceStructurePropertyMetadata.IsArray => false;
        string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => PhysicalName;
        string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? CsClassName : TsTypeName;
    }

    internal class DisplayDataChildrenDescendant : DisplayDataDescendant, IInstanceStructurePropertyMetadata {
        internal DisplayDataChildrenDescendant(ChildrenAggregate children) : base(children) {
            ChildrenAggregate = children;
        }

        internal ChildrenAggregate ChildrenAggregate { get; }

        internal override string CsClassNameAsMember => $"List<{CsClassName}>";
        internal override string TsTypeNameAsMember => $"{TsTypeName}[]";
        internal override bool HasLifeCycle => true;

        internal override string RenderNewObjectCreation() {
            return "[]";
        }

        ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => Aggregate;
        bool IInstanceStructurePropertyMetadata.IsArray => true;
        string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => PhysicalName;
        string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? CsClassName : TsTypeName;
    }
    #endregion Valuesの外に定義されるメンバー（Child, Children）
}
