
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : BoolMember.cs
// ※ このファイルを生成したメソッド: RenderStaticSources

using System;
using System.Text.Json.Serialization;

namespace MyApp {
    public class BooleanSearchCondition {
        [JsonPropertyName("trueのみ")]
        public bool Trueのみ { get; set; }
        [JsonPropertyName("falseのみ")]
        public bool Falseのみ { get; set; }

        public bool AnyChecked() => Trueのみ || Falseのみ;
    }
}
