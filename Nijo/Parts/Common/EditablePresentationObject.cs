using System;
using System.Collections.Generic;
using System.Linq;
using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models.QueryModelModules;
using Nijo.SchemaParsing;

namespace Nijo.Parts.Common;

/// <summary>
/// ユーザーが画面上で編集するオブジェクトをレンダリングするための基底クラス。
/// ユーザーが編集するという特徴から、保存時に新規追加・更新・削除・変更なしのどの処理が実行されるかを表すフラグなどが含まれる。
/// </summary>
internal abstract class EditablePresentationObject : IInstancePropertyOwnerMetadata, ICreatablePresentationLayerStructure {

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

    /// <summary>UI側で編集中のインスタンスを識別するためのID（C#）</summary>
    internal const string INSTANCE_ID_CS = "InstanceId";
    /// <summary>UI側で編集中のインスタンスを識別するためのID（TypeScript）</summary>
    internal const string INSTANCE_ID_TS = "instanceId";

    /// <summary>楽観排他制御用のバージョニング情報をもつプロパティの名前（C#側）</summary>
    internal const string VERSION_CS = "Version";
    /// <summary>楽観排他制御用のバージョニング情報をもつプロパティの名前（TypeScript側）</summary>
    internal const string VERSION_TS = "version";

    internal const string TO_CREATE_COMMAND = "ToCreateCommand";
    internal const string ASSIGN_TO_UPDATE_COMMAND = "AssignToUpdateCommand";
    internal const string TO_DELETE_COMMAND = "ToDeleteCommand";

    protected EditablePresentationObject(AggregateBase aggregate) {
        Aggregate = aggregate;
    }
    internal AggregateBase Aggregate { get; }

    /// <summary>C#クラス名</summary>
    internal abstract string CsClassName { get; }
    string IPresentationLayerStructure.CsClassName => CsClassName;
    /// <summary>C#クラス名（values）</summary>
    internal string CsValuesClassName => $"{CsClassName}Values";
    /// <summary>TypeScript型名</summary>
    internal abstract string TsTypeName { get; }
    string IPresentationLayerStructure.TsTypeName => TsTypeName;

    /// <summary>楽観排他制御用のバージョンを持つかどうか</summary>
    internal abstract bool HasVersion { get; }

    /// <summary>C#側に追加のメソッドをレンダリングする場合は指定</summary>
    protected virtual string RenderAdditionalMethodToCSharp() => string.Empty;

    /// <summary>
    /// 子孫要素でなく自身のメンバーはこのオブジェクトの中に列挙される
    /// </summary>
    internal ValuesContainer Values => _values ??= new ValuesContainer(this);
    private ValuesContainer? _values;

    /// <summary>
    /// 子要素を列挙する。
    /// </summary>
    internal IEnumerable<EditablePresentationObjectDescendant> GetChildMembers() {
        foreach (var member in Aggregate.GetMembers()) {
            if (member is ChildAggregate child) {
                yield return new EditablePresentationObjectChildDescendant(child);

            } else if (member is ChildrenAggregate children) {
                yield return new EditablePresentationObjectChildrenDescendant(children);

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
    internal string RenderCSharpDeclaring(CodeRenderingContext ctx) {

        return $$"""
            /// <summary>
            /// {{Aggregate.DisplayName}}の画面表示用データ。
            /// </summary>
            public partial class {{CsClassName}} {
                /// <summary>
                /// 画面初期表示から保存までの間でこのインスタンスを識別する番号。
                /// GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
                /// エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
                /// </summary>
                [JsonPropertyName("{{INSTANCE_ID_TS}}")]
                public string {{INSTANCE_ID_CS}} { get; set; } = Guid.NewGuid().ToString();

                /// <summary>{{Aggregate.DisplayName}}自身が持つ値</summary>
                [JsonPropertyName("{{VALUES_TS}}")]
                public {{CsValuesClassName}} {{VALUES_CS}} { get; set; } = new();
            {{GetChildMembers().SelectTextTemplate(c => $$"""
                [JsonPropertyName("{{c.PhysicalName}}")]
                public {{WithIndent(c.CsClassNameAsMember, "    ")}} {{c.PhysicalName}} { get; set; } = new();
            """)}}

                /// <summary>このデータがDBに保存済みかどうか</summary>
                [JsonPropertyName("{{EXISTS_IN_DB_TS}}")]
                public bool {{EXISTS_IN_DB_CS}} { get; set; }
                /// <summary>このデータに更新がかかっているかどうか</summary>
                [JsonPropertyName("{{WILL_BE_CHANGED_TS}}")]
                public bool {{WILL_BE_CHANGED_CS}} { get; set; }
                /// <summary>このデータが更新確定時に削除されるかどうか</summary>
                [JsonPropertyName("{{WILL_BE_DELETED_TS}}")]
                public bool {{WILL_BE_DELETED_CS}} { get; set; }
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

    internal string RenderTypeScriptType(CodeRenderingContext ctx) {
        return $$"""
            /** {{Aggregate.DisplayName}}の画面表示用データ。 */
            export type {{TsTypeName}} = {
              /**
               * 画面初期表示から保存までの間でこのインスタンスを識別する番号。
               * GUIでのでの利用を想定している。例えば通常の主キーはデータベース保存前は未定義であり
               * エラーメッセージとの紐づけなどに使用できないため、そういった用途に使用する。
               */
              {{INSTANCE_ID_TS}}: string
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

              /** このデータがDBに保存済みかどうか */
              {{EXISTS_IN_DB_TS}}: boolean
              /** このデータに更新がかかっているかどうか */
              {{WILL_BE_CHANGED_TS}}: boolean
              /** このデータが更新確定時に削除されるかどうか */
              {{WILL_BE_DELETED_TS}}: boolean
            {{If(HasVersion, () => $$"""
              /** 楽観排他制御用のバージョニング情報 */
              {{VERSION_TS}}: number | null | undefined
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
        public ValuesContainer(EditablePresentationObject owner) {
            _owner = owner;
        }
        private readonly EditablePresentationObject _owner;

        ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => ISchemaPathNode.Empty;
        string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => csts == E_CsTs.CSharp ? VALUES_CS : VALUES_TS;
        bool IInstanceStructurePropertyMetadata.IsArray => false;
        string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp
            ? _owner.CsValuesClassName
            : throw new InvalidOperationException("この分岐にくることは無いはず");

        IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => GetMembers();

        internal IEnumerable<IEditablePresentationObjectMemberInValues> GetMembers() {
            foreach (var member in _owner.Aggregate.GetMembers()) {
                if (member is ValueMember vm && !vm.OnlySearchCondition) {
                    yield return new EditablePresentationObjectValueMember(vm);

                } else if (member is RefToMember refTo) {
                    yield return new EditablePresentationObjectRefMember(refTo);

                }
            }
        }
    }
    /// <summary>
    /// Valuesオブジェクトの中のメンバー
    /// </summary>
    internal interface IEditablePresentationObjectMemberInValues : IInstancePropertyMetadata {
        string RenderCsDeclaration();
        string RenderTsDeclaration();

        string RenderNewObjectCreation();
    }
    /// <summary>
    /// Valuesオブジェクトの中のValueMember
    /// </summary>
    internal class EditablePresentationObjectValueMember : IEditablePresentationObjectMemberInValues, IInstanceValuePropertyMetadata {
        internal EditablePresentationObjectValueMember(ValueMember vm) {
            Member = vm;
        }
        internal ValueMember Member { get; }

        public string PropertyName => Member.PhysicalName;
        public string DisplayName => Member.DisplayName;

        IValueMemberType IInstanceValuePropertyMetadata.Type => Member.Type;
        ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => Member;
        string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => PropertyName;

        public string RenderCsDeclaration() {
            var comment = Member.XElement.GetCommentMultiLine().ToArray();
            return $$"""
                {{If(comment.Length > 0, () => $$"""
                /// <summary>
                {{Member.XElement.GetCommentMultiLine().SelectTextTemplate(line => $$"""
                /// {{line}}
                """)}}
                /// </summary>
                """)}}
                public {{Member.Type.CsDomainTypeName}}? {{PropertyName}} { get; set; }
                """;
        }
        public string RenderTsDeclaration() {
            var comment = Member.XElement.GetCommentMultiLine().ToArray();
            return $$"""
                {{If(comment.Length > 0, () => $$"""
                /**
                {{Member.XElement.GetCommentMultiLine().SelectTextTemplate(line => $$"""
                 * {{line}}
                """)}}
                 */
                """)}}
                {{PropertyName}}?: {{Member.Type.TsTypeName}} | null
                """;
        }

        public string RenderNewObjectCreation() {
            return Member.Type.TsTypeName switch {
                "string" => "''",
                "boolean" => "false",
                _ => "null",
            };
        }
    }
    /// <summary>
    /// Valuesオブジェクトの中のRefTo
    /// </summary>
    internal class EditablePresentationObjectRefMember : IEditablePresentationObjectMemberInValues, IInstanceStructurePropertyMetadata {
        internal EditablePresentationObjectRefMember(RefToMember refTo) {
            Member = refTo;

            var ownerModel = refTo.Owner.GetRoot().Model;
            var refToModel = refTo.RefTo.GetRoot().Model;

            if (ownerModel is Models.QueryModel || refToModel is Models.DataModel) {
                // Query => Query
                RefEntry = new DisplayDataRef.Entry(refTo.RefTo);

            } else if (refToModel is Models.StructureModel) {
                // Structure => Structure
                if (refTo.RefTo is not RootAggregate refToRoot) throw new InvalidOperationException("ありえない");
                RefEntry = new Models.StructureModelModules.PlainStructure(refToRoot);

            } else {
                // Structure => Query
                RefEntry = refTo.RefToObject switch {
                    RefToMember.E_RefToObject.SearchCondition => new SearchCondition.Entry((RootAggregate)refTo.RefTo),
                    RefToMember.E_RefToObject.DisplayData => new DisplayData(refTo.RefTo),
                    RefToMember.E_RefToObject.RefTarget => new DisplayDataRef.Entry(refTo.RefTo),
                    _ => throw new NotImplementedException("ありえない"),
                };
            }
        }
        internal RefToMember Member { get; }
        internal ICreatablePresentationLayerStructure RefEntry { get; }

        public string PropertyName => Member.PhysicalName;
        public string DisplayName => Member.DisplayName;

        public string RenderCsDeclaration() {
            var comment = Member.XElement.GetCommentMultiLine().ToArray();
            return $$"""
                {{If(comment.Length > 0, () => $$"""
                /// <summary>
                {{Member.XElement.GetCommentMultiLine().SelectTextTemplate(line => $$"""
                /// {{line}}
                """)}}
                /// </summary>
                """)}}
                public {{RefEntry.CsClassName}} {{PropertyName}} { get; set; } = new();
                """;
        }
        public string RenderTsDeclaration() {
            var comment = Member.XElement.GetCommentMultiLine().ToArray();
            return $$"""
                {{If(comment.Length > 0, () => $$"""
                /**
                {{Member.XElement.GetCommentMultiLine().SelectTextTemplate(line => $$"""
                 * {{line}}
                """)}}
                 */
                """)}}
                {{PropertyName}}: {{RefEntry.TsTypeName}}
                """;
        }

        public string RenderNewObjectCreation() {
            return $"{RefEntry.TsNewObjectFunction}()";
        }

        ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => Member;
        bool IInstanceStructurePropertyMetadata.IsArray => false;
        string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => PropertyName;
        string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp
            ? RefEntry.CsClassName
            : RefEntry.TsTypeName;
        IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => RefEntry.GetMembers();
    }
    #endregion Values


    #region TypeScript新規オブジェクト作成関数
    /// <summary>
    /// TypeScriptの新規オブジェクト作成関数の名前
    /// </summary>
    public string TsNewObjectFunction => $"createNew{TsTypeName}";

    internal static string RenderTsNewObjectFunctionRecursively(EditablePresentationObject root, CodeRenderingContext ctx) {
        var tree = new List<EditablePresentationObject>();
        tree.Add(root);
        tree.AddRange(root.Aggregate.EnumerateDescendants().Select<AggregateBase, EditablePresentationObject>(agg => agg switch {
            ChildAggregate child => new EditablePresentationObjectChildDescendant(child),
            ChildrenAggregate children => new EditablePresentationObjectChildrenDescendant(children),
            _ => throw new InvalidOperationException(),
        }));

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
    public string RenderTsNewObjectFunctionBody() {
        return $$"""
            {
              {{INSTANCE_ID_TS}}: generateRandomUniqueId(),
              {{VALUES_TS}}: {
            {{Values.GetMembers().SelectTextTemplate(m => $$"""
                {{m.GetPropertyName(E_CsTs.TypeScript)}}: {{m.RenderNewObjectCreation()}},
            """)}}
              },
              {{EXISTS_IN_DB_TS}}: false,
              {{WILL_BE_CHANGED_TS}}: true,
              {{WILL_BE_DELETED_TS}}: false,
            {{If(HasVersion, () => $$"""
              {{VERSION_TS}}: null,
            """)}}
            {{GetChildMembers().SelectTextTemplate(c => $$"""
              {{c.PhysicalName}}: {{c.RenderNewObjectCreation()}},
            """)}}
            }
            """;
    }
    #endregion TypeScript新規オブジェクト作成関数


    #region Valuesの外に定義されるメンバー（Child, Children）
    internal abstract class EditablePresentationObjectDescendant : EditablePresentationObject {
        internal EditablePresentationObjectDescendant(AggregateBase aggregate) : base(aggregate) { }

        internal string PhysicalName => Aggregate.PhysicalName;
        internal string DisplayName => Aggregate.DisplayName;
        internal override string CsClassName => $"{Aggregate.PhysicalName}DisplayData";
        internal override string TsTypeName => $"{Aggregate.PhysicalName}DisplayData";
        internal abstract string CsClassNameAsMember { get; }
        internal abstract string TsTypeNameAsMember { get; }
        internal override bool HasVersion => Aggregate is RootAggregate;

        internal abstract string RenderNewObjectCreation();
    }

    internal class EditablePresentationObjectChildDescendant : EditablePresentationObjectDescendant, IInstanceStructurePropertyMetadata {
        internal EditablePresentationObjectChildDescendant(ChildAggregate child) : base(child) {
            _child = child;
        }
        private readonly ChildAggregate _child;

        internal override string CsClassNameAsMember => CsClassName;
        internal override string TsTypeNameAsMember => TsTypeName;

        internal override string RenderNewObjectCreation() {
            return $"{TsNewObjectFunction}()";
        }

        ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => _child;
        bool IInstanceStructurePropertyMetadata.IsArray => false;
        string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => PhysicalName;
        string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => csts == E_CsTs.CSharp ? CsClassName : TsTypeName;
    }

    internal class EditablePresentationObjectChildrenDescendant : EditablePresentationObjectDescendant, IInstanceStructurePropertyMetadata {
        internal EditablePresentationObjectChildrenDescendant(ChildrenAggregate children) : base(children) {
            ChildrenAggregate = children;
        }

        internal ChildrenAggregate ChildrenAggregate { get; }

        internal override string CsClassNameAsMember => $"List<{CsClassName}>";
        internal override string TsTypeNameAsMember => $"{TsTypeName}[]";

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
