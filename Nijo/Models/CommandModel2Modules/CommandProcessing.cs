using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;
using Nijo.Parts.CSharp;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Linq;

namespace Nijo.Models.CommandModel2Modules {
    /// <summary>
    /// 旧版互換 CommandModel2 の controller / application service 生成。
    /// まずは init-value と execute の C# 枠を復旧する。
    /// </summary>
    internal class CommandProcessing {
        internal CommandProcessing(RootAggregate rootAggregate) {
            _rootAggregate = rootAggregate;
        }
        private readonly RootAggregate _rootAggregate;

        private CommandParameter Parameter => new(_rootAggregate);

        private string AppSrvInitValueGetMethod => $"Get{_rootAggregate.PhysicalName}InitValue";
        private string AppSrvExecuteMethod => $"Execute{_rootAggregate.PhysicalName}";
        private string StepEnumName => $"E_{_rootAggregate.PhysicalName}Steps";
        internal string HookName => $"use{_rootAggregate.PhysicalName}Launcher";

        private const string CONTROLLER_GET_INIT_VALUE = "get-init-value";
        private string LegacyEndpointPath => $"/api/command/{GetLegacyCommandUniqueId()}";

        internal string RenderHook(CodeRenderingContext ctx) {
            var parameter = Parameter;

            return $$"""
                /** {{_rootAggregate.DisplayName}}処理を呼び出す関数を返します。 */
                export const {{HookName}} = () => {
                  const { post, complexPost } = Util.useHttpRequest()
                  const getInitValue = useEvent(async () => {
                    return await post<{{parameter.TsTypeName}}>(`{{LegacyEndpointPath}}/{{CONTROLLER_GET_INIT_VALUE}}`)
                  })
                  const [nowProccessing, setNowProccessing] = React.useState(false)
                  const launch = useEvent(async <TReturnValue = unknown>(
                    /** パラメータ */
                    param: {{parameter.TsTypeName}},
                    /** オプション。handleDetailErrorだけは必須。 */
                    options: Omit<Util.ComplexPostOptions, 'handleDetailError'> & { handleDetailError: Exclude<Util.ComplexPostOptions['handleDetailError'], undefined> }
                  ): Promise<Util.ComplexPostResponse<TReturnValue>> => {
                    if (nowProccessing) return { ok: false, data: undefined }
                    setNowProccessing(true)
                    try {
                      return await complexPost(`{{LegacyEndpointPath}}`, param, options)
                    } finally {
                      setNowProccessing(false)
                    }
                  })

                  return {
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

        internal string RenderControllerAction(CodeRenderingContext ctx) {
            var parameter = Parameter;

            return $$"""
                /// <summary>
                /// {{_rootAggregate.DisplayName}}の初期値取得イベント
                /// </summary>
                [HttpPost("{{GetLegacyCommandUniqueId()}}/{{CONTROLLER_GET_INIT_VALUE}}")]
                public virtual IActionResult GetInitValueOf{{_rootAggregate.PhysicalName}}() {
                    if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{_rootAggregate.PhysicalName}}) < E_AuthLevel.Write_RESTRICT) return Forbid();
                    var param = _applicationService.{{AppSrvInitValueGetMethod}}();
                    _applicationService.Log.Debug("Get Init Value {{_rootAggregate.PhysicalName}}: {0}", param.ToJson());
                    return this.JsonContent(param);
                }
                /// <summary>
                /// {{_rootAggregate.DisplayName}}処理をWebAPI経由で実行するためのエンドポイント
                /// </summary>
                [HttpPost("{{GetLegacyCommandUniqueId()}}")]
                public virtual async Task<IActionResult> {{_rootAggregate.PhysicalName}}(ComplexPostRequest<{{parameter.CsClassName}}> param) {
                    _applicationService.Log.Debug("Execute {{_rootAggregate.PhysicalName}}: {0}", param.Data.ToJson());
                    if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{_rootAggregate.PhysicalName}}) < E_AuthLevel.Write_RESTRICT) return Forbid();
                    var presentationContext = new PresentationContext(
                        new {{parameter.MessageDataCsClassName}}(),
                        new() { IgnoreConfirm = param.IgnoreConfirm },
                        _applicationService);
                    await _applicationService.{{AppSrvExecuteMethod}}(param.Data, presentationContext);
                    var result = presentationContext.GetResult().ToJsonObject();
                    return this.JsonContent(result);
                }
                """;
        }

        internal string RenderAbstractMethod(CodeRenderingContext ctx) {
            var parameter = Parameter;

            return $$"""
                /// <summary>
                /// {{_rootAggregate.DisplayName}}の初期値取得処理
                /// </summary>
                public virtual {{parameter.CsClassName}} {{AppSrvInitValueGetMethod}}() {
                    return new();
                }
                /// <summary>
                /// {{_rootAggregate.DisplayName}}処理実行
                /// </summary>
                /// <param name="param">処理パラメータ</param>
                /// <param name="result">処理結果。return result.XXXXX(); のような形で記述してください。</param>
                public virtual Task<ICommandResult> {{AppSrvExecuteMethod}}({{parameter.CsClassName}} param, IPresentationContext result) {
                    throw new NotImplementedException(MSG.ERRC0024("{{_rootAggregate.DisplayName}}"));
                }
                """;
        }

        internal string RenderStepEnum() {
            var steps = _rootAggregate
                .GetMembers()
                .OfType<ChildAggregate>()
                .Select(child => new {
                    child.PhysicalName,
                    Step = child.XElement.Attribute(CommandModel2.STEP_ATTRIBUTE_NAME)?.Value,
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Step))
                .Select(x => new {
                    x.PhysicalName,
                    Value = int.Parse(x.Step!),
                })
                .OrderBy(x => x.Value)
                .ToArray();
            if (steps.Length == 0) return SKIP_MARKER;

            return $$"""
                /// <summary>{{_rootAggregate.DisplayName}}コマンドのUIのうち第何ステップかを表す列挙体</summary>
                public enum {{StepEnumName}} {
                {{steps.SelectTextTemplate(step => $$"""
                    {{step.PhysicalName}} = {{step.Value}},
                """)}}
                }
                """;
        }

        private string GetLegacyCommandUniqueId() {
            return _rootAggregate.XElement.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)?.Value
                ?? $"/{_rootAggregate.PhysicalName}".ToHashedString();
        }
    }
}
