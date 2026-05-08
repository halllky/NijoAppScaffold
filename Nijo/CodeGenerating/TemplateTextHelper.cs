global using static Nijo.CodeGenerating.TemplateTextHelper;
using Nijo.Util.DotnetEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nijo.CodeGenerating {
    /// <summary>
    /// テンプレート文字列簡略化用
    /// </summary>
    internal partial class TemplateTextHelper {
        private TemplateTextHelper(StringBuilder stringBuilder) {
            _stringBuilder = stringBuilder;
            _evaluated = false;
        }

        private readonly StringBuilder _stringBuilder;
        private bool _evaluated;

        /// <summary>
        /// 条件によってソースコードをレンダリングし分けるためのヘルパー。
        /// 必ず raw string の左端（インデント0の位置）から開始すること。
        /// </summary>
        internal static TemplateTextHelper If(bool condition, Func<string> text) {
            var helper = new TemplateTextHelper(new StringBuilder());
            if (condition) {
                helper._stringBuilder.AppendLine(text());
                helper._evaluated = true;
            }
            return helper;
        }
        /// <inheritdoc cref="If(bool, Func{string})"/>
        internal TemplateTextHelper ElseIf(bool condition, Func<string> text) {
            if (!_evaluated && condition) {
                _stringBuilder.AppendLine(text());
                _evaluated = true;
            }
            return this;
        }
        /// <inheritdoc cref="If(bool, Func{string})"/>
        internal TemplateTextHelper Else(Func<string> text) {
            if (!_evaluated) {
                _stringBuilder.AppendLine(text());
                _evaluated = true;
            }
            return this;
        }

        public override string ToString() {
            if (_evaluated) {
                _stringBuilder.Append(SKIP_MARKER);
                return _stringBuilder.ToString();

            } else {
                return SKIP_MARKER;
            }
        }

        /// <summary>
        /// Render 系メソッドの中でインデントを表すのに使う。
        /// 必ず raw string の中の、インデントを下げたい位置から開始すること。
        /// 例えば、インデント4スペース分下げたい場合は、このメソッドの呼び出しも
        /// raw string の左端からスペース4個分から開始する。
        /// </summary>
        /// <param name="content">レンダリング対象のソースコード</param>
        internal static string WithIndent(IEnumerable<string> content) {
            using var enumerator = content.GetEnumerator();
            if (!enumerator.MoveNext()) {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(enumerator.Current);

            while (enumerator.MoveNext()) {
                stringBuilder.AppendLine();
                stringBuilder.Append(enumerator.Current);
            }

            return WithIndent(stringBuilder.ToString());
        }

        /// <inheritdoc cref="WithIndent(IEnumerable{string})"/>
        internal static string WithIndent(string content) {
            if (content == string.Empty) {
                return string.Empty;
            }

            return $"{INDENT_START_MARKER}{content}{INDENT_END_MARKER}";
        }

        /// <summary>
        /// 明示的に指定したインデント文字列で、2行目以降をインデントします。
        /// 既存呼び出しとの互換性のために残しているオーバーロードです。
        /// </summary>
        internal static string WithIndent(IEnumerable<string> content, string indent) {
            return content
                .Select(x => WithIndent(x, indent))
                .Join(Environment.NewLine + indent);
        }

        /// <summary>
        /// 明示的に指定したインデント文字列で、2行目以降をインデントします。
        /// 既存呼び出しとの互換性のために残しているオーバーロードです。
        /// </summary>
        internal static string WithIndent(string content, string indent) {
            return content
                .Split(Environment.NewLine)
                .Join(Environment.NewLine + indent);
        }

        /// <summary>
        /// <see cref="WithIndent(IEnumerable{string})"/> と <see cref="WithIndent(string)"/> の開始位置を表すマーカーです。
        /// </summary>
        internal static string INDENT_START_MARKER = "\0\0\0\0\0\0\0\0\0\0\u0001";

        /// <summary>
        /// <see cref="WithIndent(IEnumerable{string})"/> と <see cref="WithIndent(string)"/> の終了位置を表すマーカーです。
        /// </summary>
        internal static string INDENT_END_MARKER = "\0\0\0\0\0\0\0\0\0\0\u0002";

        /// <summary>
        /// この文字列が存在する行はファイルにレンダリングされない。
        ///
        /// <see cref="If(bool, Func{string})"/> や
        /// <see cref="TemplateTextHelperExtensions.SelectTextTemplate{T}(IEnumerable{T}, Func{T, string})"/>
        /// によって条件に合致しなかったり要素の数が0だったりして空行が生成されてしまうのを防ぐためのもの。
        /// </summary>
        internal static string SKIP_MARKER = "\0\0\0\0\0\0\0\0\0\0\0"; // 通常のソースコード上に現れなさそうな文字列であれば何でもよい
    }
    internal static class TemplateTextHelperExtensions {
        /// <summary>
        /// Render 系メソッドの中でループを表すのに使う。
        /// 必ず raw string の左端（インデント0の位置）で呼び出すこと。
        /// また、ループ内部のソースコードも必ず raw string で記述し、
        /// そのインデントレベルは外側の raw string の終端のインデントレベルと同じにすること。
        /// </summary>
        internal static string SelectTextTemplate<T>(this IEnumerable<T> values, Func<T, string> selector) {
            var sourceCode = values.Select(selector).Join(Environment.NewLine);
            return sourceCode == string.Empty
                ? SKIP_MARKER
                : sourceCode;
        }
        /// <inheritdoc cref="SelectTextTemplate{T}(IEnumerable{T}, Func{T, string})"/>
        internal static string SelectTextTemplate<T>(this IEnumerable<T> values, Func<T, int, string> selector) {
            var sourceCode = values.Select(selector).Join(Environment.NewLine);
            return sourceCode == string.Empty
                ? SKIP_MARKER
                : sourceCode;
        }
    }
}

