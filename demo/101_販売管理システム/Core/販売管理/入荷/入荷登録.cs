
namespace MyApp;

partial class OverridedApplicationService {
    protected override async Task<入荷登録ReturnValueDisplayData?> OnExecute入荷登録Async(入荷詳細DisplayData param, 入荷詳細Messages messages, OperationContext context) {
        // 入荷ヘッダ
        var newId = $"{CurrentTime:yyyyMMdd}-{Guid.NewGuid().GetHashCode():X8}";
        var header = DbContext.StartCreating入荷();
        header.入荷ID = newId;
        header.入荷日時 = param.入荷日時;
        header.担当者 = new() { 従業員番号 = param.担当者.従業員番号 };
        header.備考 = param.備考;
        header.TryValidate(out var validatedHeader, out var headerErrors);
        headerErrors.CopyTo(messages);

        // 明細は1件以上必須
        if (param.入荷商品一覧.Count == 0) {
            messages.入荷商品一覧.AddError(MSG_DEMO101.ArrivalItemRequiredError());
        }

        // 入荷明細
        var details = new List<ValidatedCommand<入荷明細UpdateCommand>>();
        for (var i = 0; i < param.入荷商品一覧.Count; i++) {
            var item = param.入荷商品一覧[i];
            var message = messages.入荷商品一覧[i];

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

            if (detail.TryValidate(out var validated, out var errors)) {
                details.Add(validated);
            }
            errors.CopyTo(message);
            errors.入荷数量.CopyTo(message.数量);
        }

        if (validatedHeader == null || messages.HasError() || context.ValidationOnly) return null;

        // ヘッダと明細で1つのトランザクション
        await using var tran = await BeginTransactionAsync();

        var headerResult = await DbContext.TrySaveAsync(validatedHeader);
        if (!headerResult.Success) {
            headerResult.Errors.CopyTo(messages);
            return null;
        }

        var detailResult = await DbContext.TrySaveAsync(details);
        if (!detailResult.Success) {
            detailResult.Errors.CopyTo(messages);
            return null;
        }
        await Notify在庫数量Async(detailResult.Updated.Select(x => x.商品?.商品SEQ));

        await tran.CommitAsync();
        messages.AddInfo(MSG_DEMO101.ArrivalRegisteredInfo());
        return new() { 入荷ID = newId };
    }
}
