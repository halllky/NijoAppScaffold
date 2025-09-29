using System.Text.Json.Nodes;

namespace MyApp;

public static class MessageContainerExtension {
    /// <summary>
    /// このオブジェクトおよび子孫オブジェクトが持っているメッセージを再帰的に列挙します。
    /// メッセージは、パスとメッセージの組み合わせで返されます。
    /// パスは、このオブジェクトから子孫オブジェクトまでのパスをピリオドで繋いだものになります。
    /// </summary>
    /// <param name="messageContainer">メッセージコンテナ</param>
    /// <returns>メッセージ</returns>
    public static IEnumerable<string> GetAllMessages(this IReadOnlyMessageContainer messageContainer) {
        foreach (var container in messageContainer.DescendantsAndSelf()) {
            foreach (var err in container.Errors) {
                yield return $"{PathToString(container.Path)}: {err}";
            }
            foreach (var warn in container.Warns) {
                yield return $"{PathToString(container.Path)}: {warn}";
            }
            foreach (var info in container.Infos) {
                yield return $"{PathToString(container.Path)}: {info}";
            }
        }

        // パスをピリオドで繋いだ文字列に変換する。
        // 半角数値のみから構成されるキー名は配列インデックスを意味するので「(i + 1)行目」というように変換する。
        static string PathToString(IEnumerable<string> path) {
            var result = new List<string>();
            foreach (var strKey in path) {
                if (int.TryParse(strKey, out var intKey)) {
                    result.Add($"{intKey + 1}行目");
                } else {
                    result.Add(strKey);
                }
            }
            return string.Join(".", result);
        }
    }

    /// <summary>
    /// このインスタンスをJsonNode型に変換します。
    /// <list type="bullet">
    /// <item>このメソッドは、このオブジェクトおよび子孫オブジェクトが持っているメッセージを再帰的に集め、以下のようなJSONオブジェクトに変換します。</item>
    /// <item>メッセージコンテナは、エラー、警告、インフォメーションの3種類のメッセージを、それぞれ配列として持ちます。</item>
    /// <item>エラーだけ持っているなど、一部の種類のメッセージのみ持っている場合、他の種類の配列は配列自体が存在しなくなります。</item>
    /// <item>子要素は children という名前のオブジェクトにまとめて格納されます。</item>
    /// <item>
    /// 3種類のメッセージのいずれも持っていない項目のプロパティは存在しません。
    /// 例えば以下のオブジェクトで「項目A」「項目B」以外に「項目X」が存在するが、Xにはメッセージが発生していない場合、Xのプロパティは存在しません。
    /// </item>
    /// <item>ネストされたオブジェクトのメッセージも生成されます。（下記「子オブジェクトのメッセージ」）</item>
    /// <item>
    /// 配列は、配列インデックスをキーとしたオブジェクトになります。（下記「子配列のメッセージ」）
    /// 配列インデックスか否かは、 children 直下のオブジェクトのキーが半角整数のみから成るか否かで判定できます。
    /// </item>
    /// </list>
    /// <code>
    /// {
    ///   "error": ["xxxがエラーです"],
    ///   "children": {
    ///     "項目A": { "error": ["xxxがエラーです"], "warn": ["xxxという警告があります"], "info": ["xxxという情報があります"] },
    ///     "項目B": { "error": ["xxxがエラーです", "yyyがエラーです"], "warn": ["xxxという警告があります"], "info": ["xxxという情報があります"] },
    ///     "子オブジェクトのメッセージ": {
    ///       "children": {
    ///         "項目C": { "error": ["xxxがエラーです"] },
    ///         "項目D": { "error": ["xxxがエラーです"] },
    ///       },
    ///     },
    ///     "子配列のメッセージ": {
    ///       "error": ["xxxがエラーです"],
    ///       "children": {
    ///         "1": {
    ///           "children": {
    ///             "項目E": { "error": ["xxxがエラーです"] },
    ///           },
    ///         },
    ///         "5": {
    ///           "error": ["xxxがエラーです"],
    ///           "children": {
    ///             "項目E": { "error": ["xxxがエラーです"] },
    ///           },
    ///         },
    ///       },
    ///     }
    ///   }
    /// }
    /// </code>
    /// </summary>
    public static JsonObject ToJsonObject(this IReadOnlyMessageContainer messageContainer) {
        return ToJsonObjectPrivate(messageContainer) ?? [];

        static JsonObject? ToJsonObjectPrivate(IReadOnlyMessageContainer current) {
            var result = new JsonObject();

            if (current.Errors.Any()) {
                var strArray = new JsonArray();
                foreach (var str in current.Errors) strArray.Add(str);
                result["error"] = strArray;
            }
            if (current.Warns.Any()) {
                var strArray = new JsonArray();
                foreach (var str in current.Warns) strArray.Add(str);
                result["warn"] = strArray;
            }
            if (current.Infos.Any()) {
                var strArray = new JsonArray();
                foreach (var str in current.Infos) strArray.Add(str);
                result["info"] = strArray;
            }

            var children = new JsonObject();
            foreach (var child in current.Children) {
                var childJson = ToJsonObjectPrivate(child.Value);
                if (childJson != null) children[child.Key] = childJson;
            }
            if (children.Count > 0) result["children"] = children;

            return result.Count == 0 ? null : result;
        }
    }
}