using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.Models.ConstantModelModules {
    /// <summary>
    /// 定数定義
    /// </summary>
    internal class ConstantDef {
        internal ConstantDef(ConstantDefParser parser, RootAggregate rootAggregate) {
            _parser = parser;
            _rootAggregate = rootAggregate;
        }

        private readonly ConstantDefParser _parser;
        private readonly RootAggregate _rootAggregate;

        internal string DisplayName => _parser.DisplayName;
        internal string CsClassName => _parser.CsClassName;
        internal string TsConstantsName => _parser.TsConstantsName;

        /// <summary>
        /// C#の定数クラスを生成
        /// </summary>
        internal string RenderCSharp(CodeRenderingContext ctx) {
            var constants = _parser.GetConstants().ToList();
            var groups = _parser.GetConstantGroups().ToList();

            // ルートレベルの定数を生成
            var rootConstants = constants.Where(c => !c.Path.Contains('.')).ToList();
            var rootGroups = groups.Where(g => !g.Path.Contains('.')).ToList();

            var xmlComment = ctx.SchemaParser.GetComment(_parser.Element, E_CsTs.CSharp);
            var commentLines = new List<string>();
            commentLines.Add("/// <summary>");
            commentLines.Add($"/// {DisplayName}");
            if (!string.IsNullOrEmpty(xmlComment)) {
                foreach (var line in xmlComment.Split(new[] { "\\r\\n" }, StringSplitOptions.None)) {
                    commentLines.Add($"/// {line.Trim()}");
                }
            }
            commentLines.Add("/// </summary>");
            var classComment = string.Join("\n", commentLines);

            return $$"""
                namespace {{ctx.Config.RootNamespace}};

                {{classComment}}
                public static class {{CsClassName}} {
                {{rootConstants.Where(c => c.Type != "template").SelectTextTemplate(constant => $$"""
                    {{WithIndent(RenderCSharpConstant(constant), "    ")}}
                """)}}
                {{rootConstants.Where(c => c.Type == "template").SelectTextTemplate(constant => $$"""
                    {{WithIndent(constant.RenderCSharpTemplateFunction(), "    ")}}
                """)}}
                {{rootGroups.SelectTextTemplate(group => $$"""
                    {{WithIndent(RenderCSharpNestedClass(group, constants, groups, ctx), "    ")}}
                """)}}
                }
                """;
        }

        private IEnumerable<string> RenderCSharpNestedClass(ConstantGroupDef group, List<ConstantValueDef> allConstants, List<ConstantGroupDef> allGroups, CodeRenderingContext ctx) {
            // このグループ内の定数を生成
            var groupConstants = allConstants.Where(c => c.Path.StartsWith(group.Path + ".") &&
                                                        c.Path.Substring(group.Path.Length + 1).IndexOf('.') == -1).ToList();

            // 子グループを生成
            var childGroups = allGroups.Where(g => g.Path.StartsWith(group.Path + ".") &&
                                                   g.Path.Substring(group.Path.Length + 1).IndexOf('.') == -1).ToList();

            var xmlComment = ctx.SchemaParser.GetComment(group.Element, E_CsTs.CSharp);
            yield return "/// <summary>";
            yield return $"/// {group.DisplayName}";
            if (!string.IsNullOrEmpty(xmlComment)) {
                foreach (var line in xmlComment.Split(new[] { "\\r\\n" }, StringSplitOptions.None)) {
                    yield return $"/// {line.Trim()}";
                }
            }
            yield return "/// </summary>";

            yield return $$"""
                public static class {{group.Name}} {
                {{groupConstants.Where(c => c.Type != "template").SelectTextTemplate(constant => $$"""
                    {{WithIndent(RenderCSharpConstant(constant), "    ")}}
                """)}}
                {{groupConstants.Where(c => c.Type == "template").SelectTextTemplate(constant => $$"""
                    {{WithIndent(constant.RenderCSharpTemplateFunction(), "    ")}}
                """)}}
                {{childGroups.SelectTextTemplate(childGroup => $$"""
                    {{WithIndent(RenderCSharpNestedClass(childGroup, allConstants, allGroups, ctx), "    ")}}
                """)}}
                }
                """;
        }

        private string GetCSharpType(string constantType) {
            return constantType switch {
                "int" => "int",
                "decimal" => "decimal",
                _ => "string",
            };
        }

        /// <summary>
        /// TypeScriptの定数定義を生成
        /// </summary>
        internal SourceFile RenderTypeScript(CodeRenderingContext ctx) {
            var constants = _parser.GetConstants().ToList();
            var groups = _parser.GetConstantGroups().ToList();

            // ルートレベルの定数を生成
            var rootConstants = constants.Where(c => !c.Path.Contains('.')).ToList();
            var rootGroups = groups.Where(g => !g.Path.Contains('.')).ToList();

            var xmlComment = ctx.SchemaParser.GetComment(_parser.Element, E_CsTs.TypeScript);
            var commentLines = new List<string>();
            commentLines.Add("/**");
            commentLines.Add($" * {DisplayName}");
            if (!string.IsNullOrEmpty(xmlComment)) {
                foreach (var line in xmlComment.Split(new[] { "\\n" }, StringSplitOptions.None)) {
                    commentLines.Add($" * {line.Trim()}");
                }
            }
            commentLines.Add(" */");
            var classComment = string.Join("\n", commentLines);

            var contents = $$"""
                {{classComment}}
                export const {{TsConstantsName}} = {
                {{rootConstants.Where(c => c.Type != "template").SelectTextTemplate(constant => $$"""
                  {{WithIndent(RenderTypeScriptConstant(constant, ctx), "  ")}}
                """)}}
                {{rootConstants.Where(c => c.Type == "template").SelectTextTemplate(constant => $$"""
                  {{WithIndent(constant.RenderTypeScriptTemplateFunction(), "  ")}}
                """)}}
                {{rootGroups.SelectTextTemplate(group => $$"""
                  {{WithIndent(RenderTypeScriptNestedObject(group, constants, groups, ctx), "  ")}}
                """)}}
                } as const
                """;

            return new SourceFile {
                FileName = $"{TsConstantsName}.ts",
                Contents = contents
            };
        }

        private IEnumerable<string> RenderTypeScriptNestedObject(ConstantGroupDef group, List<ConstantValueDef> allConstants, List<ConstantGroupDef> allGroups, CodeRenderingContext ctx) {
            // このグループ内の定数を生成
            var groupConstants = allConstants.Where(c => c.Path.StartsWith(group.Path + ".") &&
                                                        c.Path.Substring(group.Path.Length + 1).IndexOf('.') == -1).ToList();

            // 子グループを生成
            var childGroups = allGroups.Where(g => g.Path.StartsWith(group.Path + ".") &&
                                                   g.Path.Substring(group.Path.Length + 1).IndexOf('.') == -1).ToList();

            var xmlComment = ctx.SchemaParser.GetComment(group.Element, E_CsTs.TypeScript);
            yield return $"/** {group.DisplayName}";
            if (!string.IsNullOrEmpty(xmlComment)) {
                foreach (var line in xmlComment.Split(new[] { "\\n" }, StringSplitOptions.None)) {
                    yield return $" * {line.Trim()}";
                }
            }
            yield return " */";

            yield return $$"""
                {{WithIndent(group.Name, "")}}: {
                {{groupConstants.Where(c => c.Type != "template").SelectTextTemplate(constant => $$"""
                  {{WithIndent(RenderTypeScriptConstant(constant, ctx), "  ")}}
                """)}}
                {{groupConstants.Where(c => c.Type == "template").SelectTextTemplate(constant => $$"""
                  {{WithIndent(constant.RenderTypeScriptTemplateFunction(), "  ")}}
                """)}}
                {{childGroups.SelectTextTemplate(childGroup => $$"""
                  {{WithIndent(RenderTypeScriptNestedObject(childGroup, allConstants, allGroups, ctx), "  ")}}
                """)}}
                },
                """;
        }

        /// <summary>
        /// C#の定数を生成（XMLコメント対応）
        /// </summary>
        private IEnumerable<string> RenderCSharpConstant(ConstantValueDef constant) {
            yield return "/// <summary>";
            yield return $"/// {constant.DisplayName}";
            if (!string.IsNullOrEmpty(constant.XmlComment)) {
                foreach (var line in constant.XmlComment.Split('\n')) {
                    yield return $"/// {line.Trim()}";
                }
            }
            yield return "/// </summary>";

            yield return $$"""
                public const {{GetCSharpType(constant.Type)}} {{constant.CsConstantName}} = {{constant.GetCSharpValue()}};
                """;
        }

        /// <summary>
        /// TypeScriptの定数を生成（JSDoc対応）
        /// </summary>
        private IEnumerable<string> RenderTypeScriptConstant(ConstantValueDef constant, CodeRenderingContext ctx) {
            var xmlComment = ctx.SchemaParser.GetComment(constant.Element, E_CsTs.TypeScript);
            yield return $"/** {constant.DisplayName}";
            if (!string.IsNullOrEmpty(xmlComment)) {
                foreach (var line in xmlComment.Split('\n')) {
                    yield return $" * {line.Trim()}";
                }
            }
            yield return " */";

            yield return $$"""
                {{constant.TsConstantName}}: {{constant.GetTypeScriptValue()}},
                """;
        }
    }
}
