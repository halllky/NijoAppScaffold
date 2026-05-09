using Nijo.Core;
using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.WebServer {
    /// <summary>
    /// 実行時の拡張メソッド。stringなど、.NETの基本クラスに対する拡張メソッドを記述することを想定。
    /// </summary>
    internal class DotnetExtensions {

        internal static SourceFile RenderCoreLibrary() => new SourceFile {
            FileName = $"DotnetExtensions.cs",
            RenderContent = context => $$"""
                namespace {{context.Config.RootNamespace}} {
                    using System;
                    using System.Collections;
                    using System.Collections.Generic;
                    using System.Linq;
                    using System.Text.Json;

                    public static class DotnetExtensions {

                        /// <summary>
                        /// 例外オブジェクトのメッセージを列挙します。InnerExceptionsも考慮します。
                        /// </summary>
                        public static IEnumerable<string> GetMessagesRecursively(this Exception ex, string indent = "") {
                            yield return indent + ex.Message;

                            if (ex is AggregateException aggregateException) {
                                var innerExceptions = aggregateException.InnerExceptions
                                    .SelectMany(inner => inner.GetMessagesRecursively(indent + "  "));
                                foreach (var inner in innerExceptions) {
                                    yield return inner;
                                }
                            }

                            if (ex.InnerException != null) {
                                foreach (var inner in ex.InnerException.GetMessagesRecursively(indent + "  ")) {
                                    yield return inner;
                                }
                            }
                        }


                        /// <summary>
                        /// 引数の文字列の中から、見かけ上は1文字であるものの、
                        /// Unicoceのコードポイント換算だと複数とみなされる文字をピックアップします。
                        /// </summary>
                        public static IEnumerable<string> PickupMultipleCodeUnitCharacters(string str) {
                            var stringInfo = new System.Globalization.StringInfo(str);
                            for (int i = 0; i < stringInfo.LengthInTextElements; i++) {

                                // 見かけ上の文字数基準で1文字ピックアップ
                                var character = stringInfo.SubstringByTextElements(i, 1);

                                // コードユニットが2以上なら該当
                                if (character.Length > 1) yield return character;
                            }
                        }

                        /// <summary>
                        /// decimal型の変数の整数部分の桁数を取得します。
                        /// </summary>
                        public static int GetDigitsOfIntegerPart(this decimal value) {
                            // 文字列に変換して文字列の長さで判定
                            var splitted = Math.Abs(value).ToString().Split('.');
                            return splitted.Length == 0
                                ? 0
                                : splitted[0].Length;
                        }

                        /// <summary>
                        /// decimal型の変数の小数部分の桁数を取得します。
                        /// </summary>
                        public static int GetDigitsOfDecimalPart(this decimal value) {
                            // 文字列に変換して末尾のゼロを除去して文字列の長さを調べる
                            var splitted = value.ToString().Split('.');
                            return splitted.Length <= 1
                                ? 0
                                : splitted[1].TrimEnd('0').Length;
                        }
                    }
                }
                """,
        };

        internal static SourceFile RenderToWebApiProject() => new SourceFile {
            FileName = $"DotnetExtensions.cs",
            RenderContent = context => $$"""
                namespace {{context.Config.RootNamespace}} {
                    using System;
                    using System.Collections;
                    using System.Collections.Generic;
                    using System.Linq;
                    using System.Text.Json;
                    using Microsoft.AspNetCore.Mvc;

                    public static class DotnetExtensionsInWebApi {
                        public static IActionResult JsonContent<T>(this ControllerBase controller, T obj, int? httpStatusCode = null) {
                            var json = {{UtilityClass.CLASSNAME}}.{{UtilityClass.TO_JSON}}(obj);
                            var result = controller.Content(json, "application/json");
                            result.StatusCode = httpStatusCode ?? (int?)System.Net.HttpStatusCode.OK;
                            return result;
                        }
                    }
                }
                """,
        };
    }
}
