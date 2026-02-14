using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nijo.CodeGenerating;
using Nijo.SchemaParsing;
using Nijo.WebService.Common;

namespace Nijo.WebService.SchemaEditor;

/// <summary>
/// スキーマ編集関連のエンドポイントハンドラ
/// </summary>
internal class SchemaEndpointHandlers {

    internal SchemaEndpointHandlers() {
    }

    /// <summary>
    /// 画面初期表示時データ読み込み処理
    /// </summary>
    internal async Task HandleLoadSchema(HttpContext context) {
        try {
            var project = await ProjectHelper.GetProjectAndSetResponseIfErrorAsync(context);
            if (project == null) {
                return;
            }

            var xDocument = XDocument.Load(project.SchemaXmlPath);
            var rule = SchemaParseRule.Default();

            var projectOptions = GeneratedProjectOptions.Parse(xDocument, false);

            var applicationState = new ApplicationState {
                ApplicationName = xDocument.Root?.Name.LocalName ?? "",
                XmlElementTrees = SchemaParseContext.GetAllSectionNames()
                    .Where(sectionName => sectionName != SchemaParseContext.SECTION_CUSTOM_ATTRIBUTES)
                    .Select(sectionName => xDocument.Root?.Element(sectionName))
                    .Where(section => section != null)
                    .SelectMany(section => section!.Elements())
                    .Select(root => new ModelPageForm {
                        XmlElements = XmlElementItem.FromXElement(root).ToList(),
                    }).ToList() ?? [],
                ValueMemberTypes = ValueMemberType.FromSchemaParseRule(rule),
                AttributeDefs = XmlElementAttribute.FromSchemaParseRule(rule),
                CustomAttributes = NijoXmlCustomAttribute.FromXDocument(xDocument).ToList(),
                ProjectOptions = projectOptions.GetCurrentValues(),
                ProjectOptionPropertyInfos = GeneratedProjectOptions.GetPropertyInfos().ToList(),
            };

            // nijo.viewState.jsonの読み込み
            ApplicationStateAndSchemaGraphViewState.SchemaGraphViewStateTypeByViewMode? schemaGraphViewState = null;
            var viewStatePath = project.ViewStateJsonPath;
            if (File.Exists(viewStatePath)) {
                try {
                    var viewStateJson = await File.ReadAllTextAsync(viewStatePath, context.RequestAborted);
                    schemaGraphViewState = JsonSerializer.Deserialize<ApplicationStateAndSchemaGraphViewState.SchemaGraphViewStateTypeByViewMode>(viewStateJson);
                } catch (Exception) {
                    // ファイル読み込みやデシリアライズに失敗した場合はnullのまま
                }
            }

            var response = new ApplicationStateAndSchemaGraphViewState {
                ApplicationState = applicationState,
                SchemaGraphViewState = schemaGraphViewState,
            };

            context.Response.StatusCode = StatusCodes.Status200OK;
            await HttpResponseHelper.WriteJsonResponseAsync(context, response, cancellationToken: context.RequestAborted);
        } catch (Exception ex) {
            await HttpResponseHelper.WriteErrorResponseAsync(
                context,
                (int)HttpStatusCode.InternalServerError,
                ex.Message,
                context.RequestAborted);
        }
    }

    /// <summary>
    /// 編集中のバリデーション
    /// </summary>
    internal async Task HandleValidateSchema(HttpContext context) {
        try {
            var project = await ProjectHelper.GetProjectAndSetResponseIfErrorAsync(context);
            if (project == null) {
                return;
            }

            var originalXDocument = XDocument.Load(project.SchemaXmlPath);
            var applicationState = await context.Request.ReadFromJsonAsync<ApplicationState>(context.RequestAborted)
                ?? throw new Exception("applicationState is null");

            // XMLとして正しいか検証
            if (!SchemaValidationService.TryValidateAsXml(
                applicationState, originalXDocument, out var xDocument, out var uuidToXmlElement, out var xmlErrors)) {
                context.Response.StatusCode = (int)HttpStatusCode.Accepted;
                await HttpResponseHelper.WriteJsonResponseAsync(context, xmlErrors, cancellationToken: context.RequestAborted);
                return;
            }

            // スキーマ定義として正しいか検証
            var rule = SchemaParseRule.Default();
            if (!SchemaValidationService.TryValidateAsSchema(xDocument!, rule, out var schemaErrors)) {
                var reactErrorObject = SchemaValidationService.ConvertErrorsToReactFormat(schemaErrors, uuidToXmlElement!);
                context.Response.StatusCode = (int)HttpStatusCode.Accepted;
                await HttpResponseHelper.WriteJsonResponseAsync(context, reactErrorObject, cancellationToken: context.RequestAborted);
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;

        } catch (Exception ex) {
            await HttpResponseHelper.WriteErrorResponseAsync(
                context,
                (int)HttpStatusCode.InternalServerError,
                ex.Message,
                context.RequestAborted);
        }
    }

    /// <summary>
    /// nijo.xmlの保存
    /// </summary>
    internal async Task HandleSaveSchema(HttpContext context) {
        try {
            var project = await ProjectHelper.GetProjectAndSetResponseIfErrorAsync(context);
            if (project == null) {
                return;
            }

            var applicationStateAndSchemaGraphViewState = await context.Request.ReadFromJsonAsync<ApplicationStateAndSchemaGraphViewState>(context.RequestAborted)
                ?? throw new Exception("applicationStateAndSchemaGraphViewState is null");

            var applicationState = applicationStateAndSchemaGraphViewState.ApplicationState;
            var schemaGraphViewState = applicationStateAndSchemaGraphViewState.SchemaGraphViewState;

            // XMLとして正しいか検証（スキーマ定義としてのエラーは見ない。作業中の一時保存のケースがあるため）
            var originalXDocument = XDocument.Load(project.SchemaXmlPath);
            if (!SchemaValidationService.TryValidateAsXml(applicationState, originalXDocument, out var xDocument, out var _, out var errors)) {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseHelper.WriteJsonResponseAsync(context, errors, cancellationToken: context.RequestAborted);
                return;
            }

            // プロジェクト設定をXMLルート要素の属性として保存
            if (applicationState.ProjectOptions != null) {
                var defaultOptions = GeneratedProjectOptions.Parse(null, true);
                var defaultValues = defaultOptions.GetCurrentValues();

                // クライアント側から送られてきたキーを起点に、XMLへの反映を行う。
                // デフォルト値と同じ、またはnullの場合は属性を削除する。
                foreach (var (key, value) in applicationState.ProjectOptions) {
                    var kind = value?.GetValueKind();
                    var defaultValue = defaultValues.ContainsKey(key) ? defaultValues[key] : null;

                    // 空か否か
                    var isEmpty = value == null
                        || kind == JsonValueKind.String
                        && string.IsNullOrWhiteSpace(value.GetValue<string>());

                    // デフォルト値と同じか否か
                    var isDefault = false;
                    if (value == null && defaultValue == null) {
                        isDefault = true;
                    } else if (value != null && defaultValue != null) {
                        var valueKind = value.GetValueKind();
                        var defaultKind = defaultValue.GetValueKind();
                        if (valueKind == defaultKind) {
                            isDefault = valueKind switch {
                                JsonValueKind.String => value.GetValue<string>() == defaultValue.GetValue<string>(),
                                JsonValueKind.Number => value.GetValue<decimal>() == defaultValue.GetValue<decimal>(),
                                JsonValueKind.True or JsonValueKind.False => value.GetValue<bool>() == defaultValue.GetValue<bool>(),
                                JsonValueKind.Null => true,
                                _ => false
                            };
                        }
                    }

                    if (isEmpty || isDefault) {
                        xDocument.Root?.Attribute(key)?.Remove();
                    } else {
                        xDocument.Root?.SetAttributeValue(key, value?.ToString());
                    }
                }
            }

            // nijo.xmlの保存
            XmlHelper.SortXmlAttributes(xDocument);
            using (var writer = XmlWriter.Create(project.SchemaXmlPath, new XmlWriterSettings {
                Indent = true,
                NewLineOnAttributes = true,
                Encoding = new UTF8Encoding(false, false),
                NewLineChars = "\n",
            })) {
                xDocument.Save(writer);
            }

            // ファイル末尾に改行を追加（VSCodeで保存したときの設定にあわせる。
            // Gitでファイル末尾の改行が都度差分になってしまうのを避けるため）
            var xmlContent = await File.ReadAllTextAsync(project.SchemaXmlPath, context.RequestAborted);
            if (!xmlContent.EndsWith("\n")) {
                await File.WriteAllTextAsync(project.SchemaXmlPath, xmlContent + "\n", new UTF8Encoding(false, false), context.RequestAborted);
            }

            // SchemaGraphViewStateの保存（nullでない場合のみ）
            if (schemaGraphViewState != null) {
                var viewStatePath = project.ViewStateJsonPath;
                var jsonOptions = new JsonSerializerOptions {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    Converters = {
                        new ApplicationStateAndSchemaGraphViewState.SortedJsonConverter(),
                        new ApplicationStateAndSchemaGraphViewState.SortedJsonArrayConverter()
                    }
                };
                var jsonString = JsonSerializer.Serialize(schemaGraphViewState, jsonOptions);
                await File.WriteAllTextAsync(viewStatePath, jsonString, new UTF8Encoding(false, false), context.RequestAborted);
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;

        } catch (Exception ex) {
            await HttpResponseHelper.WriteErrorResponseAsync(
                context,
                (int)HttpStatusCode.BadRequest,
                ex.Message,
                context.RequestAborted);
        }
    }

    /// <summary>
    /// コード自動生成
    /// </summary>
    internal async Task HandleGenerateCode(HttpContext context) {
        try {
            var project = await ProjectHelper.GetProjectAndSetResponseIfErrorAsync(context);
            if (project == null) {
                return;
            }

            var xDocumentToSave = XDocument.Load(project.SchemaXmlPath);
            var rule = SchemaParseRule.Default();

            // バリデーション (validate相当)
            if (!SchemaValidationService.TryValidateAsSchema(xDocumentToSave, rule, out var errors)) {
                var reactErrorObject = SchemaValidationService.ConvertErrorsToReactFormat(errors, new Dictionary<XElement, string>());
                context.Response.StatusCode = (int)HttpStatusCode.Accepted;
                await HttpResponseHelper.WriteJsonResponseAsync(context, reactErrorObject, cancellationToken: context.RequestAborted);
                return;
            }

            // コード生成処理 (エラーがなければ実行)
            var generationParseContext = new SchemaParseContext(xDocumentToSave, rule);
            var renderingOptions = new CodeRenderingOptions { AllowNotImplemented = false };

            var logger = context.RequestServices.GetRequiredService<ILogger<NijoWebServiceBuilder>>();
            if (project.GenerateCode(generationParseContext, renderingOptions, logger)) {
                context.Response.StatusCode = StatusCodes.Status200OK;
                await HttpResponseHelper.WriteJsonResponseAsync(
                    context,
                    "Code generation successful.",
                    cancellationToken: context.RequestAborted);
            } else {
                await HttpResponseHelper.WriteErrorResponseAsync(
                    context,
                    (int)HttpStatusCode.InternalServerError,
                    "Code generation failed. Check server logs for details.",
                    context.RequestAborted);
            }

        } catch (Exception ex) {
            await HttpResponseHelper.WriteErrorResponseAsync(
                context,
                (int)HttpStatusCode.InternalServerError,
                ex.Message,
                context.RequestAborted);
        }
    }

    /// <summary>
    /// XML要素の種類の候補リストを返す
    /// </summary>
    internal async Task HandleGetNodeTypes(HttpContext context) {
        try {
            var project = await ProjectHelper.GetProjectAndSetResponseIfErrorAsync(context);
            if (project == null) {
                return;
            }

            var keyword = context.Request.Query["keyword"].ToString().ToLowerInvariant();

            var candidates = new List<KeyValuePair<string, string>>();

            // 1. 固定のキーワード
            candidates.Add(KeyValuePair.Create(SchemaParseContext.NODE_TYPE_CHILD, "子要素"));
            candidates.Add(KeyValuePair.Create(SchemaParseContext.NODE_TYPE_CHILDREN, "子配列(リスト)"));

            // 2. プリミティブ型
            var rule = SchemaParseRule.Default();
            foreach (var vm in rule.ValueMemberTypes) {
                candidates.Add(KeyValuePair.Create(vm.SchemaTypeName, vm.DisplayName));
            }

            // 3. 参照 (ref-to)
            var xDocument = XDocument.Load(project.SchemaXmlPath);
            var applicationState = await context.Request.ReadFromJsonAsync<ApplicationState>(context.RequestAborted);
            if (applicationState != null) {
                SchemaValidationService.TryValidateAsXml(applicationState, xDocument, out var editingXDocument, out _, out _);
                if (editingXDocument != null) {
                    xDocument = editingXDocument;
                }
            }

            var dataStructures = xDocument.Root?.Element(SchemaParseContext.SECTION_DATA_STRUCTURES);
            if (dataStructures != null) {
                foreach (var element in dataStructures.Elements()) {
                    var name = element.Name.LocalName;
                    var value = $"{SchemaParseContext.NODE_TYPE_REFTO}:{name}";
                    candidates.Add(KeyValuePair.Create(value, value));
                }
            }

            var result = candidates
                .Where(c => string.IsNullOrEmpty(keyword) || c.Key.ToLowerInvariant().Contains(keyword))
                .Distinct()
                .OrderBy(c => c.Key)
                .Select(c => new { value = c.Key, text = c.Value })
                .ToList();

            context.Response.StatusCode = StatusCodes.Status200OK;
            await HttpResponseHelper.WriteJsonResponseAsync(context, result, cancellationToken: context.RequestAborted);

        } catch (Exception ex) {
            await HttpResponseHelper.WriteErrorResponseAsync(
                context,
                (int)HttpStatusCode.InternalServerError,
                ex.Message,
                context.RequestAborted);
        }
    }
}

