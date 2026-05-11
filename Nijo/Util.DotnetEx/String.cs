using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nijo.Util.DotnetEx {
    public static class StringExtension {
        public static string Join(this IEnumerable<string> values, string separator) => string.Join(separator, values);
        public static string ToCSharpSafe(this string str) {
            if (string.IsNullOrEmpty(str)) return str;

            var sb = new StringBuilder();
            var isFirst = true;

            foreach (var rune in str.EnumerateRunes()) {
                if (isFirst) {
                    if (IsIdentifierStart(rune)) sb.Append(rune);
                    isFirst = false;
                    continue;
                }

                if (IsIdentifierPart(rune)) sb.Append(rune);
            }

            return sb.ToString();
        }

        public static bool IsCSharpSafe(this string? value) {
            return value?.ToCSharpSafe() == value;
        }

        static bool IsIdentifierStart(Rune rune) {
            switch (Rune.GetUnicodeCategory(rune)) {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.LetterNumber:
                case UnicodeCategory.ConnectorPunctuation: // underscore
                    return true;
                default:
                    return false;
            }
        }

        static bool IsIdentifierPart(Rune rune) {
            switch (Rune.GetUnicodeCategory(rune)) {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.LetterNumber:
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.ConnectorPunctuation:
                case UnicodeCategory.Format:
                    return true;
                default:
                    return false;
            }
        }
        public static string ToFileNameSafe(this string str) {
            foreach (var c in System.IO.Path.GetInvalidFileNameChars()) {
                str = str.Replace(c, '_');
            }
            return str;
        }
        public static string ToUrlSafe(this string str) => System.Web.HttpUtility.UrlEncode(str);
        public static string ToKebabCase(this string str) => str.ToLower().Replace(" ", "-");
        public static string ToCamelCase(this string input) {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.Length == 1) return input.ToLowerInvariant();
            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }
        /// <summary>
        /// aaa-bbb-ccc => AaaBbbCcc
        /// </summary>
        public static string KebabCaseToPascalCase(this string str) {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Split('-').Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)).Join("");
        }
        public static string ToHashedString(this string str) {
            byte[] stringBytes = System.Text.Encoding.UTF8.GetBytes(str);
            byte[] hashedBytes = System.Security.Cryptography.MD5.Create().ComputeHash(stringBytes);
            byte[] guidBytes = new byte[16];
            Array.Copy(hashedBytes, 0, guidBytes, 0, 16);
            var guid = new Guid(guidBytes);
            return "x" + guid.ToString().Replace("-", "");
        }

        /// <summary>
        /// 半角文字を1、全角文字を2として横幅を算出する。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int CalculateCharacterWidth(this string str) {
            int totalWidth = 0;

            for (int i = 0; i < str.Length;) {
                var unicodeCategory = char.GetUnicodeCategory(str, i);

                if (unicodeCategory == UnicodeCategory.Surrogate || char.IsSurrogate(str[i])) {
                    totalWidth += 2; // サロゲートペア
                    i += 2;

                } else if (unicodeCategory == UnicodeCategory.OtherSymbol) {
                    totalWidth += 2; // 絵文字やIVS文字など
                    i += 2;

                } else if ((str[i] >= 0x3000 && str[i] <= 0xFF60)
                        || (str[i] >= 0xFFE0 && str[i] <= 0xFFE6)) {
                    totalWidth += 2; // 一般的な日本語の全角文字や記号
                    i += 1;

                } else {
                    totalWidth += 1;
                    i += 1;
                }
            }

            return totalWidth;
        }
    }
}

