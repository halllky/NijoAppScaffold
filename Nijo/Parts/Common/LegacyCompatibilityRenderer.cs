using Nijo.CodeGenerating;
using Nijo.Parts.CSharp;
using Nijo.Parts.JavaScript;

namespace Nijo.Parts.Common {
    /// <summary>
    /// 旧版互換モードで常に生成する固定ファイル群のレンダラー。
    /// </summary>
    internal static class LegacyCompatibilityRenderer {
        internal static void Render(CodeRenderingContext ctx) {
            ctx.Use<LegacyDefaultConfiguration>();
            LegacyCompatibilityCSharp.RenderCore(ctx);
            LegacyCompatibilityCSharp.RenderWebApi(ctx);
            LegacyCompatibilityJavaScript.RenderReact(ctx);
        }
    }
}
