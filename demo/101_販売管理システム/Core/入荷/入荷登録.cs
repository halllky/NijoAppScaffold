using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute入荷登録Async(入荷詳細DisplayData param, IPresentationContextWithReturnValue<入荷登録ReturnValueDisplayData, 入荷詳細Messages> context) {
        // 権限チェック
        if (LoginUser == null || !LoginUser.CanUse入荷登録) {
            context.Messages.AddError("入荷担当のみ実行可能です。");
            return;
        }

        // ヘッダと明細で1つのトランザクション
        await using var tran = await BeginTransactionAsync();

        // 入荷ヘッダの登録
        var newId = $"{CurrentTime:yyyyMMdd}-{Guid.NewGuid().GetHashCode():X8}";
        var headerResult = await Create入荷Async(new() {
            入荷ID = newId,
            入荷日時 = param.Values.入荷日時,
            担当者 = new() { 従業員番号 = param.Values.担当者.従業員番号 },
            備考 = param.Values.備考,
        }, context);

        // 明細は1件以上必須
        var hasErrorInDetail = false;
        if (param.入荷商品一覧.Count == 0) {
            context.Messages.入荷商品一覧.AddError("入荷商品を1件以上登録してください。");
            hasErrorInDetail = true;
        }

        // 入荷明細の処理
        for (var i = 0; i < param.入荷商品一覧.Count; i++) {
            var item = param.入荷商品一覧[i];
            var message = context.Messages.入荷商品一覧[i];

            var detailResult = await Create入荷明細Async(new() {
                入荷明細ID = Guid.NewGuid().ToString(),
                入荷 = new() { 入荷ID = newId },
                在庫調整 = null,
                商品 = new() { 商品SEQ = item.Values.商品.商品SEQ },
                仕入単価_税抜 = item.Values.仕入単価_税抜,
                消費税区分 = item.Values.消費税区分,
                入荷数量 = item.Values.数量,
                残数量 = item.Values.数量, // 入荷時は残数量＝入荷数量
                備考 = item.Values.備考,
            }, context, message);

            if (detailResult.Result != DataModelSaveResultType.Completed) {
                hasErrorInDetail = true;
            }
        }

        // すべてのヘッダ・明細の登録が成功したらコミット
        if (!context.ValidationOnly && headerResult.Result == DataModelSaveResultType.Completed && !hasErrorInDetail) {
            await tran.CommitAsync();
            context.ReturnValue.Values.入荷ID = newId;
            context.Messages.AddInfo("入荷登録が完了しました。");
        }
    }
}
