using Nijo.Core;
using Nijo.Parts.BothOfClientAndServer;
using Nijo.Parts.WebServer;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.CommandModelFeatures {
    /// <summary>
    /// <see cref="CommandMethod"/> における本処理を呼び出す処理
    /// </summary>
    internal class CommandMethod {
        internal CommandMethod(GraphNode<Aggregate> rootAggregate) {
            _rootAggregate = rootAggregate;
        }
        private readonly GraphNode<Aggregate> _rootAggregate;

        internal string HookName => $"use{_rootAggregate.Item.PhysicalName}Launcher";

        private string AppSrvInitValueGetMethod => $"Get{_rootAggregate.Item.PhysicalName}InitValue";
        private string AppSrvOnStepChangingMethod => $"On{_rootAggregate.Item.PhysicalName}StepChanging";
        private string AppSrvExecuteMethod => $"Execute{_rootAggregate.Item.PhysicalName}";

        /// <summary>C#側でステップ属性のどの要素かを表す列挙体の名前</summary>
        private string StepEnumName => $"E_{_rootAggregate.Item.PhysicalName}Steps";

        private const string USE_COMMAND_RESULT_PARSER = "useCommandResultParser";

        private const string USE_STEP_CHAGNE_HOOK = "useCommandStepChanges";
        private const string CONTROLLER_GET_INIT_VALUE = "get-init-value";
        private const string CONTROLLER_CHAGNE_STEPS = "change-step";

        // ステップ切り替え時イベント引数
        private const string STEP_CHANGING_EVENT_ARGS = "CommandStepChangingEventArgs";
        private const string DATA_TS = "data";
        private const string BEFORE_STEP_TS = "beforeStep";
        private const string AFTER_STEP_TS = "afterStep";
        private const string VISITED_STEPS_TS = "visitedSteps";

        internal string RenderHook(CodeRenderingContext context) {
            var param = new CommandParameter(_rootAggregate);
            var steps = _rootAggregate
                .GetMembers()
                .OfType<AggregateMember.RelationMember>()
                .Where(m => m.MemberAggregate.Item.Options.Step != null)
                .Select(m => m.MemberAggregate.Item.Options.Step!.Value)
                .OrderBy(step => step)
                .ToArray();

            var args = new List<string>();
            if (steps.Length != 0) {
                args.Add($"getValues: ReactHookForm.UseFormGetValues<Types.{param.TsTypeName}>");
                args.Add($"resetData: ReactHookForm.UseFormReset<Types.{param.TsTypeName}>");
            }

            return $$"""
                /** {{_rootAggregate.Item.DisplayName}}処理を呼び出す関数を返します。 */
                export const {{HookName}} = ({{args.Join(", ")}}) => {
                  const { post, complexPost } = Util.useHttpRequest()
                {{If(steps.Length != 0, () => $$"""
                  const [currentStep, setCurrentStep] = React.useState({{steps.First()}}) // 現在表示中のステップの番号
                  const [visitedSteps, setVisitedSteps] = React.useState<number[]>([]) // 一度以上表示したことがあるステップの番号
                  const callStepChangeEvent = {{USE_STEP_CHAGNE_HOOK}}(resetData, setError, clearErrors, setVisitedSteps)
                  const allSteps = React.useMemo(() => {
                    return [{{steps.Select(s => s.ToString()).Join(", ")}}]
                  }, [])
                  const toPreviousStep = useEvent(async () => {
                    const beforeStep = currentStep
                    let afterStep: number
                {{steps.Skip(1).SelectTextTemplate((step, i) => $$"""
                    {{(i == 0 ? "if" : "} else if")}} (currentStep === {{step}}) {
                      afterStep = {{steps.ElementAt(i)}}
                """)}}
                    } else {
                      return
                    }
                    const success = await callStepChangeEvent(`/{{Controller.SUBDOMAIN}}/{{CommandController.SUBDOMAIN}}/{{(_rootAggregate.Item.Options.LatinName ?? _rootAggregate.Item.UniqueId).ToKebabCase()}}/{{CONTROLLER_CHAGNE_STEPS}}`, getValues(), beforeStep, afterStep, visitedSteps)
                    if (success) setCurrentStep(afterStep)
                  })
                  const toNextStep = useEvent(async () => {
                    const beforeStep = currentStep
                    let afterStep: number
                {{steps.SkipLast(1).SelectTextTemplate((step, i) => $$"""
                    {{(i == 0 ? "if" : "} else if")}} (currentStep === {{step}}) {
                      afterStep = {{steps.ElementAt(i + 1)}}
                """)}}
                    } else {
                      return
                    }
                    const success = await callStepChangeEvent(`/{{Controller.SUBDOMAIN}}/{{CommandController.SUBDOMAIN}}/{{(_rootAggregate.Item.Options.LatinName ?? _rootAggregate.Item.UniqueId).ToKebabCase()}}/{{CONTROLLER_CHAGNE_STEPS}}`, getValues(), beforeStep, afterStep, visitedSteps)
                    if (success) setCurrentStep(afterStep)
                  })
                """)}}
                  const getInitValue = useEvent(async () => {
                    return await post<Types.{{param.TsTypeName}}>(`/{{Controller.SUBDOMAIN}}/{{CommandController.SUBDOMAIN}}/{{(_rootAggregate.Item.Options.LatinName ?? _rootAggregate.Item.UniqueId).ToKebabCase()}}/{{CONTROLLER_GET_INIT_VALUE}}`)
                  })
                  const [nowProccessing, setNowProccessing] = React.useState(false)
                  const launch = useEvent(async <TReturnValue = unknown>(
                    /** パラメータ */
                    param: Types.{{param.TsTypeName}},
                    /** オプション。handleDetailErrorだけは必須。 */
                    options: Omit<Util.ComplexPostOptions, 'handleDetailError'> & { handleDetailError: Exclude<Util.ComplexPostOptions['handleDetailError'], undefined> }
                  ): Promise<Util.ComplexPostResponse<TReturnValue>> => {
                    if (nowProccessing) return { ok: false, data: undefined }
                    setNowProccessing(true)
                    try {
                      return await complexPost(`/{{Controller.SUBDOMAIN}}/{{CommandController.SUBDOMAIN}}/{{(_rootAggregate.Item.Options.LatinName ?? _rootAggregate.Item.UniqueId).ToKebabCase()}}`, param, options)
                    } finally {
                      setNowProccessing(false)
                    }
                  })

                  return {
                {{If(steps.Length != 0, () => $$"""
                    /** 現在のステップの番号 */
                    currentStep,
                    /** このコマンドにあるステップ番号の一覧 */
                    allSteps,
                    /** 前のステップに遷移します。現在が先頭のステップである場合は何も起きません。 */
                    toPreviousStep,
                    /** 次のステップに遷移します。現在が最後尾のステップである場合は何も起きません。 */
                    toNextStep,
                """)}}
                    /** サーバー処理を呼び出して初期値を取得します。 */
                    getInitValue,
                    /** コマンドを実行します。 */
                    launch,
                    /** サーバー側処理の呼び出しを開始してから終了するまでの間、trueになります。 */
                    nowProccessing,
                  }
                }
                """;
        }

        internal string RenderController(CodeRenderingContext context) {
            var param = new CommandParameter(_rootAggregate);
            var steps = _rootAggregate
                .GetMembers()
                .OfType<AggregateMember.RelationMember>()
                .Any(m => m.MemberAggregate.Item.Options.Step != null);

            return $$"""
                /// <summary>
                /// {{_rootAggregate.Item.DisplayName}}の初期値取得イベント
                /// </summary>
                [HttpPost("{{(_rootAggregate.Item.Options.LatinName ?? _rootAggregate.Item.UniqueId).ToKebabCase()}}/{{CONTROLLER_GET_INIT_VALUE}}")]
                public virtual IActionResult GetInitValueOf{{_rootAggregate.Item.PhysicalName}}() {
                    if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{_rootAggregate.Item.PhysicalName}}) < E_AuthLevel.Write_RESTRICT) return Forbid();
                    var param = _applicationService.{{AppSrvInitValueGetMethod}}();
                    _applicationService.Log.Debug("Get Init Value {{_rootAggregate.Item.PhysicalName}}: {0}", param.ToJson());
                    return this.JsonContent(param);
                }
                {{If(steps, () => $$"""
                /// <summary>
                /// {{_rootAggregate.Item.DisplayName}}のステップ変更時イベント
                /// </summary>
                [HttpPost("{{(_rootAggregate.Item.Options.LatinName ?? _rootAggregate.Item.UniqueId).ToKebabCase()}}/{{CONTROLLER_CHAGNE_STEPS}}")]
                public virtual IActionResult {{_rootAggregate.Item.PhysicalName}}ChgngeStep(ComplexPostRequest<{{STEP_CHANGING_EVENT_ARGS}}<{{param.CsClassName}}, {{StepEnumName}}, {{param.MessageDataCsClassName}}>> e) {
                    _applicationService.Log.Debug("ChgngeStep {{_rootAggregate.Item.PhysicalName}}: {0}", e.Data.ToJson());
                    _applicationService.{{AppSrvOnStepChangingMethod}}(e.Data);
                    if (e.Data.Messages.HasError()) {
                        return this.ShowErrorsUsingReactHook(new JsonArray(e.Data.Messages.ToReactHookFormErrors().ToArray()));
                    } else if (!e.IgnoreConfirm && e.Data.HasConfirm()) {
                        return this.ShowConfirmUsingReactHook(e.Data.GetConfirms().ToArray(), new JsonArray(e.Data.Messages.ToReactHookFormErrors().ToArray()));
                    } else {
                        return this.JsonContent(e.Data);
                    }
                }
                """)}}
                /// <summary>
                /// {{_rootAggregate.Item.DisplayName}}処理をWebAPI経由で実行するためのエンドポイント
                /// </summary>
                [HttpPost("{{(_rootAggregate.Item.Options.LatinName ?? _rootAggregate.Item.UniqueId).ToKebabCase()}}")]
                public virtual async Task<IActionResult> {{_rootAggregate.Item.PhysicalName}}(ComplexPostRequest<{{param.CsClassName}}> param) {
                    _applicationService.Log.Debug("Execute {{_rootAggregate.Item.PhysicalName}}: {0}", param.Data.ToJson());
                    if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{_rootAggregate.Item.PhysicalName}}) < E_AuthLevel.Write_RESTRICT) return Forbid();
                    var presentationContext = new PresentationContext(
                        new {{param.MessageDataCsClassName}}(),
                        new() { IgnoreConfirm = param.IgnoreConfirm },
                        _applicationService);
                    await _applicationService.{{AppSrvExecuteMethod}}(param.Data, presentationContext);
                    var result = presentationContext.GetResult().ToJsonObject();
                    return this.JsonContent(result);
                }
                """;
        }

        internal string RenderAbstractMethod(CodeRenderingContext context) {
            var param = new CommandParameter(_rootAggregate);
            var steps = _rootAggregate
                .GetMembers()
                .OfType<AggregateMember.RelationMember>()
                .Where(m => m.MemberAggregate.Item.Options.Step != null)
                .Select(m => m.MemberAggregate.Item.Options.Step!.Value)
                .OrderBy(step => step)
                .ToArray();

            return $$"""
                /// <summary>
                /// {{_rootAggregate.Item.DisplayName}}の初期値取得処理
                /// </summary>
                public virtual {{param.CsClassName}} {{AppSrvInitValueGetMethod}}() {
                    return new();
                }
                {{If(steps.Length > 0, () => $$"""
                /// <summary>
                /// {{_rootAggregate.Item.DisplayName}}のステップ切り替え時やコマンド実行前のタイミングで実行される処理
                /// </summary>
                /// <param name="param">処理パラメータ</param>
                /// <param name="e">エラー情報などがある場合はこのオブジェクトに設定してください。</param>
                public virtual void {{AppSrvOnStepChangingMethod}}({{STEP_CHANGING_EVENT_ARGS}}<{{param.CsClassName}}, {{StepEnumName}}, {{param.MessageDataCsClassName}}> e) {
                    // ステップの切り替え時に何らかの処理が必要な場合、このメソッドをオーバーライドして処理を記述してください。
                }
                """)}}
                /// <summary>
                /// {{_rootAggregate.Item.DisplayName}}処理実行
                /// </summary>
                /// <param name="param">処理パラメータ</param>
                /// <param name="result">処理結果。return result.XXXXX(); のような形で記述してください。</param>
                public virtual Task<{{CommandResult.RESULT_INTERFACE_NAME}}> {{AppSrvExecuteMethod}}({{param.CsClassName}} param, {{PresentationContext.INTERFACE_NAME}} result) {
                    throw new NotImplementedException({{MessageConst.CS_CLASS_NAME}}.{{MessageConst.C_INF0010}}("{{_rootAggregate.Item.DisplayName}}"));
                }
                """;
        }

        internal string RenderStepEnum() {
            var steps = _rootAggregate
                .GetMembers()
                .OfType<AggregateMember.RelationMember>()
                .Where(m => m.MemberAggregate.Item.Options.Step != null)
                .Select(m => new {
                    m.MemberAggregate.Item.PhysicalName,
                    m.MemberAggregate.Item.Options.Step!.Value,
                })
                .OrderBy(step => step.Value)
                .ToArray();
            if (steps.Length == 0) return SKIP_MARKER;

            return $$"""
                /// <summary>{{_rootAggregate.Item.DisplayName}}コマンドのUIのうち第何ステップかを表す列挙体</summary>
                public enum {{StepEnumName}} {
                {{steps.SelectTextTemplate(x => $$"""
                    {{x.PhysicalName}} = {{x.Value}},
                """)}}
                }
                """;
        }

        internal static SourceFile RenderStepChangeEventArgs() => new() {
            FileName = "CommandEventArgs.cs",
            RenderContent = ctx => {
                return $$"""
                    using System.Text.Json.Serialization;

                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// 複数のステップから成るコマンドにおいて、ステップの切り替え時に実行されるイベントの引数。
                    /// </summary>
                    public partial class {{STEP_CHANGING_EVENT_ARGS}}<TData, TStep, TErrors>
                        where TStep : struct, Enum
                        where TErrors : {{DisplayMessageContainer.INTERFACE}}, new() {

                    #pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
                        /// <summary>
                        /// データ。このオブジェクトの内容を編集するとクライアント側に反映されます。
                        /// </summary>
                        [JsonPropertyName("{{DATA_TS}}")]
                        public TData Data { get; set; }
                        /// <summary>
                        /// 切り替え前のステップ。nullの場合はコマンド画面初期表示時を表します。
                        /// </summary>
                        [JsonPropertyName("{{BEFORE_STEP_TS}}")]
                        public TStep? BeforeStep { get; init; }
                        /// <summary>
                        /// 切り替え後のステップ
                        /// </summary>
                        [JsonPropertyName("{{AFTER_STEP_TS}}")]
                        public TStep AfterStep { get; init; }
                        /// <summary>
                        /// 開いたことのあるステップの一覧
                        /// </summary>
                        [JsonPropertyName("{{VISITED_STEPS_TS}}")]
                        public List<TStep> VisitedSteps { get; set; }
                    #pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

                        /// <summary>
                        /// エラーメッセージなど。エラーが1件以上ある場合はステップの切り替えが中断されます。
                        /// </summary>
                        [JsonIgnore]
                        public TErrors Messages { get; } = new();

                        /// <summary>
                        /// 「～ですが処理実行してよいですか？」等の確認メッセージを追加します。
                        /// </summary>
                        public void AddConfirm(string message) => _confirms.Add(message);
                        public bool HasConfirm() => _confirms.Count > 0 || Messages.HasConfirm();
                        public IEnumerable<string> GetConfirms() {
                            if (_confirms.Count > 0) {
                                return _confirms;
                            } else if (Messages.HasConfirm()) {
                                return [{{MessageConst.CS_CLASS_NAME}}.{{MessageConst.C_INF0011}}()];
                            } else {
                                return [];
                            }
                        }
                        private readonly List<string> _confirms = new();
                    }
                    """;
            },
        };
    }
}
