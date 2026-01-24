
namespace MyApp;

partial class OverridedApplicationService {

    // 売上とその明細テーブルがどこから新規登録される場合であっても
    // 絶対に整合していなければならない内容を記述する。
    public override void OnBeforeCreate売上(売上CreateCommand command, I売上SaveCommandMessages messages, IPresentationContext context) {
        // 明細は必ず1件以上
        if (command.売上の売上明細.Count == 0) {
            messages.売上の売上明細.AddError("売上明細は1件以上必要です。");
        }

        // 新規登録時に取消明細を追加することはできない
        for (var i = 0; i < command.売上の売上明細.Count; i++) {
            var detail = command.売上の売上明細[i];
            if (detail.区分 == 売上明細区分.取消) {
                messages.売上の売上明細[i].区分.AddError("新規登録時に取消明細を追加することはできません。");
            }
        }

        // 明細単位のチェック
        for (var i = 0; i < command.売上の売上明細.Count; i++) {
            var detail = command.売上の売上明細[i];

            // 明細の個数は0以上でなければならない
            if (detail.売上数量 < 0) {
                messages.売上の売上明細[i].売上数量.AddError("明細の個数は0以上でなければなりません。");
            }

            // 金額は0以上
            if (detail.売上総額_税込 < 0) {
                messages.売上の売上明細[i].売上総額_税込.AddError("金額は0のみ指定可能です。");
            }
        }
    }

    // 売上とその明細テーブルがどこから更新される場合であっても
    // 絶対に整合していなければならない内容を記述する。
    public override void OnBeforeUpdate売上(売上UpdateCommand command, 売上DbEntity oldValue, I売上SaveCommandMessages messages, IPresentationContext context) {

        // 既存の明細は在庫引当済みのため削除できない
        var deleted = oldValue.売上の売上明細
            .Where(d => !command.売上の売上明細.Any(nd => nd.明細ID == d.明細ID));
        if (deleted.Any()) {
            messages.売上の売上明細.AddError("在庫引当済みのため、既存の明細を削除することはできません。取消明細を追加して調整してください。");
        }

        // 明細単位のチェック
        for (var i = 0; i < command.売上の売上明細.Count; i++) {
            var newDetail = command.売上の売上明細[i];
            var oldDetail = oldValue.売上の売上明細.SingleOrDefault(d => d.明細ID == newDetail.明細ID);

            if (oldDetail == null) {

                // 新規明細の場合、売上なら金額は0以上、取消なら0以下
                if (newDetail.区分 == 売上明細区分.売上 && newDetail.売上総額_税込 < 0) {
                    messages.売上の売上明細[i].売上総額_税込.AddError("売上明細の金額は0以上でなければなりません。");
                }
                if (newDetail.区分 == 売上明細区分.取消 && newDetail.売上総額_税込 > 0) {
                    messages.売上の売上明細[i].売上総額_税込.AddError("取消明細の金額は0以下でなければなりません。");
                }

                // 明細の個数は0以上でなければならない
                if (newDetail.売上数量 < 0) {
                    messages.売上の売上明細[i].売上数量.AddError("明細の個数は0以上でなければなりません。");
                }

            } else {

                // 既存の明細は在庫引当済みのため変更不可
                if (newDetail.区分 != oldDetail.区分)
                    messages.売上の売上明細[i].区分.AddError($"在庫引当済みのため変更できません。新しい明細を追加して調整してください。");

                if (newDetail.商品?.商品SEQ != oldDetail.商品_商品SEQ)
                    messages.売上の売上明細[i].商品.AddError($"在庫引当済みのため変更できません。新しい明細を追加して調整してください。");

                if (newDetail.売上数量 != oldDetail.売上数量)
                    messages.売上の売上明細[i].売上数量.AddError($"在庫引当済みのため変更できません。新しい明細を追加して調整してください。");

                if (newDetail.売上総額_税込 != oldDetail.売上総額_税込)
                    messages.売上の売上明細[i].売上総額_税込.AddError($"在庫引当済みのため変更できません。新しい明細を追加して調整してください。");
            }
        }
    }
}
