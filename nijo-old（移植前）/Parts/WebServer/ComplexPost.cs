using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.WebServer;
internal class ComplexPost {

    internal static SourceFile Render() {
        return new SourceFile {
            FileName = "ComplexPost.cs",
            RenderContent = ctx => $$"""
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace {{ctx.Config.RootNamespace}} {

    /// <summary>
    /// Reactフック側の処理と組み合わせて複雑な挙動を実現するPOSTリクエスト。
    /// 例えば後述のような挙動を実現する。
    /// 詳細な挙動を調べる場合はReact側のcomplexPost関連のソースも併せて参照のこと。
    /// 
    /// <list type="bullet">
    /// <item>ブラウザからサーバーへのリクエストで入力フォームの内容とファイル内容を同時に送信し（multipart/form-data）、サーバー側ではそれを意識せず利用できるようにする</item>
    /// <item>「～ですがよろしいですか？」の確認ダイアログの表示と、それがOKされたときに同じ内容のリクエストを再送信する</item>
    /// <item>POSTレスポンスの結果を React hook forms のsetErrorを利用して画面上の各項目の脇に表示</item>
    /// <item>POSTレスポンスで返されたファイルのダウンロードを自動的に開始する</item>
    /// <item>POSTレスポンスのタイミングで React Router を使った別画面へのリダイレクト</item>
    /// </list>
    /// </summary>
    [ModelBinder(BinderType = typeof(GenericComplexPostRequestBinder))]
    public class ComplexPostRequest<T> : ComplexPostRequest {
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        /// <summary>
        /// 入力フォームの内容
        /// </summary>
        public T Data { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
    }


    /// <inheritdoc cref="ComplexPostRequest{T}"/>
    [ModelBinder(BinderType = typeof(ComplexPostRequestBinder))]
    public class ComplexPostRequest {
        /// <summary>
        /// 「～ですがよろしいですか？」の確認を無視します。
        /// </summary>
        public bool IgnoreConfirm { get; set; }


        #region HTTPリクエストとC#クラスの変換
        /// <summary>
        /// HTTPリクエストの内容をパースして <see cref="ComplexPostRequest{T}"/> クラスのインスタンスを作成します。
        /// </summary>
        protected class GenericComplexPostRequestBinder : IModelBinder {

            public Task BindModelAsync(ModelBindingContext bindingContext) {
                try {
                    // data
                    var dataJson = bindingContext.HttpContext.Request.Form[PARAM_DATA];
                    var dataType = bindingContext.ModelType.GenericTypeArguments[0];
                    var parsedData = JsonSerializer.Deserialize(dataJson!, dataType, Util.GetJsonSrializerOptions());

                    // ignoreConfirm
                    var ignoreConfirm = bindingContext.HttpContext.Request.Form.TryGetValue(PARAM_IGNORE_CONFIRM, out var strIgnoreConfirm)
                        ? (bool.TryParse(strIgnoreConfirm, out var b) ? b : false)
                        : false;

                    // パラメータクラスのインスタンスを作成
                    var instance = Activator.CreateInstance(bindingContext.ModelType) ?? throw new NullReferenceException();
                    bindingContext.ModelType.GetProperty(nameof(ComplexPostRequest<object>.Data))!.SetValue(instance, parsedData);
                    bindingContext.ModelType.GetProperty(nameof(IgnoreConfirm))!.SetValue(instance, ignoreConfirm);

                    bindingContext.Result = ModelBindingResult.Success(instance);
                    return Task.CompletedTask;

                } catch (Exception) {
                    bindingContext.Result = ModelBindingResult.Failed();
                    return Task.CompletedTask;
                }
            }
        }

        /// <summary>
        /// HTTPリクエストの内容をパースして <see cref="ComplexPostRequest"/> クラスのインスタンスを作成します。
        /// </summary>
        protected class ComplexPostRequestBinder : IModelBinder {
            public Task BindModelAsync(ModelBindingContext bindingContext) {

                var instance = new ComplexPostRequest();

                instance.IgnoreConfirm = bindingContext.HttpContext.Request.Form.TryGetValue(PARAM_IGNORE_CONFIRM, out var strIgnoreConfirm)
                    ? (bool.TryParse(strIgnoreConfirm, out var b) ? b : false)
                    : false;

                bindingContext.Result = ModelBindingResult.Success(instance);

                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// multipart/form-data 内の入力内容データJSONの項目のキー。
        /// この名前はReact側の処理と一致させておく必要がある。
        /// </summary>
        internal const string PARAM_DATA = "data";
        /// <summary>
        /// この名前はReact側の処理と一致させておく必要がある。
        /// </summary>
        private const string PARAM_IGNORE_CONFIRM = "ignoreConfirm";
        #endregion HTTPリクエストとC#クラスの変換
    }
}
""",
        };
    }

}
