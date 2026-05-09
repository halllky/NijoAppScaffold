using Castle.Components.DictionaryAdapter.Xml;
using Nijo.Core;
using Nijo.Features;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo {

    /// <summary>
    /// メッセージマスタ（メッセージ定数）。
    /// 画面上に表示するメッセージ等に番号を振って定数ないし関数にする仕組み。
    /// </summary>
    internal partial class MessageConst : IFeature {

        /// <summary>
        /// メッセージの関数が生成されるC#側クラス名
        /// </summary>
        internal const string CS_CLASS_NAME = "MSG";
        /// <summary>
        /// メッセージの関数が生成されるTypeScript側オブジェクト名
        /// </summary>
        internal const string TS_CONTAINER_OBJECT_NAME = "MSG";


        #region 自動生成されるソース内で使用されるメッセージのID（メッセージ一覧の台帳と合わせる必要あり）
        /// <summary>msg本文:Byte配列の列定義はサポートされていません。</summary>
        internal const string C_INF0003 = "ERRC0019";
        /// <summary>msg本文:設定ファイルに '{setting.CurrentDb}' という名前のDB接続設定が見つかりません。</summary>
        internal const string C_INF0004 = "ERRC0020";
        /// <summary>msg本文:Oracleのデータベースは社内の他のチームの方々も利用しているため、明示的に登録されたスキーマ名に含まれないスキーマに接続している場合は削除できません。</summary>
        internal const string C_INF0005 = "ERRC0021";
        /// <summary>msg本文:予期しないDB接続先です: ${profile.RDBMS}</summary>
        internal const string C_INF0006 = "ERRC0022";
        /// <summary>msg本文:DBを再作成しました。</summary>
        internal const string C_INF0007 = "INFC0002";
        /// <summary>msg本文:{{_rootAggregate.Item.DisplayName}}処理が成功しました。</summary>
        internal const string C_INF0008 = "INFC0003";
        /// <summary>msg本文:入力内容が不正です: ${JSON.stringify(resData.{{ComplexPost.RESPONSE_DETAIL}})}</summary>
        internal const string C_INF0009 = "ERRC0023";
        /// <summary>msg本文:{{_rootAggregate.Item.DisplayName}}処理は実装されていません。</summary>
        internal const string C_INF0010 = "ERRC0024";
        /// <summary>msg本文:警告があります。続行してよいですか？</summary>
        internal const string C_INF0011 = "CNFC0003";
        /// <summary>msg本文:パラメータを{{x.DataClass.CsClassName}}型に変換できません: {value.GetRawText()}</summary>
        internal const string C_INF0012 = "ERRC0025";
        /// <summary>msg本文:更新パラメータの種別 '{dataType}' を認識できません。</summary>
        internal const string C_INF0013 = "ERRC0026";
        /// <summary>msg本文:対象データへの更新権限がありません。</summary>
        internal const string C_INF0014 = "ERRC0027";
        /// <summary>msg本文:型 '{item?.GetType().FullName}' の登録更新用データへの変換処理が定義されていません。</summary>
        internal const string C_INF0015 = "ERRC0028";
        /// <summary>msg本文:更新対象データの更新前のバージョンが指定されていません。</summary>
        internal const string C_INF0016 = "ERRC0029";
        /// <summary>msg本文:更新対象データの更新前のバージョンが指定されていません。</summary>
        internal const string C_INF0017 = "ERRC0029";
        /// <summary>msg本文:データ読み込みに失敗しました。</summary>
        internal const string C_INF0018 = "ERRC0067";
        /// <summary>msg本文:画面を移動すると、変更内容が破棄されます。よろしいでしょうか？</summary>
        internal const string C_INF0019 = "CNFC0004";
        /// <summary>msg本文:エラーがあります。</summary>
        internal const string C_INF0020 = "ERRC0030";
        /// <summary>msg本文:更新しました。</summary>
        internal const string C_INF0021 = "INFC0001";
        /// <summary>msg本文:選択されている行の変更を元に戻しますか？</summary>
        internal const string C_INF0022 = "CNFC0005";
        /// <summary>msg本文:表示対象のデータが見つかりません。（{{urlKeysWithMember.Select(kv => $"{kv.Key.MemberName}: ${{{kv.Value}}}").Join(", ")}}）</summary>
        internal const string C_INF0023 = "ERRC0031";
        /// <summary>msg本文:遷移前画面で指定されたパラメータが不正です。データベースから読み込んだ値を表示します。</summary>
        internal const string C_INF0024 = "ERRC0032";
        /// <summary>msg本文:データの読み込みに失敗しました。</summary>
        internal const string C_INF0025 = "ERRC0068";
        /// <summary>msg本文:画面を移動すると、変更内容が破棄されます。よろしいでしょうか？</summary>
        internal const string C_INF0026 = "CNFC0004";
        /// <summary>msg本文:閲覧モードで表示中のためデータを更新することができません。</summary>
        internal const string C_INF0027 = "ERRC0033";
        /// <summary>msg本文:変更された内容がありません。</summary>
        internal const string C_INF0028 = "INFC0005";
        /// <summary>msg本文:データ読み込みに失敗しました。</summary>
        internal const string C_INF0029 = "ERRC0067";
        /// <summary>msg本文:複数選択モードの場合にこの関数が呼ばれることはあり得ない</summary>
        internal const string C_INF0030 = "ERRC0034";
        /// <summary>msg本文:1件選択モードの場合にこの関数が呼ばれることはあり得ない</summary>
        internal const string C_INF0031 = "ERRC0035";
        /// <summary>msg本文:保存しました。</summary>
        internal const string C_INF0032 = "INFC0001";
        /// <summary>msg本文:引数{nameof(itemsAndMessages)}の要素が想定外の型です: {item.ToJson()}</summary>
        internal const string C_INF0033 = "ERRC0036";
        /// <summary>msg本文:引数{nameof(itemsAndMessages)}の要素が想定外の型です: {item.ToJson()}</summary>
        internal const string C_INF0034 = "ERRC0036";
        /// <summary>msg本文:更新後処理でエラーが発生しました: {string.Join(Environment.NewLine, ex.GetMessagesRecursively())}</summary>
        internal const string C_INF0035 = "ERRC0069";
        /// <summary>msg本文:パラメータを{{x.Create.CsClassName}}型に変換できません: {value.GetRawText()}</summary>
        internal const string C_INF0036 = "ERRC0025";
        /// <summary>msg本文:パラメータを{{x.Upd.CsClassName}}型に変換できません: {value.GetRawText()}</summary>
        internal const string C_INF0037 = "ERRC0025";
        /// <summary>msg本文:パラメータを{{x.Upd.CsClassName}}型に変換できません: {value.GetRawText()}</summary>
        internal const string C_INF0038 = "ERRC0025";
        /// <summary>msg本文:パラメータを{{x.Upd.CsClassName}}型に変換できません: {value.GetRawText()}</summary>
        internal const string C_INF0039 = "ERRC0025";
        /// <summary>msg本文:更新パラメータの種別を認識できません: {jsonDocument.RootElement.GetRawText()}</summary>
        internal const string C_INF0040 = "ERRC0037";
        /// <summary>msg本文:{{k.MemberName}}が空です。</summary>
        internal const string C_INF0041 = "ERRC0038";
        /// <summary>msg本文:削除対象のデータが見つかりません。</summary>
        internal const string C_INF0042 = "ERRC0039";
        /// <summary>msg本文:ほかのユーザーが更新しました。</summary>
        internal const string C_INF0043 = "ERRC0070";
        /// <summary>msg本文:更新後処理でエラーが発生しました: {string.Join(Environment.NewLine, ex.GetMessagesRecursively())}</summary>
        internal const string C_INF0044 = "ERRC0069";
        /// <summary>msg本文:警告があります。続行してよいですか？</summary>
        internal const string C_INF0045 = "CNFC0003";
        /// <summary>msg本文:保存します。よろしいですか？</summary>
        internal const string C_INF0046 = "CNFC0006";
        /// <summary>msg本文:{{k.MemberName}}が空です。</summary>
        internal const string C_INF0047 = "ERRC0038";
        /// <summary>msg本文:更新対象のデータが見つかりません。</summary>
        internal const string C_INF0048 = "ERRC0040";
        /// <summary>msg本文:ほかのユーザーが更新しました。</summary>
        internal const string C_INF0049 = "ERRC0070";
        /// <summary>msg本文:更新後処理でエラーが発生しました: {string.Join(Environment.NewLine, ex.GetMessagesRecursively())}</summary>
        internal const string C_INF0050 = "ERRC0069";
        /// <summary>msg本文:接続文字列が未指定です</summary>
        internal const string C_INF0051 = "ERRC0041";
        /// <summary>msg本文:接続文字列 '{CurrentDb}' は無効です。</summary>
        internal const string C_INF0052 = "ERRC0042";
        /// <summary>msg本文:日付の値が不正です: year:0000/month:00/day:00</summary>
        internal const string C_INF0053 = "ERRC0043";
        /// <summary>msg本文:年月の値が不正です: year:0000/month:00</summary>
        internal const string C_INF0054 = "ERRC0044";
        /// <summary>msg本文:DBを再作成します。データは全て削除されます。よろしいですか？</summary>
        internal const string C_INF0055 = "CNFC0007";
        /// <summary>msg本文:ローカルリポジトリの初期化に失敗しました: ${error}</summary>
        internal const string C_INF0056 = "ERRC0071";
        /// <summary>msg本文:DBを再作成しましたがダミーデータ作成に失敗しました。</summary>
        internal const string C_INF0057 = "ERRC0072";
        /// <summary>msg本文:DBを再作成しました。</summary>
        internal const string C_INF0058 = "INFC0002";

        // Templateに記載されているメッセージ一覧
        /// <summary>msg本文:クリップボードからテキストを読み取れませんでした。</summary>
        internal const string C_INF0059 = "ERRC0045";
        /// <summary>msg本文:入力内容が破棄されます。よろしいでしょうか？</summary>
        internal const string C_INF0060 = "CNFC0008";
        /// <summary>msg本文:データ取得に失敗しました: ${error}</summary>
        internal const string C_INF0061 = "ERRC0073";
        /// <summary>msg本文:処理は成功しましたが処理結果の解釈に失敗しました。</summary>
        internal const string C_INF0062 = "ERRC0074";
        /// <summary>msg本文:処理に失敗しました。</summary>
        internal const string C_INF0063 = "ERRC0075";
        /// <summary>msg本文:通信でエラーが発生しました(${url})\n${parseUnknownErrors(errors).join('\n')}</summary>
        internal const string C_INF0064 = "ERRC0076";
        /// <summary>msg本文:処理は成功しましたが処理結果の解釈に失敗しました。\n${parseUnknownErrors(errors).join('\n')}</summary>
        internal const string C_INF0065 = "ERRC0077";
        /// <summary>msg本文:処理に失敗しました。\n${parseUnknownErrors(errors).join('\n')}`</summary>
        internal const string C_INF0066 = "ERRC0078";
        /// <summary>msg本文:通信でエラーが発生しました(${url})\n${parseUnknownErrors(errors).join('\n')}</summary>
        internal const string C_INF0067 = "ERRC0076";
        /// <summary>msg本文:ファイルダウンロードに失敗しました: ${error}</summary>
        internal const string C_INF0068 = "ERRC0079";
        /// <summary>msg本文:通信でエラーが発生しました(${url})\n${parseUnknownErrors(errors).join('\n')}</summary>
        internal const string C_INF0069 = "ERRC0076";
        /// <summary>msg本文:ファイルダウンロードに失敗しました: ${parseUnknownErrors(error).join('\n')}</summary>
        internal const string C_INF0070 = "ERRC0079";
        /// <summary>msg本文:データ型 '${localReposItem.dataTypeKey}' の保存処理が定義されていません。</summary>
        internal const string C_INF0071 = "ERRC0046";
        /// <summary>msg本文:保存しました。</summary>
        internal const string C_INF0072 = "INFC0001";
        /// <summary>msg本文:一部のデータの保存に失敗しました。</summary>
        internal const string C_INF0073 = "ERRC0080";
        /// <summary>msg本文:変更を確定します。よろしいですか？</summary>
        internal const string C_INF0074 = "CNFC0009";
        /// <summary>msg本文:変更を取り消します。よろしいですか？</summary>
        internal const string C_INF0075 = "CNFC0010";
        /// <summary>msg本文:Failuer to parse local storage value as '${handler.storageKey}'.</summary>
        internal const string C_INF0076 = "ERRC0047";
        /// <summary>msg本文:データベースを開けませんでした。</summary>
        internal const string C_INF0077 = "ERRC0048";
        /// <summary>msg本文:データベースが初期化されていません。</summary>
        internal const string C_INF0078 = "ERRC0049";
        /// <summary>msg本文:データベースが初期化されていません。</summary>
        internal const string C_INF0079 = "ERRC0049";
        /// <summary>msg本文:データベースが初期化されていません。</summary>
        internal const string C_INF0080 = "ERRC0049";
        /// <summary>msg本文:保存しました。</summary>
        internal const string C_INF0081 = "INF0081";
        /// <summary>msg本文:「 ${currentDb?.Name ?? 'DB'} 」を再作成します。データは全て削除されます。よろしいですか？</summary>
        internal const string C_INF0082 = "CNFC0011";
        /// <summary>msg本文:ローカルリポジトリの初期化に失敗しました: ${error}</summary>
        internal const string C_INF0083 = "ERRC0071";
        /// <summary>msg本文:DB再作成は成功しましたがログインに失敗しました。</summary>
        internal const string C_INF0084 = "ERRC0081";
        /// <summary>msg本文:DBを再作成しましたがダミーデータ作成に失敗しました。</summary>
        internal const string C_INF0085 = "ERRC0072";
        /// <summary>msg本文:DBを再作成しました。</summary>
        internal const string C_INF0086 = "INFC0002";
        /// <summary>msg本文:宛先を選択してください。</summary>
        internal const string C_INF0087 = "ERRC0050";
        /// <summary>msg本文:送信しました。</summary>
        internal const string C_INF0088 = "INFC0001";
        /// <summary>msg本文:エラーが発生しました。</summary>
        internal const string C_INF0089 = "ERRC0082";
        /// <summary>msg本文:既読にする行を選択してください。</summary>
        internal const string C_INF0090 = "ERRC0051";
        /// <summary>msg本文:未読にする行を選択してください。</summary>
        internal const string C_INF0091 = "ERRC0052";
        /// <summary>msg本文:データ読み込みに失敗しました。</summary>
        internal const string C_INF0092 = "ERRC0067";
        /// <summary>msg本文:通知が見つかりません（ID: ${key0}）</summary>
        internal const string C_INF0093 = "ERRC0053";
        /// <summary>msg本文:通信でエラーが発生しました。</summary>
        internal const string C_INF0094 = "ERRC0083";
        /// <summary>msg本文:通信でエラーが発生しました。</summary>
        internal const string C_INF0095 = "ERRC0083";
        /// <summary>msg本文:パスワードを変更します。よろしいですか？</summary>
        internal const string C_INF0096 = "CNFC0012";
        /// <summary>msg本文:パスワードを変更しました。</summary>
        internal const string C_INF0097 = "INFC0004";
        /// <summary>msg本文:【パスワードポリシー】・{{MIN_LENGTH}}桁以上で設定してください。・下記より{{REQUIRED_CHAR_TYPES}}種類以上を合わせた値にしてください。{{policy}}</summary>
        internal const string C_INF0098 = "ERRC0054";
        /// <summary>msg本文:【パスワード設定の留意事項】・パスワードは同じ日に複数回の変更はできません。・直前に利用していたパスワードと同じパスワードは設定できません。</summary>
        internal const string C_INF0099 = "ERRC0055";
        /// <summary>msg本文:ユーザーが存在しません。ユーザーID:{userId}</summary>
        internal const string C_INF0100 = "ERRC0056";
        /// <summary>msg本文:すでに更新されています。ユーザーID:{userId}</summary>
        internal const string C_INF0101 = "ERRC0057";
        /// <summary>msg本文:確認用のパスワードと一致しません。</summary>
        internal const string C_INF0102 = "ERRC0058";
        /// <summary>msg本文:パスワードは{PasswordPolicy.MIN_LENGTH}文字以上かつ、{msg}の内、{PasswordPolicy.REQUIRED_CHAR_TYPES}種類以上含むように設定してください。</summary>
        internal const string C_INF0103 = "ERRC0059";
        /// <summary>msg本文:変更禁止期間です。時間を置いてから再度設定してください。</summary>
        internal const string C_INF0104 = "ERRC0060";
        /// <summary>msg本文:初回パスワードと同じパスワードです。初回パスワードと再発行前と異なるパスワードを設定してください。</summary>
        internal const string C_INF0105 = "ERRC0061";
        /// <summary>msg本文:再発行前のパスワードと同じパスワードです。初回パスワードと再発行前と異なるパスワードを設定してください。</summary>
        internal const string C_INF0106 = "ERRC0062";
        /// <summary>msg本文:前回と同じパスワードです。前回と異なるパスワードを設定してください。</summary>
        internal const string C_INF0107 = "ERRC0063";
        /// <summary>msg本文:他のユーザーが更新しました。ユーザーID:{userId}</summary>
        internal const string C_INF0108 = "ERRC0084";
        /// <summary>msg本文:ユーザーが存在しません。ユーザーID:{userId}</summary>
        internal const string C_INF0109 = "ERRC0056";
        /// <summary>msg本文:他のユーザーが更新しました。ユーザーID:{userId}</summary>
        internal const string C_INF0110 = "ERRC0084";
        /// <summary>msg本文:通知'{type}'は宛先が1件も無いので送信されませんでした。</summary>
        internal const string C_INF0111 = "ERRC0064";
        /// <summary>msg本文:メールサーバーのホストが設定されていません。</summary>
        internal const string C_INF0112 = "ERRC0065";
        /// <summary>msg本文:メールサーバーのポートが設定されていません。</summary>
        internal const string C_INF0113 = "ERRC0066";
        /// <summary>msg本文:メール通知の送信に失敗しました: {msg}</summary>
        internal const string C_INF0114 = "ERRC0085";

        #endregion 自動生成されるソース内で使用されるメッセージのID（メッセージ一覧の台帳と合わせる必要あり）

        private class XDocumentAndPath {
            public required XDocument XDocument { get; init; }
            public required string FilePath { get; init; }
        }

        void IFeature.GenerateCode(CodeRenderingContext context) {

            List<Message> messages = new List<Message>();
            string SolutionRoot = context.GeneratedProject.SolutionRoot;
            string MessagesXmlPath = Path.Combine(SolutionRoot, "nijo.メッセージ一覧.xml");
            XDocument? MessagesXml = null;

            if (System.IO.File.Exists(MessagesXmlPath)) {
                var xDocuments = GetXDocumentsRecursively(MessagesXmlPath).ToList();
                foreach (var el in xDocuments[0].XDocument.Root?.Elements() ?? []) {
                    if ((string)el.FirstAttribute! == "message-list") {
                        foreach (var element in el.Elements()) {
                            // 各パラメータの説明を取得
                            List<string> Parameters = new List<string>();
                            foreach (var attr in element.Attributes()) {
                                Parameters.Add(attr.Value);
                            }

                            // メッセージテンプレートが複数行から成る場合、メッセージ一覧のxmlでは以下のように定義される。
                            // このとき、xml中のインデントもそのままテンプレートになってしまうため、インデントを除外してパースする。
                            //   <ERRxxxxxx>
                            //     メッセージ1行目
                            //     メッセージ2行目
                            //   </ERRxxxxxx>
                            string template;

                            // LFとスペースで始まり、LFとスペースで終わるなら複数行から成るテンプレートと判断
                            var isMultiLine = Regex.Match(element.Value, @"^\n([ ]+).*\n[ ]+$", RegexOptions.Singleline);
                            if (!isMultiLine.Success) {
                                // 1行から成るメッセージ（大半はこちら）
                                template = element.Value;

                            } else {
                                // 複数行から成るメッセージ
                                var indent = isMultiLine.Groups[1].Value;
                                var builder = new StringBuilder();
                                foreach (var line in element.Value.Trim().Split('\n')) {
                                    builder.Append(line.StartsWith(indent)
                                        ? line.Substring(indent.Length)
                                        : line);
                                    builder.Append("\\r\\n"); // C#, TypeScriptともにCRLFで問題ない
                                }
                                template = builder.ToString();
                            }

                            // メッセージ登録
                            Message message = new Message {
                                Id = element.Name.LocalName,
                                Template = template,
                                Parameters = Parameters.ToArray(),
                            };
                            messages.Add(message);
                        }
                    }
                }
            } else {
                throw new FileNotFoundException($"{MessagesXmlPath}が存在しません。");
            }

            IEnumerable<XDocumentAndPath> GetXDocumentsRecursively(string xmlFilePath) {
                var xDocument = XDocument.Load(xmlFilePath);
                if (xmlFilePath == MessagesXmlPath) MessagesXml = xDocument;
                yield return new() { XDocument = xDocument, FilePath = xmlFilePath };

                // <Include Path="(略)" /> で他のXMLファイルを読み込む
                foreach (var el in xDocument.Root?.Elements() ?? []) {
                    if (el.Name.LocalName != AppSchemaXml.INCLUDE) continue;

                    var path = el.Attribute(AppSchemaXml.PATH)?.Value;
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

            // C#側の静的関数をレンダリングする
            context.CoreLibrary.UtilDir(dir => {
                dir.Generate(new() {
                    FileName = "MSG.cs",
                    RenderContent = ctx => {

                        return $$"""
                            namespace {{ctx.Config.RootNamespace}};

                            /// <summary>
                            /// メッセージマスタ（メッセージ定数）。
                            /// 画面上に表示するメッセージ等はこの関数から取得してください。
                            /// </summary>
                            public static partial class {{CS_CLASS_NAME}} {
                            {{messages.SelectTextTemplate(msg => $$"""
                                /// <summary>
                                /// {{msg.Template}}
                                /// </summary>
                            {{msg.Parameters.SelectTextTemplate((description, i) => $$"""
                                /// <param name="arg{{i}}">{{description}}</param>
                            """)}}
                                public static string {{msg.Id}}({{msg.Parameters.Select((_, i) => $"string arg{i}").Join(", ")}}) => $"{{GetTemplateLiteral(msg, E_CsTs.CSharp)}}";
                            """)}}
                            }
                            """;
                    },
                });
            });

            // TypeScript側の静的関数をレンダリングする
            context.ReactProject.UtilDir(dir => {
                dir.Generate(new() {
                    FileName = "MSG.ts",
                    RenderContent = ctx => {

                        return $$"""
                            /**
                             * メッセージマスタ（メッセージ定数）。
                             * 画面上に表示するメッセージ等はこの関数から取得してください。
                             */
                            export const {{TS_CONTAINER_OBJECT_NAME}} = {
                            {{messages.SelectTextTemplate(msg => $$"""
                              /**
                               * {{msg.Template}}
                            {{msg.Parameters.SelectTextTemplate((description, i) => $$"""
                               * @param arg{{i}} {{description}}
                            """)}}
                               */
                              {{msg.Id}}: ({{msg.Parameters.Select((_, i) => $"arg{i}: string").Join(", ")}}) => `{{GetTemplateLiteral(msg, E_CsTs.TypeScript)}}`,
                            """)}}
                            }
                            """;
                    },
                });
            });

            // パラメータ変数を台帳の形式からソースコードの形式に変換する関数。
            // 例　台帳　: "{0}文字以下で入力してください。"
            // 　　ソース: "{arg0}以下で入力してください。"
            static string GetTemplateLiteral(Message msg, E_CsTs csts) {
                var template = msg.Template;
                for (int i = 0; i < msg.Parameters.Length; i++) {
                    var before = "{" + i.ToString() + "}";
                    var after = csts == E_CsTs.CSharp
                        ? "{arg" + i.ToString() + "}"
                        : "${arg" + i.ToString() + "}";

                    var replaced = template.Replace(before, after);

                    // 台帳の記載に誤りがある場合はここでエラー
                    if (template == replaced)
                        throw new InvalidOperationException($"メッセージ{msg.Id}のテンプレート中に変数{before}が入るべき箇所がありません。");

                    template = replaced;
                }

                // 台帳の記載に誤りがある場合はここでエラー
                if (NumberInsideCurlyBrace().IsMatch(template))
                    throw new InvalidOperationException($"メッセージ{msg.Id}のパラメータは{msg.Parameters.Length + 1}個定義されていますが、テンプレート文字列中にはそれより多い変数が含まれています。");

                // テンプレート文言に " や ` が出てきた場合はエスケープ
                return csts == E_CsTs.CSharp
                    ? template.Replace("\"", "\\\"")
                    : template.Replace("`", "\\`");
            }
        }

        private class Message {
            /// <summary>
            /// メッセージの番号。メソッド名や関数名になります。
            /// </summary>
            internal required string Id { get; init; }
            /// <summary>
            /// メッセージ文言。
            /// </summary>
            internal required string Template { get; init; }
            /// <summary>
            /// テンプレート文字列内に埋め込まれる変数の意味
            /// </summary>
            internal required string[] Parameters { get; init; }
        }

        /// <summary>
        /// {0} , {1} , ... などをひっかけるための正規表現
        /// </summary>
        [GeneratedRegex(@"\{[0-9]+\}")]
        private static partial Regex NumberInsideCurlyBrace();
    }
}
