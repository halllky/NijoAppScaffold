using Nijo.CodeGenerating;
using Nijo.Parts.CSharp;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures.ValueMemberTypes;
using Nijo.DataStructures;

namespace Nijo.DataStructures.IsStored {
    /// <summary>
    /// 永続化されるデータ構造の新規登録・更新・削除の内容を表すコマンド。
    /// ルート集約のコマンドはバリデーションと変更追跡の開始の機能を持ち、DbContextからのみインスタンス化できる。
    /// 子孫集約のコマンドは単なる入れ物であり、<c>new()</c> で作成できる。
    /// </summary>
    internal class SaveCommand : IInstancePropertyOwnerMetadata {

        internal SaveCommand(AggregateBase aggregate, E_Type type) {
            Aggregate = aggregate;
            Type = type;
        }
        internal AggregateBase Aggregate { get; }

        internal enum E_Type { Create, Update }
        internal E_Type Type { get; }

        internal string CsClassName => Type switch {
            E_Type.Create => $"{Aggregate.PhysicalName}CreateCommand",
            E_Type.Update => $"{Aggregate.PhysicalName}UpdateCommand",
            _ => throw new InvalidOperationException(),
        };
        /// <summary>新規登録・更新・削除のコマンドの基底クラス</summary>
        internal string CsBaseClassName => $"{Aggregate.PhysicalName}SaveCommand";
        /// <summary>削除コマンド</summary>
        internal string CsDeleteClassName => $"{Aggregate.PhysicalName}DeleteCommand";

        internal const string TO_DBENTITY = "ToDbEntity";
        internal const string FROM_DBENTITY = "FromDbEntity";
        internal const string START_TRACKING_CORE = "StartTrackingCore";
        internal const string GET_CURRENT_ENTITY = "GetCurrentEntity";
        internal const string GET_VALIDATION_ERRORS = "GetValidationErrors";
        internal const string TO_DELETE_COMMAND = "ToDeleteCommand";

        internal const string FIELD_DBCONTEXT = "_dbContext";
        internal const string FIELD_LOADED = "_loaded";
        internal const string FIELD_EXPECTED_VERSION = "_expectedVersion";
        internal const string FIELD_SAVED_ENTITY = "_savedEntity";

        internal const string INTERFACE = "ISaveCommand";
        internal const string INTERFACE_DELETE = "IDeleteCommand";

        internal const string ERR_ID_UNKNOWN = "UnknownUpdateError";
        internal const string ERR_DATA_NOT_FOUND = "DataNotFoundError";
        internal const string ERR_CONCURRENCY = "ConcurrencyError";
        internal const string ERR_UNIQUE_CONSTRAINT = "UniqueConstraintError";

        /// <summary>
        /// 新規・更新・削除のすべてのコマンドを、子孫要素の分も含めて再帰的にレンダリングします。
        /// </summary>
        internal static string RenderAll(RootAggregate rootAggregate, CodeRenderingContext ctx) {

            var descendants = rootAggregate.EnumerateDescendants().ToArray();
            var createCommand = new SaveCommand(rootAggregate, E_Type.Create);
            var updateCommand = new SaveCommand(rootAggregate, E_Type.Update);

            return $$"""
                {{createCommand.RenderBaseClass(ctx)}}

                #region 新規登録コマンド
                {{createCommand.RenderRootCreateCommand(ctx)}}
                {{descendants.SelectTextTemplate(agg => $$"""
                {{new SaveCommand(agg, E_Type.Create).RenderDescendantCommand(ctx)}}
                """)}}
                #endregion 新規登録コマンド


                #region 更新コマンド
                {{updateCommand.RenderRootUpdateCommand(ctx)}}
                {{descendants.SelectTextTemplate(agg => $$"""
                {{new SaveCommand(agg, E_Type.Update).RenderDescendantCommand(ctx)}}
                """)}}
                #endregion 更新コマンド


                #region 削除コマンド
                {{createCommand.RenderRootDeleteCommand(ctx)}}
                #endregion 削除コマンド
                """;
        }

        /// <summary>
        /// メンバーを列挙する。ValueMember, Ref, Child, Children すべて含む
        /// </summary>
        internal IEnumerable<ISaveCommandMember> GetMembers() {
            foreach (var member in Aggregate.GetMembers()) {
                if (member is ValueMember vm) {
                    // 新規登録時に自動採番されるものは新規登録メンバー中に含めない
                    if (Type == E_Type.Create && vm.Type is SequenceMember) continue;

                    yield return new SaveCommandValueMember(vm);

                } else if (member is RefToMember rm) {
                    yield return new SaveCommandRefMember(rm);

                } else if (member is ChildAggregate child) {
                    yield return new SaveCommandChildMember(child, Type);

                } else if (member is ChildrenAggregate children) {
                    yield return new SaveCommandChildrenMember(children, Type);
                }
            }
        }
        IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() => GetMembers();


        #region すべての集約で共通
        /// <summary>
        /// 保存処理とコマンドの間で使われるメッセージを登録します。
        /// スキーマ定義にデータ構造が1個も無くても保存処理のコンパイルが通るように、常に登録されます。
        /// </summary>
        internal static void RegisterMessages(CodeRenderingContext ctx) {
            ctx.Use<Parts.Common.MsgFactory>()
                .AddMessage(ERR_ID_UNKNOWN,
                    "登録/更新/削除のタイミングでRDBMS上で何らかのエラーが生じた場合のメッセージ",
                    "登録処理でエラーが発生しました: {0}")
                .AddMessage(ERR_DATA_NOT_FOUND,
                    "更新対象・削除対象のデータがデータベース上で見つからなかったときのメッセージ",
                    "更新対象のデータが見つかりません。")
                .AddMessage(ERR_CONCURRENCY,
                    "楽観排他制御に引っかかったときのメッセージ",
                    "ほかのユーザーが更新しました。")
                .AddMessage(ERR_UNIQUE_CONSTRAINT,
                    "一意制約に違反したときのメッセージ",
                    "同じ値のデータが既に存在します。");
        }

        /// <summary>
        /// コマンドと、アプリケーション側で実装される保存処理との間の取り決めをレンダリングします。
        /// 保存処理の流れはシステムごとに要件が大きく異なるため自動生成せず、スキーマに依存する部品だけをここで定義します。
        /// スキーマ定義にデータ構造が1個も無くても保存処理のコンパイルが通るように、常に生成されます。
        /// </summary>
        internal static SourceFile RenderInterfaces(CodeRenderingContext ctx) {
            return new SourceFile {
                FileName = $"{INTERFACE}.cs",
                Contents = $$"""
                    using System.Text.Json.Nodes;

                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// 保存処理が必要とする、コマンドの機能。
                    /// 通常の業務ロジックからは使わない。
                    /// </summary>
                    public interface {{INTERFACE}} {
                        /// <summary>このコマンドの現在の状態に対するバリデーションエラーを返します。コマンドの状態は変更しません。</summary>
                        {{Parts.CSharp.MessageContainer.SETTER_CLASS}} GetValidationErrors();
                        /// <summary>
                        /// 監査項目と楽観排他のバージョンを設定し、エンティティの変更追跡を開始します。
                        /// 1つのコマンドに対して1回しか呼べません。2回目以降は例外を送出します。
                        /// </summary>
                        /// <returns>変更追跡を開始したエンティティ。保存後にこれらの変更追跡を終了するのは呼び出し側の責務です。</returns>
                        IReadOnlyList<object> StartTracking();
                        /// <summary>同一の保存処理の中でのキー重複の検出に使うキーを返します。保存時に採番されるのでキーが未確定の場合は null を返します。</summary>
                        object? GetKeyForDuplicateCheck();
                        /// <summary>ログに出力する情報（集約名、操作の種類、キー）を返します。</summary>
                        JsonNode ToLogJson();
                    }

                    /// <inheritdoc cref="{{INTERFACE}}"/>
                    /// <typeparam name="TUpdateCommand">保存後のデータを表す更新コマンドの型</typeparam>
                    public interface {{INTERFACE}}<TUpdateCommand> : {{INTERFACE}} {
                        /// <summary>保存後のデータを表す、このインスタンスとは別の更新コマンドを返します。削除の場合は null を返します。</summary>
                        TUpdateCommand? ToSavedCommand();
                    }

                    /// <summary>
                    /// 削除コマンド。
                    /// 保存処理の戻り値に保存後のデータが含まれないことを型で表すためにオーバーロードを分けるのに使う。
                    /// </summary>
                    /// <typeparam name="TUpdateCommand">削除コマンドと同じ集約の更新コマンドの型</typeparam>
                    public interface {{INTERFACE_DELETE}}<TUpdateCommand> : {{INTERFACE}}<TUpdateCommand> {
                    }
                    """,
            };
        }
        #endregion すべての集約で共通


        #region 基底クラス
        private string RenderBaseClass(CodeRenderingContext ctx) {
            var efCoreEntity = new EFCoreEntity(Aggregate);
            var messages = new SaveCommandMessageContainer(Aggregate);
            var key = new KeyClass.KeyClassEntry(Aggregate);
            var updateCommand = new SaveCommand(Aggregate, E_Type.Update);

            var entityProps = new Variable("entity", efCoreEntity)
                .CreateProperties()
                .ToArray();
            var keysForLog = Aggregate
                .GetKeyVMs()
                .Select(vm => new {
                    DisplayName = vm.DisplayName.Replace("\"", "\\\""),
                    EntityPath = entityProps
                        .Single(x => x.Metadata.SchemaPathNode.ToMappingKey() == vm.ToMappingKey())
                        .GetJoinedPathFromInstance(E_CsTs.CSharp, "?."),
                })
                .ToArray();

            return $$"""
                /// <summary>
                /// {{Aggregate.DisplayName}} の新規登録・更新・削除コマンドの基底クラス。
                /// 一度保存処理にかけたインスタンスは再度使用できません。
                /// </summary>
                public abstract class {{CsBaseClassName}} : {{INTERFACE}}<{{updateCommand.CsClassName}}> {
                    private protected {{CsBaseClassName}}({{ctx.Config.DbContextName}} dbContext) {
                        {{FIELD_DBCONTEXT}} = dbContext;
                    }
                    private protected readonly {{ctx.Config.DbContextName}} {{FIELD_DBCONTEXT}};
                    /// <summary>変更追跡の対象となった、保存後の内容をもつエンティティ。削除の場合は設定されない。</summary>
                    private protected {{efCoreEntity.CsClassName}}? {{FIELD_SAVED_ENTITY}};
                    private bool _trackingStarted;

                    /// <summary>ユーザーに見せるメッセージの文言</summary>
                    private protected {{Parts.Common.MsgFactory.CS_INTERFACE}} {{Parts.Common.MsgFactory.MSG}} => {{FIELD_DBCONTEXT}}.{{Parts.Common.MsgFactory.MSG}};

                    /// <summary>
                    /// このコマンドの現在の状態に対するバリデーションエラーを返します。
                    /// 必須チェックなどスキーマ定義から自動生成されるチェックと楽観排他のバージョンの食い違いのチェックが行なわれます。
                    /// このメソッドはコマンドの状態を変更しません。
                    /// </summary>
                    public abstract {{messages.CsClassName}} {{GET_VALIDATION_ERRORS}}();

                    /// <summary>変更追跡を開始する。変更追跡を開始するエンティティは状態を変更する前に引数のリストに追加すること。</summary>
                    private protected abstract void {{START_TRACKING_CORE}}(List<object> tracked);
                    /// <summary>このコマンドの現在の内容を表すエンティティを返す。</summary>
                    private protected abstract {{efCoreEntity.CsClassName}} {{GET_CURRENT_ENTITY}}();
                    /// <summary>ログに出力する操作の種類</summary>
                    private protected abstract string OperationNameForLog { get; }

                    {{Parts.CSharp.MessageContainer.SETTER_CLASS}} {{INTERFACE}}.GetValidationErrors() => {{GET_VALIDATION_ERRORS}}();

                    IReadOnlyList<object> {{INTERFACE}}.StartTracking() {
                        if (_trackingStarted) throw new InvalidOperationException("一度保存処理にかけたコマンドは再度使用できません。");
                        _trackingStarted = true;

                        var tracked = new List<object>();
                        try {
                            {{START_TRACKING_CORE}}(tracked);
                        } catch {
                            foreach (var entity in tracked) {
                                {{FIELD_DBCONTEXT}}.Entry(entity).State = EntityState.Detached;
                            }
                            throw;
                        }
                        return tracked;
                    }

                    object? {{INTERFACE}}.GetKeyForDuplicateCheck() => GetKeyForDuplicateCheckCore();
                    /// <summary>同一の保存処理の中でのキー重複の検出に使うキーを返す。</summary>
                    private protected virtual object? GetKeyForDuplicateCheckCore() {
                        return {{key.ClassName}}.{{SaveCommand.FROM_DBENTITY}}({{GET_CURRENT_ENTITY}}());
                    }

                    JsonNode {{INTERFACE}}.ToLogJson() {
                        var entity = {{GET_CURRENT_ENTITY}}();
                        return new JsonObject {
                            ["集約"] = "{{Aggregate.DisplayName.Replace("\"", "\\\"")}}",
                            ["操作"] = OperationNameForLog,
                            ["キー"] = new JsonObject {
                {{keysForLog.SelectTextTemplate(k => $$"""
                                ["{{k.DisplayName}}"] = JsonValue.Create({{k.EntityPath}}),
                """)}}
                            },
                        };
                    }

                    {{updateCommand.CsClassName}}? {{INTERFACE}}<{{updateCommand.CsClassName}}>.ToSavedCommand() {
                        if ({{FIELD_SAVED_ENTITY}} == null) return null;
                        return {{updateCommand.CsClassName}}.{{FROM_DBENTITY}}({{FIELD_DBCONTEXT}}, {{FIELD_SAVED_ENTITY}}, {{FIELD_SAVED_ENTITY}}.{{EFCoreEntity.VERSION}}!.Value);
                    }
                }
                """;
        }
        #endregion 基底クラス


        #region CREATE
        private string RenderRootCreateCommand(CodeRenderingContext ctx) {
            var efCoreEntity = new EFCoreEntity(Aggregate);
            var messages = new SaveCommandMessageContainer(Aggregate);
            var hasKeyNumberedOnSave = Aggregate
                .GetMembers()
                .Any(m => m is ValueMember vm && vm.IsKey && vm.Type is SequenceMember);

            return $$"""
                /// <summary>
                /// {{Aggregate.DisplayName}} の新規登録コマンド
                /// </summary>
                {{NijoAttr.RenderAttributeValues(ctx, Aggregate)}}
                public partial class {{CsClassName}} : {{CsBaseClassName}} {
                    internal {{CsClassName}}({{ctx.Config.DbContextName}} dbContext) : base(dbContext) {
                    }

                {{GetMembers().SelectTextTemplate(m => $$"""
                    {{WithIndent(m.RenderDeclaring(ctx))}}
                """)}}

                    {{WithIndent(RenderToDbEntity(isCreate: true))}}
                    private protected override {{efCoreEntity.CsClassName}} {{GET_CURRENT_ENTITY}}() => {{FIELD_SAVED_ENTITY}} ?? {{TO_DBENTITY}}();
                    private protected override string OperationNameForLog => "新規登録";
                {{If(hasKeyNumberedOnSave, () => $$"""
                    private protected override object? GetKeyForDuplicateCheckCore() => null; // キーは保存時に採番される
                """)}}

                    /// <inheritdoc />
                    public override {{messages.CsClassName}} {{GET_VALIDATION_ERRORS}}() {
                        var messages = new {{messages.CsClassName}}([], new());
                {{ValidatorBase.EnumerateAll(ctx).SelectTextTemplate(validator => $$"""
                        {{validator.MethodName}}(messages);
                """)}}
                        return messages;
                    }

                    {{WithIndent(RenderValidators(ctx))}}

                    {{WithIndent(SaveCommandTracking.RenderCreate((RootAggregate)Aggregate))}}
                }
                """;
        }
        #endregion CREATE


        #region UPDATE
        private string RenderRootUpdateCommand(CodeRenderingContext ctx) {
            var efCoreEntity = new EFCoreEntity(Aggregate);
            var messages = new SaveCommandMessageContainer(Aggregate);
            var right = new Variable("dbEntity", efCoreEntity);
            var dict = right
                 .Create1To1PropertiesRecursively()
                 .GroupBy(x => x.Metadata.SchemaPathNode.ToMappingKey())
                 .Select(group => new {
                     group.Key,
                     // 外部キーのカラムは、参照元自身のプロパティと、ナビゲーションプロパティで辿った先の
                     // 参照先エンティティのプロパティで重複するキーが登場するので、
                     // パスが最も短いもの（= 参照元自身のプロパティ）を選択する
                     Value = group.OrderBy(x => x.GetPathFromInstance().Count()).First(),
                 })
                 .ToDictionary(x => x.Key, x => x.Value);

            return $$"""
                /// <summary>
                /// {{Aggregate.DisplayName}} の更新コマンド
                /// </summary>
                {{NijoAttr.RenderAttributeValues(ctx, Aggregate)}}
                public partial class {{CsClassName}} : {{CsBaseClassName}} {
                    private {{CsClassName}}({{ctx.Config.DbContextName}} dbContext, {{efCoreEntity.CsClassName}} loaded, int expectedVersion) : base(dbContext) {
                        {{FIELD_LOADED}} = loaded;
                        {{FIELD_EXPECTED_VERSION}} = expectedVersion;
                    }
                    /// <summary>読み込み時点のデータ</summary>
                    private readonly {{efCoreEntity.CsClassName}} {{FIELD_LOADED}};
                    /// <summary>楽観排他制御で期待するバージョン。画面からの更新の場合は画面表示時のバージョン。</summary>
                    private readonly int {{FIELD_EXPECTED_VERSION}};

                {{GetMembers().SelectTextTemplate(m => $$"""
                    {{WithIndent(m.RenderDeclaring(ctx))}}
                """)}}

                    /// <summary>
                    /// このコマンドと同じデータの削除コマンドを返します。
                    /// 論理削除の場合は集約全体が必要になるため、削除するときもいったん読み込んでからこのメソッドで変換します。
                    /// </summary>
                    public {{CsDeleteClassName}} {{TO_DELETE_COMMAND}}() => new({{FIELD_DBCONTEXT}}, {{FIELD_LOADED}}, {{FIELD_EXPECTED_VERSION}});

                    {{WithIndent(RenderToDbEntity(isCreate: false))}}
                    private protected override {{efCoreEntity.CsClassName}} {{GET_CURRENT_ENTITY}}() => {{FIELD_SAVED_ENTITY}} ?? {{TO_DBENTITY}}();
                    private protected override string OperationNameForLog => "更新";

                    /// <summary>
                    /// Entity Framework Core のエンティティからこのクラスのインスタンスを作成します。
                    /// </summary>
                    internal static {{CsClassName}} {{FROM_DBENTITY}}({{ctx.Config.DbContextName}} dbContext, {{efCoreEntity.CsClassName}} dbEntity, int expectedVersion) {
                        return new(dbContext, dbEntity, expectedVersion) {
                            {{WithIndent(EnumerateMembers(this, right, dict))}}
                        };
                    }

                    /// <inheritdoc />
                    public override {{messages.CsClassName}} {{GET_VALIDATION_ERRORS}}() {
                        var messages = new {{messages.CsClassName}}([], new());
                        if ({{FIELD_LOADED}}.{{EFCoreEntity.VERSION}} != {{FIELD_EXPECTED_VERSION}}) {
                            messages.AddError({{Parts.Common.MsgFactory.MSG}}.{{ERR_CONCURRENCY}}());
                        }
                {{ValidatorBase.EnumerateAll(ctx).SelectTextTemplate(validator => $$"""
                        {{validator.MethodName}}(messages);
                """)}}
                        return messages;
                    }

                    {{WithIndent(RenderValidators(ctx))}}

                    {{WithIndent(SaveCommandTracking.RenderUpdate((RootAggregate)Aggregate))}}
                }
                """;

            static IEnumerable<string> EnumerateMembers(IInstancePropertyOwnerMetadata left, IInstancePropertyOwner right, IReadOnlyDictionary<SchemaNodeIdentity, IInstanceProperty> rigthMembers) {
                foreach (var member in left.GetMembers()) {
                    if (member is IInstanceValuePropertyMetadata vp) {
                        var rightPath = rigthMembers.TryGetValue(member.SchemaPathNode.ToMappingKey(), out var source)
                            ? $"{source.Root.Name}.{source.GetPathFromInstance().Select(p => p.Metadata.GetPropertyName(E_CsTs.CSharp)).Join("?.")}"
                            : "null";
                        yield return $$"""
                            {{member.GetPropertyName(E_CsTs.CSharp)}} = {{vp.Type.RenderCastToDomainType()}}{{rightPath}},
                            """;

                    } else if (member is IInstanceStructurePropertyMetadata sp) {

                        if (sp.IsArray) {
                            var arrayPath = rigthMembers.TryGetValue(sp.SchemaPathNode.ToMappingKey(), out var source)
                                ? $"{source.Root.Name}.{source.GetPathFromInstance().Select(p => p.Metadata.GetPropertyName(E_CsTs.CSharp)).Join("?.")}"
                              : throw new InvalidOperationException($"右辺にChildrenのXElementが無い: {sp.DisplayName}");

                            // 辞書に、ラムダ式内部で右辺に使用できるプロパティを加える
                            var dict2 = new Dictionary<SchemaNodeIdentity, IInstanceProperty>(rigthMembers);
                            var loopVar = new Variable(((ChildrenAggregate)sp.SchemaPathNode).GetLoopVarName(), (IInstancePropertyOwnerMetadata)source.Metadata);
                            foreach (var descendant in loopVar.Create1To1PropertiesRecursively()) {
                                var key = descendant.Metadata.SchemaPathNode.ToMappingKey();

                                // dictの宣言箇所のコメントと同じ理由により重複が発生しうるのでその場合はスキップ
                                if (dict2.ContainsKey(key)) continue;

                                dict2.Add(key, descendant);
                            }

                            yield return $$"""
                                {{member.GetPropertyName(E_CsTs.CSharp)}} = {{arrayPath}}?.Select({{loopVar.Name}} => new {{sp.GetTypeName(E_CsTs.CSharp)}}() {
                                    {{WithIndent(EnumerateMembers(sp, loopVar, dict2))}}
                                }).ToList() ?? [],
                                """;

                        } else if (sp is SaveCommandChildMember) {
                            // Child が存在しないデータをそのまま保存したときに空の Child が登録されないようにする
                            var childPath = rigthMembers.TryGetValue(sp.SchemaPathNode.ToMappingKey(), out var source)
                                ? $"{source.Root.Name}.{source.GetPathFromInstance().Select(p => p.Metadata.GetPropertyName(E_CsTs.CSharp)).Join("?.")}"
                                : throw new InvalidOperationException($"右辺にChildのXElementが無い: {sp.DisplayName}");

                            yield return $$"""
                                {{member.GetPropertyName(E_CsTs.CSharp)}} = {{childPath}} == null ? null : new() {
                                    {{WithIndent(EnumerateMembers(sp, right, rigthMembers))}}
                                },
                                """;

                        } else {
                            yield return $$"""
                                {{member.GetPropertyName(E_CsTs.CSharp)}} = new() {
                                    {{WithIndent(EnumerateMembers(sp, right, rigthMembers))}}
                                },
                                """;
                        }
                    }
                }
            }
        }
        #endregion UPDATE


        #region DELETE
        private string RenderRootDeleteCommand(CodeRenderingContext ctx) {
            var efCoreEntity = new EFCoreEntity(Aggregate);
            var messages = new SaveCommandMessageContainer(Aggregate);
            var updateCommand = new SaveCommand(Aggregate, E_Type.Update);

            return $$"""
                /// <summary>
                /// {{Aggregate.DisplayName}} の削除コマンド
                /// </summary>
                public partial class {{CsDeleteClassName}} : {{CsBaseClassName}}, {{INTERFACE_DELETE}}<{{updateCommand.CsClassName}}> {
                    internal {{CsDeleteClassName}}({{ctx.Config.DbContextName}} dbContext, {{efCoreEntity.CsClassName}} loaded, int expectedVersion) : base(dbContext) {
                        {{FIELD_LOADED}} = loaded;
                        {{FIELD_EXPECTED_VERSION}} = expectedVersion;
                    }
                    /// <summary>読み込み時点のデータ</summary>
                    private readonly {{efCoreEntity.CsClassName}} {{FIELD_LOADED}};
                    /// <summary>楽観排他制御で期待するバージョン。画面からの削除の場合は画面表示時のバージョン。</summary>
                    private readonly int {{FIELD_EXPECTED_VERSION}};

                    private protected override {{efCoreEntity.CsClassName}} {{GET_CURRENT_ENTITY}}() => {{FIELD_LOADED}};
                    private protected override string OperationNameForLog => "削除";

                    /// <inheritdoc />
                    public override {{messages.CsClassName}} {{GET_VALIDATION_ERRORS}}() {
                        var messages = new {{messages.CsClassName}}([], new());
                        if ({{FIELD_LOADED}}.{{EFCoreEntity.VERSION}} != {{FIELD_EXPECTED_VERSION}}) {
                            messages.AddError({{Parts.Common.MsgFactory.MSG}}.{{ERR_CONCURRENCY}}());
                        }
                        return messages;
                    }

                    {{WithIndent(SaveCommandTracking.RenderDelete((RootAggregate)Aggregate, ctx))}}
                }
                """;
        }
        #endregion DELETE


        /// <summary>
        /// 子孫集約のコマンド。単なる値の入れ物。
        /// </summary>
        private string RenderDescendantCommand(CodeRenderingContext ctx) {
            var comment = Type == E_Type.Create ? "新規登録" : "更新";

            return $$"""
                /// <summary>
                /// {{Aggregate.DisplayName}} の{{comment}}コマンド
                /// </summary>
                {{NijoAttr.RenderAttributeValues(ctx, Aggregate)}}
                public partial class {{CsClassName}} {
                {{GetMembers().SelectTextTemplate(m => $$"""
                    {{WithIndent(m.RenderDeclaring(ctx))}}
                """)}}
                }
                """;
        }

        private string RenderValidators(CodeRenderingContext ctx) {
            return $$"""
                #region 自動生成されるバリデーション処理
                {{ValidatorBase.EnumerateAll(ctx).SelectTextTemplate(validator => $$"""
                {{validator.RenderDeclaring(this, ctx)}}
                """)}}
                #endregion 自動生成されるバリデーション処理
                """;
        }


        #region メンバー
        /// <summary>
        /// ルート集約のコマンドはDbContextが作成した後でプロパティを設定する使い方をするのでrequiredにしない。
        /// </summary>
        private static string RenderRequiredModifier(AggregateBase owner) => owner is RootAggregate ? "" : "required ";

        internal interface ISaveCommandMember : IInstancePropertyMetadata {
            bool IsKey { get; }
            ISchemaPathNode Member { get; }
            string PhysicalName { get; }

            string RenderDeclaring(CodeRenderingContext ctx);
        }
        /// <summary>
        /// 更新処理引数クラスの値メンバー
        /// </summary>
        internal class SaveCommandValueMember : ISaveCommandMember, IInstanceValuePropertyMetadata {
            internal SaveCommandValueMember(ValueMember vm) {
                Member = vm;
            }
            internal ValueMember Member { get; }
            ISchemaPathNode ISaveCommandMember.Member => Member;

            public string PhysicalName => Member.PhysicalName;
            public string DisplayName => Member.DisplayName;

            public bool IsKey => Member.IsKey;
            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => Member;
            IValueMemberType IInstanceValuePropertyMetadata.Type => Member.Type;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => PhysicalName;

            string ISaveCommandMember.RenderDeclaring(CodeRenderingContext ctx) {
                return $$"""
                    /// <summary>{{DisplayName}}</summary>
                    {{NijoAttr.RenderAttributeValues(ctx, Member)}}
                    public {{RenderRequiredModifier(Member.Owner)}}{{Member.Type.CsDomainTypeName}}? {{PhysicalName}} { get; set; }
                    """;
            }
        }
        /// <summary>
        /// 更新処理引数クラスの参照先キー項目
        /// </summary>
        internal class SaveCommandRefMember : ISaveCommandMember, IInstanceStructurePropertyMetadata {
            internal SaveCommandRefMember(RefToMember refTo) {
                Member = refTo;
                RefEntry = new KeyClass.KeyClassEntry(refTo);
            }
            internal RefToMember Member { get; }
            internal KeyClass.KeyClassEntry RefEntry { get; }

            bool ISaveCommandMember.IsKey => Member.IsKey;
            ISchemaPathNode ISaveCommandMember.Member => Member;
            string ISaveCommandMember.PhysicalName => Member.PhysicalName;

            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => Member;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => Member.PhysicalName;
            string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => RefEntry.ClassName;
            bool IInstanceStructurePropertyMetadata.IsArray => false;

            IEnumerable<IInstancePropertyMetadata> IInstancePropertyOwnerMetadata.GetMembers() {
                return ((IInstancePropertyOwnerMetadata)RefEntry).GetMembers();
            }
            string ISaveCommandMember.RenderDeclaring(CodeRenderingContext ctx) {
                return $$"""
                    /// <summary>{{Member.DisplayName}}</summary>
                    {{NijoAttr.RenderAttributeValues(ctx, Member)}}
                    public {{RenderRequiredModifier(Member.Owner)}}{{RefEntry.ClassName}}? {{Member.PhysicalName}} { get; set; }
                    """;
            }
        }
        /// <summary>
        /// 更新処理引数クラスの子メンバー
        /// </summary>
        internal class SaveCommandChildMember : SaveCommand, ISaveCommandMember, IInstanceStructurePropertyMetadata {
            internal SaveCommandChildMember(ChildAggregate child, E_Type type) : base(child, type) { }

            ISchemaPathNode ISaveCommandMember.Member => (IAggregateMember)Aggregate;
            public string PhysicalName => Aggregate.PhysicalName;
            public string DisplayName => Aggregate.DisplayName;

            bool ISaveCommandMember.IsKey => false;
            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => Aggregate;
            bool IInstanceStructurePropertyMetadata.IsArray => false;
            string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => CsClassName;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => PhysicalName;

            string ISaveCommandMember.RenderDeclaring(CodeRenderingContext ctx) {
                return $$"""
                    /// <summary>{{DisplayName}}</summary>
                    {{NijoAttr.RenderAttributeValues(ctx, Aggregate)}}
                    public {{RenderRequiredModifier(Aggregate.GetParent()!)}}{{CsClassName}}? {{PhysicalName}} { get; set; }
                    """;
            }
        }
        /// <summary>
        /// 更新処理引数クラスの子コレクションメンバー
        /// </summary>
        internal class SaveCommandChildrenMember : SaveCommand, ISaveCommandMember, IInstanceStructurePropertyMetadata {
            internal SaveCommandChildrenMember(ChildrenAggregate children, E_Type type) : base(children, type) { }

            internal new ChildrenAggregate Aggregate => (ChildrenAggregate)base.Aggregate;
            ISchemaPathNode ISaveCommandMember.Member => (IAggregateMember)base.Aggregate;
            public string PhysicalName => base.Aggregate.PhysicalName;
            public string DisplayName => base.Aggregate.DisplayName;

            bool ISaveCommandMember.IsKey => false;
            ISchemaPathNode IInstancePropertyMetadata.SchemaPathNode => base.Aggregate;
            bool IInstanceStructurePropertyMetadata.IsArray => true;
            string IInstanceStructurePropertyMetadata.GetTypeName(E_CsTs csts) => CsClassName;
            string IInstancePropertyMetadata.GetPropertyName(E_CsTs csts) => PhysicalName;

            string ISaveCommandMember.RenderDeclaring(CodeRenderingContext ctx) {
                var parent = base.Aggregate.GetParent()!;
                return $$"""
                    /// <summary>{{DisplayName}}</summary>
                    {{NijoAttr.RenderAttributeValues(ctx, Aggregate)}}
                    public {{RenderRequiredModifier(parent)}}List<{{CsClassName}}> {{PhysicalName}} { get; set; }{{(parent is RootAggregate ? " = [];" : "")}}
                    """;
            }
        }
        #endregion メンバー

        private string RenderToDbEntity(bool isCreate) {

            // 右辺の定義
            var rootInstance = new Variable("this", this);
            var rightDictOfRootInstance = rootInstance
                .Create1To1PropertiesRecursively()
                .ToDictionary(x => x.Metadata.SchemaPathNode.ToMappingKey());

            var efCoreEntity = new EFCoreEntity(Aggregate);

            return $$"""
                /// <summary>
                /// このインスタンスの現時点の値を、このインスタンスと参照を共有しない Entity Framework Core のエンティティに変換します。
                /// </summary>
                internal {{efCoreEntity.CsClassName}} {{TO_DBENTITY}}() {
                    return new {{efCoreEntity.CsClassName}} {
                        {{WithIndent(RenderToDbEntityBody(efCoreEntity, rootInstance, rightDictOfRootInstance))}}
                {{If(isCreate, () => $$"""
                        {{EFCoreEntity.VERSION}} = 0,
                """)}}
                    };
                }
                """;

            IEnumerable<string> RenderToDbEntityBody(EFCoreEntity left, IInstancePropertyOwner right, IReadOnlyDictionary<SchemaNodeIdentity, IInstanceProperty> rigthMembers) {

                // 自身のカラム、外部参照のキー、親のキー
                foreach (var col in left.GetColumns()) {
                    // シーケンス項目など登録と共に採番されるものはnullになる可能性がある
                    var mappingKey = col.Member?.ToMappingKey();
                    var sourcePath = mappingKey != null && rigthMembers.TryGetValue(mappingKey, out var source)
                        ? $"{source.Root.Name}.{source.GetPathFromInstance().Select(p => p.Metadata.GetPropertyName(E_CsTs.CSharp)).Join("?.")}"
                        : "null";
                    var copyArray = col.MemberType.CsPrimitiveTypeName == "byte[]" && sourcePath != "null"
                        ? "?.ToArray()"
                        : "";
                    yield return $$"""
                        {{col.PhysicalName}} = {{col.MemberType.RenderCastToPrimitiveType()}}{{sourcePath}}{{copyArray}},
                        """;
                }

                // 子
                var childAndChildren = left
                    .GetNavigationProperties()
                    .OfType<NavigationProperty.NavigationOfParentChild>()
                    .Where(nav => nav.Principal.ThisSide == left.Aggregate);
                foreach (var nav in childAndChildren) {
                    if (nav.Relevant.ThisSide is ChildAggregate child) {
                        // Child
                        var childEntity = new EFCoreEntity(nav.Relevant.ThisSide);
                        var childPath = rigthMembers.TryGetValue(child.ToMappingKey(), out var cs)
                            ? $"{cs.Root.Name}.{cs.GetPathFromInstance().Select(p => p.Metadata.GetPropertyName(E_CsTs.CSharp)).Join("?.")}"
                            : throw new InvalidOperationException($"右辺にChildのXElementが無い: {child}");

                        // childPath が null でない場合のみ Child を生成
                        yield return $$"""
                            {{nav.Principal.OtherSidePhysicalName}} = {{childPath}} == null ? null : new() {
                                {{WithIndent(RenderToDbEntityBody(childEntity, right, rigthMembers))}}
                            },
                            """;

                    } else if (nav.Relevant.ThisSide is ChildrenAggregate children) {
                        var childrenEntity = new EFCoreEntity(nav.Relevant.ThisSide);
                        var arrayPath = rigthMembers.TryGetValue(children.ToMappingKey(), out var source)
                            ? $"{source.Root.Name}.{source.GetPathFromInstance().Select(p => p.Metadata.GetPropertyName(E_CsTs.CSharp)).Join("?.")}"
                            : throw new InvalidOperationException($"右辺にChildrenのXElementが無い: {children}");

                        // 辞書に、ラムダ式内部で右辺に使用できるプロパティを加える
                        var dict2 = new Dictionary<SchemaNodeIdentity, IInstanceProperty>(rigthMembers);
                        var saveCommand = new SaveCommandChildrenMember(children, Type);
                        var loopVar = new Variable(children.GetLoopVarName(), saveCommand);
                        foreach (var descendant in loopVar.Create1To1PropertiesRecursively()) {
                            dict2.Add(descendant.Metadata.SchemaPathNode.ToMappingKey(), descendant);
                        }

                        yield return $$"""
                            {{nav.Principal.OtherSidePhysicalName}} = {{arrayPath}}?.Select({{loopVar.Name}} => new {{childrenEntity.CsClassName}} {
                                {{WithIndent(RenderToDbEntityBody(childrenEntity, loopVar, dict2))}}
                            }).ToHashSet() ?? [],
                            """;

                    } else {
                        throw new NotImplementedException();
                    }
                }
            }
        }
    }
}

namespace Nijo.DataStructures {
    using Nijo.DataStructures.IsStored;
    partial class SchemaPathNodeExtensions {

        /// <summary>
        /// <see cref="GetPathFromEntry(ISchemaPathNode)"/> の結果を <see cref="SaveCommand"/> のルールに沿ったパスとして返す
        /// </summary>
        public static IEnumerable<string> AsSaveCommand(this IEnumerable<ISchemaPathNode> path) {
            var isOutOfEntryTree = false;

            foreach (var node in path) {
                if (node.PreviousNode == null) continue; // パスの一番最初（エントリー）はスキップ
                if (node.PreviousNode is RefToMember) continue; // refの1つ次の要素の名前はrefで列挙済みのためスキップ

                // 外部参照のプロパティを辿るパス
                if (node is RefToMember refTo) {
                    var previous = (AggregateBase?)node.PreviousNode ?? throw new InvalidOperationException("reftoの前は必ず参照元集約か参照先集約になるのでありえない");

                    // 参照元から参照先へ辿るパス
                    if (previous == refTo.Owner) {
                        if (!isOutOfEntryTree) {
                            // エントリーの集約内部から外に出る瞬間の場合
                            var member = new SaveCommand.SaveCommandRefMember(refTo);
                            yield return member.Member.PhysicalName;

                            isOutOfEntryTree = true;
                            continue;

                        } else {
                            // 参照先のキーの中でさらに他の集約への参照が発生した場合
                            var key = new KeyClass.KeyClassRefMember(refTo);
                            yield return key.PhysicalName;
                            continue;
                        }
                    }
                    // 参照先から参照元へ辿るパス
                    if (previous == refTo.RefTo) {
                        throw new InvalidOperationException("更新処理引数クラスでは参照先から参照元へ辿ることはできない");
                    }
                    throw new InvalidOperationException("reftoの前は必ず参照元集約か参照先集約になるのでありえない");
                }

                // 親子間のナビゲーションプロパティを辿るパス
                if (node is AggregateBase curr && node.PreviousNode is AggregateBase prev) {

                    // 子から親へ辿るパス
                    if (curr.IsParentOf(prev)) {

                        // エントリーの集約内部では子から親へ辿るパターンは無い
                        if (!isOutOfEntryTree) throw new InvalidOperationException("エントリーの集約内部では子から親へ辿るパターンは無い");

                        var parentMember = new KeyClass.KeyClassEntry(curr);
                        yield return parentMember.PhysicalName;
                        continue;
                    }
                    // 親から子へ辿るパス
                    if (curr.IsChildOf(prev)) {

                        // 参照先のキーの中では親から子へ辿るパターンは無い
                        if (isOutOfEntryTree) throw new InvalidOperationException("参照先のキーの中では親から子へ辿るパターンは無い");

                        var childMemberPhysicalName = curr switch {
                            ChildAggregate child => new SaveCommand.SaveCommandChildMember(child, SaveCommand.E_Type.Create).PhysicalName, // CreateとUpdateで名前同じなのでCにしている
                            ChildrenAggregate children => new SaveCommand.SaveCommandChildrenMember(children, SaveCommand.E_Type.Create).PhysicalName, // CreateとUpdateで名前同じなのでCにしている
                            _ => throw new InvalidOperationException("ありえない"),
                        };
                        yield return childMemberPhysicalName;
                        continue;
                    }
                    throw new InvalidOperationException("必ず 親→子, 子→親 のどちらかになるのでありえない");
                }

                // 末端のメンバー
                if (node is not ValueMember vm) throw new InvalidOperationException("この分岐まで来るケースは値の場合しか無いのでありえない");

                var valueMember = new SaveCommand.SaveCommandValueMember(vm);
                yield return valueMember.PhysicalName;
            }
        }

    }
}
