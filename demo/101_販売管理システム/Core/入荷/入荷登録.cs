using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Execute入荷登録Async(入荷詳細DisplayData param, IPresentationContextWithReturnValue<入荷登録ReturnValueDisplayData, 入荷詳細Messages> context) {
        // 権限チェック
        if (LoginUser == null || !LoginUser.CanUse入荷登録) {
            context.Messages.AddError("入荷担当のみ実行可能です。");
            return;
        }

        // 入荷ヘッダ
        var newId = $"{CurrentTime:yyyyMMdd}-{Guid.NewGuid().GetHashCode():X8}";
        var header = DbContext.StartCreating入荷();
        header.入荷ID = newId;
        header.入荷日時 = param.入荷日時;
        header.担当者 = new() { 従業員番号 = param.担当者.従業員番号 };
        header.備考 = param.備考;
        header.GetValidationErrors().CopyTo(context.Messages);

        // 明細は1件以上必須
        if (param.入荷商品一覧.Count == 0) {
            context.Messages.入荷商品一覧.AddError("入荷商品を1件以上登録してください。");
        }

        // 入荷明細
        var details = new List<入荷明細CreateCommand>();
        for (var i = 0; i < param.入荷商品一覧.Count; i++) {
            var item = param.入荷商品一覧[i];
            var message = context.Messages.入荷商品一覧[i];

            var detail = DbContext.StartCreating入荷明細();
            detail.入荷明細ID = Guid.NewGuid().ToString();
            detail.入荷 = new() { 入荷ID = newId };
            detail.在庫調整 = null;
            detail.商品 = new() { 商品SEQ = item.商品.商品SEQ };
            detail.仕入単価_税抜 = item.仕入単価_税抜;
            detail.消費税区分 = item.消費税区分;
            detail.入荷数量 = item.数量;
            detail.残数量 = item.数量; // 入荷時は残数量＝入荷数量
            detail.備考 = item.備考;

            var errors = detail.GetValidationErrors();
            errors.CopyTo(message);
            errors.入荷数量.CopyTo(message.数量);
            details.Add(detail);
        }

        if (context.Messages.HasError() || context.ValidationOnly) return;

        // ヘッダと明細で1つのトランザクション
        await using var tran = await BeginTransactionAsync();

        var headerResult = await DbContext.TrySaveAsync(header);
        if (!headerResult.Success) {
            headerResult.Errors.CopyTo(context.Messages);
            return;
        }

        var detailResult = await DbContext.TrySaveAsync(details);
        if (!detailResult.Success) {
            detailResult.Errors.CopyTo(context.Messages);
            return;
        }
        await Notify在庫数量Async(details.Select(x => x.商品?.商品SEQ));

        await tran.CommitAsync();
        context.ReturnValue.入荷ID = newId;
        context.Messages.AddInfo("入荷登録が完了しました。");
    }
}
