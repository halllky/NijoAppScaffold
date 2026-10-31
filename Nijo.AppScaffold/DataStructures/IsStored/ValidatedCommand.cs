using Nijo.CodeGenerating;
using Nijo.DataStructures.Aggregates;
using Nijo.DataStructures.ValueMemberTypes;
using System.Linq;

namespace Nijo.DataStructures.IsStored;

/// <summary>
/// 検証を通過したコマンドの、検証時点の値。
/// アプリケーション側で実装される保存処理はこれだけを受け取るので、未検証のデータの保存はコンパイルエラーになる。
/// 保存処理の流れはシステムごとに要件が大きく異なるため自動生成せず、スキーマに依存する部品だけをここで定義する。
/// </summary>
internal static class ValidatedCommand {

    internal const string CLASS_NAME = "ValidatedCommand";
    internal const string DELETE_CLASS_NAME = "ValidatedDeleteCommand";
    internal const string USAGE_CLASS_NAME = "SaveCommandUsage";

    /// <summary>コマンドのクラスの中に生成される、検証済みコマンドの実装クラスの名前</summary>
    private const string IMPL_CLASS_NAME = "ValidatedSnapshot";

    internal const string START_TRACKING_CORE = "StartTrackingCore";
    /// <summary>実装クラスが持つ、元のコマンドを指すフィールド</summary>
    internal const string FIELD_COMMAND = "_command";
    /// <summary>実装クラスが持つ、検証時点の値をもつエンティティを指すフィールド</summary>
    internal const string FIELD_SNAPSHOT = "_snapshot";

    /// <summary>
    /// すべての集約で共通のクラスをレンダリングします。
    /// スキーマ定義にデータ構造が1個も無くても保存処理のコンパイルが通るように、常に生成されます。
    /// </summary>
    internal static SourceFile Render(CodeRenderingContext ctx) {
        return new SourceFile {
            FileName = $"{CLASS_NAME}.cs",
            Contents = $$"""
                using System.Text.Json.Nodes;
                using Microsoft.EntityFrameworkCore;

                namespace {{ctx.Config.RootNamespace}};

                /// <summary>
                /// 検証を通過したコマンドの、検証時点の値。
                /// 元のコマンドを検証後に書き換えても影響を受けない。
                /// 通常の業務ロジックでは保存処理に渡すだけで、メンバーを直接使うことはない。
                /// </summary>
                public abstract class {{CLASS_NAME}} {
                    private protected {{CLASS_NAME}}(DbContext dbContext, {{USAGE_CLASS_NAME}} usage) {
                        _dbContext = dbContext;
                        _usage = usage;
                    }
                    private readonly DbContext _dbContext;
                    private readonly {{USAGE_CLASS_NAME}} _usage;

                    /// <summary>
                    /// 監査項目と楽観排他のバージョンを設定し、エンティティの変更追跡を開始します。
                    /// 元のコマンドが同じであれば、検証済みコマンドが異なっていても1回しか呼べません。2回目以降は例外を送出します。
                    /// </summary>
                    /// <returns>変更追跡を開始したエンティティ。保存後にこれらの変更追跡を終了するのは呼び出し側の責務です。</returns>
                    public IReadOnlyList<object> StartTracking() {
                        _usage.MarkStarted();

                        var tracked = new List<object>();
                        try {
                            {{START_TRACKING_CORE}}(tracked);
                        } catch {
                            foreach (var entity in tracked) {
                                _dbContext.Entry(entity).State = EntityState.Detached;
                            }
                            throw;
                        }
                        return tracked;
                    }

                    /// <summary>変更追跡を開始する。変更追跡を開始するエンティティは状態を変更する前に引数のリストに追加すること。</summary>
                    private protected abstract void {{START_TRACKING_CORE}}(List<object> tracked);

                    /// <summary>同一の保存処理の中でのキー重複の検出に使うキーを返します。保存時に採番されるのでキーが未確定の場合は null を返します。</summary>
                    public abstract object? GetKeyForDuplicateCheck();

                    /// <summary>ログに出力する情報（集約名、操作の種類、キー）を返します。</summary>
                    public abstract JsonNode ToLogJson();
                }

                /// <inheritdoc cref="{{CLASS_NAME}}"/>
                /// <typeparam name="TUpdateCommand">保存後のデータを表す更新コマンドの型</typeparam>
                public abstract class {{CLASS_NAME}}<TUpdateCommand> : {{CLASS_NAME}} where TUpdateCommand : class {
                    private protected {{CLASS_NAME}}(DbContext dbContext, {{USAGE_CLASS_NAME}} usage) : base(dbContext, usage) {
                    }

                    /// <summary>保存後のデータを表す、このインスタンスとは別の更新コマンドを返します。削除の場合は null を返します。</summary>
                    public abstract TUpdateCommand? ToSavedCommand();
                }

                /// <summary>
                /// 検証を通過した削除コマンド。
                /// 保存処理の戻り値に保存後のデータが含まれないことを型で表すためにオーバーロードを分けるのに使う。
                /// </summary>
                /// <typeparam name="TUpdateCommand">削除コマンドと同じ集約の更新コマンドの型</typeparam>
                public abstract class {{DELETE_CLASS_NAME}}<TUpdateCommand> : {{CLASS_NAME}}<TUpdateCommand> where TUpdateCommand : class {
                    private protected {{DELETE_CLASS_NAME}}(DbContext dbContext, {{USAGE_CLASS_NAME}} usage) : base(dbContext, usage) {
                    }

                    /// <inheritdoc />
                    public sealed override TUpdateCommand? ToSavedCommand() => null;
                }

                /// <summary>
                /// コマンドが保存処理にかけられたか否か。
                /// 1つのコマンドを複数回検証すると検証済みコマンドが複数できるが、それらを別々に保存すると
                /// 保存時に採番されるキーをもつデータでは二重に登録されてしまうので、コマンドの単位で管理する。
                /// </summary>
                internal sealed class {{USAGE_CLASS_NAME}} {
                    private bool _started;

                    /// <summary>保存処理にかけられたことを記録します。既に記録されている場合は例外を送出します。</summary>
                    internal void MarkStarted() {
                        if (_started) throw new InvalidOperationException("一度保存処理にかけたコマンドは再度使用できません。");
                        _started = true;
                    }
                }
                """,
        };
    }

    /// <summary>新規登録コマンドの中に生成される実装クラス</summary>
    internal static string RenderCreateImpl(RootAggregate rootAggregate) {
        var createCommand = new SaveCommand(rootAggregate, SaveCommand.E_Type.Create);
        var updateCommand = new SaveCommand(rootAggregate, SaveCommand.E_Type.Update);
        var efCoreEntity = new EFCoreEntity(rootAggregate);
        var hasKeyNumberedOnSave = rootAggregate
            .GetMembers()
            .Any(m => m is ValueMember vm && vm.IsKey && vm.Type is SequenceMember);

        return $$"""
            private sealed class {{IMPL_CLASS_NAME}} : {{CLASS_NAME}}<{{updateCommand.CsClassName}}> {
                internal {{IMPL_CLASS_NAME}}({{createCommand.CsClassName}} command, {{efCoreEntity.CsClassName}} snapshot) : base(command.{{SaveCommand.FIELD_DBCONTEXT}}, command.{{SaveCommand.FIELD_USAGE}}) {
                    {{FIELD_COMMAND}} = command;
                    {{FIELD_SNAPSHOT}} = snapshot;
                }
                private readonly {{createCommand.CsClassName}} {{FIELD_COMMAND}};
                private readonly {{efCoreEntity.CsClassName}} {{FIELD_SNAPSHOT}};

            {{If(hasKeyNumberedOnSave, () => $$"""
                public override object? GetKeyForDuplicateCheck() => null; // キーは保存時に採番される
            """).Else(() => $$"""
                public override object? GetKeyForDuplicateCheck() => {{new KeyClass.KeyClassEntry(rootAggregate).ClassName}}.{{SaveCommand.FROM_DBENTITY}}({{FIELD_SNAPSHOT}});
            """)}}
                public override JsonNode ToLogJson() => {{SaveCommand.CREATE_LOG_JSON}}({{FIELD_SNAPSHOT}}, "新規登録");
                public override {{updateCommand.CsClassName}}? ToSavedCommand() {
                    return {{updateCommand.CsClassName}}.{{SaveCommand.FROM_DBENTITY}}({{FIELD_COMMAND}}.{{SaveCommand.FIELD_DBCONTEXT}}, {{FIELD_SNAPSHOT}}, {{FIELD_SNAPSHOT}}.{{EFCoreEntity.VERSION}}!.Value);
                }

                {{WithIndent(SaveCommandTracking.RenderCreate(rootAggregate))}}
            }
            """;
    }

    /// <summary>更新コマンドの中に生成される実装クラス</summary>
    internal static string RenderUpdateImpl(RootAggregate rootAggregate) {
        var updateCommand = new SaveCommand(rootAggregate, SaveCommand.E_Type.Update);
        var efCoreEntity = new EFCoreEntity(rootAggregate);

        return $$"""
            private sealed class {{IMPL_CLASS_NAME}} : {{CLASS_NAME}}<{{updateCommand.CsClassName}}> {
                internal {{IMPL_CLASS_NAME}}({{updateCommand.CsClassName}} command, {{efCoreEntity.CsClassName}} snapshot) : base(command.{{SaveCommand.FIELD_DBCONTEXT}}, command.{{SaveCommand.FIELD_USAGE}}) {
                    {{FIELD_COMMAND}} = command;
                    {{FIELD_SNAPSHOT}} = snapshot;
                }
                private readonly {{updateCommand.CsClassName}} {{FIELD_COMMAND}};
                private readonly {{efCoreEntity.CsClassName}} {{FIELD_SNAPSHOT}};

                public override object? GetKeyForDuplicateCheck() => {{new KeyClass.KeyClassEntry(rootAggregate).ClassName}}.{{SaveCommand.FROM_DBENTITY}}({{FIELD_SNAPSHOT}});
                public override JsonNode ToLogJson() => {{SaveCommand.CREATE_LOG_JSON}}({{FIELD_SNAPSHOT}}, "更新");
                public override {{updateCommand.CsClassName}}? ToSavedCommand() {
                    return {{updateCommand.CsClassName}}.{{SaveCommand.FROM_DBENTITY}}({{FIELD_COMMAND}}.{{SaveCommand.FIELD_DBCONTEXT}}, {{FIELD_SNAPSHOT}}, {{FIELD_SNAPSHOT}}.{{EFCoreEntity.VERSION}}!.Value);
                }

                {{WithIndent(SaveCommandTracking.RenderUpdate(rootAggregate))}}
            }
            """;
    }

    /// <summary>削除コマンドの中に生成される実装クラス</summary>
    internal static string RenderDeleteImpl(RootAggregate rootAggregate, CodeRenderingContext ctx) {
        var createCommand = new SaveCommand(rootAggregate, SaveCommand.E_Type.Create);
        var updateCommand = new SaveCommand(rootAggregate, SaveCommand.E_Type.Update);

        return $$"""
            private sealed class {{IMPL_CLASS_NAME}} : {{DELETE_CLASS_NAME}}<{{updateCommand.CsClassName}}> {
                internal {{IMPL_CLASS_NAME}}({{createCommand.CsDeleteClassName}} command) : base(command.{{SaveCommand.FIELD_DBCONTEXT}}, command.{{SaveCommand.FIELD_USAGE}}) {
                    {{FIELD_COMMAND}} = command;
                }
                private readonly {{createCommand.CsDeleteClassName}} {{FIELD_COMMAND}};

                public override object? GetKeyForDuplicateCheck() => {{new KeyClass.KeyClassEntry(rootAggregate).ClassName}}.{{SaveCommand.FROM_DBENTITY}}({{FIELD_COMMAND}}.{{SaveCommand.FIELD_LOADED}});
                public override JsonNode ToLogJson() => {{SaveCommand.CREATE_LOG_JSON}}({{FIELD_COMMAND}}.{{SaveCommand.FIELD_LOADED}}, "削除");

                {{WithIndent(SaveCommandTracking.RenderDelete(rootAggregate, ctx))}}
            }
            """;
    }

    /// <summary>
    /// TryValidate メソッドの末尾の、検証結果に応じて検証済みコマンドを返す部分をレンダリングします。
    /// 変数 validated と errors がレンダリング結果の挿入先のスコープに存在する前提。
    /// </summary>
    /// <param name="implConstructorArgs">実装クラスのコンストラクタの引数。検証時点の値を固定する式を含む。</param>
    internal static string RenderReturnFromTryValidate(string implConstructorArgs) {
        return $$"""
            if (errors.GetState()?.DescendantsAndSelf().Any(c => c.Errors.Count > 0) == true) {
                validated = null;
                return false;
            }
            validated = new {{IMPL_CLASS_NAME}}({{implConstructorArgs}});
            return true;
            """;
    }
}
