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
        internal ConstantDef(ConstantDefParser parser) {
            _parser = parser;
        }

        private readonly ConstantDefParser _parser;

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

            var xmlComment = ctx.SchemaParser.GetCommentLines(_parser.RootAggregateElement);

            return $$"""
                namespace {{ctx.Config.RootNamespace}};

                /// <summary>
                /// {{DisplayName}}
                {{xmlComment.SelectTextTemplate(line => $$"""
                /// {{line}}
                """)}}
                /// </summary>
                public static class {{CsClassName}} {
                {{rootConstants.Where(c => c.Type != "template").SelectTextTemplate(constant => $$"""
                    {{WithIndent(RenderCSharpConstant(constant, ctx), "    ")}}
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

        private string RenderCSharpNestedClass(ConstantGroupDef group, List<ConstantValueDef> allConstants, List<ConstantGroupDef> allGroups, CodeRenderingContext ctx) {
            // このグループ内の定数を生成
            var groupConstants = allConstants.Where(c => c.Path.StartsWith(group.Path + ".") &&
                                                        c.Path.Substring(group.Path.Length + 1).IndexOf('.') == -1).ToList();

            // 子グループを生成
            var childGroups = allGroups.Where(g => g.Path.StartsWith(group.Path + ".") &&
                                                   g.Path.Substring(group.Path.Length + 1).IndexOf('.') == -1).ToList();

            var xmlComment = ctx.SchemaParser.GetCommentLines(group.Element);

            return $$"""
                /// <summary>
                /// {{group.DisplayName}}
                {{xmlComment.SelectTextTemplate(line => $$"""
                /// {{line}}
                """)}}
                /// </summary>
                public static class {{group.Name}} {
                {{groupConstants.Where(c => c.Type != "template").SelectTextTemplate(constant => $$"""
                    {{WithIndent(RenderCSharpConstant(constant, ctx), "    ")}}
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

        private static string GetCSharpType(string constantType) {
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

            var xmlComment = ctx.SchemaParser.GetCommentLines(_parser.RootAggregateElement);

            var contents = $$"""
                /**
                 * {{DisplayName}}
                {{xmlComment.SelectTextTemplate(line => $$"""
                 * {{line}}
                """)}}
                 */
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

        private string RenderTypeScriptNestedObject(ConstantGroupDef group, List<ConstantValueDef> allConstants, List<ConstantGroupDef> allGroups, CodeRenderingContext ctx) {
            // このグループ内の定数を生成
            var groupConstants = allConstants.Where(c => c.Path.StartsWith(group.Path + ".") &&
                                                        c.Path.Substring(group.Path.Length + 1).IndexOf('.') == -1).ToList();

            // 子グループを生成
            var childGroups = allGroups.Where(g => g.Path.StartsWith(group.Path + ".") &&
                                                   g.Path.Substring(group.Path.Length + 1).IndexOf('.') == -1).ToList();

            var xmlComment = ctx.SchemaParser.GetCommentLines(group.Element);

            return $$"""
                /**
                 * {{group.DisplayName}}
                {{xmlComment.SelectTextTemplate(line => $$"""
                 * {{line}}
                """)}}
                 */
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
        private string RenderCSharpConstant(ConstantValueDef constant, CodeRenderingContext ctx) {
            var xmlComment = ctx.SchemaParser.GetCommentLines(constant.Element);

            return $$"""
                /// <summary>
                /// {{constant.DisplayName}}
                {{xmlComment.SelectTextTemplate(line => $$"""
                /// {{line}}
                """)}}
                /// </summary>
                public const {{GetCSharpType(constant.Type)}} {{constant.CsConstantName}} = {{constant.GetCSharpValue()}};
                """;
        }

        /// <summary>
        /// TypeScriptの定数を生成（JSDoc対応）
        /// </summary>
        private string RenderTypeScriptConstant(ConstantValueDef constant, CodeRenderingContext ctx) {
            var xmlComment = ctx.SchemaParser.GetCommentLines(constant.Element);

            return $$"""
                /**
                 * {{constant.DisplayName}}
                {{xmlComment.SelectTextTemplate(line => $$"""
                 * {{line}}
                """)}}
                 */
                {{constant.TsConstantName}}: {{constant.GetTypeScriptValue()}},
                """;
        }
    }
}
