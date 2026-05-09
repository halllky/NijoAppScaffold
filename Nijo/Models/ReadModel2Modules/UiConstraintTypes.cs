using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Nijo.Models.ReadModel2Modules {
    internal class UiConstraintTypes : IMultiAggregateSourceFile {
        private readonly Lock _lock = new();
        private readonly List<DisplayData> _displayDataList = [];

        internal UiConstraintTypes Add(DisplayData displayData) {
            lock (_lock) {
                _displayDataList.Add(displayData);
                return this;
            }
        }

        public void RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
        }

        public void Render(CodeRenderingContext ctx) {
            ctx.ReactProject(dir => {
                dir.Directory("util", utilDir => {
                    utilDir.Generate(new SourceFile {
                        FileName = "constraints.ts",
                        Contents = RenderCommonConstraint(ctx),
                    });
                });
            });
        }

        private static string RenderCommonConstraint(CodeRenderingContext ctx) {
            var characterTypes = EnumerateCharacterTypeNames(ctx).ToArray();

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

        private static IEnumerable<string> EnumerateCharacterTypeNames(CodeRenderingContext ctx) {
            return ctx.Schema
                .GetRootAggregates()
                .SelectMany(aggregate => aggregate.EnumerateThisAndDescendants())
                .SelectMany(aggregate => aggregate.GetMembers())
                .OfType<ValueMember>()
                .Select(vm => vm.CharacterType)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .OrderBy(name => name)!;
        }
    }
}
