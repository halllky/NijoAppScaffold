using Nijo.CodeGenerating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Nijo.Parts.Common {
    /// <summary>
    /// 旧版 MessageConst 機能の互換レンダラー。
    /// </summary>
    internal static partial class LegacyMessageConst {
        private const string MESSAGE_XML_FILE_NAME = "nijo.メッセージ一覧.xml";

        internal static void Render(CodeRenderingContext ctx) {
            if (!ctx.IsLegacyCompatibilityMode()) return;
            if (!ctx.Config.ForceLegacyCompatibilityMode) return;

            var messagesXmlPath = Path.Combine(ctx.Project.ProjectRoot, MESSAGE_XML_FILE_NAME);
            if (!File.Exists(messagesXmlPath)) return;

            var messages = LoadMessages(messagesXmlPath).ToArray();

            ctx.CoreLibrary(dir => {
                dir.Directory("Util", utilDir => {
                    utilDir.Generate(new SourceFile {
                        FileName = "MSG.cs",
                        Contents = $$"""
                            namespace {{ctx.Config.RootNamespace}};

                            /// <summary>
                            /// メッセージマスタ（メッセージ定数）。
                            /// 画面上に表示するメッセージ等はこの関数から取得してください。
                            /// </summary>
                            public static partial class MSG {
                            {{messages.SelectTextTemplate(msg => $$"""
                                /// <summary>
                                /// {{msg.Template}}
                                /// </summary>
                            {{msg.Parameters.SelectTextTemplate((description, i) => $$"""
                                /// <param name="arg{{i}}">{{description}}</param>
                            """)}}
                                public static string {{msg.Id}}({{string.Join(", ", msg.Parameters.Select((_, i) => $"string arg{i}"))}}) => $"{{GetTemplateLiteral(msg, E_CsTs.CSharp)}}";
                            """)}}
                            }
                            """,
                    });
                });
            });

            ctx.ReactProject(dir => {
                dir.Directory("util", utilDir => {
                    utilDir.Generate(new SourceFile {
                        FileName = "MSG.ts",
                        Contents = $$"""
                            /**
                             * メッセージマスタ（メッセージ定数）。
                             * 画面上に表示するメッセージ等はこの関数から取得してください。
                             */
                            export const MSG = {
                            {{messages.SelectTextTemplate(msg => $$"""
                              /**
                               * {{msg.Template}}
                            {{msg.Parameters.SelectTextTemplate((description, i) => $$"""
                               * @param arg{{i}} {{description}}
                            """)}}
                               */
                              {{msg.Id}}: ({{string.Join(", ", msg.Parameters.Select((_, i) => $"arg{i}: string"))}}) => `{{GetTemplateLiteral(msg, E_CsTs.TypeScript)}}`,
                            """)}}
                            }
                            """,
                    });
                });
            });
        }

        private static IEnumerable<Message> LoadMessages(string messagesXmlPath) {
            var messages = new List<Message>();

            var xDocuments = GetXDocumentsRecursively(messagesXmlPath).ToList();
            foreach (var el in xDocuments[0].XDocument.Root?.Elements() ?? []) {
                if ((string?)el.FirstAttribute != "message-list") continue;

                foreach (var element in el.Elements()) {
                    var parameters = element.Attributes().Select(attr => attr.Value).ToArray();

                    string template;
                    var isMultiLine = Regex.Match(element.Value, @"^\n([ ]+).*\n[ ]+$", RegexOptions.Singleline);
                    if (!isMultiLine.Success) {
                        template = element.Value;
                    } else {
                        var indent = isMultiLine.Groups[1].Value;
                        var builder = new StringBuilder();
                        foreach (var line in element.Value.Trim().Split('\n')) {
                            builder.Append(line.StartsWith(indent)
                                ? line[indent.Length..]
                                : line);
                            builder.Append("\\r\\n");
                        }
                        template = builder.ToString();
                    }

                    messages.Add(new Message {
                        Id = element.Name.LocalName,
                        Template = template,
                        Parameters = parameters,
                    });
                }
            }

            return messages;

            static IEnumerable<XDocumentAndPath> GetXDocumentsRecursively(string xmlFilePath) {
                var xDocument = XDocument.Load(xmlFilePath);
                yield return new XDocumentAndPath { XDocument = xDocument, FilePath = xmlFilePath };

                foreach (var el in xDocument.Root?.Elements() ?? []) {
                    if (el.Name.LocalName != "Include") continue;

                    var path = el.Attribute("Path")?.Value;
                    if (string.IsNullOrWhiteSpace(path)) continue;

                    var dirName = Path.GetDirectoryName(xmlFilePath);
                    var absolutePath = dirName == null
                        ? path
                        : Path.GetFullPath(Path.Combine(dirName, path));
                    foreach (var includedXDocument in GetXDocumentsRecursively(absolutePath)) {
                        yield return includedXDocument;
                    }
                }
            }
        }

        private static string GetTemplateLiteral(Message msg, E_CsTs csts) {
            var template = msg.Template;
            for (int i = 0; i < msg.Parameters.Length; i++) {
                var before = "{" + i.ToString() + "}";
                var after = csts == E_CsTs.CSharp
                    ? "{arg" + i.ToString() + "}"
                    : "${arg" + i.ToString() + "}";

                var replaced = template.Replace(before, after);
                if (template == replaced) {
                    throw new InvalidOperationException($"メッセージ{msg.Id}のテンプレート中に変数{before}が入るべき箇所がありません。");
                }

                template = replaced;
            }

            if (NumberInsideCurlyBrace().IsMatch(template)) {
                throw new InvalidOperationException($"メッセージ{msg.Id}のパラメータは{msg.Parameters.Length + 1}個定義されていますが、テンプレート文字列中にはそれより多い変数が含まれています。");
            }

            return csts == E_CsTs.CSharp
                ? template.Replace("\"", "\\\"")
                : template.Replace("`", "\\`");
        }

        private class XDocumentAndPath {
            public required XDocument XDocument { get; init; }
            public required string FilePath { get; init; }
        }

        private class Message {
            internal required string Id { get; init; }
            internal required string Template { get; init; }
            internal required string[] Parameters { get; init; }
        }

        [GeneratedRegex(@"\{[0-9]+\}")]
        private static partial Regex NumberInsideCurlyBrace();
    }
}
