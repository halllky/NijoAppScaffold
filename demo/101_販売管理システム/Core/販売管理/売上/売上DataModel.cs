
namespace MyApp;

partial class OverridedApplicationService {

    // 売上とその明細テーブルがどこから新規登録される場合であっても
    // 絶対に整合していなければならない内容を記述する。
    private static void Validate売上新規(売上CreateCommand command, 売上SaveCommandMessages messages) {
        // 明細は必ず1件以上
        if (command.売上の売上明細.Count == 0) {
            messages.売上の売上明細.AddError(MSG_DEMO101.SalesDetailMinCountError());
        }

        // 新規登録時に取消明細を追加することはできない
        for (var i = 0; i < command.売上の売上明細.Count; i++) {
            var detail = command.売上の売上明細[i];
            if (detail.区分 == 売上明細区分.取消) {
                messages.売上の売上明細[i].区分.AddError(MSG_DEMO101.CancelDetailOnCreateError());
            }
        }

        // 明細単位のチェック
        for (var i = 0; i < command.売上の売上明細.Count; i++) {
            var detail = command.売上の売上明細[i];

            // 明細の個数は0以上でなければならない
            if (detail.売上数量 < 0) {
                messages.売上の売上明細[i].売上数量.AddError(MSG_DEMO101.DetailQuantityNegativeError());
            }

            // 金額は0以上
            if (detail.売上総額_税込 < 0) {
                messages.売上の売上明細[i].売上総額_税込.AddError(MSG_DEMO101.AmountMustBeZeroError());
            }
        }
    }

    // 売上とその明細テーブルがどこから更新される場合であっても
    // 絶対に整合していなければならない内容を記述する。
    private static void Validate売上修正(売上UpdateCommand command, IReadOnlyList<売上明細の更新前の値> before, 売上SaveCommandMessages messages) {

        // 既存の明細は在庫引当済みのため削除できない
        var deleted = before
            .Where(d => !command.売上の売上明細.Any(nd => nd.明細ID == d.明細ID));
        if (deleted.Any()) {
            messages.売上の売上明細.AddError(MSG_DEMO101.AllocatedDetailDeleteError());
        }

        // 明細単位のチェック
        for (var i = 0; i < command.売上の売上明細.Count; i++) {
            var newDetail = command.売上の売上明細[i];
            var oldDetail = before.SingleOrDefault(d => d.明細ID == newDetail.明細ID);

            if (oldDetail == null) {

                // 新規明細の場合、売上なら金額は0以上、取消なら0以下
                if (newDetail.区分 == 売上明細区分.売上 && newDetail.売上総額_税込 < 0) {
                    messages.売上の売上明細[i].売上総額_税込.AddError(MSG_DEMO101.SalesAmountNegativeError());
                }
                if (newDetail.区分 == 売上明細区分.取消 && newDetail.売上総額_税込 > 0) {
                    messages.売上の売上明細[i].売上総額_税込.AddError(MSG_DEMO101.CancelAmountPositiveError());
                }

                // 明細の個数は0以上でなければならない
                if (newDetail.売上数量 < 0) {
                    messages.売上の売上明細[i].売上数量.AddError(MSG_DEMO101.DetailQuantityNegativeError());
                }

            } else {

                // 既存の明細は在庫引当済みのため変更不可
                if (newDetail.区分 != oldDetail.区分)
                    messages.売上の売上明細[i].区分.AddError(MSG_DEMO101.AllocatedDetailChangeError());

                if (newDetail.商品?.商品SEQ != oldDetail.商品SEQ)
                    messages.売上の売上明細[i].商品.AddError(MSG_DEMO101.AllocatedDetailChangeError());

                if (newDetail.売上数量 != oldDetail.売上数量)
                    messages.売上の売上明細[i].売上数量.AddError(MSG_DEMO101.AllocatedDetailChangeError());

                if (newDetail.売上総額_税込 != oldDetail.売上総額_税込)
                    messages.売上の売上明細[i].売上総額_税込.AddError(MSG_DEMO101.AllocatedDetailChangeError());
            }
        }
    }

    /// <summary>
    /// 更新コマンドを書き換える前の売上明細の値を控えたもの
    /// </summary>
    private sealed record 売上明細の更新前の値(string? 明細ID, 売上明細区分? 区分, int? 商品SEQ, int? 売上数量, int? 売上総額_税込);
}
