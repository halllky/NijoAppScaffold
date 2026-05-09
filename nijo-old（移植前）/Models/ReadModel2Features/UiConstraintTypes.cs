using Nijo.Models.WriteModel2Features;
using Nijo.Parts.WebServer;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.ReadModel2Features {
    /// <summary>
    /// UIコンポーネントの桁数などの制約
    /// </summary>
    internal class UiConstraintTypes : ISummarizedFile {

        internal void Add(DataClassForDisplay displayData) {
            _displayDataList.Add(displayData);
        }
        private readonly List<DataClassForDisplay> _displayDataList = new();

        int ISummarizedFile.RenderingOrder => -1; // ControllerActions（AggregateFileのレンダリング）より前

        public void OnEndGenerating(CodeRenderingContext context) {

            context.ReactProject.UtilDir(dir => {
                dir.Generate(new SourceFile {
                    FileName = "constraints.ts",
                    RenderContent = ctx => {
                        return RenderCommonConstraint(context);
                    },
                });
            });

            foreach (var disp in _displayDataList) {
                var aggregateFile = context.CoreLibrary.UseAggregateFile(disp.Aggregate);
                aggregateFile.TypeScriptFile.Add($$"""
                        {{WithIndent(disp.RenderUiConstraintType(context), "")}}
                        {{WithIndent(disp.RenderUiConstraintValue(context), "")}}
                        """);
            }
        }

        private static string RenderCommonConstraint(CodeRenderingContext ctx) {
            var characterTypes = CharacterTypeCheck.EnumerateCharacterTypeNames(ctx).ToArray();

            return $$"""
                import Decimal from "decimal.js"

                /** AggregateMemberの制約 */
                export type MemberConstraintBase = {
                  /** 必須か否か */
                  required?: boolean
                }

                /** 文字列項目がとることのできる文字の種類 */
                export type CharacterType = {{(characterTypes.Length == 0 ? "never" : characterTypes.Select(type => $"'{type}'").Join(" | "))}}

                /** 単語型の制約 */
                export type StringMemberConstraint = MemberConstraintBase & {
                  /** 最大長。文字数でカウントする */
                  maxLength?: number
                  /** この値がとることのできる文字種。未指定の場合は制約なし */
                  characterType?: CharacterType
                }

                /** 整数型と実数型の制約 */
                export type NumberMemberConstraint = MemberConstraintBase & {
                  /** 整数部と小数部をあわせた桁数 */
                  totalDigit?: number
                  /** 小数部桁数 */
                  decimalPlace?: number
                  /** マイナス値入力不可制限の有無 */
                  notNegative?: boolean
                  /** 小数部の丸め方式を指定 */
                  rounding?: Decimal.Rounding
                }

                /** いずれかの型の制約 */
                export type AnyMemberConstraints = Partial<
                  StringMemberConstraint
                  & NumberMemberConstraint
                >
                """;
        }
    }
}
