using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.WebServer;

internal class SavingUploadedFilesFilter {
    internal static SourceFile Render() {
        return new SourceFile {
            FileName = "SavingUploadedFilesFilter.cs",
            RenderContent = ctx => $$"""
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace {{ctx.Config.RootNamespace}} {
    /// <summary>
    /// クライアント側からアップロードされたファイルをサーバー側のストレージに保存します。
    /// </summary>
    public class SavingUploadedFilesFilter : IAsyncActionFilter {
        public SavingUploadedFilesFilter(IFileAttachmentRepository attachmentRepository) {
            _attachmentRepository = attachmentRepository;
        }
        private readonly IFileAttachmentRepository _attachmentRepository;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {

            // ファイルを添付することができるContent-Typeの場合
            if (context.HttpContext.Request.HasFormContentType) {

                foreach (var file in context.HttpContext.Request.Form.Files) {

                    // 入力エラーチェック
                    if (string.IsNullOrWhiteSpace(file.Name)) {
                        context.Result = new BadRequestObjectResult($"ファイル '{file.FileName}' のName属性が指定されていません。");
                        return;
                    }

                    var errors = new List<string>();
                    var id = new FileAttachmentId(file.Name);
                    using var stream = file.OpenReadStream();
                    await _attachmentRepository.SaveFileAsync(id, file.FileName, stream, errors);

                    if (errors.Count > 0) {
                        context.Result = new BadRequestObjectResult(string.Join(Environment.NewLine, errors));
                        return;
                    }
                }
            }

            await next();

        }
    }
}
""",
        };
    }
}
