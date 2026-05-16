using Nijo.Core;
using Nijo.Models.WriteModel2Features;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models {
    internal class DynamicEnum {

        internal const string CSHARP_UTIL_CLASS = "区分マスタUtil";
        internal const string CSHARP_UTIL_PROPERTY = "区分マスタUtil";

        internal const string PK_PROP_NAME = "内部キー";
        internal const string TYPE_PROP_NAME = "種別";
        internal const string VALUE_PROP_NAME = "値CD";
        internal const string DISPLAY_NAME_PROP_NAME = "表示名称";

        internal static string CONTAINS_PK => $"Contains{PK_PROP_NAME}";
        internal static string CONTAINS_VALUE => $"Contains{VALUE_PROP_NAME}";
        internal static GraphNode<Aggregate>? FindDynamicEnum(CodeRenderingContext ctx) {
            return ctx.Schema.RootAggregates().SingleOrDefault(agg => agg.Item.Options.IsDynamicEnumWriteModel);
        }
        internal static bool ExistsDynamicEnum(CodeRenderingContext ctx) {
            return FindDynamicEnum(ctx) != null;
        }

        internal static void GenerateSourceCode(CodeRenderingContext ctx) {

            // 区分マスタが無いならレンダリングしない
            var dynamicEnum = FindDynamicEnum(ctx);
            if (dynamicEnum == null) return;

            ctx.CoreLibrary.AppSrvMethods.Add(RenderAppSrvUtilProperty());
            ctx.CoreLibrary.UtilDir(dir => {
                dir.Generate(RenderCSharp());
            });

            var aggregateFile = ctx.CoreLibrary.UseAggregateFile(dynamicEnum);
            aggregateFile.TypeScriptFile.Add(RenderTypeScript(ctx));
        }

        private static string RenderAppSrvUtilProperty() {
            return $$"""
                /// <summary>
                /// 区分マスタを参照する処理に関するユーティリティ。
                /// 主な役割は何度もDBへのアクセスが発生するのを防ぐためにメモリ上にキャッシュを持つこと。
                /// </summary>
                public {{CSHARP_UTIL_CLASS}} {{CSHARP_UTIL_PROPERTY}} { get; }
                """;
        }

        private static SourceFile RenderCSharp() {
            return new SourceFile {
                FileName = $"{CSHARP_UTIL_CLASS}.cs",
                RenderContent = ctx => {
                    var app = new Parts.WebServer.ApplicationService();
                    var writeModel = ctx.Schema.GetDynamicEnumWriteModel()!;
                    var efCoreEntity = new EFCoreEntity(writeModel);
                    var pkSeqName = ((AggregateMember.ValueMember)writeModel.GetMembers().Single(m => m.MemberName == PK_PROP_NAME)).Options.SeqName;

                    return $$"""
                        using System.Collections;

                        namespace {{ctx.Config.RootNamespace}};

                        public class {{CSHARP_UTIL_CLASS}} {
                            public {{CSHARP_UTIL_CLASS}}({{app.AbstractClassName}} app) {
                        {{ctx.Schema.DynamicEnumTypeInfo.SelectTextTemplate(info => $$"""
                                {{info.PhysicalName}} = new("{{info.TypeKey}}", "{{info.DisplayName.Replace("\"", "\\\"")}}", app);
                        """)}}
                            }

                        {{ctx.Schema.DynamicEnumTypeInfo.SelectTextTemplate(info => $$"""
                            public DynamicEnumCache {{info.PhysicalName}} { get; }
                        """)}}

                            /// <summary>
                            /// 区分の種類を列挙します。
                            /// </summary>
                            public IEnumerable<KeyValuePair<string, string>> EnumerateTypes() {
                        {{ctx.Schema.DynamicEnumTypeInfo.SelectTextTemplate(info => $$"""
                                yield return KeyValuePair.Create("{{info.TypeKey}}", "{{info.DisplayName.Replace("\"", "\\\"")}}");
                        """)}}
                            }
                        }

                        /// <summary>
                        /// 区分マスタの内容をメモリ上にキャッシュしておく仕組み。
                        /// </summary>
                        public partial class DynamicEnumCache : IEnumerable<{{efCoreEntity.ClassName}}> {

                            public DynamicEnumCache(string typeKey, string displayName, {{app.AbstractClassName}} app) {
                                {{TYPE_PROP_NAME}} = typeKey;
                                {{DISPLAY_NAME_PROP_NAME}} = displayName;
                                _app = app;
                            }
                            /// <summary>
                            /// この区分の種別コード
                            /// </summary>
                            public string {{TYPE_PROP_NAME}} { get; }
                            /// <summary>
                            /// この区分の種類の名前
                            /// </summary>
                            public string {{DISPLAY_NAME_PROP_NAME}} { get; }

                            private readonly {{app.AbstractClassName}} _app;

                            /// <summary>
                            /// この種類の区分を全てDBから読み込んだキャッシュ。辞書のキーは{{VALUE_PROP_NAME}}。
                            /// </summary>
                            public IReadOnlyDictionary<string, {{efCoreEntity.ClassName}}> Cache {
                                get {
                                    return _cache ??= _app.DbContext.{{efCoreEntity.DbSetName}}
                                        .Where(x => x.{{TYPE_PROP_NAME}} == this.{{TYPE_PROP_NAME}})
                                        .ToDictionary(x => x.{{VALUE_PROP_NAME}}!);
                                }
                            }
                            private IReadOnlyDictionary<string, {{efCoreEntity.ClassName}}>? _cache;
                            
                            /// <summary>
                            /// この区分に指定の{{PK_PROP_NAME}}が含まれているかどうかを返します。
                            /// </summary>
                            /// <param name="pk">{{PK_PROP_NAME}}</param>
                            public bool {{CONTAINS_PK}}(int pk) {
                                return Cache.Values.Any(x => x.{{PK_PROP_NAME}} == pk);
                            }
                            /// <summary>
                            /// この区分に指定の{{VALUE_PROP_NAME}}が含まれているかどうかを返します。
                            /// </summary>
                            /// <param name="ataiCd">{{VALUE_PROP_NAME}}</param>
                            public bool {{CONTAINS_VALUE}}(string ataiCd) {
                                return Cache.Values.Any(x => x.{{VALUE_PROP_NAME}} == ataiCd);
                            }

                            public IEnumerator<{{efCoreEntity.ClassName}}> GetEnumerator() {
                                return Cache.Values.GetEnumerator();
                            }
                            IEnumerator IEnumerable.GetEnumerator() {
                                return GetEnumerator();
                            }
                        }
                        """;
                },
            };
        }

        private static string RenderTypeScript(CodeRenderingContext ctx) {
            return $$"""
                /** 区分マスタの種類名 */
                export type 区分マスタ種別 = keyof typeof 区分マスタ種別Key
                /** 区分マスタの種類と対応するキー */
                export const 区分マスタ種別Key = {
                {{ctx.Schema.DynamicEnumTypeInfo.SelectTextTemplate((x, i) => $$"""
                  {{x.PhysicalName}}: '{{x.TypeKey}}' as const,
                """)}}
                }
                """;
        }


        #region 登録更新時に異なる種別のデータが登録されてしまわないようにするためのチェック
        internal const string APP_SRV_CHECK_METHOD = "CheckKbnType";
        internal static string RenderAppSrvCheckMethod(GraphNode<Aggregate> aggregate, CodeRenderingContext ctx) {
            var rootDbEntity = new EFCoreEntity(aggregate);
            var dataClass = new DataClassForSave(aggregate, DataClassForSave.E_Type.UpdateOrDelete);

            return $$"""
                /// <summary>
                /// 異なる種類の区分値が登録されないかのチェック処理。違反する項目があった場合はその旨が第2引数のオブジェクト内に追記されます。
                /// </summary>
                public virtual void {{APP_SRV_CHECK_METHOD}}({{rootDbEntity.ClassName}} dbEntity, {{SaveContext.BEFORE_SAVE}}<{{dataClass.MessageDataCsInterfaceName}}> e) {
                    {{WithIndent(RenderAggregate(aggregate, "dbEntity", aggregate), "    ")}}
                }

                """;

            IEnumerable<string> RenderAggregate(GraphNode<Aggregate> renderingAggregate, string instance, GraphNode<Aggregate> instanceAggregate) {
                foreach (var member in renderingAggregate.GetMembers()) {
                    var memberDisplayName = member.DisplayName.Replace("\"", "\\\"");

                    if (member is AggregateMember.Schalar) {
                        continue;
                    } else if (member is AggregateMember.Parent) {
                        continue;

                    } else if (member is AggregateMember.Ref @ref) {
                        var dEnum = @ref.GetDynamicEnumTypeInfo(ctx);
                        if (dEnum == null) continue;

                        var path = @ref.GetFullPathAsDbEntity(since: instanceAggregate);

                        yield return $$"""
                            if ({{instance}}.{{path.Join("?.")}}?.{{PK_PROP_NAME}} != null
                                && !{{CSHARP_UTIL_PROPERTY}}.{{dEnum.PhysicalName}}.{{CONTAINS_PK}}({{instance}}.{{path.Join(".")}}.{{PK_PROP_NAME}}.Value)) {
                                e.{{GetErrorMemberPath(member).Join(".")}}.AddError("区分値の種類が不正です。");
                            }
                            """;

                    } else if (member is AggregateMember.Child child) {
                        yield return $$"""

                        {{WithIndent(RenderAggregate(child.ChildAggregate, instance, instanceAggregate), "")}}
                        """;

                    } else if (member is AggregateMember.VariationItem variationItem) {
                        yield return $$"""

                            {{WithIndent(RenderAggregate(variationItem.VariationAggregate, instance, instanceAggregate), "")}}
                            """;

                    } else if (member is AggregateMember.Variation) {
                        continue;

                    } else if (member is AggregateMember.Children children) {
                        var childrenPath = children.GetFullPathAsDbEntity(since: instanceAggregate);

                        var depth = renderingAggregate.EnumerateAncestors().Count();
                        var i = depth == 0 ? "i" : $"i{depth}";
                        var item = depth == 0 ? "item" : $"item{depth}";
                        yield return $$"""

                            for (var {{i}} = 0; {{i}} < {{instance}}.{{childrenPath.Join("?.")}}.Count; {{i}}++) {
                                var {{item}} = {{instance}}.{{childrenPath.Join("!.")}}.ElementAt({{i}});

                                {{WithIndent(RenderAggregate(children.ChildrenAggregate, item, children.ChildrenAggregate), "    ")}}
                            }
                            """;
                    }
                }
            }

            /// <summary>
            /// エラーメッセージの該当プロパティのパスを返す。
            /// 配列インデックスの名前は i, i1, i2, ... で決め打ち。
            /// </summary>
            IEnumerable<string> GetErrorMemberPath(AggregateMember.AggregateMemberBase member) {
                /// 決め打ち。<see cref="SaveContext"/> のファイルを参照。
                yield return "Messages";

                foreach (var e in member.Owner.PathFromEntry()) {
                    var edge = e.As<Aggregate>();

                    if (!edge.IsParentChild()) throw new InvalidOperationException("この分岐にくることは無いはず");

                    var child = edge.Terminal.AsChildRelationMember();
                    if (child is AggregateMember.Children children) {
                        var depth = children.ChildrenAggregate.EnumerateAncestors().Count() - 1; // 深さはChildren自身ではなく親基準なのでマイナス1
                        var i = depth == 0 ? "i" : $"i{depth}";
                        yield return $"{child.MemberName}[{i}]";

                    } else {
                        yield return child.MemberName;
                    }
                }

                yield return member.MemberName;
            }
        }
        #endregion 登録更新時に異なる種別のデータが登録されてしまわないようにするためのチェック
    }
}
