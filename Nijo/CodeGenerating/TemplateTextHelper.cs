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
        /// 条件によってテンプレート片をレンダリングし分けるためのヘルパー。
        /// Render 系メソッドの raw string 補間内で使い、各分岐の戻り値も raw string で記述すること。
        /// If, ElseIf, Else の連鎖全体は同じ挿入位置で開始し、各 raw string の終端インデントも揃えること。
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
        /// Render 系メソッドの中で、複数行のテンプレート片に挿入位置と同じだけのインデントを与えるために使う。
        /// raw string の補間内で、インデントを下げたい位置から開始すること。
        /// この呼び出しの左側には半角スペースだけを置き、その幅が Render 時に 2 行目以降へ適用される。
        /// content 側は原則としてインデント 0 から組み立て、さらに深いネストは WithIndent を重ねて表現すること。
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
        /// 明示的に指定したインデント文字列で 2 行目以降をインデントします。
        /// 既存呼び出しとの互換性のために残しているオーバーロードで、新規コードでは 1 引数版を優先します。
        /// </summary>
        [Obsolete("WithIndent(string) を優先してください。")]
        internal static string WithIndent(IEnumerable<string> content, string indent) {
            return content
                .Select(x => WithIndent(x, indent))
                .Join(Environment.NewLine + indent);
        }

        /// <summary>
        /// 明示的に指定したインデント文字列で 2 行目以降をインデントします。
        /// 既存呼び出しとの互換性のために残しているオーバーロードで、新規コードでは 1 引数版を優先します。
        /// </summary>
        [Obsolete("WithIndent(string) を優先してください。")]
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
        /// Render 系メソッドの中で、複数要素に対して縦方向にテンプレート片を反復するときに使う。
        /// raw string の補間内で呼び出し、selector の戻り値も raw string で記述すること。
        /// selector が返す raw string の終端インデントは、挿入先の raw string でこの補間式を置いた位置に揃えること。
        /// 反復の各要素に対してさらに深いインデントが必要なら、selector 内で WithIndent を使うこと。
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

