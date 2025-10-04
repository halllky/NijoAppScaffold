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

            var response = new ApplicationState {
                ApplicationName = xDocument.Root?.Name.LocalName ?? "",
                XmlElementTrees = xDocument.Root?.Elements().Select(root => new ModelPageForm {
                    XmlElements = XmlElementItem.FromXElement(root).ToList(),
                }).ToList() ?? [],
                ValueMemberTypes = ValueMemberType.FromSchemaParseRule(rule),
                AttributeDefs = XmlElementAttribute.FromSchemaParseRule(rule),
            };

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

            // nijo.xmlの保存
            XmlHelper.SortXmlAttributes(xDocument);
            using (var writer = XmlWriter.Create(project.SchemaXmlPath, new XmlWriterSettings {
                Indent = true,
                Encoding = new UTF8Encoding(false, false),
                NewLineChars = "\n",
            })) {
                xDocument.Save(writer);
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
}

