using Nijo.CodeGenerating;
using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Nijo.SchemaParsing.BasicNodeOptions;

namespace Nijo.Models.ConstantModelModules {
    /// <summary>
    /// 定数値の定義
    /// </summary>
    internal class ConstantValueDef {
        internal ConstantValueDef(XElement element, string path, SchemaParseContext schemaParser) {
            _element = element;
            Path = path;
            _schemaParser = schemaParser;

            Name = element.Name.LocalName;
            Type = element.Attribute(ConstantType.AttributeName)?.Value ?? "string";
            Value = element.Attribute(ConstantValue.AttributeName)?.Value ?? string.Empty;
            DisplayName = element.Attribute(BasicNodeOptions.DisplayName.AttributeName)?.Value ?? Name;

            // テンプレート文字列の場合、{0}, {1}, ... から自動的に引数を判定
            if (Type == "template") {
                TemplateParams = ExtractTemplateParameters(Value);
            } else {
                TemplateParams = Array.Empty<string>();
            }

            // XMLコメントを取得
            XmlCommentLines = _schemaParser.GetCommentLines(element).ToArray();
        }

        private readonly XElement _element;
        private readonly SchemaParseContext _schemaParser;
        internal XElement Element => _element;

        internal string Name { get; }
        internal string Path { get; }
        internal string Type { get; }
        internal string Value { get; }
        internal string[] TemplateParams { get; }
        internal string DisplayName { get; }
        internal string[] XmlCommentLines { get; }

        /// <summary>
        /// C#用の定数名を取得（ドキュメントに合わせて元の名前をそのまま使用）
        /// </summary>
        internal string CsConstantName => Name;

        /// <summary>
        /// TypeScript用の定数名を取得（ドキュメントに合わせて元の名前をそのまま使用）
        /// </summary>
        internal string TsConstantName => Name;

        /// <summary>
        /// C#用の値を取得
        /// </summary>
        internal string GetCSharpValue() {
            return Type switch {
                "string" => $"\"{Value.Replace("\"", "\\\"")}\"",
                "int" => Value,
                "decimal" => $"{Value}m",
                "template" => $"\"{Value.Replace("\"", "\\\"")}\"", // テンプレート文字列も文字列として定義
                _ => $"\"{Value.Replace("\"", "\\\"")}\"",
            };
        }

        /// <summary>
        /// TypeScript用の値を取得
        /// </summary>
        internal string GetTypeScriptValue() {
            return Type switch {
                "string" => $"'{Value.Replace("'", "\\'")}'",
                "int" => Value,
                "decimal" => Value,
                "template" => $"'{Value.Replace("'", "\\'")}'", // テンプレート文字列も文字列として定義
                _ => $"'{Value.Replace("'", "\\'")}'",
            };
        }

        /// <summary>
        /// C#用のテンプレート関数を生成（ドキュメントに合わせて関数として生成）
        /// </summary>
        internal string RenderCSharpTemplateFunction() {
            if (Type != "template") return string.Empty;

            var parameters = string.Join(", ", TemplateParams.Select((p, i) => $"string arg{i}"));
            var formatString = EscapeCSharpString(Value);
            var formatArgs = string.Join(", ", TemplateParams.Select((_, i) => $"arg{i}"));

            return $$"""
                /// <summary>
                /// {{DisplayName}}
                {{XmlCommentLines.SelectTextTemplate(line => $$"""
                /// {{line}}
                """)}}
                /// </summary>
                public static string {{CsConstantName}}({{parameters}}) => $"{{formatString}}";
                """;
        }

        /// <summary>
        /// TypeScript用のテンプレート関数を生成（ドキュメントに合わせて関数として生成）
        /// </summary>
        internal string RenderTypeScriptTemplateFunction() {
            if (Type != "template") return string.Empty;

            var parameters = string.Join(", ", TemplateParams.Select((p, i) => $"arg{i}: string"));
            var templateString = EscapeTypeScriptTemplate(Value);

            return $$"""
                /**
                 * {{DisplayName}}
                {{XmlCommentLines.SelectTextTemplate(line => $$"""
                 * {{line}}
                """)}}
                 */
                {{TsConstantName}}: ({{parameters}}): string => `{{templateString}}`,
                """;
        }

        /// <summary>
        /// テンプレート文字列から引数を抽出
        /// </summary>
        private string[] ExtractTemplateParameters(string templateValue) {
            var matches = Regex.Matches(templateValue, @"\{(\d+)\}");
            var maxIndex = -1;

            foreach (Match match in matches) {
                if (int.TryParse(match.Groups[1].Value, out var index)) {
                    maxIndex = Math.Max(maxIndex, index);
                }
            }

            if (maxIndex == -1) return Array.Empty<string>();

            var parameters = new string[maxIndex + 1];
            for (int i = 0; i <= maxIndex; i++) {
                parameters[i] = $"arg{i}";
            }
            return parameters;
        }



        /// <summary>
        /// C#文字列をエスケープ（補間文字列対応）
        /// </summary>
        private string EscapeCSharpString(string value) {
            var result = value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");

            // プレースホルダーを {0} から {arg0} に変換
            for (int i = 0; i < TemplateParams.Length; i++) {
                result = result.Replace($"{{{i}}}", $"{{arg{i}}}");
            }

            // 残りの波括弧をエスケープ
            result = result
                .Replace("{", "{{")
                .Replace("}", "}}");

            // プレースホルダーを元に戻す
            for (int i = 0; i < TemplateParams.Length; i++) {
                result = result.Replace($"{{{{arg{i}}}}}", $"{{arg{i}}}");
            }

            return result;
        }

        /// <summary>
        /// TypeScript テンプレートリテラルをエスケープ
        /// </summary>
        private string EscapeTypeScriptTemplate(string value) {
            var result = value
                .Replace("\\", "\\\\")
                .Replace("`", "\\`")
                .Replace("$", "\\$");

            // {0}, {1}, ... を ${arg0}, ${arg1}, ... に変換
            for (int i = 0; i < TemplateParams.Length; i++) {
                result = result.Replace($"{{{i}}}", $"${{arg{i}}}");
            }

            return result;
        }
    }
}
